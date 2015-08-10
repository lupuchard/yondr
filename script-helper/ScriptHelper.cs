using System.Reflection;
using System.CodeDom.Compiler;
using System;

[System.Serializable]
public class ScriptHelper {
	public Assembly Load(string dll) {
		return Assembly.LoadFile(dll);
	}

	public CompilerResults Compile(string[] paths, string outDir) {

		var options = new CompilerParameters();
		options.GenerateExecutable = false;
		options.GenerateInMemory   = true;
		options.OutputAssembly     = outDir;

		options.ReferencedAssemblies.Add("System.Numerics.Vectors.dll");
		options.ReferencedAssemblies.Add("script-context.dll");

		// find and add System.Runtime.dll
		foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies()) {
			if (assembly.GetName().Name == "System.Runtime") {
				options.ReferencedAssemblies.Add(assembly.Location);
				break;
			}
		}

		var provider = new Microsoft.CSharp.CSharpCodeProvider();

		return provider.CompileAssemblyFromFile(options, paths);
	}
}
