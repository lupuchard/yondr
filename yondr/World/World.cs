using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NodeDeserializers;
using YamlDotNet.Serialization.NamingConventions;

public class World {

	private class PropertyData {
		public string Name { get; set; }
		public Val.Ty Type { get; set; }
		public string Default { get; set; } = null;
	}
	private class GroupData {
		public ComponentType[] Components { get; set; } = null;
		public PropertyData[]  Properties { get; set; } = null;
		public Dictionary<string, Dictionary<string, string>>[] Bases { get; set; } = null;
	}
	private class PackageData {
		public PackageData(Res.Package package) {
			this.package = package;
		}
		public Res.Package package;
		public string deps     = null;
		public string world    = null;
		public string controls = null;
		public bool loaded     = false;
	}

	/// Loads all the world data from the resource manager.
	/// @return The packages in dependency order.
	public List<Res.Package> Load(Res.Manager resourceManager) {
		var packages    = new Dictionary<string, PackageData>();
		var packageList = new List<Res.Package>();

		// Get all important data resources.
		foreach (Res.Package package in resourceManager.Packages.Values) {
			var packageData = new PackageData(package);
			packages.Add(package.Name, packageData);
			foreach (Res.Res res in package.Resources.Values) {
				switch (res.Type) {
					case Res.Type.YAML:
						switch (res.Name) {
							case "deps":     packageData.deps     = res.Path; break;
							case "world":    packageData.world    = res.Path; break;
							case "controls": packageData.controls = res.Path; break;
							default: continue;
						}
						res.Used = true;
						break;
					default: continue;
				}
			}
		}
		foreach (var packageData in packages.Values) {
			loadPackage(packages, packageList, packageData.package);
		}
		foreach (EntityGroup group in Groups) {
			group.Init();
		}

		return packageList;
	}
	private void loadPackage(Dictionary<string, PackageData> packages,
	                         List<Res.Package> packageList,
	                         Res.Package package) {
		// could already be loaded
		var packageData = packages[package.Name];
		if (packageData.loaded) return;
		packageData.loaded = true;
		
		Log.Info("Loading package {0}.", package.Name);
		
		// gotta deserialize the yamls
		var deserializer = new Deserializer(namingConvention: new HyphenatedNamingConvention());
		
		// insert ImportNodeDeserializer and EnumNodeSerializer before the ScalarNodeDeserializer
		int scalarIdx = deserializer.NodeDeserializers.Select((d, i) => new {D=d, I=i}).
		                First(d => d.D is ScalarNodeDeserializer).I;
		deserializer.NodeDeserializers.Insert(scalarIdx, new EnumNodeDeserializer());
		var inputNodeDeserializer = new ImportNodeDeserializer(deserializer, package);
		deserializer.NodeDeserializers.Insert(scalarIdx, inputNodeDeserializer);

		// insert VecNodeDeserializer before ObjectNodeDeserializer
		int objIdx = deserializer.NodeDeserializers.Select((d, i) => new {D=d, I=i}).
		             First(d => d.D is ObjectNodeDeserializer).I;
		deserializer.NodeDeserializers.Insert(objIdx, new VecNodeDeserializer());

		// load dependencies
		if (packageData.deps != null) {
			Log.Info("Parsing {0}.", packageData.deps);
			var input = new StreamReader(packageData.deps);
			var deps = deserializer.Deserialize<string[]>(input);
			foreach (string depName in deps) {
				PackageData dep;
				string simpleName = StringUtil.Simplify(depName);
				if (packages.TryGetValue(simpleName, out dep)) {
					loadPackage(packages, packageList, dep.package);
				} else {
					Log.Info("'{0}' is not an existing package.", depName);
				}
			}
		}
		
		// load world
		if (packageData.world != null) {
			Log.Info("Parsing {0}.", packageData.world);
			var input = new StreamReader(packageData.world);
			var world = deserializer.Deserialize<Dictionary<string, GroupData>>(input);
			foreach (var pair in world) {
				string name = StringUtil.Simplify(pair.Key);
				EntityGroup group;
				// check if group already exists
				if (!groupD.TryGetValue(name, out group)) {
					if (groups.Count >= 256) {
						throw new InvalidOperationException("Too many groups! ( >256 )");
					}
					group = new EntityGroup(name, (byte)groups.Count);
					groups.Add(group);
					groupD[name] = group;
				}
				
				// load properties
				if (pair.Value.Properties != null) {
					loadProperties(pair.Value.Properties, group);
				}
				
				// load bases
				if (pair.Value.Bases != null) {
					loadBases(pair.Value.Bases, group);
				}

				if (pair.Value.Components != null) {
					foreach (ComponentType component in pair.Value.Components) {
						switch (component) {
							case ComponentType.GRAPHICAL:
								group.AddComponent(new GraphicalComponent());
								break;
							case ComponentType.GRID:
								throw new NotImplementedException();
							case ComponentType.SPACIAL:
								group.AddComponent(new SpacialComponent());
								break;
						}
					}
				}
			}
		}

		// load controls
		if (Controls != null && packageData.controls != null) {
			Log.Info("Parsing {0}.", packageData.controls);
			var input = new StreamReader(packageData.controls);
			var map = deserializer.Deserialize<Dictionary<string, string[]>>(input);
			foreach (var pair in map) {
				foreach (string key in pair.Value) {
					Controls.Add(key, pair.Key);
				}
			}
		}

		packageList.Add(package);
	}
	private static void loadProperties(PropertyData[] data, EntityGroup group) {
		foreach (PropertyData prop in data) {
			string propName = StringUtil.Simplify(prop.Name);
			group.PropertySystem.Add(propName, loadVal(prop));
		}
	}
	private static void loadBases(Dictionary<string, Dictionary<string, string>>[] data,
	                              EntityGroup group) {
		foreach (var baseDataDict in data) {
			var baseData = baseDataDict.First(_ => true);
			string baseName = StringUtil.Simplify(baseData.Key);
			Entity.Base entityBase = new Entity.Base(group.PropertySystem);
			foreach (var valPair in baseData.Value) {
				string propName = StringUtil.Simplify(valPair.Key);
				Property prop = group.PropertySystem.WithName(propName);
				if (prop == null) {
					Log.Warn("In base {0} in group {1}: '{2}' is not an existing property.",
							  baseName, group.Name, propName);
					continue;
				}
				Val? val = getVal(prop.Value.Type, valPair.Value);
				if (val == null) {
					Log.Warn("In base {0} in group {1}: '{2}' is given value of wrong type.",
							  baseName, group.Name, propName);
					continue;
				}
				entityBase[prop.Index] = (Val)val;
			}
			group.AddBase(baseName, entityBase);
		}
	}
	private static Val loadVal(PropertyData prop) {
		Val val = new Val(0);
		switch (prop.Type) {
			case Val.Ty.Bool:   val = new Val(false); break;
			case Val.Ty.Float:  val = new Val(0.0);   break;
			case Val.Ty.Int:    val = new Val(0);     break;
			case Val.Ty.String: val = new Val("");    break;
		}
		if (prop.Default != null) {
			Val? defVal = getVal(val.Type, prop.Default);
			if (defVal == null) {
				Log.Info("Default value of {0} does not match type ({1}).", prop.Name, val.Type);
			} else {
				val = (Val)defVal;
			}
		}
		return val;
	}
	private static Val? getVal(Val.Ty type, string val) {
		try {
			switch (type) {
				case Val.Ty.Bool:   return new Val(Convert.ToBoolean(val));
				case Val.Ty.Float:  return new Val(Convert.ToDouble(val));
				case Val.Ty.Int:    return new Val(Convert.ToInt32(val));
				case Val.Ty.String: return new Val(val);	
				default: return null;				
			}
		} catch (FormatException) {
			return null;
		}
	}
	
	private readonly List<EntityGroup> groups = new List<EntityGroup>();
	public IList<EntityGroup> Groups { get { return groups.AsReadOnly(); } }

	private readonly Dictionary<string, EntityGroup> groupD = new Dictionary<string, EntityGroup>();
	public IDictionary<string, EntityGroup> GroupDictionary { get { return groupD; } }

	public IControls Controls { get; set; }
}
