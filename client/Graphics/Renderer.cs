using System;
using System.IO;
using System.Drawing;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

public class Renderer: RendererI {
	
	public const float NEAR =  1f;
	public const float FAR  = 10f;
	public const float FOVY = (float)Math.PI / 2.0f;
	
	public Renderer(Res.Manager resManager, World world) {
		resources  = resManager;
		this.world = world;

		Res.Package core = resManager.Packages["core"];
		program = new Shader.Program(
			new Shader(core.Resources["vert"]),
			new Shader(core.Resources["frag"])
		);

		foreach (Res.Res res in resources.Resources) {
			switch (res.Type) {
				case Res.Type.MESH:
					IFormatter formatter = new BinaryFormatter();
					Mesh mesh = (Mesh)formatter.Deserialize(new MemoryStream(res.Data));
					meshes.Add(res.Package.Name + ":" + res.Name, new GMesh(mesh, program));
					break;
				case Res.Type.PNG:
					Texture tex = new Texture(res);
					textures.Add(res.Package.Name + ":" + res.Name, tex);
					break;
			}
		}
		
		calculatePerspective(FOVY, 1, NEAR, FAR);

		GL.ClearColor(Color.Chocolate);
	}
	
	public void Resize(int width, int height) {
		calculatePerspective(FOVY, (float)width / (float)height, NEAR, FAR);
	}
	
	public void Render() {
		if (camera == null) throw new InvalidOperationException("Camera not set.");

		GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

		Vector3 eye = cameraSpace.Position(Camera);
		Vector3 dir, at;
		float angle;
		cameraSpace.Orientation(Camera).ToAxisAngle(out dir, out angle);
		Vector3.Add(ref eye, ref dir, out at);
		Vector3 up = new Vector3(0, 1, 0);
		Matrix4 view = Matrix4.LookAt(eye, at, up);
		Matrix4 mvp;
		Matrix4.Mult(ref perspective, ref view, out mvp);
		
		program.Use();
		int mvpID = GL.GetUniformLocation(program.ID, "mvpMatrix");
		GL.UniformMatrix4(mvpID, true, ref mvp);
		int texID = GL.GetUniformLocation(program.ID, "tex");
		int spID = GL.GetUniformLocation(program.ID, "spMatrix");
		Matrix4 spMatrix;
		
		GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill); // TODO: per mesh
		
		for (int i = 0; i < objects.owners.Count; i++) {
			Entity entity = objects.owners[i];

			GMesh mesh = objects.meshes[i];
			mesh.Bind();
			
			Mesh.Geometry geom = mesh.Mesh.Geometries[0];
			
			BeginMode primitiveType;
			switch (geom.Type) {
				case Mesh.Primitive.TRIANGLES: primitiveType = BeginMode.Triangles;     break;
				case Mesh.Primitive.TRIFANS:   primitiveType = BeginMode.TriangleFan;   break;
				case Mesh.Primitive.TRISTRIPS: primitiveType = BeginMode.TriangleStrip; break;
				default: throw new InvalidOperationException();
			}
			
			GL.Uniform1(texID, objects.textures[i].ID);

			// TODO
			world.Groups[entity.PropertySystem.Index].GetComponent<SpacialComponent>().Matrix(entity, out spMatrix);
			GL.UniformMatrix4(spID, false, ref spMatrix);

			GL.DrawElements(primitiveType, geom.Indices.Length, DrawElementsType.UnsignedInt, 0);
		}
		
		GraphicsContext.CurrentContext.SwapBuffers();
	}
	
	public void AddObject(Entity entity) {
		var graphics = world.Groups[entity.PropertySystem.Index].GetComponent<GraphicalComponent>();
		if (graphics == null) {
			Log.Error("This entity can't be rendered because its Group" + 
				"does not have a GraphicsComponent.");
			return;
		}

		GMesh  mesh =   meshes[entity[graphics.MeshProperty   ].AsString()];
		Texture tex = textures[entity[graphics.TextureProperty].AsString()];
		objects.Add(entity, mesh, tex);
	}
	
	private void calculatePerspective(float fovy, float aspect, float near, float far) {
		Matrix4.CreatePerspectiveFieldOfView(fovy, aspect, near, far, out perspective);
	}
	
	private class Objects {
		public void Add(Entity entity, GMesh mesh, Texture tex) {
			owners.Add(entity);
			meshes.Add(mesh);
			textures.Add(tex);
		}
		public List<Entity>  owners   = new List<Entity>();
		public List<GMesh>   meshes   = new List<GMesh>();
		public List<Texture> textures = new List<Texture>();
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
	private Objects objects = new Objects();

	private readonly World world;

	private Res.Manager resources;
	private Dictionary<string, GMesh>   meshes   = new Dictionary<string, GMesh>();
	private Dictionary<string, Texture> textures = new Dictionary<string, Texture>();
	
	private Shader.Program program;
}

public static class SpacialComponentExtension {
	public static Vector3 Position(this SpacialComponent spacial, Entity entity) {
		return new Vector3(
			spacial.X[entity.Index],
			spacial.Y[entity.Index],
			spacial.Z[entity.Index]
		);
	}
	public static Quaternion Orientation(this SpacialComponent spacial, Entity entity) {
		return new Quaternion(
			spacial.A[entity.Index],
			spacial.B[entity.Index],
			spacial.C[entity.Index],
			spacial.D[entity.Index]
		);
	}
	public static void Matrix(this SpacialComponent spacial, Entity entity, out Matrix4 mat) {
		Matrix4 orientation = Matrix4.CreateFromQuaternion(spacial.Orientation(entity));
		Matrix4 translation = Matrix4.CreateTranslation(spacial.Position(entity));
		Matrix4.Mult(ref translation, ref orientation, out mat);
	}
}
