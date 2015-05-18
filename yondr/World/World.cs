using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NodeDeserializers;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Core.Events;

public class World {
	
	public World() { }
	
	public EntityContainer GetContainer(ushort index) {
		return containers[index];
	}
	public EntityContainer GetContainer(string name) {
		return containerD[name];
	}

	/*public void Init() {
		Lua.Init();
	}
	public void Update(double secs) {
		Lua.Update(secs);
	}
	public void Exit() {
		Lua.Exit();
	}*/

	// When parsing a non-scalar, it should accept
	// strings as other yaml resources to import.
	private class ImportNodeDeserializer: INodeDeserializer {
		private Deserializer deserializer;
		private Res.Manager  resources;
		private int package;
		public ImportNodeDeserializer(Deserializer d, Res.Manager res, int p) {
			deserializer = d;
			resources = res;
			package = p;
		}

		public bool Deserialize(EventReader reader, Type expectedType,
		                        Func<EventReader, Type, object> nested,
		                        out object value) {
			if (expectedType.IsValueType || expectedType == typeof(string)) {
				value = null;
				return false;
			}
			var scalar = reader.Allow<Scalar>();
			if (scalar == null) {
				value = null;
				return false;
			}
			var resName = StringUtil.Simplify(Path.GetFileNameWithoutExtension(scalar.Value));
			var res     = resources.ResourceDictionary[new Res.Name(resName, package)];
			if (res.Type != Res.Type.YAML) {
				throw new YamlException(scalar.Start, scalar.End,
				                        String.Format("Resource {0} is not yaml.", resName));
			} else if (res.Used) {
				Log.Info(res.Name.ID);
				throw new YamlException(scalar.Start, scalar.End,
				                        String.Format("Circular dependency?", res.Path));
			}
			res.Used = true;
			var input = new StreamReader(res.Path);
			value = deserializer.Deserialize(input, expectedType);
			res.Used = false;
			return true;
		}
	}
	
	// Parsing enums should not be case-sensitive.
	private class EnumNodeDeserializer: INodeDeserializer {
		public bool Deserialize(EventReader reader, Type expectedType,
		                        Func<EventReader, Type, object> nested,
		                        out object value) {
			if (expectedType.IsEnum) {
				var scalar = reader.Allow<Scalar>();
				if (scalar != null) {
					value = Enum.Parse(expectedType, scalar.Value, true);
					return true;
				}
			}
			value = null;
			return false;
		}
	}

