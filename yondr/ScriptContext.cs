﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Numerics;

// An IContext is passed into all the scripting functions.
// It is defined in the script-context helper library.
public class ScriptContext: Yondr.IContext {
	public ScriptContext(World w, IRenderer r, IControls c) {
		world    = w;
		renderer = r;
		controls = c;

		foreach (var group in w.Groups) {
			spacialComponents.Add(group.GetComponent<SpacialComponent>());
		}
	}

	public TextWriter Out { get { return Console.Out; } }

	public bool Control(string control) {
		if (controls == null) return false;
		return controls.IsDown(control);
	}

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
			return;
		}
		space.X[entity.Idx] = position.X;
		space.Y[entity.Idx] = position.Y;
		space.Z[entity.Idx] = position.Z;
	}
	public Vector3 EntityGetPosition(EntityIdx entity) {
		var space = spacialComponents[entity.Group];
		if (space == null) {
			Log.Error("Can't get position of non-spacial entity!");
			return new Vector3(0);
		}
		return new Vector3(space.X[entity.Idx], space.Y[entity.Idx], space.Z[entity.Idx]);
	}
	public void EntityMove(EntityIdx entity, Vector3 amount) {
		var space = spacialComponents[entity.Group];
		if (space == null) {
			Log.Error("Can't move non-spacial entity!");
			return;
		}
		space.X[entity.Idx] += amount.X;
		space.Y[entity.Idx] += amount.Y;
		space.Z[entity.Idx] += amount.Z;
	}

	public void EntityLookAt(EntityIdx entity, Vector3 at) {
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
	public void EntityRotateX(EntityIdx entity, float radians) {
		var space = spacialComponents[entity.Group];
		if (space == null) {
			Log.Error("Can't rotate non-spacial entity!");
			return;
		}
		space.RotateX(entity.Idx, radians);
	}
	public void EntityRotateY(EntityIdx entity, float radians) {
		var space = spacialComponents[entity.Group];
		if (space == null) {
			Log.Error("Can't rotate non-spacial entity!");
			return;
		}
		space.RotateY(entity.Idx, radians);
	}
	public void EntityRotateZ(EntityIdx entity, float radians) {
		var space = spacialComponents[entity.Group];
		if (space == null) {
			Log.Error("Can't rotate non-spacial entity!");
			return;
		}
		space.RotateZ(entity.Idx, radians);
	}
	public Vector3 EntityGetDirection(EntityIdx entity) {
		var space = spacialComponents[entity.Group];
		if (space == null) {
			Log.Error("Can't rotate non-spacial entity!");
			return new Vector3(0);
		}
		return space.GetDirection(entity.Idx);
	}

	public void SetCamera(EntityIdx entity) {
		if (renderer == null) return;
		renderer.Camera = world.Groups[entity.Group].Entities[entity.Idx];
	}
	
	private List<SpacialComponent> spacialComponents = new List<SpacialComponent>();
	private readonly World world;
	private readonly IRenderer renderer;
	private readonly IControls controls;
}
