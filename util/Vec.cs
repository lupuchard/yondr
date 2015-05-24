using System;
using MiscUtil;

public struct Vec2<T> where T: IConvertible {
	public T X { get; }
	public T Y { get; }
	
	public Vec2() {
		X = Operator<T>.Zero;
		Y = Operator<T>.Zero;
	}
	public Vec2(T x, T y) {
		X = x;
		Y = y;
	}
	
	public T SqrLength() {
		return Operator<T>.Add(Operator<T>.Multiply(X, X), Operator<T>.Multiply(Y, Y));
	}
	public double Length() {
		return Math.Sqrt(Convert.ToDouble(SqrLength()));
	}
	public Vec2<T> Abs() {
		return new Vec2<T>(GenericMath.Abs(X), GenericMath.Abs(Y));
	}
	
	public static Vec2<T> operator +(Vec2<T> mhs) {
		return mhs;
	}
	public static Vec2<T> operator +(Vec2<T> lhs, Vec2<T> rhs) {
		return new Vec2<T>(Operator<T>.Add(lhs.X, rhs.X), Operator<T>.Add(lhs.Y, rhs.Y));
	}
	public static Vec2<T> operator +(Vec2<T> lhs, T rhs) {
		return new Vec2<T> (Operator<T>.Add(lhs.X, rhs), Operator<T>.Add(lhs.Y, rhs));
	}
	
	public static Vec2<T> operator -(Vec2<T> mhs) {
		return new Vec2<T>(Operator<T>.Negate(mhs.X), Operator<T>.Negate(mhs.Y));
	}
	public static Vec2<T> operator -(Vec2<T> lhs, Vec2<T> rhs) {
		return new Vec2<T>(Operator<T>.Subtract(lhs.X, rhs.X), Operator<T>.Subtract(lhs.Y, rhs.Y));
	}
	public static Vec2<T> operator -(Vec2<T> lhs, T rhs) {
		return new Vec2<T> (Operator<T>.Subtract(lhs.X, rhs), Operator<T>.Subtract(lhs.Y, rhs));
	}
	
	public static Vec2<T> operator *(Vec2<T> lhs, Vec2<T> rhs) {
		return new Vec2<T>(Operator<T>.Multiply(lhs.X, rhs.X), Operator<T>.Multiply(lhs.Y, rhs.Y));
	}
	public static Vec2<T> operator *(Vec2<T> lhs, T rhs) {
		return new Vec2<T> (Operator<T>.Multiply(lhs.X, rhs), Operator<T>.Multiply(lhs.Y, rhs));
	}
	
	public static Vec2<T> operator /(Vec2<T> lhs, Vec2<T> rhs) {
		return new Vec2<T>(Operator<T>.Divide(lhs.X, rhs.X), Operator<T>.Divide(lhs.Y, rhs.Y));
	}
	public static Vec2<T> operator /(Vec2<T> lhs, T rhs) {
		return new Vec2<T> (Operator<T>.Divide(lhs.X, rhs), Operator<T>.Divide(lhs.Y, rhs));
	}
}

public struct Vec3<T> where T: IConvertible {
	public T X { get; }
	public T Y { get; }
	public T Z { get; }
	
	public Vec3() {
		X = Operator<T>.Zero;
		Y = Operator<T>.Zero;
		Z = Operator<T>.Zero;
	}
	public Vec3(T x, T y, T z) {
		X = x;
		Y = y;
		Z = z;
	}
	
	public T SqrLength() {
		T tmp = Operator<T>.Add(Operator<T>.Multiply(X, X), Operator<T>.Multiply(Y, Y));
		return Operator<T>.Add(tmp, Operator<T>.Multiply(Z, Z));
	}
	public double Length() {
		return Math.Sqrt(Convert.ToDouble(SqrLength()));
	}
	public Vec3<T> Abs() {
		return new Vec3<T>(GenericMath.Abs(X), GenericMath.Abs(Y), GenericMath.Abs(Z));
	}
	
	public static Vec3<T> operator +(Vec3<T> mhs) {
		return mhs;
	}
	public static Vec3<T> operator +(Vec3<T> lhs, Vec3<T> rhs) {
		return new Vec3<T>(Operator<T>.Add(lhs.X, rhs.X),
		                   Operator<T>.Add(lhs.Y, rhs.Y),
		                   Operator<T>.Add(lhs.Z, rhs.Z));
	}
	public static Vec3<T> operator +(Vec3<T> lhs, T rhs) {
		return new Vec3<T> (Operator<T>.Add(lhs.X, rhs),
		                    Operator<T>.Add(lhs.Y, rhs),
							Operator<T>.Add(lhs.Z, rhs));
	}
	
	public static Vec3<T> operator -(Vec3<T> mhs) {
		return new Vec3<T>(Operator<T>.Negate(mhs.X),
		                   Operator<T>.Negate(mhs.Y),
						   Operator<T>.Negate(mhs.Y));
	}
	public static Vec3<T> operator -(Vec3<T> lhs, Vec3<T> rhs) {
		return new Vec3<T>(Operator<T>.Subtract(lhs.X, rhs.X),
		                   Operator<T>.Subtract(lhs.Y, rhs.Y),
						   Operator<T>.Subtract(lhs.Z, rhs.Z));
	}
	public static Vec3<T> operator -(Vec3<T> lhs, T rhs) {
		return new Vec3<T> (Operator<T>.Subtract(lhs.X, rhs),
		                    Operator<T>.Subtract(lhs.Y, rhs),
							Operator<T>.Subtract(lhs.Z, rhs));
	}
	
	public static Vec3<T> operator *(Vec3<T> lhs, Vec3<T> rhs) {
		return new Vec3<T>(Operator<T>.Multiply(lhs.X, rhs.X),
		                   Operator<T>.Multiply(lhs.Y, rhs.Y),
						   Operator<T>.Multiply(lhs.Z, rhs.Z));
	}
	public static Vec3<T> operator *(Vec3<T> lhs, T rhs) {
		return new Vec3<T> (Operator<T>.Multiply(lhs.X, rhs),
		                    Operator<T>.Multiply(lhs.Y, rhs),
							Operator<T>.Multiply(lhs.Z, rhs));
	}
	
	public static Vec3<T> operator /(Vec3<T> lhs, Vec3<T> rhs) {
		return new Vec3<T>(Operator<T>.Divide(lhs.X, rhs.X),
		                   Operator<T>.Divide(lhs.Y, rhs.Y),
						   Operator<T>.Divide(lhs.Z, rhs.Z));
	}
	public static Vec3<T> operator /(Vec3<T> lhs, T rhs) {
		return new Vec3<T> (Operator<T>.Divide(lhs.X, rhs),
		                    Operator<T>.Divide(lhs.Y, rhs),
							Operator<T>.Divide(lhs.Z, rhs));
	}
}
