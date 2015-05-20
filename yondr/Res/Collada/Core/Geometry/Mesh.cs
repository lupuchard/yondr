using System.Xml;
using System.Xml.Serialization;
namespace Collada {
	[System.SerializableAttribute()]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
	public class Mesh {
		[XmlElement(ElementName = "source")]
		public Source[] Source;	
		

		[XmlElement(ElementName = "polylist")]
		public Polylist[] Polylist;
		
		[XmlElement(ElementName = "triangles")]
		public Triangles[] Triangles;
		
		[XmlElement(ElementName = "trifans")]
		public Trifans[] Trifans;
		
		[XmlElement(ElementName = "tristrips")]
		public Tristrips[] Tristrips;
		
		
		[XmlElement(ElementName = "vertices")]
		public Vertices Vertices;	
	}
}
