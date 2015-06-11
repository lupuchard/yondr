using System;
using System.IO;
using System.Drawing;
using System.Collections.Generic;
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

		Res.Package core = resManager.Packages["core"];
		program = new Shader.Program(
			new Shader(core.Resources["vert"]),
			new Shader(core.Resources["frag"])
		);

		programPos      = GL.GetUniformLocation(program.ID, "vPosition");
		programTexcoord = GL.GetUniformLocation(program.ID, "vTexcoord");
		programMvp      = GL.GetUniformLocation(program.ID, "mvpMatrix");
		programSp       = GL.GetUniformLocation(program.ID, "spMatrix");
		programTex      = GL.GetUniformLocation(program.ID, "tex");

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

		foreach (EntityGroup g in world.Groups) {
			try {
				Objects obj = new Objects(g);
				objects.Add(obj);
			} catch (ArgumentException) {
				objects.Add(null);
			}
		}

		tesmes();
		
		calculatePerspective(FOVY, 1, NEAR, FAR);

		GL.Enable(EnableCap.DepthTest);
		GL.ClearColor(Color.Chocolate);
	}
	
	public void Resize(int width, int height) {
		calculatePerspective(FOVY, (float)width / (float)height, NEAR, FAR);
	}
	
	public void Render() {
		if (camera == null) throw new InvalidOperationException("Camera not set.");

		GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

		Vector3 eye = cameraSpace.Position(Camera.Index);
		Vector3 dir, at;
		float angle;
		cameraSpace.Orientation(Camera.Index).ToAxisAngle(out dir, out angle);
		Vector3.Add(ref eye, ref dir, out at);
		Vector3 up = new Vector3(0, 1, 0);
		Matrix4 view = Matrix4.LookAt(eye, at, up);
		Matrix4 mvp;
		Matrix4.Mult(ref perspective, ref view, out mvp);
		
		program.Use();
		GL.UniformMatrix4(programMvp, true, ref mvp);
		//Matrix4 spMatrix;
		
		GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill); // TODO: per mesh

		/*foreach (Objects objs in objects) {
			if (objs == null) continue;
			for (int i = 0; i < objs.meshes.Count; i++) {
				if (!objs.Has(i)) continue;

				GMesh mesh = objs.meshes[i];
				Mesh.Geometry geom = mesh.Mesh.Geometries[0];
				
				BeginMode primitiveType;
				switch (geom.Type) {
					case Mesh.Primitive.TRIANGLES:
						primitiveType = BeginMode.Triangles;
						break;
					case Mesh.Primitive.TRIFANS:
						primitiveType = BeginMode.TriangleFan;
						break;
					case Mesh.Primitive.TRISTRIPS:
						primitiveType = BeginMode.TriangleStrip;
						break;
					default:
						throw new InvalidOperationException();
				}

				mesh.Bind();
				GL.EnableVertexAttribArray(programPos);
				GL.EnableVertexAttribArray(programTexcoord);
				GL.EnableVertexAttribArray(programNorm);

				var f = VertexAttribPointerType.Float;
				GL.VertexAttribPointer(programPos     , 3, f, false, 0, 0);
				GL.VertexAttribPointer(programTexcoord, 3, f, false, 0, testvlen);
				GL.VertexAttribPointer(programNorm    , 2, f, false, 0, testvlen + testnlen);

				GL.Uniform1(programTex, objs.textures[i].ID);

				objs.Spacial.Matrix(i, out spMatrix);
				GL.UniformMatrix4(programSp, false, ref spMatrix);

				GL.DrawElements(primitiveType, geom.Indices.Length, DrawElementsType.UnsignedInt, 0);

				GL.DisableVertexAttribArray(programPos);
				GL.DisableVertexAttribArray(programTexcoord);
				GL.DisableVertexAttribArray(programNorm);
			}
		}*/

		// test 1
		/*Mesh testMesh = new Mesh();
		var vertSource = new Mesh.Source<float>();
		vertSource.Arr = new float[] {
			50, 50 , 1,
			50, 300, 1,
			300, 50, 1
		};
		vertSource.Stride = 3;
		var normSource = new Mesh.Source<float>();
		normSource.Arr = new float[] { 1, 0, 0, 1, 0, 0, 1, 0, 0 };
		normSource.Stride = 3;
		var texSource = new Mesh.Source<float>();
		texSource.Arr = new float[] { 0, 0, 1, 0, 0, 1 };
		texSource.Stride = 2;
		testMesh.Geometries.Add(
			new Mesh.Geometry(Mesh.Primitive.TRIANGLES, true,
				vertSource, 0, normSource, 0, texSource, 0, new[] { 0, 1, 2 }
			)
		);
		GMesh testGMesh = new GMesh(testMesh, program);

		var ident = Matrix4.Identity;
		GL.UniformMatrix4(mvpID, true, ref ident);
		GL.UniformMatrix4(spID, true, ref ident);

		testGMesh.Bind();
		program.Use();
		var geom0 = testGMesh.Mesh.Geometries[0];
		GL.DrawElements(BeginMode.Triangles, geom0.Indices.Length, DrawElementsType.UnsignedInt, 0);*/


		// test 2
		//GL.BindVertexArray(testvaoID);
		program.Use();
		//var ident = Matrix4.Identity;
		//GL.UniformMatrix4(programMvp, true, ref ident);
		//GL.UniformMatrix4(programSp, true, ref ident);
		//GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);

		int pos = GL.GetUniformLocation(program.ID, "vPosition");

		GL.EnableVertexAttribArray(pos);
		//GL.EnableVertexAttribArray(norm);
		//GL.EnableVertexAttribArray(tex);
		GL.BindBuffer(BufferTarget.ArrayBuffer, testvboID);

		GL.VertexAttribPointer(pos, 3, VertexAttribPointerType.Float, false, 0, 0);
		//GL.VertexAttribPointer(norm, 3, VertexAttribPointerType.Float, false, 0, testvlen);
		//GL.VertexAttribPointer(tex, 2, VertexAttribPointerType.Float, false, 0, testvlen + testnlen);

		//Texture texture = textures["space:yams"];
		//GL.BindTexture(TextureTarget.Texture2D, texture.ID);
		//GL.Uniform1(programTex, texture.ID);


		GL.DrawArrays(PrimitiveType.Triangles, 0, 3);
		//GL.DrawElements(BeginMode.Triangles, 3, DrawElementsType.UnsignedInt, 0);

		GL.DisableVertexAttribArray(pos);
		//GL.DisableVertexAttribArray(norm);
		//GL.DisableVertexAttribArray(tex);

		GraphicsContext.CurrentContext.SwapBuffers();
	}

	int testvaoID;
	int testvboID;
	//int testindexID;
	int testvlen;
	//int testnlen;
	void tesmes() {
		//GL.Enable(EnableCap.Texture2D);

		var vertices  = new float[] {-1, -1, 0, 1, -1, 0, 0, 1, 0};
		//var texcoords = new float[] { 0, 0, 1, 0, 0.5f, 1 };
		//var indices   = new int[] { 0, 2, 1 };

		GL.GenVertexArrays(1, out testvaoID);
		GL.BindVertexArray(testvaoID);

		testvlen = vertices.Length * sizeof(float);
		//int ilen = indices.Length * sizeof(int);
		//int tlen = texcoords.Length * sizeof(float);

		GL.GenBuffers(1, out testvboID);
		GL.BindBuffer(BufferTarget.ArrayBuffer, testvboID);
		GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)testvlen, vertices, BufferUsageHint.StaticDraw);
		//GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(testvlen/* + testnlen/* + tlen*/), (IntPtr)0, BufferUsageHint.StaticDraw);
		//GL.BufferSubData(BufferTarget.ArrayBuffer, (IntPtr)0, (IntPtr)testvlen, vertices);
		//GL.BufferSubData(BufferTarget.ArrayBuffer, (IntPtr)(testvlen + testnlen), (IntPtr)tlen, texcoords);

		//GL.GenBuffers(1, out testindexID);
		//GL.BindBuffer(BufferTarget.ElementArrayBuffer, testindexID);
		//GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)ilen, indices, BufferUsageHint.StaticDraw);
	}
	
	public void AddObject(Entity entity) {
		EntityGroup entityGroup = world.Groups[entity.PropertySystem.Index];
		if (objects[entityGroup.Index] == null) throw new InvalidOperationException();
		Objects obj = objects[entityGroup.Index];

		string meshName = entity[obj.Graphical.MeshProperty].AsString();
		GMesh mesh;
		if (!meshes.TryGetValue(meshName, out mesh)) {
			Log.Error("No mesh named {0} found.", meshName);
			return;
		}

		string texName = entity[obj.Graphical.TextureProperty].AsString();
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
