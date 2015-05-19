using System.IO;

public static class Log {
	public const int ERROR   = 0;
	public const int WARNING = 1;
	public const int INFO    = 2;
	public const int DEBUG   = 3;
	public static int Level { get; set; } = -1;
	
	
	public static void Init(string file, int level) {
		Directory.CreateDirectory(Path.GetDirectoryName(file));
		logfile = new StreamWriter(file);
		logfile.AutoFlush = true;
		Level = level;
	}
	
	
	public static void Debug(string s) {
		debug(s, new object[] {});
	}
	public static void Debug(string s, object o) {
		debug(s, new object[] {o});
	}
	public static void Debug(string s, object o1, object o2) {
		debug(s, new object[] {o1, o2});
	}
	public static void Debug(string s, object o1, object o2, object o3) {
		debug(s, new object[] {o1, o2, o3});
	}
	public static void Debug(string s, object[] o) {
		debug(s, o);
	}
	private static void debug(string s, object[] o) {
		if (Level < DEBUG) return;
		s = System.String.Format(s, o);
		System.Console.WriteLine("[{0}]", s);
		logfile.WriteLine("[{0}]", s);
	}
	
	
	public static void Info(string s) {
		info(s, new object[] {});
	}
	public static void Info(string s, object o) {
		info(s, new object[] {o});
	}
	public static void Info(string s, object o1, object o2) {
		info(s, new object[] {o1, o2});
	}
	public static void Info(string s, object o1, object o2, object o3) {
		info(s, new object[] {o1, o2, o3});
	}
	public static void Info(string s, object[] o) {
		info(s, o);
	}
	private static void info(string s, object[] o) {
		if (Level < INFO) return;
		System.Console.WriteLine(s, o);
		logfile.WriteLine(s, o);
	}
	
	
	public static void Warn(string s) {
		warn(s, new object[] {});
	}
	public static void Warn(string s, object o) {
		warn(s, new object[] {o});
	}
	public static void Warn(string s, object o1, object o2) {
		warn(s, new object[] {o1, o2});
	}
	public static void Warn(string s, object o1, object o2, object o3) {
		warn(s, new object[] {o1, o2, o3});
	}
	public static void Warn(string s, object[] o) {
		warn(s, o);
	}
	private static void warn(string s, object[] o) {
		if (Level < WARNING) return;
		s = System.String.Format(s, o);
		System.Console.WriteLine("\x1B[33mWarning:\x1B[39m {0}", s);
		logfile.WriteLine("Warning: {0}", s);
	}
	
	
	public static void Error(string s) {
		error(s, new object[] {});
	}
	public static void Error(string s, object o) {
		error(s, new object[] {o});
	}
	public static void Error(string s, object o1, object o2) {
		error(s, new object[] {o1, o2});
	}
	public static void Error(string s, object o1, object o2, object o3) {
		error(s, new object[] {o1, o2, o3});
	}
	public static void Error(string s, object[] o) {
		error(s, o);
	}
	private static void error(string s, object[] o) {
		if (Level < ERROR) return;
		s = System.String.Format(s, o);
		System.Console.WriteLine("\x1B[31mERROR:\x1B[39m {0}", s);
		logfile.WriteLine("ERROR: {0}", s);
	}
	
	
	private static StreamWriter logfile;
}
