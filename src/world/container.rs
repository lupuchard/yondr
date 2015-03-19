
use std::u16;
use std::collections::HashMap;
use rustc_serialize::json;
use rustc_serialize::json::Json;

use util::vec::Vec3;
use util::json::ApplyJsonResult;
use property_system::property::{Val, PropertySystem};
use property_system::entity::{Entity, EntityBase, EntityIdx, EntityBaseIdx};
use name::Name;

/// Something that contains entities.
pub trait EntityContainer<'a> {

	/// Each EntityContainer gets to have it's own PropertySystem, and
	/// this is how you access it.
	fn get_property_system(&self) -> &PropertySystem;

	/// Returns the entity with the given index.
	/// Each container has rules regarding what indicies
	/// are valid and may panic on an invalid index.
	fn get_entity(&mut self, idx: u16) -> &mut Entity<'a>;

	/// Creates a new entity with the given base and index.
	fn create_entity(&'a mut self, base: EntityBaseIdx, idx: EntityIdx) -> u16;

	/// Adds a base yes.
	fn add_base(&mut self, name: Name, base: EntityBase<'a>) -> EntityBaseIdx;

	/// Gets a base yes.
	fn get_base(&self, base: EntityBaseIdx) -> &EntityBase;
	fn get_base_with_name(&self, name: &Name) -> Option<&EntityBase>;

	// Loads json stuff.
	//fn apply_json(&'a mut self, data: &str) -> ApplyJsonResult;
}


pub struct ListContainer<'a> {
	property_system: &'a PropertySystem,
	
	bases: Vec<EntityBase<'a>>,
	base_name_map: HashMap<Name, EntityBaseIdx>,

	entities: Vec<Entity<'a>>,
}
impl<'a> ListContainer<'a> {
	pub fn new(props: &'a PropertySystem) -> ListContainer<'a> {
		ListContainer {
			property_system: props,
			bases:           Vec::new(),
			base_name_map:   HashMap::new(),
			entities:        Vec::new(),
		}
	}
}
impl<'a> EntityContainer<'a> for ListContainer<'a> {
	fn get_entity(&mut self, idx: u16) -> &mut Entity<'a> {
		&mut self.entities[idx as usize]
	}
	fn get_property_system(&self) -> &PropertySystem {
		&self.property_system
	}
	fn create_entity(&'a mut self, base: EntityBaseIdx, idx: EntityIdx) -> u16 {
		self.entities.push(Entity::new(&self.bases[base as usize], idx));
		(self.entities.len() - 1) as u16
	}
	fn add_base(&mut self, name: Name, base: EntityBase<'a>) -> EntityBaseIdx {
		self.bases.push(base);
		let idx = (self.bases.len() - 1) as EntityBaseIdx;
		self.base_name_map.insert(name, idx);
		idx
	}
	fn get_base(&self, base: EntityBaseIdx) -> &EntityBase {
		&self.bases[base as usize]
	}
	fn get_base_with_name(&self, base: &Name) -> Option<&EntityBase> {
		match self.base_name_map.get(base) {
			Some(i) => Some(self.get_base(*i)),
			None    => None,
		}
	}
	/*fn apply_json(&'a mut self, data: &str) -> ApplyJsonResult {
		apply_json(self, data)
	}*/
}


const GRID_EMPTY: u16 = u16::MAX;
pub struct GridContainer<'a> {
	lc: ListContainer<'a>,
	dim: Vec3<u16>,
	grid: Vec<u16>,
}
impl<'a> GridContainer<'a> {
	pub fn new(props: &'a PropertySystem, dim: Vec3<u16>) -> GridContainer<'a> {
		let size = dim.x * dim.y * dim.z;
		GridContainer {
			lc: ListContainer::new(props),
			dim: dim,
			grid: (0..size).map(|_| GRID_EMPTY).collect(), // there must be a better way
		}
	}
	pub fn get_cell(&mut self, pos: Vec3<u16>) -> Option<&mut Entity<'a>> {
		let idx = self.grid[(pos.x + pos.y * self.dim.x + pos.z * self.dim.x * self.dim.y) as usize];
		if idx != GRID_EMPTY {
			return Some(&mut self.lc.entities[idx as usize]);
		}
		None
	}
}
impl<'a> EntityContainer<'a> for GridContainer<'a> {
	fn get_entity(&mut self, idx: u16) -> &mut Entity<'a> {
		self.lc.get_entity(idx)
	}
	fn get_property_system(&self) -> &PropertySystem {
		self.lc.get_property_system()
	}
	fn create_entity(&'a mut self, base: EntityBaseIdx, idx: EntityIdx) -> u16 {
		self.lc.create_entity(base, idx)
	}
	fn add_base(&mut self, name: Name, base: EntityBase<'a>) -> EntityBaseIdx {
		self.lc.add_base(name, base)
	}
	fn get_base(&self, base: EntityBaseIdx) -> &EntityBase {
		self.lc.get_base(base)
	}
	fn get_base_with_name(&self, base: &Name) -> Option<&EntityBase> {
		self.lc.get_base_with_name(base)
	}
	/*fn apply_json(&'a mut self, data: &str) -> ApplyJsonResult {
		apply_json(self, data)
	}*/
}

