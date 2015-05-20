using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

public class Display {
	public Display(Vec2<int> windowSize) {
		var window = new GameWindow(windowSize.X, windowSize.Y, new GraphicsMode(), "Yondr");
		GraphicsContext.CurrentContext.SwapInterval = 60;
	}
	public void Render() {
		GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
		
		
		
		GraphicsContext.CurrentContext.SwapBuffers();
	}
}
