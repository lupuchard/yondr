using NUnit.Framework;

[TestFixture]
public class MeshTest {
	[Test]
	public void FromColladaTest() {
		Mesh mesh = Mesh.FromCollada("../../mesh.dae");

		Assert.NotNull(mesh);
		Assert.AreEqual(mesh.Geometries.Count, 1);
		
		Mesh.Geometry geom = mesh.Geometries[0];

		Assert.AreEqual(false, geom.Holes);
		Assert.AreEqual(Mesh.Primitive.TRIANGLES, geom.Type);
		Assert.AreEqual(24, geom.Vertices.Arr.Length);
		Assert.AreEqual(72, geom.Indices.Length);
		Assert.AreEqual(1, geom.Indices[2]);
	}
	
}
