using System;
using System.IO;
using System.Collections.Generic;
using System.Numerics;
using Assimp;
using Assimp.Configs;

[Serializable]
public class Mesh {
	public Mesh() { }
	public Mesh(string path) {
		var importer = new AssimpContext();
		var config = new NormalSmoothingAngleConfig(66.0f);
		importer.SetConfig(config);
		importer.SetConfig(new RemoveComponentConfig(
			ExcludeComponent.Animations | ExcludeComponent.Boneweights |
			ExcludeComponent.Cameras | ExcludeComponent.Colors |
			ExcludeComponent.Lights | ExcludeComponent.Materials
		));
		Scene scene = importer.ImportFile(
			path,
			PostProcessSteps.RemoveComponent | // Optimization. Excluded components listed above.
			PostProcessSteps.PreTransformVertices | // Because there is no scenegraph.
			PostProcessSteps.TransformUVCoords | // Per-texture UV transforms not supported either.
			PostProcessSteps.OptimizeMeshes |
			PostProcessPreset.TargetRealTimeQuality
		);

		if (!scene.HasMeshes) return;
		foreach (var sceneMesh in scene.Meshes) {
			if (!sceneMesh.HasVertices)         continue;
			if (!sceneMesh.HasTextureCoords(0)) continue;
			if (!sceneMesh.HasFaces)            continue;

			SubMesh subMesh = new SubMesh();

			subMesh.Vertices = new float[sceneMesh.Vertices.Count * 3];
			for (int i = 0; i < sceneMesh.Vertices.Count; i++) {
				subMesh.Vertices[i * 3 + 0] = sceneMesh.Vertices[i].X;
				subMesh.Vertices[i * 3 + 1] = sceneMesh.Vertices[i].Y;
				subMesh.Vertices[i * 3 + 2] = sceneMesh.Vertices[i].Z;
			}

			var texcoords = sceneMesh.TextureCoordinateChannels[0];
			subMesh.Texcoords = new float[texcoords.Count * 2];
			for (int i = 0; i < texcoords.Count; i++) {
				subMesh.Texcoords[i * 2 + 0] = texcoords[i].X;
				subMesh.Texcoords[i * 2 + 1] = texcoords[i].Y;
			}

			subMesh.Indices = new int[sceneMesh.Faces.Count * 3];
			for (int i = 0; i < sceneMesh.Faces.Count; i++) {
				subMesh.Indices[i * 3 + 0] = sceneMesh.Faces[i].Indices[0];
				subMesh.Indices[i * 3 + 1] = sceneMesh.Faces[i].Indices[1];
				subMesh.Indices[i * 3 + 2] = sceneMesh.Faces[i].Indices[2];
			}

			SubMeshes.Add(subMesh);
		}
	}

	[Serializable]
	public class SubMesh {
		public SubMesh() { }
		public float[] Vertices  { get; set; }
		public float[] Texcoords { get; set; }
		public int[] Indices { get; set; }
	}
	public List<SubMesh> SubMeshes { get; set; } = new List<SubMesh>();
}
