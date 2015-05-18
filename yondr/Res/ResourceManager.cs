using System.IO;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Res {

public class Metadata {
	public SerializableDictionary<string, long> Hashes { get; set; } =
		new SerializableDictionary<string, long>();
}

public class Package {

	public Package(string name, string path, int index) {
		this.Path = path;
		this.Name = name;
		Index = index;
		resources = new Dictionary<string, string>();
	}

	public void LoadMetadata() {
		var metaPath = System.IO.Path.Combine(Path, "_metadata");
		metaPath = System.IO.Path.ChangeExtension(metaPath, "xml");
		try {
			var reader = new XmlSerializer(typeof(Metadata));
			var file = new StreamReader(metaPath);
			metadata = (Metadata)reader.Deserialize(file);
			file.Close();
		} catch (System.Xml.XmlException) {
			metadata = new Metadata();
		} catch (IOException) {
			metadata = new Metadata();
		}
	}
	public void StoreMetadata() {
		if (metadata == null) return;
		var metaPath = System.IO.Path.Combine(Path, "_metadata");
		metaPath = System.IO.Path.ChangeExtension(metaPath, "xml");
		var writer = new XmlSerializer(typeof(Metadata));
		var file = new StreamWriter(metaPath);
		writer.Serialize(file, Metadata);
		file.Close();
	}

	public string Name  { get; }
	public string Path  { get; }
	public int    Index { get; }

	private Metadata metadata;
	public Metadata Metadata { get { return metadata; } }

	private readonly Dictionary<string, string> resources;
	public IDictionary<string, string> Resources { get { return resources; } }
}

public class Manager {
	public Manager(string directory) {
		Directory = directory;
		packages   = new List<Package>();
		packageD   = new Dictionary<string, Package>();
		resources  = new List<Res>();
		resourceD  = new Dictionary<Name, Res>();
		sessionIDD = new Dictionary<ushort, Res>();
		nextSessionID = 1;

		var dir = new DirectoryInfo(directory);
		if (!dir.Exists) dir.Create();

		// scan packages
		foreach (var subdir in dir.EnumerateDirectories()) {
			var path = subdir.ToString();
			
			// skip directories that start with dots and underscores
			char first = Path.GetFileName(path)[0];
			if (first == '_' || first == '-' || first == '.') continue;
			
			var package = new Package(Path.GetFileName(path), path, packages.Count);
			package.LoadMetadata();
			packageD.Add(package.Name, package);
			packages.Add(package);
			
			System.Console.WriteLine("p: {0}", package.Name);
		}
	}

	/// @throws IOException.
	public Package CreatePackage(string name) {
		name = StringUtil.Simplify(name);
		string path = Path.Combine(Directory, name);
		var dir = new DirectoryInfo(path);
		if (!dir.Exists) dir.Create();

		var package = new Package(name, path, packages.Count);
		packageD.Add(name, package);
		packages.Add(package);
		package.LoadMetadata();
		return package;
	}

	/// Loads all the resources from the given package into the Manager.
	/// This generates both their session id and version id.
	/// @throws IOException.
	/// (server function)
	public void LoadPackage(Package package) {
		var dir = new DirectoryInfo(package.Path);
		foreach (var file in dir.EnumerateFiles("*.*", SearchOption.AllDirectories)) {
			var entryPath = file.ToString();
			
			// skip files that start with dots and underscores
			char first = Path.GetFileName(entryPath)[0];
			if (first == '_' || first == '-' || first == '.') continue;
			
			byte[] data = File.ReadAllBytes(entryPath);
			newResource(entryPath, package, hash(data), nextSessionID, data);
		}
	}
	private long hash(byte[] data) {
		long hash = 0;
		long next = 0;
		int idx = 0;
		foreach (byte b in data) {
			next = (next << 8) + b;
			if (idx % 8 == 0) {
				hash ^= next;
			}
			idx++;
		}
		return hash;
	}

	/// Tells the resource manager to attempt to load the resource with the given name.
	/// If the resource exists, successfully loads, and matches the given version_id, then it
	/// is added to the resource manager and given the provided session id.
	/// @throws IOException.
	/// @return True if the resource exists, is valid and was loaded. False otherwise.
	/// (client function)
	public bool CheckResource(string name, Package package, long hash, ushort sessionID) {
		name = StringUtil.Simplify(name);
		if (package.Resources.Count == 0)
			scanResources(package);

		long prevHash;
		if (!package.Metadata.Hashes.TryGetValue(name, out prevHash) || prevHash != hash) {
		    return false;
		}
		
		string path;
		if (!package.Resources.TryGetValue(name, out path)) return false;
		
		byte[] data = File.ReadAllBytes(path);
		newResource(path, package, hash, sessionID, data);
		
		return true;
	}
	private void scanResources(Package package) {
		var dir = new DirectoryInfo(package.Path);
		foreach (var file in dir.EnumerateFiles("*.*", SearchOption.AllDirectories)) {
			var entryPath = file.ToString();
			string stem = Path.GetFileNameWithoutExtension(entryPath);
			package.Resources.Add(StringUtil.Simplify(stem), entryPath);
		}
	}
	
	public void CreateResource(string name, Package package, Type type,
	                           long hash, ushort sessionID, byte[] data) {
		name = StringUtil.Simplify(name);
		string path = Path.ChangeExtension(Path.Combine(package.Path, name), type.GetExtension());
		File.WriteAllBytes(path, data);
		
		package.Metadata.Hashes[name] = hash;
		package.StoreMetadata();
		
		newResource(path, package, hash, sessionID, data);
	}

	private void newResource(string path, Package package,
	                         long hash, ushort sessionID, byte[] data) {
		var name = StringUtil.Simplify(Path.GetFileNameWithoutExtension(path));
		var type = TypeMethods.FromExtension(Path.GetExtension(path));
		var res  = new Res(new Name(name, package.Index), type, path);
		res.SessionID = sessionID;
		res.Hash = hash;
		res.Data = data;
		
		if (resourceD.ContainsKey(res.Name)) {
			throw new System.ArgumentException("Resource in package " + package.Name +
			                                   " with name " + res.Name + " already exists.");
		}
		resourceD.Add(res.Name, res);
		resources.Add(res);
		sessionIDD.Add(sessionID, res);
		
		if (nextSessionID <= sessionID) nextSessionID = ++sessionID;
	}
	
	public void clean() {
		packages.Clear();
		packageD.Clear();
		resourceD.Clear();
	}

	public string Directory { get; set; }

	private readonly List<Package> packages;
	public IList<Package> Packages { get { return packages.AsReadOnly(); } }
	private readonly Dictionary<string, Package> packageD;
	public IDictionary<string, Package> PackageDictionary { get { return packageD; } }

	private readonly List<Res> resources;
	public IList<Res> Resources { get { return resources.AsReadOnly(); } }
	private readonly Dictionary<Name, Res> resourceD;
	public IDictionary<Name, Res> ResourceDictionary { get { return resourceD; } }

	private readonly Dictionary<ushort, Res> sessionIDD;
	public IDictionary<ushort, Res> SessionIDResourceDictionary { get { return sessionIDD; } }
	private ushort nextSessionID;
}

}