/*fn apply_json<'a, T: EntityContainer<'a>>(container: &'a mut T, data: &str) -> ApplyJsonResult {
	let data = json_parse!(data, "world");

	let properties = data.search("properties");
	if properties.is_some() {
		match properties.unwrap().as_array() {
			Some(a) => apply_json_properties(container, a),
			None    => warn!("Properties is not an object."),
		}
	} else { warn!("Container has no properties object."); }

	let bases = data.search("bases");
	if bases.is_some() {
		match bases.unwrap().as_object() {
			Some(o) => apply_json_bases(container, o),
			None    => warn!("Bases is not an object."),
		}
	} else { warn!("Container has no bases object."); }

	ApplyJsonResult::Success
}
fn apply_json_properties<'a, T: EntityContainer<'a>>(container: &mut T, data: &json::Array) {
	for obj in data.iter() {
		let prop = match obj.as_object() {
			Some(o) => o,
			None => warn_cont!("Property is not an object."),
		};

		let name = json_str!(prop, "name", warn_cont!("Property has no name."));
		let mut val = match json_str!(prop, "type", warn_cont!("Property '{}' has no type.", name)) {
			"bool"   => Val::Bool(false),
			"int"    => Val::Int(0),
			"float"  => Val::Float(0.0),
			"string" => Val::Str(Box::new(String::new())),
			n        => warn_cont!("'{}' is not a valid type name.", n),
		};
		
		match prop.get("default") {
			Some(o) => {
				val = match apply_json_val(val, o) {
					Some(v) => v,
					None    => warn_cont!("Property '{}' default is invalid.", name),
				}
			}, None => (),
		}

		container.get_property_system_mut().add(name, val);
	}
}
fn apply_json_bases<'a, T: EntityContainer<'a>>(container: &'a mut T, data: &json::Object) {
	for (name, obj) in data.iter() {
		let base_info = match obj.as_object() {
			Some(o) => o,
			None => warn_cont!("Base is not an object."),
		};

		let props = container.get_property_system();
		let mut base = EntityBase::new(props);
		for (pname, val) in base_info.iter() {
			let prop = match props.with_name(pname) {
				Some(p) => p,
				None    => warn_cont!("'{}' is not an existing property.", pname),
			};
			let value = match apply_json_val(prop.get_type(), val) {
				Some(v) => v,
				None => warn_cont!("Value for '{}' in base '{}' is invalid.", name, pname),
			};
			base.set_value(prop.get_index(), value);
		}

		container.add_base(name, base);
	}
}
fn apply_json_val(val_type: Val, value: &json::Json) -> Option<Val> {
	match val_type {
		Val::Bool(_)  => match value.as_boolean() {
			Some(v) => Some(Val::Bool(v)),
			None    => None,
		},
		Val::Int(_)   => match value.as_i64() {
			Some(v) => Some(Val::Int(v as i32)),
			None    => None,
		},
		Val::Float(_) => match value.as_f64() {
			Some(v) => Some(Val::Float(v as f32)),
			None    => None,
		},
		Val::Str(_)   => match value.as_string() {
			Some(v) => Some(Val::Str(Box::new(v.to_string()))),
			None    => None,
		},
	}
}

#[cfg(test)]
mod test {
	use super::*;

	const JSON1: &'static str = r#"
{
	"for": "container",
	"properties": [
		{ "name": "weight",   "type": "int"  },
		{ "name": "is_food",  "type": "bool" },
		{ "name": "quantity", "type": "int", "default": 1 }
	],
	"bases": {
		"carrot": { "weight": 3, "is_food": true }
	}
}
"#;

	#[test]
	fn test_json() {
		let container = ListContainer::new();
		container.apply_json(JSON1);
	}
}*/
