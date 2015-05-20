using System;
using System.Xml.Serialization;
namespace Collada {
	[SerializableAttribute()]
	[XmlTypeAttribute(AnonymousType=true)]
	public partial class BoolArrayString {
		[XmlTextAttribute()]
	    public string ValueAsString;
		
		public bool[] Value(){
			return ParseUtils.StringToBool(ValueAsString);
		}
	}
}


