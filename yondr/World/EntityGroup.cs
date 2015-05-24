using System;
using System.Collections.Generic;

public class EntityGroup {
	public EntityGroup(string name, byte index) {
		this.PropertySystem = new PropertySystem(index);
		Name = name;
		Index = index;
	}
	
	public Entity CreateEntity(Entity.Base bass) {
		if (bass.PropertySystem != PropertySystem) {
			Log.Error("Attempted to create base of wrong group.");
			return null;
		}
		
		Entity entity;
		if (availableEntityIndices.Count > 0) {
			ushort index = availableEntityIndices.Dequeue();
			entity = new Entity(bass, index);
			entities[index] = entity;
		} else {
			if (entities.Count > ushort.MaxValue) {
				throw new InvalidOperationException("Too many entities!");
			}
			entity = new Entity(bass, (ushort)entities.Count);
			entities.Add(entity);
		}
		
		foreach (var component in components.Values) {
			component.Add(entity);
		}
		return entity;
	}
	public void RemoveEntity(Entity entity) {
		entities[entity.Index] = null;
		availableEntityIndices.Enqueue(entity.Index);

		foreach (var component in components.Values) {
			component.Remove(entity);
		}
	}
	
	public void AddBase(string name, Entity.Base bass) {
		bases.Add(name, bass);
	}
	public Entity.Base GetBase(string name) {
		Entity.Base bass;
		return bases.TryGetValue(name, out bass) ? bass : null;
	}
	
	public void AddComponent<T>(T component) where T: class, IComponent {
		components.Add(typeof(T), component);
	}
	public T GetComponent<T>() where T: class, IComponent {
		IComponent component;
		return components.TryGetValue(typeof(T), out component) ? (T)component : null;
	}

	public void Init() {
		// update bases
		foreach (Entity.Base bass in bases.Values) {
			bass.Update();
		}
		
		foreach (var component in components.Values) {
			component.Init(PropertySystem);
		}
	}
	
	public PropertySystem PropertySystem { get; }
	public string Name { get; }
	public byte Index  { get; }
	
	private readonly Dictionary<string, Entity.Base> bases = new Dictionary<string, Entity.Base>();
	private readonly Dictionary<Type, IComponent> components = new Dictionary<Type, IComponent>();
	
	private readonly List<Entity> entities = new List<Entity>();
	public IList<Entity> Entities { get { return entities.AsReadOnly(); } }
	private Queue<ushort> availableEntityIndices = new Queue<ushort>();
}
