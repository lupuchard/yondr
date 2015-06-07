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
			X.AddRange( Enumerable.Repeat(0.0f, entity.Index -  X.Count + 1));
			Y.AddRange( Enumerable.Repeat(0.0f, entity.Index -  Y.Count + 1));
			Z.AddRange( Enumerable.Repeat(0.0f, entity.Index -  Z.Count + 1));
			Qw.AddRange(Enumerable.Repeat(0.0f, entity.Index - Qw.Count + 1));
			Qx.AddRange(Enumerable.Repeat(0.0f, entity.Index - Qx.Count + 1));
			Qy.AddRange(Enumerable.Repeat(0.0f, entity.Index - Qy.Count + 1));
			Qz.AddRange(Enumerable.Repeat(0.0f, entity.Index - Qz.Count + 1));
		}
		X[entity.Index] = 0;
		Y[entity.Index] = 0;
		Z[entity.Index] = 0;
		Qw[entity.Index] = 1;
		Qx[entity.Index] = 0;
		Qy[entity.Index] = 0;
		Qz[entity.Index] = 0;
	}

	public void Remove(Entity entity) { }
	
	private PropertySystem props;
	
	// position vector
	public readonly List<float> X = new List<float>();
	public readonly List<float> Y = new List<float>();
	public readonly List<float> Z = new List<float>();
	
	// orientation quaternion
	public readonly List<float> Qw = new List<float>();
	public readonly List<float> Qx = new List<float>();
	public readonly List<float> Qy = new List<float>();
	public readonly List<float> Qz = new List<float>();
}
