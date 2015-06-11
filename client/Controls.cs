using System;
using System.Collections.Generic;
using OpenTK.Input;

public class Controls: IControls {
	public void Add(string key, string name) {
		key  = StringUtil.Simplify(key);
		name = StringUtil.Simplify(name);
		Key tkey;

		if (!Enum.TryParse(key, true, out tkey)) {
			switch (key) {
				case "0": tkey = Key.Number0; break;
				case "1": tkey = Key.Number1; break;
				case "2": tkey = Key.Number2; break;
				case "3": tkey = Key.Number3; break;
				case "4": tkey = Key.Number4; break;
				case "5": tkey = Key.Number5; break;
				case "6": tkey = Key.Number6; break;
				case "7": tkey = Key.Number7; break;
				case "8": tkey = Key.Number8; break;
				case "9": tkey = Key.Number9; break;
				default:
					Log.Error("'{0}' is not an existing key.", key);
					return;
			}
		}

		string prevAction;
		if (keyToAction.TryGetValue(tkey, out prevAction)) {
			actionToKeys[prevAction].Remove(tkey);
			keyToAction[tkey] = name;
		} else {
			HashSet<Key> keys;
			if (actionToKeys.TryGetValue(name, out keys)) {
				keys.Add(tkey);
			} else {
				keys = new HashSet<Key>();
				keys.Add(tkey);
				actionToKeys.Add(name, keys);
			}
			keyToAction.Add(tkey, name);
		}
	}

	public bool IsDown(string name) {
		KeyboardState state = Keyboard.GetState();
		name = StringUtil.Simplify(name);
		HashSet<Key> keys;
		if (actionToKeys.TryGetValue(name, out keys)) {
			foreach (Key key in keys) {
				if (state[key]) return true;
			}
		} else {
			Log.Error("'{0}' is not an existing control.", name);
		}
		return false;
	}

	//private KeyboardState state = Keyboard.GetState();
	private Dictionary<Key, string> keyToAction = new Dictionary<Key, string>();
	private Dictionary<string, HashSet<Key>> actionToKeys = new Dictionary<string, HashSet<Key>>();
}
