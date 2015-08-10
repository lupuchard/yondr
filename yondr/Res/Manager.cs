using System;
using System.IO;
using System.Collections.Generic;

// TODO: unit tests

namespace Res {

public class Manager {
	public Manager(string directory) {
		Dir = directory;

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
		string path = Path.Combine(Dir, name);
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
				Log.Warn("Resource in package {0} named {1} already exists.", package.Name, name);
				continue;
			}
			
			var type = TypeMethods.FromExtension(Path.GetExtension(path));
			
			if (type.TransformsTo() != null) {
				var transType = (Type)type.TransformsTo();
				string transFilename = "_" + name + "." + transType.GetExtension();
				string transPath = Path.Combine(package.Path, transFilename);
				bool shouldTransform = false;
				if (File.Exists(transPath)) {
					DateTime transDate = File.GetLastWriteTime(transPath);
					DateTime prevDate = Directory.GetLastWriteTime(path);
					if (prevDate > transDate) {
						shouldTransform = true;
					}
				} else shouldTransform = true;
				if (shouldTransform) using (var transFile = new StreamWriter(transPath)) {
						type = type.Transform(path, transFile);
				} else {
					type = transType;
				}
				path = transPath;
			}
			
			var res  = new Res(name, path, type, package);
			res.SessionID = nextSessionID;
			res.Data = File.ReadAllBytes(path);
			res.Hash = hash(res.Data);
			
			resources.Add(res);
			package.Resources.Add(name, res);
			NextSessionID(res);
		}
	}
	private static long hash(byte[] data) {
		long hashValue = 0;
		long next = 0;
		int idx = 0;
		foreach (byte b in data) {
			next = (next << 8) + b;
			if (idx % 8 == 0) {
				hashValue ^= next;
			}
			idx++;
		}
		return hashValue;
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

	public string Dir { get; set; }

	private readonly Dictionary<string, Package> packages = new Dictionary<string, Package>();
	public IReadOnlyDictionary<string, Package> Packages { get { return packages; } }

	private readonly List<Res> resources = new List<Res>();
	public IReadOnlyList<Res> Resources { get { return resources; } }
	private readonly Dictionary<ushort, Res> resourceD = new Dictionary<ushort, Res>();
	public IReadOnlyDictionary<ushort, Res> ResourceDictionary { get { return resourceD; } }
	private ushort nextSessionID = 1;
}

}
