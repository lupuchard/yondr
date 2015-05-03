use std::ops::{Add, Sub, Mul, Div, Neg, Rem};
//use std::num::{Float, NumCast, cast};
use num::traits::{Float, NumCast, cast};
use num::{Num, Signed, Unsigned, Zero, One, zero, one};

/// Vec2 is a two dimensional vector for dimensions and positions and vector math and stuff.
#[derive(Copy, Clone, Default, Debug, PartialEq, PartialOrd, Hash)]
pub struct Vec2<T> {
	pub x: T,
	pub y: T,
}
impl<T> Vec2<T> {
	pub fn new(x: T, y: T) -> Self {
		Vec2 { x: x, y: y }
	}
}
impl<T: Num + NumCast + Copy> Vec2<T> {
	pub fn sqr_length(&self) -> T {
		self.x * self.x + self.y * self.y
	}
	pub fn length<F>(&self) -> F where F: Float {
		cast::<T, F>(self.sqr_length()).unwrap().sqrt()
	}
}

// vector arithmetic
impl<T: Add<Output=T>> Add for Vec2<T> {
	type Output = Self;
	fn add(self, rhs: Self) -> Self { Vec2::new(self.x + rhs.x, self.y + rhs.y) }
}
impl<T: Sub<Output=T>> Sub for Vec2<T> {
	type Output = Self;
	fn sub(self, rhs: Self) -> Self { Vec2::new(self.x - rhs.x, self.y - rhs.y) }
}
impl<T: Mul<Output=T>> Mul for Vec2<T> {
	type Output = Self;
	fn mul(self, rhs: Self) -> Self { Vec2::new(self.x * rhs.x, self.y * rhs.y) }
}
impl<T: Div<Output=T>> Div for Vec2<T> {
	type Output = Self;
	fn div(self, rhs: Self) -> Self { Vec2::new(self.x / rhs.x, self.y / rhs.y) }
}

// scalar arithmetic
impl<T: Add<Output=T> + Copy> Add<T> for Vec2<T> {
	type Output = Self;
	fn add(self, rhs: T) -> Self { Vec2::new(self.x + rhs, self.y + rhs) }
}
impl<T: Sub<Output=T> + Copy> Sub<T> for Vec2<T> {
	type Output = Self;
	fn sub(self, rhs: T) -> Self { Vec2::new(self.x - rhs, self.y - rhs) }
}
impl<T: Mul<Output=T> + Copy> Mul<T> for Vec2<T> {
	type Output = Self;
	fn mul(self, rhs: T) -> Self { Vec2::new(self.x * rhs, self.y * rhs) }
}
impl<T: Div<Output=T> + Copy> Div<T> for Vec2<T> {
	type Output = Self;
	fn div(self, rhs: T) -> Self { Vec2::new(self.x / rhs, self.y / rhs) }
}

// negation
impl<T: Neg<Output=T>> Neg for Vec2<T> {
	type Output = Self;
	fn neg(self) -> Self { Vec2::new(-self.x, -self.y) }
}

// identities
impl<T: Zero> Zero for Vec2<T> {
	fn zero() -> Self { Vec2::new(zero(), zero()) }
	fn is_zero(&self) -> bool { self.x.is_zero() && self.y.is_zero() }
}
impl<T: One> One for Vec2<T> {
	fn one() -> Self { Vec2::new(one(), one()) }
}

// remainder, why not?
impl<T: Rem<Output=T>> Rem for Vec2<T> {
	type Output = Self;
	fn rem(self, rhs: Self) -> Self { Vec2::new(self.x % rhs.x, self.y % rhs.y) }
}
impl<T: Rem<Output=T> + Copy> Rem<T> for Vec2<T> {
	type Output = Self;
	fn rem(self, rhs: T) -> Self { Vec2::new(self.x % rhs, self.y % rhs) }
}

// make it official
impl<E, T: Copy+Num<FromStrRadixErr=E>> Num for Vec2<T> {
	type FromStrRadixErr = E;
	fn from_str_radix(str: &str, radix: u32) -> Result<Self, E> {
		let split: Vec<&str> = str.split(',').collect();
		match split.len() {
			0 => Ok(Vec2::new(zero(), zero())),
			1 => {
				let val = try!(T::from_str_radix(str, radix));
				Ok(Vec2::new(val, val))
			},
			_ => {
				let val1 = try!(T::from_str_radix(split[0], radix));
				let val2 = try!(T::from_str_radix(split[1], radix));
				Ok(Vec2::new(val1, val2))
			},
		}
	}
}
impl<T: Copy+Unsigned> Unsigned for Vec2<T> { }
impl<T: Copy+Signed> Signed for Vec2<T> {
	fn signum(&self) -> Self { Vec2::new(self.x.signum(), self.y.signum()) }
	fn abs(&self)    -> Self { Vec2::new(self.x.abs()   , self.y.abs())    }
	fn abs_sub(&self, other: &Self) -> Self {
		Vec2::new(self.x.abs_sub(&other.x), self.y.abs_sub(&other.y))
	}
	fn is_positive(&self) -> bool { self.x.is_positive() && self.y.is_positive() }
	fn is_negative(&self) -> bool { self.x.is_negative() && self.y.is_negative() }
}


/// Vec3 is a three dimensional vector for dimensions and positions and vector math and stuff.
#[derive(Copy, Clone, Default, Debug, PartialEq, PartialOrd, Hash)]
pub struct Vec3<T> {
	pub x: T,
	pub y: T,
	pub z: T,
}
impl<T> Vec3<T> {
	pub fn new(x: T, y: T, z: T) -> Self {
		Vec3 { x: x, y: y, z: z }
	}
}
impl<T: Num + NumCast + Copy> Vec3<T> {
	pub fn sqr_length(&self) -> T {
		self.x * self.x + self.y * self.y + self.z * self.z
	}
	pub fn length<F>(&self) -> F where F: Float {
		cast::<T, F>(self.sqr_length()).unwrap().sqrt()
	}
}

