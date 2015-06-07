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
	
		int vlen = geom.Vertices. Arr.Length * sizeof(float);
		int tlen = geom.Texcoords.Arr.Length * sizeof(float);
		int ilen = geom.Indices.Length       * sizeof(int);
		
		BufferTarget arrBuf  = BufferTarget.ArrayBuffer;
		BufferTarget elemBuf = BufferTarget.ElementArrayBuffer;
		
		// Create and initialize a buffer object
		GL.GenBuffers(1, out vboID);
		GL.BindBuffer(arrBuf, vboID);
		GL.BufferData(arrBuf,
			(IntPtr)(vlen + tlen),
			(IntPtr)0, BufferUsageHint.DynamicDraw // TODO: better hint?
		);
		
		// Set the buffer data
		GL.BufferSubData(arrBuf, (IntPtr)0            , (IntPtr)vlen, geom.Vertices.Arr);
		GL.BufferSubData(arrBuf, (IntPtr)vlen         , (IntPtr)tlen, geom.Texcoords.Arr);

		// Bind the index buffer
		GL.GenBuffers(1, out indexID);
		GL.BindBuffer(elemBuf, indexID);
		GL.BufferData(elemBuf, (IntPtr)ilen, geom.Indices, BufferUsageHint.DynamicDraw);

		Util.CheckGL("mesh");
	}
	~GMesh() {
		GL.DeleteVertexArrays(1, ref vaoID);
		GL.DeleteBuffers(     1, ref vboID);
		GL.DeleteBuffers(     1, ref indexID);
	}
	
	public void Bind() {
		GL.BindVertexArray(vaoID);
	}
	
	public Mesh Mesh { get; }
	private int vaoID;
	private int vboID;
	private int indexID;
}
