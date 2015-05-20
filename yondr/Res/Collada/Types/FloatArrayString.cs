using System;
using System.Xml.Serialization;
namespace Collada {
	[SerializableAttribute()]
	[XmlTypeAttribute(AnonymousType=true)]
	public partial class FloatArrayString {
		[XmlTextAttribute()]
	    public string ValueAsString;
		
		public float[] Value(){
			return ParseUtils.StringToFloat(ValueAsString);
		}
	}
}