// vector arithmetic
impl<T: Add<Output=T>> Add for Vec3<T> {
	type Output = Self;
	fn add(self, rhs: Self) -> Self { Vec3::new(self.x + rhs.x, self.y + rhs.y, self.z + rhs.z) }
}
impl<T: Sub<Output=T>> Sub for Vec3<T> {
	type Output = Self;
	fn sub(self, rhs: Self) -> Self { Vec3::new(self.x - rhs.x, self.y - rhs.y, self.z - rhs.z) }
}
impl<T: Mul<Output=T>> Mul for Vec3<T> {
	type Output = Self;
	fn mul(self, rhs: Self) -> Self { Vec3::new(self.x * rhs.x, self.y * rhs.y, self.z * rhs.z) }
}
impl<T: Div<Output=T>> Div for Vec3<T> {
	type Output = Self;
	fn div(self, rhs: Self) -> Self { Vec3::new(self.x / rhs.x, self.y / rhs.y, self.z / rhs.z) }
}

// scalar arithmetic
impl<T: Add<Output=T> + Copy> Add<T> for Vec3<T> {
	type Output = Self;
	fn add(self, rhs: T) -> Self { Vec3::new(self.x + rhs, self.y + rhs, self.z + rhs) }
}
impl<T: Sub<Output=T> + Copy> Sub<T> for Vec3<T> {
	type Output = Self;
	fn sub(self, rhs: T) -> Self { Vec3::new(self.x - rhs, self.y - rhs, self.z - rhs) }
}
impl<T: Mul<Output=T> + Copy> Mul<T> for Vec3<T> {
	type Output = Self;
	fn mul(self, rhs: T) -> Self { Vec3::new(self.x * rhs, self.y * rhs, self.z * rhs) }
}
impl<T: Div<Output=T> + Copy> Div<T> for Vec3<T> {
	type Output = Self;
	fn div(self, rhs: T) -> Self { Vec3::new(self.x / rhs, self.y / rhs, self.z / rhs) }
}

// negation
impl<T: Neg<Output=T>> Neg for Vec3<T> {
	type Output = Self;
	fn neg(self) -> Self { Vec3::new(-self.x, -self.y, -self.z) }
}

// identities
impl<T: Zero> Zero for Vec3<T> {
	fn zero() -> Self { Vec3::new(zero(), zero(), zero()) }
	fn is_zero(&self) -> bool { self.x.is_zero() && self.y.is_zero() && self.z.is_zero() }
}
impl<T: One> One for Vec3<T> {
	fn one() -> Self { Vec3::new(one(), one(), one()) }
}

// remainder, why not?
impl<T: Rem<Output=T>> Rem for Vec3<T> {
	type Output = Self;
	fn rem(self, rhs: Self) -> Self { Vec3::new(self.x % rhs.x, self.y % rhs.y, self.z % rhs.z) }
}
impl<T: Rem<Output=T> + Copy> Rem<T> for Vec3<T> {
	type Output = Self;
	fn rem(self, rhs: T) -> Self { Vec3::new(self.x % rhs, self.y % rhs, self.z % rhs) }
}

// make it official
impl<E, T: Copy+Num<FromStrRadixErr=E>> Num for Vec3<T> {
	type FromStrRadixErr = E;
	fn from_str_radix(str: &str, radix: u32) -> Result<Self, E> {
		let split: Vec<&str> = str.split(',').collect();
		match split.len() {
			0 => Ok(Vec3::new(zero(), zero(), zero())),
			1 => {
				let val = try!(T::from_str_radix(str, radix));
				Ok(Vec3::new(val, val, val))
			},
			2 => {
				let val1 = try!(T::from_str_radix(split[0], radix));
				let val2 = try!(T::from_str_radix(split[1], radix));
				Ok(Vec3::new(val1, val2, val2))
			},
			_ => {
				let val1 = try!(T::from_str_radix(split[0], radix));
				let val2 = try!(T::from_str_radix(split[1], radix));
				let val3 = try!(T::from_str_radix(split[2], radix));
				Ok(Vec3::new(val1, val2, val3))
			},
		}
	}
}
impl<T: Copy+Unsigned> Unsigned for Vec3<T> { }
impl<T: Copy+Signed> Signed for Vec3<T> {
	fn signum(&self) -> Self { Vec3::new(self.x.signum(), self.y.signum(), self.z.signum()) }
	fn abs(&self)    -> Self { Vec3::new(self.x.abs()   , self.y.abs()   , self.z.abs())    }
	fn abs_sub(&self, other: &Self) -> Self {
		Vec3::new(self.x.abs_sub(&other.x), self.y.abs_sub(&other.y), self.z.abs_sub(&other.z))
	}
	fn is_positive(&self) -> bool {
		self.x.is_positive() && self.y.is_positive() && self.z.is_positive()
	}
	fn is_negative(&self) -> bool {
		self.x.is_negative() && self.y.is_negative() && self.z.is_positive()
	}
}


#[cfg(test)]
mod test {
	use super::*;

	#[test]
	fn dumb() {
		let mut x = Vec2::new(0, 0);
		x = x - 2;
		let y = Vec2::new(16, 17);
		let z = x + y;
		assert_eq!(z, Vec2::new(14, 15));
	}
}
