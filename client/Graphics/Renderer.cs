using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

public class Renderer: IRenderer {

	public const float NEAR = 0.1f;
	public const float FAR  = 100f;
	public const float FOVY = (float)Math.PI / 2.0f;

	public Renderer(Res.Manager resManager, World world) {
		resources  = resManager;
		this.world = world;

		// load core shader
		Res.Package core = resManager.Packages["core"];
		program = new Shader.Program(
			new Shader(core.Resources["vert"]),
			new Shader(core.Resources["frag"])
		);
		programPos      = (int)program.GetAttrib("vPosition");
		programTexcoord = (int)program.GetAttrib("vTexcoord");
		programMvp      = (int)program.GetUniform("mvpMatrix");
		programSp       = (int)program.GetUniform("spMatrix");
		programTex      = (int)program.GetUniform("tex");

		// get all meshes and textures from the resource manager
		foreach (Res.Res res in resources.Resources) {
			switch (res.Type) {
				case Res.Type.MESH:
					IFormatter formatter = new BinaryFormatter();
					Mesh mesh = (Mesh)formatter.Deserialize(new MemoryStream(res.Data));
					GMesh gmesh = new GMesh(mesh);
					gmesh.Link(program, programPos, programTexcoord);
					meshes.Add(res.Package.Name + ":" + res.Name, gmesh);
					break;
				case Res.Type.PNG:
				case Res.Type.JPG:
					Texture tex = new Texture(res);
					textures.Add(res.Package.Name + ":" + res.Name, tex);
					break;
			}
		}

		// create object array for each graphical entity group
		foreach (EntityGroup g in world.Groups) {
			try {
				Objects obj = new Objects(g);
				objects.Add(obj);
			} catch (ArgumentException) {
				objects.Add(null);
			}
		}

		calculatePerspective(FOVY, 1, NEAR, FAR);

		GL.Enable(EnableCap.DepthTest);
		GL.ClearColor(System.Drawing.Color.Chocolate);
	}

	public void Render() {
		if (camera == null) throw new InvalidOperationException("Camera not set.");

		GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

		var eye = cameraSpace.Position(Camera.Index);
		var dir = cameraSpace.GetDirection(Camera.Index).ToOpenTK();
		var up  = cameraSpace.GetUp(       Camera.Index).ToOpenTK();
		var at = eye + dir;
		Matrix4 view = Matrix4.LookAt(eye, at, up);
		Matrix4 mvp;
		Matrix4.Mult(ref view, ref perspective, out mvp);

		program.Use();

		GL.UniformMatrix4(programMvp, false, ref mvp);
		Matrix4 spMatrix;

		GL.ActiveTexture(TextureUnit.Texture0);

		foreach (Objects objs in objects) {
			if (objs == null) continue;
			for (int i = 0; i < objs.meshes.Count; i++) {
				if (!objs.Has(i)) continue;
				foreach (var mesh in objs.meshes[i].SubMeshes) {
					GL.BindTexture(TextureTarget.Texture2D, objs.textures[i].ID);
					GL.Uniform1(programTex, 0);

					objs.Spacial.Matrix(i, out spMatrix);
					GL.UniformMatrix4(programSp, false, ref spMatrix);

					GL.BindVertexArray(mesh.VaoID);

					GL.EnableVertexAttribArray(programPos);
					GL.EnableVertexAttribArray(programTexcoord);

					GL.BindBuffer(BufferTarget.ElementArrayBuffer, mesh.IndexID);
					GL.DrawElements(BeginMode.Triangles, mesh.NumIndices, DrawElementsType.UnsignedInt, 0);

					GL.DisableVertexAttribArray(programPos);
					GL.DisableVertexAttribArray(programTexcoord);
				}
			}
		}

		Util.CheckGL("render");

		GraphicsContext.CurrentContext.SwapBuffers();
	}

	public void Resize(int width, int height) {
		calculatePerspective(FOVY, (float)width / (float)height, NEAR, FAR);
	}

	public void AddObject(Entity entity) {
		EntityGroup entityGroup = world.Groups[entity.PropertySystem.Index];
		if (objects[entityGroup.Index] == null) throw new InvalidOperationException();
		Objects obj = objects[entityGroup.Index];

		string meshName = entity[obj.Graphical.MeshProperty].As<string>();
		GMesh mesh;
		if (!meshes.TryGetValue(meshName, out mesh)) {
			Log.Error("No mesh named {0} found.", meshName);
			return;
		}

		string texName = entity[obj.Graphical.TextureProperty].As<string>();
		Texture tex;
		if (!textures.TryGetValue(texName, out tex)) {
			Log.Error("No texture named {0} found.", texName);
			return;
		}

		obj.Add(entity, mesh, tex);
	}

