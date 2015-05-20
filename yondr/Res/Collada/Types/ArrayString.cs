using System;
using System.Xml.Serialization;
namespace Collada {
	[SerializableAttribute()]
	[XmlTypeAttribute(AnonymousType=true)]
	public partial class StringArrayString{
		[XmlTextAttribute()]
	    public string ValuePreParse;

		public string[] Value() {
			return this.ValuePreParse.Split(' ');
		}
	}
}
