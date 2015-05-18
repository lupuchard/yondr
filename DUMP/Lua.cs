using System.Collections.Generic;
using NLua;

public enum LuaEvent {
	INIT, UPDATE, EXIT,
	KEY_PRESS, KEY_RELEASE, CHAR_INPUT,
	MOUSE_PRESS, MOUSE_RELEASE, MOUSE_MOVE, MOUSE_WHEEL,
	COUNT,
}

public class LuaManager {
	
	public LuaManager() {
		State = new Lua();
		State.DoFile("data/sandbox.lua");
		Sandbox = State.GetTable("env");
		// TODO assert sandbox
		
		funcs = new List<LuaFunction>[(int)LuaEvent.COUNT];
		for (int i = 0; i < funcs.Length; i++) {
			funcs[i] = new List<LuaFunction>();
		}
	}
	
	public void AddEvent(string name, LuaFunction func) {
		switch (name.ToLower()) {
			case "init":          funcs[(int)LuaEvent.INIT         ].Add(func); break;
			case "update":        funcs[(int)LuaEvent.UPDATE       ].Add(func); break;
			case "exit":          funcs[(int)LuaEvent.EXIT         ].Add(func); break;
			case "key_press":     funcs[(int)LuaEvent.KEY_PRESS    ].Add(func); break;
			case "key_release":   funcs[(int)LuaEvent.KEY_RELEASE  ].Add(func); break;
			case "char_input":    funcs[(int)LuaEvent.CHAR_INPUT   ].Add(func); break;
			case "mouse_press":   funcs[(int)LuaEvent.MOUSE_PRESS  ].Add(func); break;
			case "mouse_release": funcs[(int)LuaEvent.MOUSE_RELEASE].Add(func); break;
			case "mouse_move":    funcs[(int)LuaEvent.MOUSE_MOVE   ].Add(func); break;
			case "mouse_wheel":   funcs[(int)LuaEvent.MOUSE_WHEEL  ].Add(func); break;
			default: break;
		}
	}
	
	public IList<LuaFunction> GetEvents(LuaEvent e) {
		return funcs[(int)e].AsReadOnly();
	}
	
	public void Init() {
		foreach (LuaFunction func in funcs[(int)LuaEvent.INIT]) {
			func.Call();
		}
	}
	public void Update(double secs) {
		foreach (LuaFunction func in funcs[(int)LuaEvent.UPDATE]) {
			func.Call(secs);
		}
	}
	public void Exit() {
		foreach (LuaFunction func in funcs[(int)LuaEvent.EXIT]) {
			func.Call();
		}
	}
	
	public void run(LuaFunction func) {
		State["func"] = func;
		State.DoString(@"setfenv(func, env)
		                 func()");
	}
	
	public LuaTable load(LuaFunction func, string tableName) {
		State.NewTable(tableName);
		LuaTable table = State.GetTable(tableName);
		Sandbox[tableName] = table;
		State["func"]    = func;
		State["loadenv"] = table;
		State.DoString(@"setfenv(func, loadenv)
		                 func()");
		return table;
	}
	
	public Lua State { get; }
	public LuaTable Sandbox { get; }
	private List<LuaFunction>[] funcs;
}
