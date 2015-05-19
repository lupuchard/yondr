using System.IO;

public class ScriptContext: IContext {
	public ScriptContext() { }

	public TextWriter Out {
		get {
			return System.Console.Out;
		}
	}
}
