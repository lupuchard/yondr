using System;
using System.IO;
using System.Numerics;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Core.Events;

/// When parsing a non-scalar, it should accept
/// strings as other yaml resources to import.
public class ImportNodeDeserializer: INodeDeserializer {
	private readonly Deserializer deserializer;
	private readonly Res.Package package;
	public ImportNodeDeserializer(Deserializer d, Res.Package p) {
		deserializer = d;
		package = p;
	}

	public bool Deserialize(EventReader reader, Type expectedType,
	                        Func<EventReader, Type, object> nested,
	                        out object value) {

		if (expectedType.IsValueType || expectedType == typeof(string)) {
			value = null;
			return false;
		}
		var scalar = reader.Allow<Scalar>();
		if (scalar == null) {
			value = null;
			return false;
		}

		// If the expected type is an array of scalars, then a scalar will
		// be interpreted as an array of length 1 instead of a file name.
		if (expectedType.IsArray) {
			Type innerType = expectedType.GetElementType();
			if (innerType.IsValueType || innerType == typeof(string)) {
				var arr = Array.CreateInstance(innerType, 1);
				arr.SetValue(Convert.ChangeType(scalar.Value, innerType), 0);
				value = arr;
				return true;
			}
		}

		var resName = StringUtil.Simplify(Path.GetFileNameWithoutExtension(scalar.Value));
		var res = package.Resources[resName];
		if (res.Type != Res.Type.YAML) {
			throw new YamlException(scalar.Start, scalar.End,
				String.Format("Resource {0} is not yaml.", resName));
		} else if (res.Used) {
			throw new YamlException(scalar.Start, scalar.End,
				String.Format("Circular dependency? {0}", res.Path));
		}
		res.Used = true;
		var input = new StreamReader(res.Path);
		value = deserializer.Deserialize(input, expectedType);
		res.Used = false;
		return true;
	}
}

/// Parsing enums should not be case-sensitive.
public class EnumNodeDeserializer: INodeDeserializer {
	public bool Deserialize(EventReader reader, Type expectedType,
		Func<EventReader, Type, object> nested,
		out object value) {
		if (expectedType.IsEnum) {
			var scalar = reader.Allow<Scalar>();
			if (scalar != null) {
				value = Enum.Parse(expectedType, scalar.Value, true);
				return true;
			}
		}
		value = null;
		return false;
	}
}

/// Yaml deserializer for the vecs.
/// Deserializes them from an array.
public class VecNodeDeserializer: INodeDeserializer {
	public bool Deserialize(EventReader reader, Type expectedType,
	                        Func<EventReader, Type, object> nested,
	                        out object value) {
		if (expectedType == typeof(Vector2)) {
			value = callVec<Vector2, float>(2, reader, nested);
			return true;
		} else if (expectedType == typeof(Vector3)) {
			value = callVec<Vector3, float>(3, reader, nested);
			return true;
		} else if (expectedType == typeof(Vector4)) {
			value = callVec<Vector4, float>(4, reader, nested);
			return true;
		}
		value = null;
		return false;
	}
	private object callVec<V, T>(int numArgs, EventReader reader,
	                             Func<EventReader, Type, object> nested) {
		reader.Expect<SequenceStart>();
		var args = new object[numArgs];
		for (int i = 0; i < args.Length; i++) {
			args[i] = nested(reader, typeof(T));
		}
		object value = Activator.CreateInstance(typeof(V), args);
		reader.Accept<SequenceEnd>();
		reader.Expect<SequenceEnd>();

		return value;
	}
}
