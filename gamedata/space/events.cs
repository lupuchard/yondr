using Yondr;
using System.Collections.Specialized;
using System.Numerics;

public static class Events {
	
	static Entity player;

	public static void Init(IContext context) {
		for (int x = -1; x <= 1; x++) {
			for (int y = -1; y <= 1; y++) {
				for (int z = -1; z <= 1; z++) {
					if (x != 0 || y != 0 || z != 0) {
						var yams = (Entity)context.CreateEntity("objects", "yambox");
						yams.Position = new Vector3(x * 3, y * 3, z * 3);
					}
				}
			}
		}

		player = (Entity)context.CreateEntity("mobs", "player");
		player.LookAt(new Vector3(1, 0, 0));
		player.SetAsCamera();
	}

	public static void Update(IContext context, float diff) {
		if (context.Control("forward")) {
			player.Move(player.Direction * diff);
		} else if (context.Control("backward")) {
			player.Move(-player.Direction * diff);
		}
		if (context.Control("strafe left")) {
			var amount = player.Direction * diff;
			player.Move(new Vector3(-amount.Y, amount.X, amount.Z));
		} else if (context.Control("strafe right")) {
			var amount = player.Direction * diff;
			player.Move(new Vector3 (amount.Y, -amount.X, amount.Z));
		}
		if (context.Control("turn left")) {
			player.RotateZ(diff);
		} else if (context.Control("turn right")) {
			player.RotateZ(-diff);
		}
	}
}
