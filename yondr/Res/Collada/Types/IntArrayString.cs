using System;
using System.Xml.Serialization;
namespace Collada {
	[SerializableAttribute()]
	[XmlTypeAttribute(AnonymousType=true)]
	public class IntArrayString {
		
		[XmlTextAttribute()]
	    public string ValueAsString;
		
		public int[] Value() {
			return ParseUtils.StringToInt(ValueAsString);
		}
	}
}

