using System;

namespace Collada {
	public static class ParseUtils {
		public static int[] StringToInt(string intArray) {
			string[] str = intArray.Split(' ');
			int[] array = new int[str.GetLongLength(0)];
			try {
				for (long i = 0; i < str.GetLongLength(0); i++) {
					array[i] = Convert.ToInt32(str[i]);
				}
			} catch (Exception e) {
				Console.WriteLine(e.ToString());
				Console.WriteLine();
				Console.WriteLine(intArray);
			}
			return array;
		}
		
		public static float[] StringToFloat(string floatArray) {
			string[] str = floatArray.Split(' ');
			float[] array = new float[str.GetLongLength(0)];
			try {
				for (long i = 0; i < str.GetLongLength(0); i++) {
					array[i] = Convert.ToSingle(str[i]);
				}
			} catch (Exception e) {
				Console.WriteLine(e.ToString());
				Console.WriteLine();
				Console.WriteLine(floatArray);
			}
			return array;
		}
	
		public static bool[] StringToBool(string boolArray) {
			string[] str = boolArray.Split(' ');
			bool[] array = new bool[str.GetLongLength(0)];
			try {
				for (long i = 0; i < str.GetLongLength(0); i++) {
					array[i] = Convert.ToBoolean(str[i]);
				}
			} catch (Exception e) {
				Console.WriteLine(e.ToString());
				Console.WriteLine();
				Console.WriteLine(boolArray);
			}
			return array;
		}
	}
}