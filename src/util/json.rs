
use rustc_serialize::json;

pub enum ApplyJsonResult {
	Success,
	WrongType,
	BuilderError(json::BuilderError),
	DataError(&'static str),
}

macro_rules! json_parse {
	( $data:expr, $who:expr ) => {{
		let jsm_data = match Json::from_str($data) {
			Ok(v)  => v,
			Err(e) => return ApplyJsonResult::BuilderError(e),
		};
		match jsm_data.search("for") {
			Some(j) => match j.as_string() {
				Some(s) => {
					if s != $who { return ApplyJsonResult::WrongType; }
				},
				None    => return ApplyJsonResult::WrongType,
			},
			None    => return ApplyJsonResult::WrongType,
		};
		jsm_data
	}}
}

macro_rules! json_str {
	( $obj:expr, $key:expr, $def:expr ) => {
		match $obj.get($key) {
			Some(v) => match v.as_string() {
				Some(s) => s,
				None    => $def,
			},
			None => $def,
		}
	}
}
