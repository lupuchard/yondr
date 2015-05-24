using System.Collections.Generic;

// TODO: unit tests
public class Entity {
	
	public class Base {
		public Base(PropertySystem props) {
			this.PropertySystem  = props;
			Update();
		}

		public void Update() {
			while (values.Count < PropertySystem.Count) {
				values.Add(PropertySystem.At((ushort)values.Count).Value);
			}
		}

		public Val this[ushort i] {
			get { return values[i];  }
			set { values[i] = value; }
		}

		public PropertySystem PropertySystem { get; }
		private readonly List<Val> values = new List<Val>();
	}
	
	public Entity(Base bas, ushort idx) {
		bass  = bas;
		Index = idx;
	}
	
	public ushort Index { get; set; }
	
	private Base bass;
	public PropertySystem PropertySystem { get { return bass.PropertySystem; } }
	
	private readonly Dictionary<ushort, Val> values = new Dictionary<ushort, Val>();
	public Val this[ushort i] {
		get {
			Val val;
			return values.TryGetValue(i, out val) ? val : bass[i];
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
			return prop != null ? (Val?)this[prop] : null;
		}
		set {
			Property prop = PropertySystem.WithName(name);
			if (prop != null && value != null) {
				this[prop] = (Val)value;
			}
		}
	}
}
