using System;
using System.Collections.Generic;
using OpenTK.Input;
using OpenTK;

public class Controls: IControls {

	public void Add(string key, string name) {
		key  = StringUtil.Simplify(key);
		name = StringUtil.Simplify(name);
		Key tkey;

		if (!Enum.TryParse(key, true, out tkey)) {
			if (!Enum.TryParse("Number" + key, out tkey)) {
				Log.Error("'{0}' is not an existing key.", key);
				return;
			}
		}
			
		Control prevControl;
		if (keyToControl.TryGetValue(tkey, out prevControl)) {
			prevControl.keys.Remove(tkey);
			Control control = nameToControl[name];
			nameToControl[name].keys.Add(tkey);
			keyToControl[tkey] = control;
		} else {
			Control control;
			if (nameToControl.TryGetValue(name, out control)) {
				control.keys.Add(tkey);
			} else {
				control = new Control();
				control.name = name;
				control.keys.Add(tkey);
				nameToControl.Add(name, control);
			}
			keyToControl.Add(tkey, control);
		}
	}

	public void Update(float diff) {
		var state = OpenTK.Input.Keyboard.GetState();
		foreach (var pair in keyToControl) {
			if (state.IsKeyDown(pair.Key)) {
				pair.Value.whileFunc.Invoke(diff);
			}
		}
	}

	public void AddWhile(string name, Action<float> whileFunc) {
		name = StringUtil.Simplify(name);
		Control control;
		if (nameToControl.TryGetValue(name, out control)) {
			control.whileFunc = whileFunc;
		} else {
			Log.Error("'{0}' is not an existing control.", name);
		}
	}

	public void AddOn(string name, Action onFunc) {
		name = StringUtil.Simplify(name);
		Control control;
		if (nameToControl.TryGetValue(name, out control)) {
			control.onFunc = onFunc;
		} else {
			Log.Error("'{0}' is not an existing control.", name);
		}
	}

	public void SetWindow(INativeWindow window) {
		window.KeyDown += (sender, e) => {
			Control action;
			if (keyToControl.TryGetValue(e.Key, out action)) {
				if (action.onFunc != null) action.onFunc.Invoke();
			}
		};
	}

	private class Control {
		public string name;
		public HashSet<Key> keys = new HashSet<Key>();
		public Action<float> whileFunc = null;
		public Action        onFunc = null;
	}
	private Dictionary<Key,    Control> keyToControl  = new Dictionary<Key,    Control>();
	private Dictionary<string, Control> nameToControl = new Dictionary<string, Control>();
}
