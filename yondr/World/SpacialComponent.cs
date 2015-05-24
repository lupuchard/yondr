using System;
using System.Collections.Generic;
using System.Linq;

public class SpacialComponent: IComponent {
	public void Init(PropertySystem propertySystem) {
		props = propertySystem;
	}
	
	public void Add(Entity entity) {
		if (entity.PropertySystem != props) throw new ArgumentException();
		if (entity.Index >= X.Count) {
			X.AddRange(Enumerable.Repeat(0.0f, entity.Index - X.Count + 1));
			Y.AddRange(Enumerable.Repeat(0.0f, entity.Index - Y.Count + 1));
			Z.AddRange(Enumerable.Repeat(0.0f, entity.Index - Z.Count + 1));
			A.AddRange(Enumerable.Repeat(0.0f, entity.Index - A.Count + 1));
			B.AddRange(Enumerable.Repeat(0.0f, entity.Index - B.Count + 1));
			C.AddRange(Enumerable.Repeat(0.0f, entity.Index - C.Count + 1));
			D.AddRange(Enumerable.Repeat(0.0f, entity.Index - D.Count + 1));
		}
		X[entity.Index] = 0;
		Y[entity.Index] = 0;
		Z[entity.Index] = 0;
		A[entity.Index] = 1;
		B[entity.Index] = 0;
		C[entity.Index] = 0;
		D[entity.Index] = 0;
	}

	public void Remove(Entity entity) { }
	
	private PropertySystem props;
	
	// position vector
	public readonly List<float> X = new List<float>();
	public readonly List<float> Y = new List<float>();
	public readonly List<float> Z = new List<float>();
	
	// orientation quaternion
	public readonly List<float> A = new List<float>();
	public readonly List<float> B = new List<float>();
	public readonly List<float> C = new List<float>();
	public readonly List<float> D = new List<float>();
}
