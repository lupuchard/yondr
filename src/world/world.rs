
use std::collections::{HashMap, VecMap};
use rustc_serialize::json::Json;

use util::json::ApplyJsonResult;
use world::container::{EntityContainer, ListContainer};
use property_system::entity::EntityIdx;
use resource::ResourceManager;
use stuff::Stuff;

pub struct World<'a> {
	containers: Vec<Box<EntityContainer<'a> + 'a>>,
	container_name_map: HashMap<String, u16>,

	current_entity_idx: EntityIdx,
	entity_idx_map: HashMap<EntityIdx, (u16, u16)>,
}

/*enum ContainerType {
	List,
	Grid(Vec3),
}
struct ContainerPrototype<'a> {
	name: String,
	filename: &'a str,
	ctype: ContainerType,
}
impl<'a> ContainerPrototype<'a> {
	pub fn new_list(name: String, filename: &'a str) -> ContainerPrototype<'a> {
		ContainerPrototype { name: name, filename: filename, ctype: ContainerType::List }
	}
	pub fn new_grid(name: String, filename: &'a str, size: Vec3) -> ContainerPrototype<'a> {
		ContainerPrototype { name: name, filename: filename, ctype: ContainerType::Grid(size) }
	}
}*/

impl<'a> World<'a> {
	pub fn new() -> World<'a> {
		World {
			containers: Vec::new(),
			container_name_map: HashMap::new(),
			current_entity_idx: 0,
			entity_idx_map: HashMap::new(),
		}
	}

	pub fn get_container(&mut self, idx: u16) -> &mut EntityContainer<'a> {
		&mut *self.containers[idx as usize]
	}
	pub fn get_container_with_name(&mut self, name: &str) -> Option<&mut EntityContainer<'a>> {
		match self.container_name_map.get(name) {
			Some(v) => Some(&mut *self.containers[*v as usize]),
			None    => None,
		}
	}

	/// Loads world data from the resource manager.
	/// It looks at all the jsons and does stuff.
	/// Any time a particular value is expected to be an object or a list, a string
	/// with a file name can be supplied instead, and the object or list is expected
	/// to be in that file instead.
	/// Most names are not case sensitive.
	/// When referencing properties, bases or files from other packages, the names should 
	/// be prefixed with the package name and a colon. Like `"doors:height"`, for example.
	///
	/// The starting point files are *deps.json* and *world.json*.
	///
	/// # deps.json
	/// A list of the packages that should be loaded before the one this file is in.
	/// ### Example
	///
	///     ["doors", "cats", "body parts"]
	///
	/// # world.json
	/// A map of entity containers to add to the world.
	/// ### Container values:
	/// * `"type"`: Either `"list"` or `"grid"`.
	/// * `"properties"`: A list of properties.
	/// * `"bases"`: A list of entity bases.
	/// * `"size"`: Only for grid containers. A length 3 list of integers for the dimensions of the grid.
	/// ### Property values:
	/// * `"name"`: The name of the property.
	/// * `"type"`: Either `"int"`, `"float"`, `"bool"` or `"string"`.
	/// * `"default"`: The default value for that property. This is optional, and if none is supplied
	///              then int, float, bool and string properties default to `0`, `0`, `false` and `""` respectively.
	/// ### Entity base values:
	/// * "name": The name of the base.
	/// * "values": A map of property names to values.
	///
	/// ### Example
	///
	///     {
	///       "items": { "type": "list", "properties": "item_props.json", "bases": "item_bases.json" },
	///       "phys":  {
	///         "type": "list",
	///         "properties": [
	///           { "name": "x", "type": "float" },
	///           { "name": "y", "type": "float" },
	///           { "name": "xvel", "type": "float" },
	///           { "name": "yvel", "type": "float" }
	///         ],
	///         "bases": [{ "name": "sanic", "values": { "xvel": 9999999999, "yvel": 9999999999 } }]
	///       ]}
	///     }
	///
	/// 
	pub fn load(&mut self, resources: &ResourceManager, stuff: &mut Stuff) -> ApplyJsonResult {
		let mut packages: VecMap<(&Json, &Json)> = VecMap::new();
		

		/*let json: json::Json = resources.get("world");
		let data = json_parse!(data, "world");

		let container_prototypes: Vec<ContainerPrototype> = Vec::new();
		let containers = data.search("containers");
		if containers.is_some() {
			match containers.unwrap().as_object() {
				Some(o) => self.load_json_containers(o, stuff, &containers),
				None    => warn!("Containers is not an object."),
			};
		} else { warn!("World has no containers object."); }

		for proto in container_prototypes {
			load_property_system(res.get(proto.filename), stuff.get(proto.name));
		}*/



		// create containers

		ApplyJsonResult::Success
	}

	/*fn load_containers(&mut self, data: &json::Object, stuff: &mut Stuff,
	                         container_prototypes: &mut Vec<ContainerPrototype>) {
		for (name, info) in data {

			let info_obj = match info.as_object() {
				Some(o) => o,
				None => warn_cont!("Container '{}' is not an object.", name),
			};

			let fil = json_str!(info_obj, "file", warn_cont!("Container '{}' lacks file.", name));
			
			match json_str!(info_obj, "type", warn_cont!("Container '{}' has no type.", name)) {
				"list" => container_prototypes.push(ContainerPrototype::new_list(name, fil)),
				"grid" => {
					let size = match info_obj.get("dim") {
						Some(d) => Vec2::from_json(d),
						None => warn_cont!("Container '{}' requires size 'cus a grid.", name),
					};
					container_prototypes.push(ContainerPrototype::new_grid(name, fil, size));
				},
				n => warn_cont!("'{}' is not a valid container type.", n),
			}

			stuff.store(name, PropertySystem::new());
		}
	}*/
}

/*#[cfg(test)]
mod test {
	use super::*;

	const JSON0: &'static str = r#"
{
	"for": "world",
	"containers": {
		"Phys":  { "type": "list", "file": "phys_cont" },
		"Item":  { "type": "list", "file": "item_cont" },
		"Local": { "type": "list", "file": "local_cont" },
		"Grid":  { "type": "grid", "file": "grid_cont", "size": [10, 10, 10] }
	},
}
"#;
	#[test]
	fn test_json() {
		let mut world = World::new();
		world.apply_json(JSON);
		assert!(world.get_container_with_name("Item").is_some());
		assert!(world.get_container_with_name("Butt").is_none());
	}
}*/
