using System.Text;

static class StringUtil {
	
	public static string Simplify(string s) {
		var builder = new StringBuilder();
		foreach (char c in s) {
			if (char.IsLetterOrDigit(c) || c == ':') {
				builder.Append(char.ToLower(c));
			} else if (c == '-' || c == '_') {
				builder.Append('_');
			}
		}
		return builder.ToString();
	}
}
