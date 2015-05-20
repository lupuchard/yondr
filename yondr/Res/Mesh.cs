using System;
using System.Collections.Generic;
using Collada;

[Serializable]
public class Mesh {
	
	// Converts a Collada mesh into our own "superior" mesh format.
	public static Mesh FromCollada(string path) {
		try {
			return fromCollada(path);
		} catch (Exception e) {
			if (e is NullReferenceException    ||
			    e is InvalidCastException      ||
			    e is KeyNotFoundException      ||
				e is InvalidOperationException ||
			    e is ArgumentException) {
				Log.Error("Could not load mesh {0}", path);
				Log.Debug("Because: {0}", e.Message);
			} else throw;
		}
		return null;
	}
	private static Mesh fromCollada(string path) {
		Mesh mesh = new Mesh();
		var collada = ColladaFile.Load(path);
		var cGeoms = collada.LibraryGeometries;
		var sources = new Dictionary<Tuple<string, InputSemantic>, object>();
		foreach (var cGeom in cGeoms.Geometry) {
			if (cGeom.Mesh == null) {
				Log.Warn("Only mesh geometries are yet supported. ({0})", path);
				continue;
			}
			var cMesh = cGeom.Mesh;
			foreach (var cSource in cMesh.Source) {
				sources.Add(Tuple.Create(cSource.ID, InputSemantic.NONE), readSource(cSource));
			}
			
			var cVerts = cMesh.Vertices;
			foreach (var cInput in cVerts.Input) {
				sources.Add(
					Tuple.Create(cVerts.ID, (InputSemantic)cInput.Semantic),
					sources[Tuple.Create(cInput.source.Substring(1), InputSemantic.NONE)]
				);
			}
			
			foreach (var cPolylist in cMesh.Polylist) {
				Source<float> vertices, normals;
				int verticesOffset, normalsOffset;
				primitiveInputs(
					cPolylist.Input, sources,
					out vertices, out verticesOffset, out normals, out normalsOffset
				);
				
				// convert polys to triangles
				List<int> indices = new List<int>();
				List<int> offsets = new List<int>();
				int i = 0;
				var cp      = cPolylist.P.Value();
				while (i < cp.Length) {
					offsets.Add(i);
					foreach (int v in cPolylist.VCount.Value()) {
						for (int j = 2; j < v; j++) {
							indices.Add(cp[i]);
							indices.Add(cp[i + j - 1]);
							indices.Add(cp[i + j]);
						}
						i += v;
					}
				}
				
				mesh.Geometries.Add(new Geometry(
					Primitive.TRIANGLES, false,
					vertices, offsets[verticesOffset],
					normals,  offsets[normalsOffset],
					indices.ToArray()
				));
			}
			if (cMesh.Triangles != null)
				foreach (var cTriangles in cMesh.Triangles)
					mesh.Geometries.Add(createGeom(cTriangles, Primitive.TRIANGLES, sources));
			if (cMesh.Trifans != null)
				foreach (var cTrifans in cMesh.Trifans)
					mesh.Geometries.Add(createGeom(cTrifans, Primitive.TRIFANS, sources));
			if (cMesh.Tristrips != null)
				foreach (var cTristrips in cMesh.Tristrips)
					mesh.Geometries.Add(createGeom(cTristrips, Primitive.TRISTRIPS, sources));
		}
		return mesh;
	}
	
	private static object readSource(Source cSource) {
		if (cSource.BoolArray != null) {
			return readSource2(cSource.BoolArray.Value(), cSource);
		} else if (cSource.FloatArray != null) {
			return readSource2(cSource.FloatArray.Value(), cSource);
		} else if (cSource.IntArray != null) {
			return readSource2(cSource.IntArray.Value(), cSource);
		} else throw new NotImplementedException();
	}
	private static object readSource2<T>(T[] array, Source cSource)
	where T: IConvertible {
		var cTech = cSource.Technique_Common;
		var source = new Source<T>();
		source.Arr = array;
		source.Stride = cTech == null ? 1 : (int)cTech.Accessor.Stride;
		return source;
	}
	
	private static Geometry createGeom(GeometryCommonFields cPrim, Primitive type,
	                                   Dictionary<Tuple<string, InputSemantic>, object> sources) {
		Source<float> vertices, normals;
		int verticesOffset, normalsOffset;
		primitiveInputs(
			cPrim.Input, sources,
			out vertices, out verticesOffset, out normals, out normalsOffset
		);
		return new Geometry(
			type, true,
			vertices, verticesOffset * cPrim.Count * 3,
			normals,   normalsOffset * cPrim.Count * 3,
			cPrim.P.Value()
		);
	}
	private static void primitiveInputs(InputShared[] inputs, 
	                                    Dictionary<Tuple<string, InputSemantic>, object> sources,
	                                    out Source<float> vertices, out int verticesOffset,
	                                    out Source<float> normals,  out int normalsOffset) {
		vertices = null;
		verticesOffset = 0;
		normals  = null;
		normalsOffset  = 0;
		foreach (var cIn in inputs) {
			switch (cIn.Semantic) {
				case InputSemantic.VERTEX: {
					vertices = getSource<float>(sources, cIn, InputSemantic.POSITION);
					verticesOffset = cIn.Offset;
				} break;
				case InputSemantic.NORMAL: {
					normals = getSource<float>(sources, cIn);
					normalsOffset  = cIn.Offset;
				} break;
			}
		}
	}
	private static Source<T> getSource<T>(Dictionary<Tuple<string, InputSemantic>, object> sources,
	                                      InputShared input,
	                                      InputSemantic semantic = InputSemantic.NONE) {
		return (Source<T>)sources[Tuple.Create(input.source.Substring(1), semantic)];
	}
	
	public enum Primitive { TRIANGLES, TRIFANS, TRISTRIPS };

	[Serializable]
	public class Source<T> {
		public T[] Arr         { get; set; }
		public int Stride      { get; set; }
		InputSemantic semantic { get; set; }
	}

	[Serializable]
	public class Geometry {
		public Geometry() { }
		public Geometry(Primitive type, bool holes,
		                Source<float> vertices, int verticesOffset,
						Source<float> normals, int normalsOffset,
						int[] indices) {
			this.Type      = type;
			Holes          = holes;
			Vertices       = vertices;
			VerticesOffset = verticesOffset;
			Normals        = normals;
			NormalsOffset  = normalsOffset;
			Indices        = indices;
		}
		
		public Primitive Type { get; set; }
		public bool Holes     { get; set; } = true;
		
		public Source<float> Vertices { get; set; }
		public int VerticesOffset     { get; set; }
		public Source<float> Normals  { get; set; }
		public int NormalsOffset      { get; set; }
		public int[] Indices          { get; set; }
	}
	public List<Geometry> Geometries { get; set; } = new List<Geometry>();
}
