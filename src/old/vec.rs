
extern crate vecmath;
use std::num;

/// The vector newtypes. These are wrappers around all the vecmath.

#[deriving(Eq, PartialEq, Ord, PartialOrd, Hash)]
pub struct Vec2<N: Primitive>([N,..2]);

#[deriving(Eq, PartialEq, Ord, PartialOrd, Hash)]
pub struct Vec3<N: Primitive>([N,..3]);

// TODO Show, Zero, Clone

// Implement Vec2
impl<N: Primitive> Vec2<N> {
	#[inline]
	pub fn as_array(&self) -> [N,..2] {
		let Vec2(a) = *self; a
	}
	#[inline]
	pub fn to<T: Primitive>(&self) -> Vec2<T> {
		let Vec2(a) = *self;
		let x: T = num::cast(a[0]).unwrap();
		let y: T = num::cast(a[1]).unwrap();
		Vec2([x, y])
	}
	#[inline]
	pub fn from<T: Primitive>(&Vec2(a): &Vec2<T>) -> Vec2<N> {
		let x: N = num::cast(a[0]).unwrap();
		let y: N = num::cast(a[1]).unwrap();
		Vec2([x, y])
	}
	#[inline]
	pub fn x(&self) -> N { self.as_array()[0] }
	#[inline]
	pub fn y(&self) -> N { self.as_array()[1] }

	#[inline]
	pub fn cross(&self, other: &Vec2<N>) -> N {
		vecmath::vec2_cross(self.as_array(), other.as_array())
	}
	#[inline]
	pub fn dot(&self, other: &Vec2<N>) -> N {
		vecmath::vec2_dot(self.as_array(), other.as_array())
	}
}
impl<N: Primitive+Float> Vec2<N> {
	#[inline]
	pub fn len(&self) -> N {
		vecmath::vec2_len(self.as_array())
	}
	#[inline]
	pub fn square_len(&self) -> N {
		vecmath::vec2_square_len(self.as_array())
	}
	#[inline]
	pub fn inv_len(&self) -> N {
		vecmath::vec2_inv_len(self.as_array())
	}
	#[inline]
	pub fn normalized(&self) -> Vec2<N> {
		Vec2(vecmath::vec2_normalized(self.as_array()))
	}
	#[inline]
	pub fn normalized_sub(&self, other: &Vec2<N>) -> Vec2<N> {
		Vec2(vecmath::vec2_normalized_sub(self.as_array(), other.as_array()))
	}
}

// implement vec3
impl<N: Primitive> Vec3<N> {
	#[inline]
	pub fn as_array(&self) -> [N,..3] {
		let Vec3(a) = *self; a
	}
	#[inline]
	pub fn to<T: Primitive>(&self) -> Vec3<T> {
		let Vec3(a) = *self;
		let x: T = num::cast(a[0]).unwrap();
		let y: T = num::cast(a[1]).unwrap();
		let z: T = num::cast(a[2]).unwrap();
		Vec3([x, y, z])
	}
	#[inline]
	pub fn from<T: Primitive>(&Vec3(a): &Vec3<T>) -> Vec3<N> {
		let x: N = num::cast(a[0]).unwrap();
		let y: N = num::cast(a[1]).unwrap();
		let z: N = num::cast(a[2]).unwrap();
		Vec3([x, y, z])
	}
	#[inline]
	pub fn x(&self) -> N { self.as_array()[0] }
	#[inline]
	pub fn y(&self) -> N { self.as_array()[1] }
	#[inline]
	pub fn z(&self) -> N { self.as_array()[2] }

	#[inline]
	pub fn cross(&self, other: &Vec3<N>) -> Vec3<N> {
		Vec3(vecmath::vec3_cross(self.as_array(), other.as_array()))
	}
	#[inline]
	pub fn dot(&self, other: &Vec3<N>) -> N {
		vecmath::vec3_dot(self.as_array(), other.as_array())
	}
	#[inline]
	pub fn dot_pos2(&self, other: &Vec2<N>) -> N {
		vecmath::vec3_dot_pos2(self.as_array(), other.as_array())
	}
	#[inline]
	pub fn dot_vec2(&self, other: &Vec2<N>) -> N {
		vecmath::vec3_dot_vec2(self.as_array(), other.as_array())
	}
}
impl<N: Primitive+Float> Vec3<N> {
	#[inline]
	pub fn len(&self) -> N {
		vecmath::vec3_len(self.as_array())
	}
	#[inline]
	pub fn square_len(&self) -> N {
		vecmath::vec3_square_len(self.as_array())
	}
	#[inline]
	pub fn inv_len(&self) -> N {
		vecmath::vec3_inv_len(self.as_array())
	}
	#[inline]
	pub fn normalized(&self) -> Vec3<N> {
		Vec3(vecmath::vec3_normalized(self.as_array()))
	}
	#[inline]
	pub fn normalized_sub(&self, other: &Vec3<N>) -> Vec3<N> {
		Vec3(vecmath::vec3_normalized_sub(self.as_array(), other.as_array()))
	}
}

macro_rules! op_binary {
	(impl $trait_: ident for $type_: ident { $op_method: ident, $vm_method: ident }) => {
		impl<N: Primitive> $trait_<$type_<N>, $type_<N>> for $type_<N> {
			#[inline]
			fn $op_method(&self, other: &$type_<N>) -> $type_<N> {
				$type_(vecmath::$vm_method(self.as_array(), other.as_array()))
			}
		}
	}
}

op_binary!(impl Add for Vec2 { add, vec2_add })
op_binary!(impl Sub for Vec2 { sub, vec2_sub })
impl<N: Primitive> Mul<N, Vec2<N>> for Vec2<N> {
	#[inline]
	fn mul(&self, b: &N) -> Vec2<N> {
		Vec2(vecmath::vec2_scale(self.as_array(), *b))
	}
}

op_binary!(impl Add for Vec3 { add, vec3_add })
op_binary!(impl Sub for Vec3 { sub, vec3_sub })
impl<N: Primitive> Mul<N, Vec3<N>> for Vec3<N> {
	#[inline]
	fn mul(&self, b: &N) -> Vec3<N> {
		Vec3(vecmath::vec3_scale(self.as_array(), *b))
	}
}

#[cfg(test)]
mod test {
	use super::Vec2;

	#[test]
	fn vec2() {
		let a = Vec2([ 1i, 2]);
		let b = Vec2([-3i, 1]);
		let c = a + b;
		assert!(c == Vec2([-2, 3]));
	}
}
