using System;
using System.Reflection;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Security.Permissions;
using System.Security.Policy;

/// Manages the scripting.
/// Scripts run in a new AppDomain with the help of the script-helper helper library.
public class ScriptManager {

	private const string HELPER_ASSEMBLY = "script-helper.dll";
	private const string HELPER_TYPE     = "ScriptHelper";
	
	public ScriptManager(World world, IRenderer renderer, IControls controls) {
		context = new ScriptContext(world, renderer);
		Yondr.Entity.Context = context;
		this.world = world;

		if (controls != null) {
			// this is a client
			this.controls = controls;
			var group = world.GroupDictionary[World.Self];
			self = new Yondr.Entity(group.Index, group.Entities[1].Index);
		} else {
			// this is a server
			self = null;
		}

		methods = new List<MethodInfo>[(int)Event.COUNT];
		for (int i = 0; i < methods.Length; i++) {
			methods[i] = new List<MethodInfo>();
		}

		createAppDomain();
	}
	private void createAppDomain() {
		// Scripts only have execution permissions.
		var permissions = new PermissionSet(PermissionState.None);
		permissions.AddPermission(new SecurityPermission(SecurityPermissionFlag.Execution));

		var helperType = typeof(ScriptHelper);
		var  helperStrongName = GetStrongName(helperType.Assembly);
		var contextStrongName = GetStrongName(typeof(Yondr.IContext).Assembly);

		// Create appdomain.
		domain = AppDomain.CreateDomain(
			"ScriptDomain", new Evidence(), new AppDomainSetup(),
			permissions, new[] { helperStrongName, contextStrongName }
		);

		helper = (ScriptHelper)domain.CreateInstanceFromAndUnwrap(HELPER_ASSEMBLY, HELPER_TYPE);
	}
	~ScriptManager() {
		//AppDomain.Unload(domain); (times out)
	}
	
	private static StrongName GetStrongName(Assembly ass) {
		AssemblyName assemblyName = ass.GetName();
		byte[] publicKey = assemblyName.GetPublicKey();
		if (publicKey == null || publicKey.Length == 0) {
			throw new InvalidOperationException(String.Format("{0} is not strongly named", ass));
		}
		var keyBlob = new StrongNamePublicKeyBlob(publicKey);
		return new StrongName(keyBlob, assemblyName.Name, assemblyName.Version);
	}
	
	public void Compile(Res.Package package) {
		string outDir = getDllFilename(package);
		Assembly ass = null;

		// Get all the scripts in the package.
		DateTime pakDate = DateTime.MinValue;
		var scripts = new List<string>();
		foreach (Res.Res res in package.Resources.Values) {
			if (res.Type == Res.Type.SCRIPT) {
				scripts.Add(res.Path);
				DateTime scriptDate = File.GetLastWriteTime(res.Path);
				if (scriptDate > pakDate) {
					// package date is the last modified date
					pakDate = scriptDate;
				}
			}
		}
		if (scripts.Count == 0) return;
		
		// Check if package is already compiled.
		if (File.Exists(outDir)) {
			DateTime outDate = File.GetLastWriteTime(outDir);
			if (pakDate < outDate) ass = helper.Load(outDir);
			if (ass != null) Log.Info("Loaded {0}", outDir);
		}
		if (ass == null) {
			// Compile them.
			string path = generateProperties(package);
			scripts.Add(path);
			var result = helper.Compile(scripts.ToArray(), outDir);

			foreach (var error  in result.Errors) Log.Info("  {0}", error);
			foreach (var output in result.Output) Log.Info("  {0}", output);
			
			if (result.Errors.HasErrors) {
				Log.Error("Failed to compile {0}:", outDir);
				return;
			}
			Log.Info("Compiled {0}", outDir);

			ass = result.CompiledAssembly;
		}

		findMethods(ass);

		// Set the context static value.
		//var entities = ass.GetType("Yondr.Entity");
		//foreach (var type in ass.GetTypes()) {
		//	Log.Debug("Poof: {0}", type.Name);
		//}
		//var prop = entities.GetProperty("Context");
		//prop.SetValue(null, context);
	}
	private string generateProperties(Res.Package package) {
		// Generate the file containing all the property constants.
		string filename = Path.Combine(package.Path, "_properties.cs");
		using (var file = new StreamWriter(filename)) {
			file.WriteLine("using System;");
			file.WriteLine("namespace Yondr {");
			file.WriteLine("namespace Groups {");
			foreach (var group in world.Groups) {
				file.WriteLine("\tpublic static class {0} {{", group.CamelCaseName);
				for (ushort i = 0; i < group.PropertySystem.Count; i++) {
					Property prop = group.PropertySystem.At(i);
					file.WriteLine(
						"\t\tpublic static Property<{0}> {1} = new Property<{0}>({2}, {3});",
						prop.Value.val.GetType().Name, prop.CamelCaseName, group.Index, prop.Index
					);
				}
				file.WriteLine("\t}");
			}
			file.WriteLine("}");
			file.WriteLine("}");
		}
		return filename;
	}
	private void findMethods(Assembly ass) {
		// We look for public static methods in a class called Events,
		// and add them to our collection of scripts.
		var flags = BindingFlags.Public | BindingFlags.Static;
		var events = ass.GetType("Events", false);
		if (events != null) {
			foreach (MethodInfo meth in events.GetMethods(flags)) {
				Event? even = null;
				switch (meth.Name) {
					case "Init":   even = Event.INIT;   break;
					case "Update": even = Event.UPDATE; break;
					case "Exit":   even = Event.EXIT;   break;
					default:
						string[] parts = meth.Name.Split(new[] { '_' }, 2);
						if (parts.Length < 2) break;
						switch (parts[0]) {
							case "While": AddWhile(parts[1], meth); continue;
							case "On":    AddOn(   parts[1], meth); continue;
						}
						break;
				}
				if (even == null) {
					Log.Warn("{0} does not match any known events. " +
						"If it's a helper function, make it private.", meth.Name);
					continue;
				}
				if (!checkMethodParams(meth, EventParameters[(int)even])) continue;
				methods[(int)even].Add(meth);
			}
		}
	}

