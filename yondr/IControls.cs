using System;

public interface IControls {
	/// Adds a new control mapping.
	/// Multiple keys can be mapped to the same action. If the given key is already mapped to a
	/// different action, that mapping will be removed.
	/// @param key Should match one of the OpenTK keys (but is not case sensitive).
	/// @param name The name of the action. Not case sensitive.
	void Add(string key, string name);

	void Update(float diff);

	void AddWhile(string name, Action<float> whileFunc);
	void AddOn(string name, Action onFunc);
}
