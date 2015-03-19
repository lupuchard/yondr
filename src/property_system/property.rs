
use std::collections::HashMap;

use name::Name;


pub type PropertyIdx = u16;

/// Value variant for properties.
#[derive(Clone, Debug, PartialEq, PartialOrd)]
pub enum Val {
	Bool(bool),
	Int(i32),
	Float(f32),
	Str(Box<String>),
} // TODO unsafe enum
impl Val {
	pub fn get_type_index(&self) -> u16 {
		match *self {
			Val::Bool(_)   => 1,
			Val::Int(_)    => 2,
			Val::Float(_)  => 3,
			Val::Str(_)    => 4,
		}
	}
	pub fn is_same_type(&self, other: &Val) -> bool {
		self.get_type_index() == other.get_type_index()
	}

	pub fn new_str(string: &str) -> Val {
		Val::Str(Box::new(string.to_string()))
	}
}

/// A Property a certain type of thing has.
pub struct Property {
	pub name: Name,
	pub value: Val, // type and default value
	pub index: PropertyIdx,
	// formula;
}
impl Property {
	pub fn get_type(&self) -> Val {
		self.value.clone()
	}
	pub fn get_name(&self) -> &str {
		&self.name.id[..]
	}
	pub fn get_package(&self) -> u32 {
		self.name.package
	}
	pub fn get_index(&self) -> PropertyIdx {
		self.index
	}
}

/// Manages the properties. Retrieve by index or name.
pub struct PropertySystem {
	properties: Vec<Box<Property>>,
	name_map:   HashMap<Name, PropertyIdx>,
}
impl PropertySystem {

	/// Constructs a new PropertySystem.
	pub fn new() -> PropertySystem {
		PropertySystem {
			properties: Vec::new(),
			name_map:   HashMap::new(),
		}
	}

	/// Creates an property of the specified name and adds it to the manager.
	/// # Arguments
	///   * name - The name of the created property.
	///   * value - Both the type and default of the created property.
	/// # Return value
	///   Index of created property.
	pub fn add(&mut self, name: Name, value: Val) -> PropertyIdx {
		let index = self.properties.len() as PropertyIdx;
		self.name_map.insert(name.clone(), index);
		self.properties.push(Box::new(Property {
			name: name, value: value, index: index
		}));
		index
	}

	/// Returns the number of objects currently in this Manager.
	pub fn size(&self) -> usize {
		self.properties.len()
	}

	#[inline]
	/// Returns the object of the given index.
	/// Panics if index >= self.size().
	pub fn at(&self, index: PropertyIdx) -> &Property {
		&*self.properties[index as usize]
	}

	/// Returns the object of the given name,
	/// or None if no object of the given name is in the Manager.
	pub fn with_name(&self, name: &Name) -> Option<&Property> {
		match self.name_map.get(name) {
			Some(i) => Some(self.at(*i)),
			None    => None,
		}
	}

	/// Returns index of property with given name.
	pub fn name_to_index(&self, name: &Name) -> Option<PropertyIdx> {
		match self.name_map.get(name) {
			Some(i) => Some(*i),
			None    => None,
		}
	}
}

#[cfg(test)]
mod test {
	use super::*;
	use name::Name;

	#[test]
	fn use_property() {
		let prop = Property { name: Name::new("Cats", 0), value: Val::Int(3), index: 0 };
		assert_eq!(prop.get_name(), "cats");
		match prop.get_type() {
			Val::Int(val) => assert!(val == 3),
			_ => assert!(false),
		}
		assert_eq!(prop.get_index(), 0);
		println!("whats");
	}

	#[test]
	fn use_manager() {
		let mut man = PropertySystem::new();
		man.add(Name::new("fred", 0), Val::Float(4.5));
		man.add(Name::new("paul", 0), Val::new_str("paulerson"));
		man.add(Name::new("carl", 0), Val::Bool(true));
		assert_eq!(man.size(), 3);
		let fred = man.with_name(&Name::new("fred", 0)).unwrap();
		let paul = man.with_name(&Name::new("paul", 0)).unwrap();
		let carl = man.with_name(&Name::new("carl", 0)).unwrap();
		assert!(fred.index != paul.index && fred.index != carl.index);
		match paul.value {
			Val::Str(ref val) => assert!(&val[..] == "paulerson"),
			_ => assert!(false),
		}
		assert_eq!(man.at(carl.index).get_name(), "carl");
		assert!(man.with_name(&Name::new("phil", 0)).is_none());
		assert!(man.with_name(&Name::new("fred", 3)).is_none());
	}
}