	public void DeleteDll(Res.Package package) {
		string filename = getDllFilename(package);
		Log.Info("Deleting {0}", filename);
		File.Delete(filename);
	}

	private static string getDllFilename(Res.Package package) {
		return "_" + package.Name + ".dll";
	}

	private static bool checkMethodParams(MethodInfo meth, Type[] expectedParams) {
		var foundParams = meth.GetParameters().Select((p, _) => p.ParameterType);
		if (!foundParams.SequenceEqual(expectedParams)) {
			Log.Error("'{0}' does not have the correct parameters.\n" + 
			          "  Expected: {1}\n  Found: {2}",
			          meth.Name, expectedParams, foundParams);
			return false;
		}
		return true;
	}

	private void AddWhile(string name, MethodInfo meth) {
		if (!checkMethodParams(meth, new[] { typeof(Yondr.Entity), typeof(float) })) return;
		if (controls != null) {
			// is client
			controls.AddWhile(name, d => meth.Invoke(null, new object[] { self, d }));
		}
	}

	private void AddOn(string name, MethodInfo meth) {
		if (!checkMethodParams(meth, new[] { typeof(Yondr.Entity) })) return;
		if (controls != null) {
			// is client
			controls.AddOn(name, () => meth.Invoke(null, new object[] { self }));
		}
	}
	
	public enum Event {
		INIT,
		UPDATE,
		EXIT,
		COUNT,
	}
	private Type[][] EventParameters = new Type[][] {
		new Type[] { typeof(Yondr.Entity?), typeof(Yondr.IContext) },
		new Type[] { typeof(float) },
		new Type[] { },
	};
	
	public void Init() {
		foreach (MethodInfo meth in methods[(int)Event.INIT]) {
			meth.Invoke(null, new object[] { self, context });
		}
	}
	public void Update(float diff) {
		foreach (MethodInfo meth in methods[(int)Event.UPDATE]) {
			meth.Invoke(null, new object[] { diff });
		}
	}
	public void Exit() {
		foreach (MethodInfo meth in methods[(int)Event.EXIT]) {
			meth.Invoke(null, new object[] { });
		}
	}

	private readonly IControls controls;
	private readonly ScriptContext context;
	private AppDomain domain;
	private ScriptHelper helper;
	private readonly List<MethodInfo>[] methods;
	private Yondr.Entity? self;
	private World world;
}
