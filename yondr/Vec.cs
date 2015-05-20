using System;
using MiscUtil;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

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

public struct Vec4<T> where T: IConvertible {
	public T W { get; }
	public T X { get; }
	public T Y { get; }
	public T Z { get; }
	
	public Vec4() {
		W = Operator<T>.Zero;
		X = Operator<T>.Zero;
		Y = Operator<T>.Zero;
		Z = Operator<T>.Zero;
	}
	public Vec4(T w, T x, T y, T z) {
		W = w;
		X = x;
		Y = y;
		Z = z;
	}
}

public class VecNodeDeserializer: INodeDeserializer {
	public bool Deserialize(EventReader reader, Type expectedType,
	                        Func<EventReader, Type, object> nested,
	                        out object value) {
		if (expectedType.IsGenericType) {
			if (expectedType.GetGenericTypeDefinition() == typeof(Vec2<>)) {
				value = callVec(2, reader, expectedType, nested);
				return true;
			} else if (expectedType.GetGenericTypeDefinition() == typeof(Vec3<>)) {
				value = callVec(3, reader, expectedType, nested);
				return true;
			} else if (expectedType.GetGenericTypeDefinition() == typeof(Vec4<>)) {
				value = callVec(4, reader, expectedType, nested);
				return true;
			}
		}
		value = null;
		return false;
	}
	private object callVec(int numArgs, EventReader reader, Type expectedType,
	                       Func<EventReader, Type, object> nested) {
		Type t = expectedType.GetGenericArguments()[0];
		
		reader.Expect<SequenceStart>();
		var args = new object[numArgs];
		for (int i = 0; i < args.Length; i++) {
			args[i] = nested(reader, t);
		}
		object value = Activator.CreateInstance(expectedType, args);
		reader.Accept<SequenceEnd>();
		reader.Expect<SequenceEnd>();
		
		return value;
	}
}
