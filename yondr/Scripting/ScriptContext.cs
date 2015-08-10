using System;
using System.IO;
using System.Collections.Generic;
using System.Numerics;

// An IContext is passed into all the scripting functions.
// Represents the interface between the script and the game.
// It is defined in the script-context helper library.
public class ScriptContext: Yondr.IContext {
	public ScriptContext(World w, IRenderer r) {
		world    = w;
		renderer = r;

		foreach (var group in w.Groups) {
			spacialComponents.Add(group.GetComponent<SpacialComponent>());
		}
		foreach (var group in w.Groups) {
			graphicalComponents.Add(group.GetComponent<GraphicalComponent>());
		}
	}

	public TextWriter Out { get { return Console.Out; } }

	public Yondr.Entity? CreateEntity(string group, string bass) {
		EntityGroup cont = world.GroupDictionary[group];
		if (cont == null) {
			Log.Error("'{0}' is not an existing group.", group);
			return null;
		}

		Entity.Base entityBase = cont.GetBase(bass);
		if (entityBase == null) {
			Log.Error("'{0}' is not an existing base in group {1}", bass, group);
			return null;
		}

		return new Yondr.Entity(cont.Index, cont.CreateEntity(entityBase).Index);
	}

	public void EntitySetPosition(Yondr.Entity entity, Vector3 position) {
		var space = spacialComponents[entity.Group];
		if (space == null) {
			Log.Error("Can's set position of non-spacial entity!");
			return;
		}
		space.X[entity.Idx] = position.X;
		space.Y[entity.Idx] = position.Y;
		space.Z[entity.Idx] = position.Z;
	}
	public Vector3 EntityGetPosition(Yondr.Entity entity) {
		var space = spacialComponents[entity.Group];
		if (space == null) {
			Log.Error("Can't get position of non-spacial entity!");
			return new Vector3(0);
		}
		return new Vector3(space.X[entity.Idx], space.Y[entity.Idx], space.Z[entity.Idx]);
	}
	public void EntityMove(Yondr.Entity entity, Vector3 amount) {
		var space = spacialComponents[entity.Group];
		if (space == null) {
			Log.Error("Can't move non-spacial entity!");
			return;
		}
		space.X[entity.Idx] += amount.X;
		space.Y[entity.Idx] += amount.Y;
		space.Z[entity.Idx] += amount.Z;
	}

	public void EntityLookAt(Yondr.Entity entity, Vector3 at) {
		var space = spacialComponents[entity.Group];
		if (space == null) {
			Log.Error("Can't set orientation of non-spacial entity!");
			return;
		}
		int idx = entity.Idx;

		var dir = Vector3.Normalize(at - new Vector3(space.X[idx], space.Y[idx], space.Z[idx]));
		space.ResetOrientation(idx);
		space.RotateY(idx, (float)Math.Atan2(dir.X, dir.Z));
		space.RotateX(idx, (float)Math.Atan2(dir.Y, dir.X));
	}
	public void EntityRotateX(Yondr.Entity entity, float radians) {
		var space = spacialComponents[entity.Group];
		if (space == null) {
			Log.Error("Can't rotate non-spacial entity!");
			return;
		}
		space.RotateX(entity.Idx, radians);
	}
	public void EntityRotateY(Yondr.Entity entity, float radians) {
		var space = spacialComponents[entity.Group];
		if (space == null) {
			Log.Error("Can't rotate non-spacial entity!");
			return;
		}
		space.RotateY(entity.Idx, radians);
	}
	public void EntityRotateZ(Yondr.Entity entity, float radians) {
		var space = spacialComponents[entity.Group];
		if (space == null) {
			Log.Error("Can't rotate non-spacial entity!");
			return;
		}
		space.RotateZ(entity.Idx, radians);
	}
	public Vector3 EntityGetDirection(Yondr.Entity entity) {
		var space = spacialComponents[entity.Group];
		if (space == null) {
			Log.Error("Can't rotate non-spacial entity!");
			return new Vector3(0);
		}
		return space.GetDirection(entity.Idx);
	}

	public T EntityGet<T>(Yondr.Entity entity, Yondr.Property<T> prop) {
		return world.Groups[entity.Group].Entities[entity.Idx][prop.Index].As<T>();
	}

	public void EntitySet<T>(Yondr.Entity entity, Yondr.Property<T> prop, T val) {
		world.Groups[entity.Group].Entities[entity.Idx][prop.Index] = new Val(val);
	}

	public void EntitySetAsCamera(Yondr.Entity entity) {
		if (renderer == null) return;
		renderer.Camera = world.Groups[entity.Group].Entities[entity.Idx];
	}

	public void EntitySetTexture(Yondr.Entity entity, string texture) {
		GraphicalComponent graphics = graphicalComponents[entity.Group];
		if (graphics == null) {
			Log.Error("Can't set mesh of non-graphical entity!");
			return;
		}
		var e = world.Groups[entity.Group].Entities[entity.Idx];
		e[graphics.TextureProperty] = new Val(texture);

		if (renderer == null) return;
		renderer.SetTexture(e, texture);
	}
	public void EntitySetMesh(Yondr.Entity entity, string mesh) {
		GraphicalComponent graphics = graphicalComponents[entity.Group];
		if (graphics == null) {
			Log.Error("Can't set mesh of non-graphical entity!");
			return;
		}
		var e = world.Groups[entity.Group].Entities[entity.Idx];
		e[graphics.MeshProperty] = new Val(mesh);

		if (renderer == null) return;
		renderer.SetMesh(e, mesh);
	}
	
	private List<SpacialComponent>     spacialComponents = new List<SpacialComponent>();
	private List<GraphicalComponent> graphicalComponents = new List<GraphicalComponent>();
	private readonly World world;
	private readonly IRenderer renderer;
}
