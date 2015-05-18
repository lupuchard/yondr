
public struct Val {
	
	public enum Ty { String, Int, Float, Bool };
	
	private readonly object val;

	public Val(string str) { val = str; }
	public Val(int num)    { val = num; }
	public Val(double num) { val = num; }
	public Val(bool b)     { val = b; }

	public string  AsString() { return val as string; }
	public int?    AsInt()    { return val as int?; }
	public double? AsFloat()  { return val as double?; }
	public bool?   AsBool()   { return val as bool?; }

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
			} else {
				return Ty.Bool;
			}
		}
	}
}
