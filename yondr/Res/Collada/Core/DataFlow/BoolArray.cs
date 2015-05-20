using System;
using System.Xml;
using System.Xml.Serialization;
namespace Collada {
	[SerializableAttribute()]
	[XmlTypeAttribute(AnonymousType=true)]
	public class BoolArray: BoolArrayString {
		[XmlAttribute("id")]
		public string ID;
		
		[XmlAttribute("name")]
		public string Name;
		
		[XmlAttribute("count")]
		public int Count;
	}
}

