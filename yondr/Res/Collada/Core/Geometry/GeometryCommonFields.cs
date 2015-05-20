using System;
using System.Xml;
using System.Xml.Serialization;
namespace Collada {
	[SerializableAttribute()]
	[XmlTypeAttribute(AnonymousType=true)]
	public class GeometryCommonFields {
		[XmlAttribute("count")]
		public int Count;
		
		[XmlAttribute("name")]
		public string Name;

	    [XmlElement(ElementName = "p")]
		public IntArrayString P;		

		[XmlElement(ElementName = "input")]
		public InputShared[] Input;	
	}
}
