use std::collections::HashMap;

use property_system::property::{Val, PropertyIdx, PropertySystem};

pub type EntityIdx     = u16;
pub type EntityBaseIdx = u16;

/// Stores values in a vector long enough to store all values.
pub struct EntityBase<'a> {
	props: &'a PropertySystem,
	values: Vec<Val>,
}
impl<'a> EntityBase<'a> {
	// Create a new EntityBase that uses the given PropertySystem.
	// It will give each value the default from the PropertySystem.
	pub fn new(property_system: &'a PropertySystem) -> EntityBase<'a> {
		EntityBase {
			props: property_system,
			values: (0..property_system.size()).map(
				|idx| property_system.at(idx as PropertyIdx).get_type()
			).collect(),
		}
	}
	pub fn get_system(&self) -> &'a PropertySystem {
		self.props
	}

	pub fn get_ref(&self, property_index: PropertyIdx) -> &Val {
		&self.values[property_index as usize]
	}
	pub fn get_value(&self, property_index: PropertyIdx) -> Val {
		self.values[property_index as usize].clone()
	}
	pub fn set_value(&mut self, property_index: PropertyIdx, value: Val) {
		self.values[property_index as usize] = value;
	}
}

/// An Entity is a flyweight object that you can get and set
/// the values of various properties with. You pass it a base
/// on construction which contains all the default values.
pub struct Entity<'a> {
	base: &'a EntityBase<'a>,
	values: HashMap<PropertyIdx, Val>,
	idx: EntityIdx,
}
impl<'a> Entity<'a> {
	pub fn new(base: &'a EntityBase, idx: EntityIdx) -> Entity<'a> {
		Entity { base: base, values: HashMap::new(), idx: idx }
	}
	pub fn get_system(&self) -> &'a PropertySystem {
		self.base.get_system()
	}
	pub fn get_index(&self) -> EntityIdx {
		self.idx
	}

	pub fn get_ref(&self, property_index: PropertyIdx) -> &Val {
		match self.values.get(&property_index) {
			Some(v) => &v,
			None    => self.base.get_ref(property_index),
		}
	}
	pub fn get_ref_by_name(&self, name: &str) -> Option<&Val> {
		match self.base.get_system().name_to_index(name) {
			Some(i) => Some(self.get_ref(i)),
			None    => None,
		}
	}

	pub fn get_value(&self, property_index: PropertyIdx) -> Val {
		match self.values.get(&property_index) {
			Some(v) => v.clone(),
			None    => self.base.get_value(property_index),
		}
	}
	pub fn get_value_by_name(&self, name: &str) -> Option<Val> {
		match self.base.get_system().name_to_index(name) {
			Some(i) => Some(self.get_value(i)),
			None    => None,
		}
	}

	pub fn set_value(&mut self, property_index: PropertyIdx, value: Val) {
		let property = self.base.get_system().at(property_index);
		debug_assert!(
			property.get_type().is_same_type(&value),
			"{:?} passed to set_value for property of type {:?}", value, property.get_type()
		);

		self.values.insert(property_index, value);
	}
	pub fn set_value_by_name(&mut self, name: &str, value: Val) {
		match self.base.get_system().name_to_index(name) {
			Some(p) => self.set_value(p, value),
			_ => (), // TODO: what to do on fail?
		}
	}
}

#[cfg(test)]
mod test {
	use super::*;
	use property_system::{PropertySystem, Val};

	#[test]
	fn entity() {
		let mut props = PropertySystem::new();
		let tits  = props.add("titanium".to_string(), Val::Int(0));
		let black = Val::new_str("black");
		let color = props.add("fav_color".to_string(), black.clone());

		let mut base = EntityBase::new(&props);
		base.set_value(tits, Val::Int(4));

		let mut entity = Entity::new(&base, 0);
		assert_eq!(entity.get_ref(tits), &Val::Int(4));
		assert_eq!(entity.get_ref_by_name("fav_color").unwrap(), &black);

		entity.set_value(tits, Val::Int(6));
		entity.set_value_by_name("fav_color", black.clone());
		assert_eq!(entity.get_value(color), black);
	}

	#[test]
	#[should_panic]
	fn thing_fail() {
		let mut props = PropertySystem::new();
		let soda = props.add("soda".to_string(), Val::Int(0));
		let base = EntityBase::new(&props);
		let mut entity = Entity::new(&base, 0);

		entity.set_value(soda, Val::Float(0.5));
	}
}
