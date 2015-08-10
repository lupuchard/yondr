using Yondr;
using Yondr.Groups;
using System.Numerics;

public static class Events {

	//#if !SERVER
	//static Entity self;
	//static Entity character;
	//#endif

	private static IContext context;

	public static void Init(Entity? player, IContext c) {
		context = c;

		for (int x = -1; x <= 1; x++) {
			for (int y = -1; y <= 1; y++) {
				for (int z = -1; z <= 1; z++) {
					if (x != 0 || y != 0 || z != 0) {
						var yams = (Entity)context.CreateEntity("objects", "yambox");
						yams.Position = new Vector3(x * 3, y * 3, z * 3);
						if (x == 1 && y == 0 && z == 0) yams.Texture = "space:yamsx";
						if (x == 0 && y == 1 && z == 0) yams.Texture = "space:yamsy";
						if (x == 0 && y == 0 && z == 1) yams.Texture = "space:yamsz";
					}
				}
			}
		}

		if (player != null) {
			var character = (Entity)context.CreateEntity("mobs", "player");
			character.LookAt(new Vector3(1, 0, 0));
			character.SetAsCamera();
			player.Value.Set(Self.Character, character);
		}

		/*#if !SERVER
		self = context.GetEntity("self");
		character = self.Get(Groups.Client.Character);
		player.LookAt(new Vector3(1, 0, 0));
		player.SetAsCamera();
		#endif*/
	}

	/*public static void PlayerJoin(IContext context, Entity player) {
		character = (Entity)context.CreateEntity("mobs", "player");
		player.Assign(Groups.Self.Character, character);
	}*/

	public static void While_Forward(Entity player, float diff) {
		Entity character = player.Get(Self.Character);
		character.Move(character.Direction * diff);
	}
	public static void While_Backward(Entity player, float diff) {
		Entity character = player.Get(Self.Character);
		var dir = character.Position;
		context.Out.WriteLine("pos ({0}, {1}, {2})", dir.X, dir.Y, dir.Z);
		character.Move(-character.Direction * diff);
	}
	public static void While_StrafeLeft(Entity player, float diff) {
		Entity character = player.Get(Self.Character);
		var amount = character.Direction * diff;
		character.Move(new Vector3(-amount.Y, amount.X, amount.Z));
	}
	public static void While_StrafeRight(Entity player, float diff) {
		Entity character = player.Get(Self.Character);
		var amount = character.Direction * diff;
		character.Move(new Vector3 (amount.Y, -amount.X, amount.Z));
	}
	public static void While_TurnLeft(Entity player, float diff) {
		Entity character = player.Get(Self.Character);
		character.RotateZ(diff);
	}
	public static void While_TurnRight(Entity player, float diff) {
		Entity character = player.Get(Self.Character);
		character.RotateZ(-diff);
	}
}
