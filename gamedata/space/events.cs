using Yondr;

public static class Events {
	public static void Init(IContext context) {
		var yams = (Entity)context.CreateEntity("objects", "yambox");
		yams.Position = new Vec3<float>(1, 1, 1);

		var player = (Entity)context.CreateEntity("mobs", "player");
		player.LookAt(yams.Position);
		player.SetAsCamera();
	}
}
