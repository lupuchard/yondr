using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

public class Renderer {
	public Renderer() {
		
	}
	
	public void Render() {
		
	}
	
	private class Objects {
		List<Entity>     owners       = new List<Entity>();
		List<Mesh>       meshes       = new List<Mesh>();
		// List<Texture>    textures     = new List<Texture>();
		List<Vector3>    positions    = new List<Vector3>();
		List<Quaternion> orientations = new List<Quaternion>();
	}
	
	private Objects objects;
}
