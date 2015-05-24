using System.Reflection;
using System.CodeDom.Compiler;

[System.Serializable]
public class ScriptHelper {
	public Assembly Load(string dll) {
		return Assembly.LoadFile(dll);
	}
	public CompilerResults Compile(string[] paths, string outDir) {
		var options = new CompilerParameters();
		options.GenerateExecutable   = false;
		options.GenerateInMemory     = true;
		options.OutputAssembly       = outDir;
		options.ReferencedAssemblies.Add("script-context.dll");
		options.ReferencedAssemblies.Add("util.dll");
		
		var provider = new Microsoft.CSharp.CSharpCodeProvider();

		return provider.CompileAssemblyFromFile(options, paths);
	}
}
