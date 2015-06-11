using System;
using System.Threading;
using OpenTK;
using OpenTK.Input;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

public class Game {

	public Game(Res.Manager res, World world, uint width, uint height) {
		Toolkit.Init();

		window = new NativeWindow(
			(int)width, (int)height, "Yondr",
			GameWindowFlags.Default,
			GraphicsMode.Default,
			DisplayDevice.Default
		);

		context = new GraphicsContext(GraphicsMode.Default, window.WindowInfo);
		context.MakeCurrent(window.WindowInfo);
		context.LoadAll();

		// Setup event handlers
		window.Resize  += OnResized;
		window.KeyDown += OnKeyPressed;
		window.Closing += (sender, e) => done = true;

		this.Renderer = new TestRenderer(res, world);

		this.Controls = world.Controls;
	}

	public void Run(Action<float> updateFunc) {
		window.Visible = true;
		done = false;

		/*GL.Enable(EnableCap.DepthTest);
		GL.DepthMask(true);
		GL.ClearDepth(1);
		GL.Enable(EnableCap.Texture2D);*/

		//GL.Viewport(0, 0, window.Width, window.Height);

		var prev = DateTime.Now;
		while (window.Exists && !done) {
			var now = DateTime.Now;
			double diff = (now - prev).TotalSeconds;
			if (diff < 1.0 / Net.LogicalFPS) {
				Thread.Sleep((prev + new TimeSpan(0, 0, 0, 0, 1000 / Net.LogicalFPS)) - now);
				diff = (DateTime.Now - prev).TotalSeconds;
			}
			updateFunc((float)diff);
			Renderer.Render();
			prev = now;

			context.SwapBuffers();
			window.ProcessEvents();
		}
	}

	private void OnKeyPressed(object sender, KeyboardKeyEventArgs e) {
		if (e.Key == Key.Escape) {
			done = true;
			window.Close();
		}
	}

	private void OnResized(object sender, EventArgs e) {
		GL.Viewport(0, 0, window.Width, window.Height);
	}

	public IControls Controls { get; }
	public IRenderer Renderer { get; }
	private readonly NativeWindow window;
	private readonly GraphicsContext context;
	private bool done = false;
}
