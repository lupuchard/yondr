
public interface IControls {
	/// Adds a new control mapping.
	/// Multiple keys can be mapped to the same action. If the given key is already mapped to a
	/// different action, that mapping will be removed.
	/// @param key Should match one of the OpenTK keys (but is not case sensitive).
	/// @param name The name of the action. Not case sensitive.
	void Add(string key, string name);

	/// Tells you if any of the keys mapped to this action is being pressed.
	bool IsDown(string name);
}
