using System;
using System.IO;
using System.Collections.Generic;
using System.Numerics;

// An IContext is passed into all the scripting functions.
// It is defined in the script-context helper library.
public class ScriptContext: Yondr.IContext {
	public ScriptContext(World w, IRenderer r) {
		world = w;
		renderer = r;

		foreach (var group in w.Groups) {
			spacialComponents.Add(group.GetComponent<SpacialComponent>());
		}
	}

	public TextWriter Out { get { return Console.Out; } }

	public EntityIdx? CreateEntityI(string group, string bass) {
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

		return new EntityIdx(cont.Index, cont.CreateEntity(entityBase).Index);
	}

	public void EntitySetPosition(EntityIdx entity, Vector3 position) {
		var space = spacialComponents[entity.Group];
		if (space == null) {
			Log.Error("Can's set position of non-spacial entity!");
		} else {
			space.X[entity.Idx] = position.X;
			space.Y[entity.Idx] = position.Y;
			space.Z[entity.Idx] = position.Z;
		}
	}

	public Vector3 EntityGetPosition(EntityIdx entity) {
		var space = spacialComponents[entity.Group];
		if (space == null) {
			Log.Error("Can't set position of non-spacial entity!");
			return new Vector3(0);
		} else {
			return new Vector3(space.X[entity.Idx], space.Y[entity.Idx], space.Z[entity.Idx]);
		}
	}

	public void EntityLookAt(EntityIdx entity, Vector3 at) {
		var space = spacialComponents[entity.Group];
		if (space == null) {
			Log.Error("Can't set orientation of non-spacial entity!");
			return;
		}
		int idx = entity.Idx;

		var eye = new Vector3(space.X[idx], space.Y[idx], space.Z[idx]);
		var mat = Matrix4x4.CreateLookAt(eye, at, new Vector3(0, 1, 0));

		space.Qw[idx] = (float)Math.Sqrt(1 + mat.M11 + mat.M22 + mat.M33) / 2;
		space.Qx[idx] = (mat.M32 - mat.M23) / (4 * space.Qw[idx]);
		space.Qy[idx] = (mat.M13 - mat.M31) / (4 * space.Qw[idx]);
		space.Qz[idx] = (mat.M21 - mat.M12) / (4 * space.Qw[idx]);
	}

	public void SetCamera(EntityIdx entity) {
		if (renderer == null) return;
		renderer.Camera = world.Groups[entity.Group].Entities[entity.Idx];
	}
	
	private List<SpacialComponent> spacialComponents = new List<SpacialComponent>();
	private readonly World world;
	private readonly IRenderer renderer;
}
