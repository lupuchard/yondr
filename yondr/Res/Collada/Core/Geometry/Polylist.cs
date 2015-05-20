using System;
using System.Xml;
using System.Xml.Serialization;
namespace Collada {
	[SerializableAttribute()]
	[XmlTypeAttribute(AnonymousType=true)]
	public class Polylist: GeometryCommonFields {
		[XmlElement(ElementName = "vcount")]
		public IntArrayString VCount;					
	}
}
