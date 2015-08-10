using System.IO;
using System.Numerics;

namespace Yondr {

public interface IContext {
	TextWriter Out { get; }

	/// Create a new Entity in the given group with the given base.
	/// If no such group or base exists, this will return null.
	Entity? CreateEntity(string group, string bass);

	/// Returns the position of the given entity. Fails on non-spacial entities.
	Vector3 EntityGetPosition(Entity entity);
	/// Sets the position of the given entity. Fails on non-spacial entities.
	void EntitySetPosition(Entity entity, Vector3 pos);
	/// Modifies the position of the given entity. Fails on non-spacial entities.
	void EntityMove(Entity entity, Vector3 amount);

	/// Sets the given entity's orientation so that it is facing position (with an up-vector of +Z).
	void EntityLookAt(Entity entity, Vector3 position);
	/// Rotates the entity the given amount around the x-axis.
	void EntityRotateX(Entity entity, float radians);
	/// Rotates the entity the given amount around the y-axis.
	void EntityRotateY(Entity entity, float radians);
	/// Rotates the entity the given amount around the z-axis. 
	void EntityRotateZ(Entity entity, float radians);
	/// Returns the current direction (ignoring up) of the given entity.
	Vector3 EntityGetDirection(Entity entity);

	/// Sets the texture for the entity.
	void EntitySetTexture(Entity entity, string texture);
	/// Sets the mesh for the entity.
	void EntitySetMesh(Entity entity, string mesh);

	T EntityGet<T>(Entity entity, Property<T> prop);
	void EntitySet<T>(Entity entity, Property<T> prop, T val);

	void EntitySetAsCamera(Entity entity);
}

public struct Property<T> {
	public Property(ushort index, byte group) {
		Index = index;
		Group = group;
	}
	public ushort Index { get; }
	public byte   Group { get; }
}

public struct Entity {
	public static IContext Context { get; set; }

	public Entity(byte grup, ushort idx) {
		Idx = idx;
		Group = grup;
	}

	public Vector3 Position {
		get { return Context.EntityGetPosition(this); }
		set { Context.EntitySetPosition(this, value); }
	}
	public void Move(Vector3 amount) { Context.EntityMove(this, amount); }

	public void LookAt(Vector3 position) { Context.EntityLookAt(this, position); }
	public void RotateX(float radians)  { Context.EntityRotateX(this, radians); }
	public void RotateY(float radians)  { Context.EntityRotateY(this, radians); }
	public void RotateZ(float radians)  { Context.EntityRotateZ(this, radians); }
	public Vector3 Direction { get { return Context.EntityGetDirection(this); } }

	public string Texture { set { Context.EntitySetTexture(this, value); } }
	public string Mesh { set { Context.EntitySetMesh(this, value); } }

	public T Get<T>(Property<T> prop)    { return Context.EntityGet(this, prop); }
	public void Set<T>(Property<T> prop, T val) { Context.EntitySet(this, prop, val); }

	public void SetAsCamera() { Context.EntitySetAsCamera(this); }

	public ushort Idx { get; set; }
	public byte Group { get; set; }
}

}
