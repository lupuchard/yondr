use luajit_rs as lua;

pub struct LuaManager<'a> {
	state: lua::State,
	funcs: Vec<Vec<lua::FuncRef<'a>>>,
}
impl<'a> LuaManager<'a> {
	pub fn new() -> LuaManager<'a> {
		let mut funcs = Vec::with_capacity(LuaEvent::Total as usize);
		for _ in 0..(LuaEvent::Total as usize) {
			funcs.push(Vec::new());
		}
		LuaManager {
			state: lua::State::new(),
			funcs: funcs,
		}
	}
	pub fn get_state(&mut self) -> &mut lua::State {
		&mut self.state
	}
	pub fn add_event(&mut self, event: LuaEvent, func: lua::FuncRef<'a>) {
		self.funcs[event as usize].push(func);
	}
	pub fn get_events(&self, event: LuaEvent) -> &Vec<lua::FuncRef<'a>> {
		&self.funcs[event as usize]
	}
}

pub enum LuaEvent {
	/// Called once at initialization.
	/// `init()`
	Init,

	/// Called every frame.
	/// `update(seconds_passed: Number)`
	Update, // called every frame

	/// Called once before exiting.
	/// `exit()`
	Exit,


	/// Called when a key is pressed.
	/// `key_press(name: String, scancode: Number)`
	KeyPress,

	/// Called when a key is released.
	/// `key_release(name: String, scancode: Number)`
	KeyRelease,

	/// Called when a unicode character is received.
	/// `char_input(char: String)`
	CharInput,


	/// Called when a mouse button is pressed.
	/// `mouse_press(button: String, index: Number)`
	MousePress,

	/// Called when a mouse button is released.
	/// `mouse_release(button: String, index: Number)`
	MouseRelease,

	/// Called when the mouse is moved.
	/// `mouse_move(move_x: Number, move_y: Number, current_x: Number, current_y: Number)`
	MouseMove,

	/// Called when the mouse wheel is turned.
	/// `mouse_wheel(amount: Number)`
	MouseWheel,


	Total,
}
