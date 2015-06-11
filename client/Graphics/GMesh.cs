using System;
using OpenTK.Graphics.OpenGL;

public class GMesh {
	public GMesh(Mesh m) {
		this.Mesh = m;
		
		// TODO: multiple geometries
		Mesh.Geometry geom = Mesh.Geometries[0];
		
		// Create and initialize a vertex array object
		GL.GenVertexArrays(1, out vaoID);
		GL.BindVertexArray(vaoID);

		// Vertex buffer
		GL.GenBuffers(1, out vertID);
		GL.BindBuffer(BufferTarget.ArrayBuffer, vertID);
		GL.BufferData(
			BufferTarget.ArrayBuffer,
			new IntPtr(geom.Vertices.Arr.Length * sizeof(float)),
			geom.Vertices.Arr, BufferUsageHint.StaticDraw
		);

		// Texcoord buffer
		GL.GenBuffers(1, out texID);
		GL.BindBuffer(BufferTarget.ArrayBuffer, texID);
		GL.BufferData(
			BufferTarget.ArrayBuffer,
			new IntPtr(geom.Texcoords.Arr.Length * sizeof(float)),
			geom.Texcoords.Arr, BufferUsageHint.StaticDraw
		);

		Log.Info("kay {0}", geom.NormalsOffset - geom.VerticesOffset);
		Log.Info("{0}", string.Join(",", geom.Vertices.Arr));
		Log.Info("{0}", string.Join(",", geom.Indices));

		// Index buffer
		GL.GenBuffers(1, out indexID);
		GL.BindBuffer(BufferTarget.ElementArrayBuffer, indexID);
		GL.BufferData(
			BufferTarget.ElementArrayBuffer,
			new IntPtr((geom.NormalsOffset - geom.VerticesOffset) * sizeof(int)),
			geom.Indices, BufferUsageHint.StaticDraw
		);

		Util.CheckGL("create mesh");
	}
	~GMesh() {
		GL.DeleteVertexArrays(1, ref vaoID);
		GL.DeleteBuffers(1, ref vertID);
		GL.DeleteBuffers(1, ref texID);
		GL.DeleteBuffers(1, ref indexID);

		Util.CheckGL("delete mesh");
	}

	public void Link(Shader.Program program, int posAttrib, int texcoordAttrib) {
		program.Use();

		GL.BindBuffer(BufferTarget.ArrayBuffer, vertID);
		GL.VertexAttribPointer(posAttrib, 3, VertexAttribPointerType.Float, false, 0, 0);

		GL.BindBuffer(BufferTarget.ArrayBuffer, texID);
		GL.VertexAttribPointer(texcoordAttrib, 2, VertexAttribPointerType.Float, false, 0, 0);

		GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

		Util.CheckGL("link mesh");
	}
	
	public Mesh Mesh { get; }

	private int vaoID;
	public int VaoID { get { return vaoID; } }

	private int vertID;
	private int texID;

	private int indexID;
	public int IndexID { get { return indexID; } }
}
