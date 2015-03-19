
/*macro_rules! unwrap_or {
	( $res:expr, $def:expr ) => {
		match $res {
			Some(v) => v,
			None    => $def,
		}
	}
}*/

macro_rules! warn_cont {
	($($arg:tt)*) => {{
		warn!($($arg)*);
		continue;
	}}
}
