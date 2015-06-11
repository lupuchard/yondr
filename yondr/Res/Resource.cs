using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace Res {

public enum Type {
	UNKNOWN,
	YAML,    // YAML data file
	SCRIPT,  // CSharp script

	// Image formats
	BMP, GIF, JPG, PNG, // tiff, xif

	// Mesh formats. They all transform to MESH.
	OBJ, MD3, COLLADA, BLENDER, // md2

	MESH, // Custom mesh format

	VERT, // GLSL fragment shader
	FRAG, // GLSL vertex shader
};
	
public static class TypeMethods {
	public static string GetExtension(this Type type) {
		switch (type) {
			case Type.UNKNOWN: return "what";
			case Type.YAML:    return "yaml";
			case Type.SCRIPT:  return "cs";
			
			case Type.BMP:     return "bmp";
			case Type.GIF:     return "gif";
			case Type.PNG:     return "png";
			case Type.JPG:     return "jpg";

			case Type.MESH:    return "ym";
				
			case Type.VERT:    return "vert";
			case Type.FRAG:    return "frag";
			default: throw new System.ArgumentOutOfRangeException();
		}
	}
	public static Type FromExtension(string extension) {
		switch (extension) {
			case ".yaml":
			case ".yml":   return Type.YAML;
			case ".cs":    return Type.SCRIPT;

			case ".bmp":   return Type.BMP;
			case ".gif":   return Type.GIF;
			case ".jpeg":
			case ".jpg":   return Type.JPG;
			case ".png":   return Type.PNG;
				
			case ".obj":   return Type.OBJ;
			case ".md3":   return Type.MD3;
			case ".dae":   return Type.COLLADA;
			case ".blend": return Type.BLENDER;
			case ".ym":    return Type.MESH;
				
			case ".glslv":
			case ".vert":  return Type.VERT;
			case ".glslf":
			case ".frag":  return Type.FRAG;

			default:       return Type.UNKNOWN;
		}
	}
	public static Type? TransformsTo(this Type type) {
		switch (type) {
			case Type.COLLADA: return Type.MESH;
			default: return null;
		}
	}
	
	public static Type Transform(this Type type, string path, StreamWriter output) {
		switch (type) {
			case Type.COLLADA:
				Mesh mesh = Mesh.FromCollada(path);
				IFormatter formatter = new BinaryFormatter();
				formatter.Serialize(output.BaseStream, mesh);
				break;
			default: throw new System.ArgumentException("Does not transform.");
		}
		return (Type)type.TransformsTo();
	}
}
	
public class Res {
	public Res(string name, string path, Type type, Package package) {
		Name    = name;
		Type    = type;
		Path    = path;
		Data    = null;
		Package = package;
	}
	
	public Type        Type { get; set; }
	public string      Path { get; set; }
	public string      Name { get; set; }
	public Package  Package { get; set; }
	public long        Hash { get; set; }
	public ushort SessionID { get; set; } = 0;
	public byte[]      Data { get; set; } = null;
	public bool        Used { get; set; } = false;
}

public class Metadata {
	public SerializableDictionary<string, long> Hashes { get; set; } =
		new SerializableDictionary<string, long>();
}

public class Package {

	public Package(string name, string path) {
		this.Path = path;
		this.Name = name;
		resources = new Dictionary<string, Res>();
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

	private Metadata metadata;
	public Metadata Metadata { get { return metadata; } }

	private readonly Dictionary<string, Res> resources;
	public IDictionary<string, Res> Resources { get { return resources; } }
}

}
