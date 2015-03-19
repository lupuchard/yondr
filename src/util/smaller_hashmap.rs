
// TODO wait until new destructor semantics

extern crate libc;
use std::{ptr, mem};
use std::rt::heap::{allocate, reallocate, deallocate};

const PRIMES: [u16; 16] = [   1, 3, 7, 13, 31, 61, 127, 251, 509, 1021, 2039, 4093, 8191, 16381, 32749, 65521];
const PROBES: [u16; 16] = [0, 1, 4, 9, 16, 32, 64, 128, 256, 512, 1024, 2048, 4096, 8192, 16384, 32768];

pub struct SmallerHashMap<K, V> {
	pub data: *mut V,
	pub size: u16,
	pub capacity: u16,
	pub padding: i32, // DONT LET THIS PRECIOUS MEMORY GO TO WASTE
}

impl<K, V> SmallerHashMap<K, V> {
	pub fn new() -> SmallerHashMap<K, V> {
		SmallerHashMap {
			data: ptr::null_mut(),
			size: 0,
			capacity: 0,
			padding: 0,
		}
	}

	pub fn with_capacity(capacity: u16) -> SmallerHashMap<K, V> {
		SmallerHashMap {
			data: allocate(mem::size_of::<V>() * capacity, 8),
			size: 0,
			capacity: capacity,
			padding: 0,
		}
	}


	/// Returns the number of elements the map can hold without reallocating.
	pub fn capacity(&self) -> u16 {
		self.capacity
	}



	/// This value is not actually used by the HashMap but we don't
	/// want to let that precious 4 bytes of padding go to waste.
	pub fn get_padding(&self) -> i32 {
		self.padding
	}
	pub fn set_padding(&mut self, val: i32) {
		self.padding = val;
	}
}

impl<K, V> Drop for SmallerHashMap<K, V> {
	fn drop(&mut self) {
		deallocate(self.data, mem::size_of::<V>() * self.capacity, 8);
	}
}
