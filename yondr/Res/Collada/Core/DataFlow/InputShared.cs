using System;
using System.Xml;
using System.Xml.Serialization;
namespace Collada {
	[SerializableAttribute()]
	[XmlTypeAttribute(AnonymousType=true)]
	public partial class InputShared: InputUnshared {
		[XmlAttribute("offset")]
		public int Offset;
		
		[XmlAttribute("set")]
		public int Set;
	}
}

