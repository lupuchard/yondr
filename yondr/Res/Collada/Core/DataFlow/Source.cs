using System;
using System.Xml;
using System.Xml.Serialization;
namespace Collada {
	[SerializableAttribute()]
	[XmlTypeAttribute(AnonymousType=true)]
	public partial class Source {
		[XmlAttribute("id")]
		public string ID;
		
		[XmlAttribute("name")]
		public string Name;			
		

		[XmlElement(ElementName = "bool_array")]
		public BoolArray BoolArray;
		
		[XmlElement(ElementName = "float_array")]
		public FloatArray FloatArray;
		
		[XmlElement(ElementName = "int_array")]
		public IntArray IntArray;
		
		
		[XmlElement(ElementName = "technique_common")]
		public TechniqueCommonSource Technique_Common;
	    
		[XmlElement(ElementName = "technique")]
		public Technique[] Technique;
	}
}

