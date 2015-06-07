using System;
using OpenTK.Graphics.OpenGL;

public static class Util {
	public static void CheckGL(string wher) {
		ErrorCode err = GL.GetError();
		while (err != ErrorCode.NoError) {
			Log.Error("GL Error in {0}: {1}", wher, err);
			err = GL.GetError();
		}
	}
}
