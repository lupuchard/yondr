using System;
using NUnit.Framework;
using System.Numerics;

[TestFixture]
public class SpacialComponentTest {
	[Test]
	public void RotationTest() {
		var props = new PropertySystem(0);
		var space = new SpacialComponent();
		space.Init(props);
		var entity = new Entity(new Entity.Base(props), 0);
		space.Add(entity);

		Assert.AreEqual(space.GetDirection(0), new Vector3( 0, 0, 1));
		Assert.AreEqual(space.GetUp(0)       , new Vector3(-1, 0, 0));

		space.RotateY(0, (float)Math.PI / 2);
		var dir = space.GetDirection(0);
		Assert.AreEqual(1, dir.X, 0.000001);
		Assert.AreEqual(0, dir.Y, 0.000001);
		Assert.AreEqual(0, dir.Z, 0.000001);
		var up = space.GetUp(0);
		Assert.AreEqual(0, up.X, 0.000001);
		Assert.AreEqual(0, up.Y, 0.000001);
		Assert.AreEqual(1, up.Z, 0.000001);

		space.RotateZ(0, (float)Math.PI / 2 * 3);
		dir = space.GetDirection(0);
		Assert.AreEqual( 0, dir.X, 0.000001);
		Assert.AreEqual(-1, dir.Y, 0.000001);
		Assert.AreEqual( 0, dir.Z, 0.000001);
		up = space.GetUp(0);
		Assert.AreEqual(0, up.X, 0.000001);
		Assert.AreEqual(0, up.Y, 0.000001);
		Assert.AreEqual(1, up.Z, 0.000001);

		space.RotateX(0, -(float)Math.PI / 4);
		dir = space.GetDirection(0);
		Assert.AreEqual(0, dir.X, 0.000001);
		Assert.AreEqual(-Math.Sqrt(2) / 2, dir.Y, 0.000001);
		Assert.AreEqual( Math.Sqrt(2) / 2, dir.Z, 0.000001);
		up = space.GetUp(0);
		Assert.AreEqual(0, up.X, 0.000001);
		Assert.AreEqual(Math.Sqrt(2) / 2, up.Y, 0.000001);
		Assert.AreEqual(Math.Sqrt(2) / 2, up.Z, 0.000001);
	}
}
