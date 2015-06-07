using System;
using System.Text;
using OpenTK.Graphics.OpenGL;

public class Shader {
	public Shader(Res.Res res) {
		switch (res.Type) {
			case Res.Type.VERT: this.Type = ShaderType.VertexShader;   break;
			case Res.Type.FRAG: this.Type = ShaderType.FragmentShader; break;
			default: throw new ArgumentException("Given resource is not a shader source.");
		}
		
		ID = GL.CreateShader(Type);
		if (ID == 0) throw new InvalidOperationException("Failed to create shader.");

		int len = res.Data.Length;
		GL.ShaderSource(ID, Encoding.Default.GetString(res.Data));
		GL.CompileShader(ID);

		int statusCode;
		GL.GetShader(ID, ShaderParameter.CompileStatus, out statusCode);
		if (statusCode != 1) {
			string info;
			GL.GetShaderInfoLog(ID, out info);
			Log.Error("Failed to compile {0}.", res.Path);
			Log.Info(info);
			throw new InvalidOperationException("Failed to compile shader.");
		}
	}

	public class Program {
		public Program(Shader vert, Shader frag) {
			if (vert.Type != ShaderType.VertexShader)   throw new ArgumentException();
			if (frag.Type != ShaderType.FragmentShader) throw new ArgumentException();

			Vert = vert;
			Frag = frag;
			
			ID = GL.CreateProgram();
			GL.AttachShader(ID, frag.ID);
			GL.AttachShader(ID, vert.ID);
			
			GL.LinkProgram(ID);
			GL.UseProgram(ID);
		}
		~Program() {
			GL.DeleteProgram(ID);
		}
		
		public void Use() {
			GL.UseProgram(ID);
		}

		public int? GetUniform(string name) {
			int index = GL.GetUniformLocation(ID, name);
			if (index == -1) {
				Log.Warn("Uniform '{0}' cannot be found in shader {1}", name, ID);
			}
			return index;
		}

		public int? GetAttrib(string name) {
			int index = GL.GetAttribLocation(ID, name);
			if (index == -1) {
				Log.Warn("Attribute '{0}' cannot be found in shader {1}", name, ID);
			}
			return index;
		}
		
		public int ID { get; }
		public Shader Vert { get; }
		public Shader Frag { get; }
	}
	
	public int ID { get; }
	public ShaderType Type { get; }
}
