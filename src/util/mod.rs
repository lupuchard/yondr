
use std::ascii::AsciiExt;
use std::mem::transmute;

pub mod vec;
pub use self::vec::{Vec2, Vec3};

pub mod stuff;
pub use self::stuff::Stuff;

#[macro_use] pub mod macros;

/// Removes all non-alphanumeric characters and makes all lowercase.
pub fn simplify_str(string: &str) -> String {
	let mut res = String::new();
	for c in string.chars() {
		match c {
			'a'...'z' | '0' ... '9' => res.push(c),
			'A'...'Z' => res.push(c.to_ascii_lowercase()),
			'-' | '_' => res.push('_'),
			':'       => res.push(':'),
			_ => (),
		}
	}
	res
}

pub fn hash(data: &Vec<u8>) -> u64 {
	let mut hash = 0u64;

	// idk how to from byte to long
	let mut buffer = [0u8, 0, 0, 0, 0, 0, 0, 0];
	let mut idx = 0;
	for &byte in data {
		buffer[idx % 8] = byte;
		if idx % 8 == 0 {
			hash ^= unsafe { transmute::<[u8; 8], u64>(buffer) };
		}
		idx = idx + 1;
	}

	hash
}

#[cfg(test)]
mod test {
	use super::*;

	#[test]
	fn simplify_str_test() {
		assert_eq!(simplify_str("Spork!!!1one"), "spork1one".to_string());
		assert_eq!(simplify_str("  5_W_E_L_L  "), "5well".to_string());
	}

	#[test]
	fn hash_test() {
		let vec = vec![2u8, 3, 4, 5, 2, 10, 200, 210,
		               36, 46, 56, 66, 76, 86, 96, 106,
		               0, 7, 14, 21, 28, 35, 42, 49];
		assert_eq!(hash(&vec), 13305986590338460966);
	}
}
