using System;
using System.Xml.Serialization;
namespace Collada {
	[SerializableAttribute()]
	[XmlTypeAttribute(Namespace="http://www.collada.org/2005/11/COLLADASchema" )]
	public enum InputSemantic {
		NONE,
		BINORMAL,
		COLOR,
		CONTINUITY,
		IMAGE,
		INPUT,
		IN_TANGENT,
		INTERPOLATION,
		INV_BIND_MATRIX,
		JOINT,
		LINEAR_STEPS,
		MORPH_TARGET,
		MORPH_WEIGHT,
		NORMAL,
		OUTPUT,
		OUT_TANGENT,
		POSITION,
		TANGENT,
		TEXBINORMAL,
		TEXCOORD,
		TEXTANGENT,
		UV,
		VERTEX,
		WEIGHT
	}
}

