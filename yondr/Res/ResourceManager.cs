using System.IO;
using System.Collections.Generic;

// TODO: unit tests

namespace Res {

public class Manager {
	public Manager(string directory) {
		Directory = directory;
		packages   = new Dictionary<string, Package>();
		resources  = new List<Res>();
		resourceD  = new Dictionary<ushort, Res>();
		nextSessionID = 1;

		var dir = new DirectoryInfo(directory);
		if (!dir.Exists) dir.Create();

		// scan packages
		foreach (var subdir in dir.EnumerateDirectories()) {
			var path = subdir.ToString();
			
			// skip directories that start with dots and underscores
			char first = Path.GetFileName(path)[0];
			if (first == '_' || first == '-' || first == '.') continue;
			
			var package = new Package(Path.GetFileName(path), path);
			package.LoadMetadata();
			packages.Add(package.Name, package);
		}
	}

	/// @throws IOException.
	public Package CreatePackage(string name) {
		name = StringUtil.Simplify(name);
		string path = Path.Combine(Directory, name);
		var dir = new DirectoryInfo(path);
		if (!dir.Exists) dir.Create();

		var package = new Package(name, path);
		packages.Add(name, package);
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
			var path = file.ToString();
			
			// skip files that start with dots and underscores
			char first = Path.GetFileName(path)[0];
			if (first == '_' || first == '-' || first == '.') continue;
			
			var name = StringUtil.Simplify(Path.GetFileNameWithoutExtension(path));
			if (package.Resources.ContainsKey(name)) {
				Log.Warn("Resource in package " + package.Name +
				         " with name " + name + " already exists.");
				continue;
			}
			
			var type = TypeMethods.FromExtension(Path.GetExtension(path));
			var res  = new Res(name, path, type, package);
			res.SessionID = nextSessionID;
			res.Data = File.ReadAllBytes(path);
			res.Hash = hash(res.Data);
			
			resources.Add(res);
			package.Resources.Add(name, res);
			NextSessionID(res);
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
		
		Res res;
		if (!package.Resources.TryGetValue(name, out res)) return false;
		
		res.Data = File.ReadAllBytes(res.Path);
		res.Hash = hash;
		res.SessionID = sessionID;
		NextSessionID(res);
		
		return true;
	}
	private void scanResources(Package package) {
		var dir = new DirectoryInfo(package.Path);
		foreach (var file in dir.EnumerateFiles("*.*", SearchOption.AllDirectories)) {
			var path = file.ToString();
			var stem = Path.GetFileNameWithoutExtension(path);
			var type = TypeMethods.FromExtension(Path.GetExtension(path));
			Res res = new Res(StringUtil.Simplify(stem), path, type, package);
			package.Resources.Add(res.Name, res);
			resources.Add(res);
		}
	}
	
	/// A new resource will be created in the package directory
	/// as well as loaded into the resource manager. If a resource of this
	/// name already exists, it will be replaced.
	public void CreateResource(string name, Package package, Type type,
	                           long hash, ushort sessionID, byte[] data) {
		name = StringUtil.Simplify(name);

		Res res;
		if (!package.Resources.TryGetValue(name, out res)) {
			var path = Path.ChangeExtension(Path.Combine(package.Path, name), type.GetExtension());
			res = new Res(name, path, type, package);
			package.Resources.Add(name, res);
			resources.Add(res);
		}
		File.WriteAllBytes(res.Path, data);
		
		package.Metadata.Hashes[name] = hash;
		package.StoreMetadata();

		res.Hash = hash;
		res.Data = data;
		res.SessionID = sessionID;
		NextSessionID(res);
	}

	private void NextSessionID(Res res) {
		if (nextSessionID <= res.SessionID) nextSessionID = (ushort)(res.SessionID + 1);
		resourceD.Add(res.SessionID, res);
	}

	public string Directory { get; set; }

	private readonly Dictionary<string, Package> packages;
	public IReadOnlyDictionary<string, Package> Packages { get { return packages; } }

	private readonly List<Res> resources;
	public IReadOnlyList<Res> Resources { get { return resources; } }
	private readonly Dictionary<ushort, Res> resourceD;
	public IReadOnlyDictionary<ushort, Res> ResourceDictionary { get { return resourceD; } }
	private ushort nextSessionID;
}

}
