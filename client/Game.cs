using System;
using System.Threading;
using OpenTK;
using OpenTK.Input;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

public class Game {

	public Game(Res.Manager res, World world, uint width, uint height) {
		Toolkit.Init();

		Window = new NativeWindow(
			(int)width, (int)height, "Yondr",
			GameWindowFlags.Default,
			GraphicsMode.Default,
			DisplayDevice.Default
		);

		context = new GraphicsContext(GraphicsMode.Default, Window.WindowInfo);
		context.MakeCurrent(Window.WindowInfo);
		context.LoadAll();

		// Setup event handlers
		Window.Resize  += OnResized;
		Window.KeyDown += OnKeyPressed;
		Window.Closing += (sender, e) => done = true;

		this.Renderer = new Renderer(res, world);

		this.Controls = world.Controls;
	}

	public void Run(Action<float> updateFunc) {
		Window.Visible = true;
		done = false;

		/*GL.Enable(EnableCap.DepthTest);
		GL.DepthMask(true);
		GL.ClearDepth(1);
		GL.Enable(EnableCap.Texture2D);*/

		//GL.Viewport(0, 0, window.Width, window.Height);

		var prev = DateTime.Now;
		while (Window.Exists && !done) {
			var now = DateTime.Now;
			double diff = (now - prev).TotalSeconds;
			if (diff < 1.0 / Net.Consts.LogicalFPS) {
				Thread.Sleep((prev + new TimeSpan(0, 0, 0, 0, 1000 / Net.Consts.LogicalFPS)) - now);
				diff = (DateTime.Now - prev).TotalSeconds;
			}
			Controls.Update((float)diff);
			updateFunc((float)diff);
			Renderer.Render();
			prev = now;

			context.SwapBuffers();
			Window.ProcessEvents();
		}
	}

	private void OnKeyPressed(object sender, KeyboardKeyEventArgs e) {
		if (e.Key == Key.Escape) {
			done = true;
			Window.Close();
		}
	}

	private void OnResized(object sender, EventArgs e) {
		GL.Viewport(0, 0, Window.Width, Window.Height);
	}

	public IControls Controls { get; }
	public IRenderer Renderer { get; }
	public NativeWindow Window { get; }
	private readonly GraphicsContext context;
	private bool done = false;
}
