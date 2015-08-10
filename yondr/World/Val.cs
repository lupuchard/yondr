using System;

/// A "variant" representing a value stored in an entity.
public struct Val {
	
	public enum Ty { String, Int, Float, Bool, Entity, Unknown };
	
	public readonly object val;

	public Val(object v) { val = v; }
	public Val(Ty type) {
		switch (type) {
			case Ty.String: val = "";    break;
			case Ty.Int:    val = 0;     break;
			case Ty.Float:  val = 0.0f;  break;
			case Ty.Bool:   val = false; break;
			case Ty.Entity: val = new Yondr.Entity(0, 0); break;
			default:        val = null;  break;
		}
	}

	public static Val? FromString(Ty type, string str) {
		try {
			switch (type) {
				case Val.Ty.Bool:   return new Val(Convert.ToBoolean(str));
				case Val.Ty.Float:  return new Val(Convert.ToDouble(str));
				case Val.Ty.Int:    return new Val(Convert.ToInt32(str));
				case Val.Ty.String: return new Val(str);
				default: return null;				
			}
		} catch (FormatException) {
			return null;
		}
	}

	public bool Is<T>() { return val is T; }
	public T As<T>() { return (T)val; }

	public bool IsSameVal(Val other) {
		return val.Equals(other.val);
	}
	
	public Ty Type {
		get {
			var type = val.GetType();
			if (type == typeof(string)) {
				return Ty.String;
			} else if (type == typeof(int)) {
				return Ty.Int;
			} else if (type == typeof(double)) {
				return Ty.Float;
			} else if (type == typeof(bool)) {
				return Ty.Bool;
			} else if (type == typeof(Yondr.Entity)) {
				return Ty.Entity;
			} else {
				return Ty.Unknown;
			}
		}
	}
}
