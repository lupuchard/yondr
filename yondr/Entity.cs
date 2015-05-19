using System.Collections.Generic;

// TODO: unit tests
public class Entity {
	
	public class Base {
		public Base(PropertySystem props) {
			this.PropertySystem  = props;
			values = new List<Val>();
			Update();
		}
		public void Update() {
			while (values.Count < PropertySystem.Count) {
				values.Add(PropertySystem.At((ushort)values.Count).Value);
			}
		}

		public PropertySystem PropertySystem { get; }
		
		private List<Val> values;
		
		public Val this[ushort i] {
			get { return values[i];  }
			set { values[i] = value; }
		}
	}
	
	public Entity(Base bas, ushort idx) {
		bass  = bas;
		Index = idx;
	}
	
	public ushort Index { get; set; }
	
	private Base bass;
	public PropertySystem PropertySystem { get { return bass.PropertySystem; } }
	
	private Dictionary<ushort, Val> values;
	public Val this[ushort i] {
		get {
			Val val;
			if (values.TryGetValue(i, out val)) return val;
			return bass[i];
		}
		set { values.Add(i, value); }
	}
	public Val this[Property p] {
		get { return this[p.Index]; }
		set { this[p.Index] = value; }
	}
	public Val? this[string name] {
		get {
			Property prop = PropertySystem.WithName(name);
			if (prop == null) return null;
			return this[prop];
		}
		set {
			Property prop = PropertySystem.WithName(name);
			if (prop != null && value != null) {
				this[prop] = (Val)value;
			}
		}
	}
}
