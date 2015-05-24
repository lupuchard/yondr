using System;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

public class Display {
	public Display(Res.Manager resManager, World world,
	               int width, int height,
	               Action<Renderer> load, Action<double> update) {
		using (var game = new GameWindow(width, height, new GraphicsMode(), "Yondr")) {
			game.Load += (sender, e) => {
				GraphicsContext.CurrentContext.SwapInterval = 60;
				renderer = new Renderer(resManager, world);
				load(renderer);
			};
			game.Resize += (sender, e) => {
				GL.Viewport(0, 0, game.Width, game.Height);
				renderer.Resize(game.Width, game.Height);
			};
			game.UpdateFrame += (sender, e) => {
				if (game.Keyboard[Key.Escape]) {
					game.Exit();
				}
				update(game.UpdatePeriod);
			};
			game.RenderFrame += (sender, e) => renderer.Render();
			game.Run(60.0);
		}
	}
	
	private Renderer renderer;
}