	public void RemoveObject(Entity entity) {
		EntityGroup entityGroup = world.Groups[entity.PropertySystem.Index];
		if (objects[entityGroup.Index] == null) throw new InvalidOperationException();
		Objects obj = objects[entityGroup.Index];
		obj.Remove(entity);
	}

	public void SetTexture(Entity entity, string texName) {
		EntityGroup entityGroup = world.Groups[entity.PropertySystem.Index];
		if (objects[entityGroup.Index] == null) throw new InvalidOperationException();

		Texture tex;
		if (!textures.TryGetValue(texName, out tex)) {
			Log.Error("No texture named {0} found.", texName);
			return;
		}

		objects[entityGroup.Index].textures[entity.Index] = tex;
	}

	public void SetMesh(Entity entity, string meshName) {
		EntityGroup entityGroup = world.Groups[entity.PropertySystem.Index];
		if (objects[entityGroup.Index] == null) throw new InvalidOperationException();

		GMesh mesh;
		if (!meshes.TryGetValue(meshName, out mesh)) {
			Log.Error("No mesh named {0} found.", meshName);
			return;
		}

		objects[entityGroup.Index].meshes[entity.Index] = mesh;
	}

	private void calculatePerspective(float fovy, float aspect, float near, float far) {
		Matrix4.CreatePerspectiveFieldOfView(fovy, aspect, near, far, out perspective);
	}

	private class Objects {
		public Objects(EntityGroup entityGroup) {
			Spacial = entityGroup.GetComponent<SpacialComponent>();
			if (Spacial == null) throw new ArgumentException();

			Graphical = entityGroup.GetComponent<GraphicalComponent>();
			if (Graphical == null) throw new ArgumentException();
		}
		public void Add(Entity entity, GMesh mesh, Texture tex) {
			while (meshes.Count <= entity.Index) {
				meshes.Add(null);
				textures.Add(null);
			}
			meshes[entity.Index] = mesh;
			textures[entity.Index] = tex;
		}
		public void Remove(Entity entity) {
			meshes[entity.Index] = null;
		}
		public bool Has(int index) {
			return meshes[index] != null;
		}
		public List<GMesh>   meshes   = new List<GMesh>();
		public List<Texture> textures = new List<Texture>();
		public SpacialComponent   Spacial   { get; }
		public GraphicalComponent Graphical { get; }
	}

	private Entity camera = null;
	public Entity Camera {
		get { return camera; }
		set {
			camera = value;
			int groupIndex = camera.PropertySystem.Index;
			cameraSpace = world.Groups[groupIndex].GetComponent<SpacialComponent>();
		}
	}
	private SpacialComponent cameraSpace = null;

	private Matrix4 perspective;
	private List<Objects> objects = new List<Objects>();

	private readonly World world;

	private Res.Manager resources;
	private Dictionary<string, GMesh>   meshes   = new Dictionary<string, GMesh>();
	private Dictionary<string, Texture> textures = new Dictionary<string, Texture>();

	private Shader.Program program;
	private int programPos;
	private int programTexcoord;
	private int programMvp;
	private int programSp;
	private int programTex;
}

public static class SpacialComponentExtension {
	public static Vector3 Position(this SpacialComponent spacial, int entityIdx) {
		return new Vector3(
			spacial.X[entityIdx],
			spacial.Y[entityIdx],
			spacial.Z[entityIdx]
		);
	}
	public static Quaternion Orientation(this SpacialComponent spacial, int entityIdx) {
		return new Quaternion(
			spacial.Qx[entityIdx],
			spacial.Qy[entityIdx],
			spacial.Qz[entityIdx],
			spacial.Qw[entityIdx]
		);
	}
	public static void Matrix(this SpacialComponent spacial, int entityIdx, out Matrix4 mat) {
		Matrix4 orientation = Matrix4.CreateFromQuaternion(spacial.Orientation(entityIdx));
		Matrix4 translation = Matrix4.CreateTranslation(spacial.Position(entityIdx));
		Matrix4.Mult(ref translation, ref orientation, out mat);
	}
}

public static class VectorConversion {
	public static Vector3 ToOpenTK(this System.Numerics.Vector3 vec) {
		return new Vector3(vec.X, vec.Y, vec.Z);
	}
	public static System.Numerics.Vector3 ToNumerics(this Vector3 vec) {
		return new System.Numerics.Vector3(vec.X, vec.Y, vec.Z);
	}
}
