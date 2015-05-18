using System;
using System.Collections.Generic;

public interface EntityContainer {
	PropertySystem PropertySystem { get; }
	string Name { get; }
	
	Entity GetEntity(ushort index);
	ushort CreateEntity(Entity.Base bass, ushort index);
	
	void AddBase(string name, Entity.Base bass);
	Entity.Base GetBase(string name);
	void UpdateBases();
}

public class ListContainer: EntityContainer {
	public ListContainer(string name) {
		this.PropertySystem = new PropertySystem();
		bases    = new Dictionary<string, Entity.Base>();
		entities = new List<Entity>();
		Name = name;
	}
	
	public Entity GetEntity(ushort index) {
		return entities[index];
	}
	public ushort CreateEntity(Entity.Base bass, ushort index) {
		if (bass.PropertySystem != PropertySystem) {
			throw new ArgumentException("Base must have same PropertySystem as Container.");
		}
		entities.Add(new Entity(bass, index));
		return (ushort)(entities.Count - 1);
	}
	
	public void AddBase(string name, Entity.Base bass) {
		bases.Add(name, bass);
	}
	public Entity.Base GetBase(string name) {
		Entity.Base bass = null;
		bases.TryGetValue(name, out bass);
		return bass;
	}
	public void UpdateBases() {
		foreach (Entity.Base bass in bases.Values) {
			bass.Update();
		}
	}
	
	public PropertySystem PropertySystem { get; }
	public string Name { get; }
	
	private Dictionary<string, Entity.Base> bases;
	private List<Entity> entities;
}

public class GridContainer: ListContainer {
	
	const ushort GRID_EMPTY = ushort.MaxValue;
	
	public GridContainer(string name, Vec3<ushort> dim): base(name) {
		Dimensions = dim;
		grid = new ushort[dim.X * dim.Y * dim.Z];
		for (int i = 0; i < grid.Length; i++) {
			grid[i] = GRID_EMPTY;
		}
	}
	
	/// @return The Entity at the given cell, or null if there isn't one.
	public Entity GetCell(Vec3<ushort> pos) {
		ushort index = grid[pos.X + pos.Y * Dimensions.X + pos.Z * Dimensions.X * Dimensions.Y];
		return index != GRID_EMPTY ? GetEntity(index) : null;
	}
	
	public Vec3<ushort> Dimensions { get; }
	private ushort[] grid;
}
