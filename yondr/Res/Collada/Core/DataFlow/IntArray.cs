using System;
using System.Xml;
using System.Xml.Serialization;
namespace Collada {
	[SerializableAttribute()]
	[XmlTypeAttribute(AnonymousType=true)]
	public class IntArray: IntArrayString {
		[XmlAttribute("id")]
		public string ID;
		
		[XmlAttribute("name")]
		public string Name;			
		
		[XmlAttribute("count")]
		public int Count;		
		
		[XmlAttribute("minInclusive")]
	    [System.ComponentModel.DefaultValueAttribute(typeof(int), "-2147483648")]
		public int MinInclusive;		

		[XmlAttribute("maxInclusive")]
	    [System.ComponentModel.DefaultValueAttribute(typeof(int), "2147483647")]
		public int MaxInclusive;		

	}
}

