namespace Res {

public enum Type {
	UNKNOWN,
	YAML,
	SCRIPT,
};
	
static class TypeMethods {
	public static string GetExtension(this Type type) {
		switch (type) {
			case Type.UNKNOWN: return "what";
			case Type.YAML:    return "yaml";
			case Type.SCRIPT:  return "cs";
			default: return "fixme";
		}
	}
	public static Type FromExtension(string extension) {
		switch (extension) {
			case ".yaml": return Type.YAML;
			case ".yml":  return Type.YAML;
			case ".cs":   return Type.SCRIPT;
			default:     return Type.UNKNOWN;
		}
	}
}
	
public struct Name {
	public Name(string id, int package) {
		ID = id;
		Package = package;
	}
	public string   ID { get; set; }
	public int Package { get; set; }
}
	
public class Res {
	public Res(Name name, Type type, string path) {
		Name = name;
		Type = type;
		Path = path;
		Data = null;
	}
	
	public Type        Type { get; set; }
	public string      Path { get; set; }
	public Name        Name { get; set; }
	public long        Hash { get; set; }
	public ushort SessionID { get; set; } = 0;
	public byte[]      Data { get; set; } = null;
	public bool        Used { get; set; } = false;
}

}
