﻿using System;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK;

public class TestRenderer: IRenderer {

	public TestRenderer(Res.Manager resManager, World world) {
		Res.Package core = resManager.Packages["core"];
		program = new Shader.Program(
			new Shader(core.Resources["vert"]),
			new Shader(core.Resources["frag"])
		);

		programPos = (int)program.GetAttrib("vPosition");
		Log.Info("program pos: {0}", programPos);

		GL.ClearColor(System.Drawing.Color.Chocolate);

		GL.GenVertexArrays(1, out vaoID);
		GL.BindVertexArray(vaoID);

		var vertices  = new Vector3[] {
			new Vector3(-1, -1, 0),
			new Vector3( 1, -1, 0),
			new Vector3( 0,  1, 0)
		};

		GL.GenBuffers(1, out vboID);
		GL.BindBuffer(BufferTarget.ArrayBuffer, vboID);
		GL.BufferData<Vector3>(
			BufferTarget.ArrayBuffer,
			new IntPtr(vertices.Length * Vector3.SizeInBytes),
			vertices, BufferUsageHint.StaticDraw
		);
	}
	~TestRenderer() {
		GL.DeleteBuffers(1, ref vboID);
		GL.DeleteVertexArrays(1, ref vaoID);
	}

	public void Render() {
		GL.Clear(ClearBufferMask.ColorBufferBit);

		program.Use();

		GL.EnableVertexAttribArray(programPos);
		GL.BindBuffer(BufferTarget.ArrayBuffer, vboID);
		GL.VertexAttribPointer(programPos, 3, VertexAttribPointerType.Float, false, 0, 0);

		GL.DrawArrays(PrimitiveType.Triangles, 0, 3);

		GL.DisableVertexAttribArray(programPos);

		GraphicsContext.CurrentContext.SwapBuffers();
	}

	public void AddObject(Entity obj) { }

	public void RemoveObject(Entity obj) { }

	public Entity Camera {
		get {
			throw new NotImplementedException();
		}
		set { }
	}

	int vaoID, vboID;

	Shader.Program program;
	int programPos;
}