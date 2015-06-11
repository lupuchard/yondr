using System.Text;

public static class StringUtil {
	
	/// "Simplifies" the given string.
	/// It makes it all lowercase and removes special
	/// characters that are not underscores or periods.
	/// Also hyphens turn into underscores.
	public static string Simplify(string s) {
		var builder = new StringBuilder();
		foreach (char c in s) {
			if (char.IsLetterOrDigit(c) || c == '.') {
				builder.Append(char.ToLower(c));
			} else if (char.IsWhiteSpace(c) || c == '-' || c == '_') {
				builder.Append('_');
			}
		}
		return builder.ToString();
	}
}