	private class PropertyData {
		public string Name { get; set; }
		public Val.Ty Type { get; set; }
		public string Default { get; set; } = null;
	}
	private class ContainerData {
		public enum Ty { List, Grid }
		public Ty Type { get; set; } = Ty.List;
		public Vec3<ushort> Dim { get; set; } = new Vec3<ushort>(1, 1, 1);
		public PropertyData[] Properties { get; set; } = null;
		public Dictionary<string, Dictionary<string, string>>[] Bases { get; set; } = null;
	}
	private class PackageData {
		public string deps   = null;
		public string world  = null;
		public string events = null;
		public bool loaded   = false;
	}
	public void Load(Res.Manager resourceManager) {
		var packages = new Dictionary<int, PackageData>();

		// Get all important data resources.
		foreach (Res.Res res in resourceManager.Resources) {
			PackageData package;
			if (!packages.TryGetValue(res.Name.Package, out package)) {
				package = new PackageData();
				packages.Add(res.Name.Package, package);
			}

			switch (res.Type) {
				case Res.Type.YAML: {
					switch (res.Name.ID) {
						case "deps":   package.deps   = res.Path; break;
						case "world":  package.world  = res.Path; break;
						case "events": package.events = res.Path; break;
						default: continue;
					}
					res.Used = true;
				} break;
				default: continue;
			}
		}
		foreach (int index in packages.Keys) {
			loadPackage(resourceManager, packages, index);
		}
		foreach (EntityContainer container in containers) {
			container.UpdateBases();
		}
	}
	private void loadPackage(Res.Manager resourceManager,
	                         Dictionary<int, PackageData> packages, int index) {
		// could already be loaded
		if (packages[index].loaded) return;
		var package = packages[index];
		package.loaded = true;
		string packageName = resourceManager.Packages[index].Name;
		
		Log.Info("Loading package {0}.", packageName);
		
		var deserializer = new Deserializer(namingConvention: new HyphenatedNamingConvention());
		
		// insert ImportNodeDeserializer and EnumNodeSerializer before the ScalarNodeDeserializer
		int scalarIdx = deserializer.NodeDeserializers.Select((d, i) => new {D=d, I=i}).
		                First(d => d.D is ScalarNodeDeserializer).I;
		deserializer.NodeDeserializers.Insert(scalarIdx, new EnumNodeDeserializer());
		var inputNodeDeserializer = new ImportNodeDeserializer(deserializer, resourceManager, index);
		deserializer.NodeDeserializers.Insert(scalarIdx, inputNodeDeserializer);

		// insert VecNodeDeserializer before ObjectNodeDeserializer
		int objIdx = deserializer.NodeDeserializers.Select((d, i) => new {D=d, I=i}).
		             First(d => d.D is ObjectNodeDeserializer).I;
		deserializer.NodeDeserializers.Insert(objIdx, new VecNodeDeserializer());

		// load dependencies
		if (package.deps != null) {
			Log.Info("Parsing {0}.", package.deps);
			var input = new StreamReader(package.deps);
			var deps = deserializer.Deserialize<string[]>(input);
			foreach (string depName in deps) {
				Res.Package dep;
				string simpleName = StringUtil.Simplify(depName);
				if (resourceManager.PackageDictionary.TryGetValue(simpleName, out dep)) {
					loadPackage(resourceManager, packages, dep.Index);
				} else {
					Log.Info("'{0}' is not an existing package.", depName);
				}
			}
		}
		
		// load world
		if (package.world != null) {
			global::Log.Info("Parsing {0}.", package.world);
			var input = new StreamReader(package.world);
			var world = deserializer.Deserialize<Dictionary<string, ContainerData>>(input);
			foreach (var pair in world) {
				string name = StringUtil.Simplify(pair.Key);
				EntityContainer container = null;
				// check if container already exists
				if (!containerD.TryGetValue(name, out container)) {
					// if it doesnt, create a new one
					switch (pair.Value.Type) {
						case ContainerData.Ty.Grid: {
							container = new GridContainer(name, pair.Value.Dim);
						} break;
						case ContainerData.Ty.List: container = new ListContainer(name); break;
					}
					containers.Add(container);
					containerD[name] = container;
				}
				
				if (pair.Value.Properties != null) {
					foreach (PropertyData prop in pair.Value.Properties) {
						string propName = StringUtil.Simplify(prop.Name);
						container.PropertySystem.Add(propName, loadVal(prop));
					}
				}
				
				if (pair.Value.Bases != null) {
					foreach (var baseDataDict in pair.Value.Bases) {
						var baseData = baseDataDict.First((_) => true);
						string baseName = StringUtil.Simplify(baseData.Key);
						Entity.Base entityBase = new Entity.Base(container.PropertySystem);
						foreach (var valPair in baseData.Value) {
							string propName = StringUtil.Simplify(valPair.Key);
							Property prop = container.PropertySystem.WithName(propName);
							if (prop == null) {
								Log.Warn("In base {0} in container {1}: " +
								         "'{2}' is not an existing property.",
										  baseName, container.Name, propName);
								continue;
							}
							Val? val = getVal(prop.Value.Type, valPair.Value);
							if (val == null) {
								Log.Warn("In base {0} in container {1}: " +
								         "'{2}' is given value of wrong type.",
										  baseName, container.Name, propName);
								continue;
							}
							entityBase[prop.Index] = (Val)val;
						}
						container.AddBase(baseName, entityBase);
					}
				}
			}
		}


		
		// load events
		/*if (package.events != null) {
			log.Write("Loading {0}.", package.events);
			LuaTable eventTable = loadFileIntoTable(package.events, "events");
			if (eventTable == null) return;
			foreach (KeyValuePair<string, LuaFunction> pair in eventTable) {
				Lua.AddEvent(pair.Key, pair.Value);
			}
		}*/
	}
	private Val loadVal(PropertyData prop) {
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
	private Val? getVal(Val.Ty type, string val) {
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
	
	List<EntityContainer>     containers = new List<EntityContainer>();
	Dictionary<string, EntityContainer> containerD = new Dictionary<string, EntityContainer>();
	
	//ushort currentEntityIndex = 0;
	//Dictionary<ushort, Tuple<EntityContainer, ushort>> entityIndexD =
	//	new Dictionary<ushort, Tuple<EntityContainer, ushort>>();
}
