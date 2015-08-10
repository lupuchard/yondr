using System.Linq;
using System.IO;
using NUnit.Framework;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization.NodeDeserializers;
using System.Numerics;

[TestFixture]
public class YamlTest {

	private enum TestEnum { Spheres, Balls }
	private class TestObj {
		public Vector2  Vec2 { get; set; } = new Vector2(0);
		public Vector3  Vec3 { get; set; } = new Vector3(0);
		public TestEnum Enum { get; set; } = TestEnum.Spheres;
	}

	[Test]
	public void UsageTest() {
		var deserializer = new Deserializer(namingConvention: new HyphenatedNamingConvention());

		// insert EnumNodeSerializer before the ScalarNodeDeserializer
		int scalarIdx = deserializer.NodeDeserializers.Select((d, i) => new {D=d, I=i}).
			First(d => d.D is ScalarNodeDeserializer).I;
		deserializer.NodeDeserializers.Insert(scalarIdx, new Yaml.EnumNodeDeserializer());

		// insert VecNodeDeserializer before ObjectNodeDeserializer
		int objIdx = deserializer.NodeDeserializers.Select((d, i) => new {D=d, I=i}).
			First(d => d.D is ObjectNodeDeserializer).I;
		deserializer.NodeDeserializers.Insert(objIdx, new Yaml.VecNodeDeserializer());

		var obj = deserializer.Deserialize<TestObj>(new StreamReader("../gamedata/test.yaml"));
		Assert.AreEqual(obj.Vec2, new Vector2(3, 4));
		Assert.AreEqual(obj.Vec3, new Vector3(4, 5, 6));
		Assert.AreEqual(obj.Enum, TestEnum.Balls);
	}
}
