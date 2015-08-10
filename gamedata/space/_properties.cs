using System;
namespace Yondr {
namespace Groups {
	public static class Self {
		public static Property<Entity> Character = new Property<Entity>(0, 0);
	}
	public static class Objects {
		public static Property<String> Mesh = new Property<String>(1, 0);
		public static Property<String> Texture = new Property<String>(1, 1);
	}
	public static class Mobs {
		public static Property<String> Name = new Property<String>(2, 0);
		public static Property<String> Mesh = new Property<String>(2, 1);
		public static Property<String> Texture = new Property<String>(2, 2);
	}
}
}
