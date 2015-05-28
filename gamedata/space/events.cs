using Yondr;
using System.Collections.Specialized;
using System.Numerics;

public static class Events {
	public static void Init(IContext context) {
		for (int x = -1; x <= 1; x++) {
			for (int y = -1; y <= 1; y++) {
				for (int z = -1; z <= 1; z++) {
					if (x != 0 || y != 0 || z != 0) {
						var yams = (Entity)context.CreateEntity("objects", "yambox");
						yams.Position = new Vector3(x, y, z);
					}
				}
			}
		}

		var player = (Entity)context.CreateEntity("mobs", "player");
		player.LookAt(new Vector3(1, 0, 0));
		player.SetAsCamera();
	}
}
