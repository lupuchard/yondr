
use std::u16;
use std::collections::HashMap;

use util::vec::Vec3;
use property_system::property::{PropertySystem};
use property_system::entity::{Entity, EntityBase, EntityIdx, EntityBaseIdx};

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
	fn add_base(&mut self, name: String, base: EntityBase<'a>) -> EntityBaseIdx;

	/// Gets a base yes.
	fn get_base(&self, base: EntityBaseIdx) -> &EntityBase;
	fn get_base_with_name(&self, name: &str) -> Option<&EntityBase>;

	// Loads json stuff.
	//fn apply_json(&'a mut self, data: &str) -> ApplyJsonResult;
}


pub struct ListContainer<'a> {
	property_system: &'a PropertySystem,

	bases: Vec<EntityBase<'a>>,
	base_name_map: HashMap<String, EntityBaseIdx>,

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
	fn add_base(&mut self, name: String, base: EntityBase<'a>) -> EntityBaseIdx {
		self.bases.push(base);
		let idx = (self.bases.len() - 1) as EntityBaseIdx;
		self.base_name_map.insert(name, idx);
		idx
	}
	fn get_base(&self, base: EntityBaseIdx) -> &EntityBase {
		&self.bases[base as usize]
	}
	fn get_base_with_name(&self, base: &str) -> Option<&EntityBase> {
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
	fn add_base(&mut self, name: String, base: EntityBase<'a>) -> EntityBaseIdx {
		self.lc.add_base(name, base)
	}
	fn get_base(&self, base: EntityBaseIdx) -> &EntityBase {
		self.lc.get_base(base)
	}
	fn get_base_with_name(&self, base: &str) -> Option<&EntityBase> {
		self.lc.get_base_with_name(base)
	}
}
