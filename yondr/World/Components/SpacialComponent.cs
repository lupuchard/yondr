using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

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
		ResetOrientation(entity.Index);
	}

	public void Remove(Entity entity) { }

	public void NormalizeOrientation(int idx) {
		float len = (float)Math.Sqrt(Qw[idx] * Qw[idx] +
		                             Qx[idx] * Qx[idx] +
		                             Qy[idx] * Qy[idx] +
		                             Qz[idx] * Qz[idx]);
		Qw[idx] /= len;
		Qx[idx] /= len;
		Qy[idx] /= len;
		Qz[idx] /= len;
	}

	// The identity orientation is facing up (positive z-axis)
	// with the "up" along the negative x-axis.
	public void ResetOrientation(int idx) {
		Qw[idx] = 1;
		Qx[idx] = 0;
		Qy[idx] = 0;
		Qz[idx] = 0;
	}

	public void RotateX(int idx, float radians) {
		float qx = (float)Math.Sin(radians / 2);
		float qw = (float)Math.Cos(radians / 2);
		float newX = qw * Qx[idx] + Qw[idx] * qx;
		float newY = qw * Qy[idx] - qx * Qz[idx];
		float newZ = qw * Qz[idx] + qx * Qy[idx];
		Qw[idx] = qw * Qw[idx] - qx * Qx[idx];
		Qx[idx] = newX;
		Qy[idx] = newY;
		Qz[idx] = newZ;
	}

	public void RotateY(int idx, float radians) {
		float qy = (float)Math.Sin(radians / 2);
		float qw = (float)Math.Cos(radians / 2);
		float newX = qw * Qx[idx] + qy * Qz[idx];
		float newY = qw * Qy[idx] + Qw[idx] * qy;
		float newZ = qw * Qz[idx] - qy * Qx[idx];
		Qw[idx] = qw * Qw[idx] - qy * Qy[idx];
		Qx[idx] = newX;
		Qy[idx] = newY;
		Qz[idx] = newZ;
	}

	public void RotateZ(int idx, float radians) {
		float qz = (float)Math.Sin(radians / 2);
		float qw = (float)Math.Cos(radians / 2);
		float newX = qw * Qx[idx] - qz * Qy[idx];
		float newY = qw * Qy[idx] + qz * Qx[idx];
		float newZ = qw * Qz[idx] + Qw[idx] * qz;
		Qw[idx] = qw * Qw[idx] - qz * Qz[idx];
		Qx[idx] = newX;
		Qy[idx] = newY;
		Qz[idx] = newZ;
	}

	public Vector3 GetDirection(int idx) {
		return new Vector3(
			(Qx[idx] * Qz[idx] + Qw[idx] * Qy[idx]) * 2,
			(Qy[idx] * Qz[idx] - Qw[idx] * Qx[idx]) * 2,
			1 - (Qx[idx] * Qx[idx] + Qy[idx] * Qy[idx]) * 2
		);
	}

	public Vector3 GetUp(int idx) {
		return new Vector3(
			 (Qy[idx] * Qy[idx] + Qz[idx] * Qz[idx]) * 2 - 1,
			-(Qx[idx] * Qy[idx] + Qw[idx] * Qz[idx]) * 2,
			 (Qw[idx] * Qy[idx] - Qx[idx] * Qz[idx]) * 2
		);
	}
	
	private PropertySystem props;
	
	// position vector
	// Z is the up axis.
	public readonly List<float> X = new List<float>();
	public readonly List<float> Y = new List<float>();
	public readonly List<float> Z = new List<float>();
	
	// orientation quaternion
	// Rotates from forward = ( 0, 0, 1)
	//               and up = (-1, 0, 0)
	public readonly List<float> Qw = new List<float>();
	public readonly List<float> Qx = new List<float>();
	public readonly List<float> Qy = new List<float>();
	public readonly List<float> Qz = new List<float>();
}
