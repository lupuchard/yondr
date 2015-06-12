using System;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL;

public class GMesh {
	public GMesh(Mesh m) {
		this.Mesh = m;
		foreach (var subMesh in this.Mesh.SubMeshes) {
			SubMesh newMesh = new SubMesh();

			// Create and initialize a vertex array object
			GL.GenVertexArrays(1, out newMesh.VaoID);
			GL.BindVertexArray(newMesh.VaoID);

			// Vertex buffer
			GL.GenBuffers(1, out newMesh.VertID);
			GL.BindBuffer(BufferTarget.ArrayBuffer, newMesh.VertID);
			GL.BufferData(
				BufferTarget.ArrayBuffer,
				new IntPtr(subMesh.Vertices.Length * sizeof(float)),
				subMesh.Vertices, BufferUsageHint.StaticDraw
			);

			// Texcoord buffer
			GL.GenBuffers(1, out newMesh.TexID);
			GL.BindBuffer(BufferTarget.ArrayBuffer, newMesh.TexID);
			GL.BufferData(
				BufferTarget.ArrayBuffer,
				new IntPtr(subMesh.Texcoords.Length * sizeof(float)),
				subMesh.Texcoords, BufferUsageHint.StaticDraw
			);

			// Index buffer
			GL.GenBuffers(1, out newMesh.IndexID);
			GL.BindBuffer(BufferTarget.ElementArrayBuffer, newMesh.IndexID);
			GL.BufferData(
				BufferTarget.ElementArrayBuffer,
				new IntPtr(subMesh.Indices.Length * sizeof(int)),
				subMesh.Indices, BufferUsageHint.StaticDraw
			);

			newMesh.NumIndices = subMesh.Indices.Length;

			SubMeshes.Add(newMesh);
		}

		Util.CheckGL("create mesh");
	}
	~GMesh() {
		foreach (var subMesh in SubMeshes) {
			GL.DeleteVertexArrays(1, ref subMesh.VaoID);
			GL.DeleteBuffers(1, ref subMesh.VertID);
			GL.DeleteBuffers(1, ref subMesh.TexID);
			GL.DeleteBuffers(1, ref subMesh.IndexID);
		}

		Util.CheckGL("delete mesh");
	}

	public void Link(Shader.Program program, int posAttrib, int texcoordAttrib) {
		program.Use();

		foreach (var subMesh in SubMeshes) {
			GL.BindBuffer(BufferTarget.ArrayBuffer, subMesh.VertID);
			GL.VertexAttribPointer(posAttrib, 3, VertexAttribPointerType.Float, false, 0, 0);

			GL.BindBuffer(BufferTarget.ArrayBuffer, subMesh.TexID);
			GL.VertexAttribPointer(texcoordAttrib, 2, VertexAttribPointerType.Float, false, 0, 0);
		}

		GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

		Util.CheckGL("link mesh");
	}

	public class SubMesh {
		public int VaoID;
		public int VertID;
		public int TexID;
		public int IndexID;
		public int NumIndices;
	}
	
	public Mesh Mesh { get; }
	public List<SubMesh> SubMeshes { get; } = new List<SubMesh>();
}
