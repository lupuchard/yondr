using System;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
namespace Collada {
	[SerializableAttribute()]
	[System.Diagnostics.DebuggerStepThroughAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	[XmlTypeAttribute(AnonymousType=true)]
	[XmlRootAttribute(
		ElementName="COLLADA",
		Namespace="http://www.collada.org/2005/11/COLLADASchema",
		IsNullable=false
	)]
	public class ColladaFile {
		
		[XmlAttribute("version")]
		public string Version;
		
		[XmlElement(ElementName = "library_geometries")]
		public LibraryGeometries LibraryGeometries;
		
		public static ColladaFile Load(string fileName){
			try {
				ColladaFile colScenes = null;

				XmlSerializer sr = new XmlSerializer(typeof(ColladaFile));
				TextReader tr = new StreamReader(fileName);
				colScenes = (ColladaFile)sr.Deserialize(tr);
				tr.Close();

				return colScenes;
				
			} catch (Exception ex) {
				Console.WriteLine(ex.ToString());
				Console.ReadLine();
				return null;
			}
		}
	}
}

