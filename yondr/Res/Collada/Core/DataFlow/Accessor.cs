using System;
using System.Xml;
using System.Xml.Serialization;
namespace Collada {
	[SerializableAttribute()]
	[XmlTypeAttribute(AnonymousType=true)]
	public partial class Accessor {
		[XmlAttribute("count")]
		public uint Count;

		[XmlAttribute("offset")]
		public uint Offset;
		
		[XmlAttribute("source")]
		public string Source;
		
		[XmlAttribute("stride")]
		public uint Stride;
	}
}
