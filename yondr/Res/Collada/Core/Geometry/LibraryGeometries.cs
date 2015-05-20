using System;
using System.Xml;
using System.Xml.Serialization;
namespace Collada {
	[SerializableAttribute()]
	[XmlTypeAttribute(AnonymousType=true)]
	public class LibraryGeometries {
		[XmlAttribute("id")]
		public string ID;
		
		[XmlAttribute("name")]
		public string Name;
		
		[XmlElement(ElementName = "geometry")]
		public Geometry[] Geometry;
	}
}
