using System;
using System.Xml;
using System.Xml.Serialization;
namespace Collada {
	[SerializableAttribute()]
	[XmlTypeAttribute(AnonymousType=true)]
	public class TechniqueCommonSource: TechniqueCommon {
		[XmlElement(ElementName = "accessor")]
		public Accessor Accessor;
	}
}

