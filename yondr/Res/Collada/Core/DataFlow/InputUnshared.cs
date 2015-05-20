using System;
using System.Xml;
using System.Xml.Serialization;
namespace Collada {
	[SerializableAttribute()]
	[XmlTypeAttribute(AnonymousType=true)]
	public class InputUnshared {
		[XmlAttribute("semantic")]
		[System.ComponentModel.DefaultValueAttribute(InputSemantic.NORMAL)]
		public InputSemantic Semantic;	

		[XmlAttribute("source")]
		public string source;
	}
}

