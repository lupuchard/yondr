using System.Collections.Generic;

public class Property {
	public Property(string name, Val val, ushort index) {
		Name  = name;
		Value = val;
		Index = index;
	}

	public string Name  { get; set; }
	public Val Value    { get; set; }
	public ushort Index { get; set; }
}

public class PropertySystem {
	public PropertySystem(byte index) {
		Index = index;
		properties = new List<Property>();
		nameMap    = new Dictionary<string, ushort>();
	}

	/// @param name The name of the created property.
	/// @param val Both the type and default of the created property.
	/// @return Index of created property.
	public ushort Add(string name, Val val) {
		var index = (ushort)properties.Count;
		nameMap.Add(name, index);
		properties.Add(new Property(name, val, index));
		return index;
	}

	/// @return Number of objects currently in the manager.
	public int Count { get { return properties.Count; } }

	/// @return The property of the given indexer.
	public Property At(ushort idx) {
		return properties[idx];
	}

	/// @return The property with the given name, or null if there is none.
	public Property WithName(string name) {
		ushort idx;
		return nameMap.TryGetValue(name, out idx) ? properties[idx] : null;
	}

	public byte Index { get; }
	private readonly List<Property> properties;
	private readonly Dictionary<string, ushort> nameMap;
}
