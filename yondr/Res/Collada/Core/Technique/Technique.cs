using System;
using System.Xml;
using System.Xml.Serialization;
namespace Collada {
	[SerializableAttribute()]
	[XmlTypeAttribute(AnonymousType=true)]
	public class Technique {
		[XmlAttribute("profile")]
		public string profile;
		[XmlAttribute("xmlns")]
		public string xmlns;

		[XmlAnyElement]
		public XmlElement[] Data;
	}
}

