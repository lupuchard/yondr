using System;
using System.Collections.Generic;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class TestRenderer: IRenderer {

	public const float NEAR = 0.1f;
	public const float FAR  = 10f;
	public const float FOVY = (float)Math.PI / 2.0f;

	public TestRenderer(Res.Manager resManager, World world) {
		resources  = resManager;
		this.world = world;

		Res.Package core = resManager.Packages["core"];
		program = new Shader.Program(
			new Shader(core.Resources["vert"]),
			new Shader(core.Resources["frag"])
		);

		Res.Package space = resManager.Packages["space"];
		tex = new Texture(space.Resources["yams"]);

		programPos      = (int)program.GetAttrib("vPosition");
		programTexcoord = (int)program.GetAttrib("vTexcoord");
		programMvp      = (int)program.GetUniform("mvpMatrix");
		programSp       = (int)program.GetUniform("spMatrix");
		programTex      = (int)program.GetUniform("tex");

		foreach (Res.Res res in resources.Resources) {
			switch (res.Type) {
				case Res.Type.MESH:
					IFormatter formatter = new BinaryFormatter();
					Mesh mesh = (Mesh)formatter.Deserialize(new MemoryStream(res.Data));
					meshes.Add(res.Package.Name + ":" + res.Name, new GMesh(mesh));
					break;
				case Res.Type.PNG:
				case Res.Type.JPG:
					Texture tex = new Texture(res);
					textures.Add(res.Package.Name + ":" + res.Name, tex);
					break;
			}
		}

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

		GL.GenVertexArrays(1, out vaoID);
		GL.BindVertexArray(vaoID);

		var vertices = new Vector3[] {
			new Vector3(-1, -1, 0),
			new Vector3( 1, -1, 0),
			new Vector3( 0,  1, 0)
		};

		var texcoords = new Vector2[] {
			new Vector2(0, 1),
			new Vector2(1, 1),
			new Vector2(0.5f, 0),
		};

		var indices = new int[] { 0, 1, 2 };

		GL.GenBuffers(1, out vboID);
		GL.BindBuffer(BufferTarget.ArrayBuffer, vboID);
		GL.BufferData<Vector3>(
			BufferTarget.ArrayBuffer,
			new IntPtr(vertices.Length * Vector3.SizeInBytes),
			vertices, BufferUsageHint.StaticDraw
		);

		GL.GenBuffers(1, out texID);
		GL.BindBuffer(BufferTarget.ArrayBuffer, texID);
		GL.BufferData<Vector2>(
			BufferTarget.ArrayBuffer,
			new IntPtr(texcoords.Length * Vector2.SizeInBytes),
			texcoords, BufferUsageHint.StaticDraw
		);

		GL.GenBuffers(1, out idxID);
		GL.BindBuffer(BufferTarget.ElementArrayBuffer, idxID);
		GL.BufferData<int>(
			BufferTarget.ElementArrayBuffer,
			new IntPtr(indices.Length * sizeof(int)),
			indices, BufferUsageHint.StaticDraw
		);

		program.Use();

		GL.BindBuffer(BufferTarget.ArrayBuffer, vboID);
		GL.VertexAttribPointer(programPos, 3, VertexAttribPointerType.Float, false, 0, 0);

		GL.BindBuffer(BufferTarget.ArrayBuffer, texID);
		GL.VertexAttribPointer(programTexcoord, 2, VertexAttribPointerType.Float, false, 0, 0);

		GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
	}
	~TestRenderer() {
		GL.DeleteBuffers(1, ref vboID);
		GL.DeleteVertexArrays(1, ref vaoID);
	}

	public void Render() {
		if (camera == null) throw new InvalidOperationException("Camera not set.");

		GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

		GL.BindVertexArray(vaoID);

		program.Use();

		GL.ActiveTexture(TextureUnit.Texture0);
		GL.BindTexture(TextureTarget.Texture2D, tex.ID);
		GL.Uniform1(programTex, 0);

		GL.EnableVertexAttribArray(programPos);
		GL.EnableVertexAttribArray(programTexcoord);

		GL.BindBuffer(BufferTarget.ElementArrayBuffer, idxID);
		GL.DrawElements(BeginMode.Triangles, 3, DrawElementsType.UnsignedInt, 0);

		GL.DisableVertexAttribArray(programPos);
		GL.DisableVertexAttribArray(programTexcoord);

		Util.CheckGL("render");

		GraphicsContext.CurrentContext.SwapBuffers();
	}

	public void Resize(int width, int height) {
		calculatePerspective(FOVY, (float)width / (float)height, NEAR, FAR);
	}

	public void AddObject(Entity obj) { }

	public void RemoveObject(Entity obj) { }

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

	private int vaoID, vboID, texID, idxID;

	private Texture tex;

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
			spacial.A[entityIdx],
			spacial.B[entityIdx],
			spacial.C[entityIdx],
			spacial.D[entityIdx]
		);
	}
	public static void Matrix(this SpacialComponent spacial, int entityIdx, out Matrix4 mat) {
		Matrix4 orientation = Matrix4.CreateFromQuaternion(spacial.Orientation(entityIdx));
		Matrix4 translation = Matrix4.CreateTranslation(spacial.Position(entityIdx));
		Matrix4.Mult(ref translation, ref orientation, out mat);
	}
}
