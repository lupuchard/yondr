
use std::collections::{HashMap, VecMap};
use std::{io, result};
use std::path::Path;
use std::string::FromUtf8Error;

use luajit_rs as lua;

use world::container::{EntityContainer, ListContainer, GridContainer};
use world::lua::LuaManager;
use property_system::{Val, PropertySystem, EntityIdx, EntityBase};
use resource;
use util::{simplify_str, Stuff, Vec3};

macro_rules! err {
	($($arg:tt)*) => ({
		panic!("Unimplemented error {}", format!($($arg)*));
		Error::Todo
	})
}
macro_rules! try_cont {
	($expr:expr) => (match $expr {
		Ok(val) => val,
		Err(err) => {
			error!("{:?}", err);
			continue;
		}
	})
}

pub struct World<'a> {
	container_protos: Vec<ContainerProto>,
	containers: Vec<Box<EntityContainer<'a> + 'a>>,
	container_name_map: HashMap<String, u16>,

	current_entity_idx: EntityIdx,
	entity_idx_map: HashMap<EntityIdx, (u16, u16)>,

	lua_manager: LuaManager<'a>,
}

enum ContainerType {
	List,
	Grid(Vec3<u16>),
}
struct ContainerProto {
	ty: ContainerType,
	name: String,
	bases: Vec<(String, Vec<(String, Val)>)>,
}

impl<'a> World<'a> {
	pub fn new() -> World<'a> {
		World {
			container_protos: Vec::new(),
			containers: Vec::new(),
			container_name_map: HashMap::new(),
			current_entity_idx: 0,
			entity_idx_map: HashMap::new(),
			lua_manager: LuaManager::new(),
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

	pub fn update(&mut self, seconds: f32) {

	}

	/// See data_format.txt
	pub fn load(&mut self, rm: &resource::Manager, stuff: &'a mut Stuff) {
		let mut packages: VecMap<PackageData> = VecMap::new();

		// get all the base tomls
		for res in rm.resources() {

			// get the package for this resource
			let pidx = res.get_name().package as usize;
			let package = if packages.contains_key(&pidx) {
				packages.get_mut(&pidx).unwrap()
			} else {
				packages.insert(res.get_name().package as usize, PackageData::new());
				packages.get_mut(&(res.get_name().package as usize)).unwrap()
			};

			match res.get_type() {
				resource::Type::Lua => {
					match &res.get_name().id[..] {
						"deps"  => package.deps  = Some(res.get_path()),
						"world" => package.world = Some(res.get_path()),
						_ => (),
					}
				},
				_ => (),
			}
		}

		let keys: Vec<usize> = packages.keys().collect();
		for pidx in keys {
			let res = self.load_package(rm, stuff, &mut packages, pidx);
			if res.is_err() {
				error!("Failed to load package: {:?}", res.err().unwrap());
				return;
			}
		}

		// finalize containers from prototypes
		for container_proto in self.container_protos.drain() {
			let property_system = stuff.get(&container_proto.name).unwrap();

			// create container
			let mut container: Box<EntityContainer> = match container_proto.ty {
				ContainerType::List       => Box::new(ListContainer::new(property_system)),
				ContainerType::Grid(size) => Box::new(GridContainer::new(property_system, size)),
			};

			// load bases
			for (name, props) in container_proto.bases {
				let mut base = EntityBase::new(property_system);
				for (prop_name, val) in props {
					let property = try_cont!(property_system.with_name(&prop_name).ok_or(
						Error::UnknownProp(prop_name, container_proto.name.clone())));

					// assert correct type
					if !property.get_type().is_same_type(&val)
						{ try_cont!(Err(Error::WrongType(val.clone(), property.get_type()))); }

					base.set_value(property.get_index(), val);
				}
				container.add_base(name, base);
			}
			self.containers.push(container);
		}
	}

	fn load_package(&mut self, rm: &resource::Manager, stuff: &mut Stuff,
	                packages: &mut VecMap<PackageData>, pidx: usize) -> Result<()> {
		// could be already loaded
		if packages[pidx].loaded { return Ok(()); }
		packages[pidx].loaded = true;

		info!("Loading package '{}'.", rm.get_package(pidx as u32).name);

		let l = lua::State::new();
		l.load_file("data/sandbox.lua").ok().expect("sandbox.lua failed to load").call_(());
		let mut sandbox: lua::TableRef = l.get("env").expect("sandbox.lua has no env");

		// load dependencies
		sandbox.set("deps", lua::EmptyTable);
		if packages[pidx].deps.is_some() {
			let deps_table: lua::TableRef = sandbox.get("deps").unwrap();
			let func = try!(l.load_file(packages[pidx].deps.unwrap()));
			deps_table.set_as_env(&func);
			func.call_(());
			for (dep_name, value) in deps_table.iter::<String, bool>() {
				if !value { continue; }
				let dependency_idx = rm.package_name_to_idx(&dep_name[..]);
				let dependency_idx = try!(dependency_idx.ok_or(Error::NotPackage(dep_name)));
				try!(self.load_package(rm, stuff, packages, dependency_idx as usize));
			}
		}

		// load world
		sandbox.set("world", lua::EmptyTable);
		if packages[pidx].world.is_some() {
			let world_table: lua::TableRef = sandbox.get("world").unwrap();
			let func = try!(l.load_file(packages[pidx].world.unwrap()));
			world_table.set_as_env(&func);
			func.call_(());
			try!(self.load_world(stuff, world_table));
		}

		Ok(())
	}

	fn load_world(&mut self, stuff: &mut Stuff, world: lua::TableRef) -> Result<()> {
		for (name, table) in world.iter::<String, lua::TableRef>() {
			let name  = simplify_str(&name[..]);
			let ty    = try!(self.load_container_type(&name[..], &table));
			let bases = match table.get::<lua::TableRef>("bases") {
				Some(bases_table) => try!(self.load_bases(&bases_table)),
				None => Vec::new(),
			};
			match table.get("properties") {
				Some(props_table) => try!(self.load_props(&name[..], &props_table, stuff)),
				None => (),
			}
			self.container_name_map.insert(name.clone(), self.container_protos.len() as u16);
			self.container_protos.push(ContainerProto { ty: ty, name: name, bases: bases });
		}
		Ok(())
	}

	fn load_container_type(&self, name: &str, table: &lua::TableRef) -> Result<ContainerType> {
		let ty = table.get::<String>("type").ok_or(Error::ContainerLacksType(name.to_string()));
		match &simplify_str(&try!(ty)[..])[..] {
			"list" => Ok(ContainerType::List),
			"grid" => match table.get::<Vec<u16>>("size") {
				Some(size) => match size.len() {
					2 => Ok(ContainerType::Grid(Vec3::new(size[0], size[1], 1))),
					3 => Ok(ContainerType::Grid(Vec3::new(size[0], size[1], size[2]))),
					_ => Err(Error::InvalidGridSize(name.to_string())),
				},
				None  => Err(Error::InvalidGridSize(name.to_string())),
			},
			tn => Err(Error::InvalidContainerType(tn.to_string(), name.to_string())),
		}
	}

	fn load_bases(&self, table: &lua::TableRef) -> Result<Vec<(String, Vec<(String, Val)>)>> {
		let mut bases = Vec::new();
		for (name, base_table) in table.iter::<String, lua::TableRef>() {
			let mut base = Vec::new();
			for (prop, val) in base_table.iter::<String, lua::AnyRef>() {
				let val = match val {
					lua::AnyRef::Bool(v) => Val::Bool(v),
					lua::AnyRef::Num(v)  => Val::Float(v),
					lua::AnyRef::Str(v) => Val::Str(Box::new(v.as_str().to_string())),
					_ => return Err(Error::InvalidLiteral(format!("{:?}", val), prop, name)),
				};
				base.push((prop, val));
			}
			bases.push((name, base));
		}
		Ok(bases)
	}

	fn load_props(&self, cname: &str, table: &lua::TableRef, stuff: &mut Stuff) -> Result<()> {
		let mut property_system = PropertySystem::new();
		for prop_table in table.array_iter::<lua::TableRef>() {
			let name = match prop_table.get::<String>("name") {
				Some(str) => str,
				None => return Err(Error::PropertyLacksName(cname.to_string())),
			};
			let type_name = prop_table.get::<String>("type");
			let tn = type_name.ok_or(Error::PropertyLacksType(name.clone(), cname.to_string()));
			let mut val = match &simplify_str(&try!(tn)[..])[..] {
				"bool"   => Val::Bool(false),
				"float"  => Val::Float(0.),
				"int"    => Val::Int(0),
				"string" => Val::Str(Box::new(String::new())),
				ty => return Err(Error::InvalidType(ty.to_string(), name, cname.to_string())),
			};
			if prop_table.get::<lua::AnyRef>("default").is_some() {
				val = match val {
					Val::Bool(_)  => Val::Bool(try!(prop_table.get("default").ok_or(
						Error::InvalidDefault("bool", name.clone())))),
					Val::Float(_) => Val::Float(try!(prop_table.get("default").ok_or(
						Error::InvalidDefault("float", name.clone())))),
					Val::Int(_)   => Val::Int(try!(prop_table.get("default").ok_or(
						Error::InvalidDefault("int", name.clone())))),
					Val::Str(_)   => Val::Str(Box::new(try!(prop_table.get("default").ok_or(
						Error::InvalidDefault("str", name.clone()))))),
				};
			}
			property_system.add(name, val);
		}
		stuff.insert(cname.to_string(), property_system);
		Ok(())
	}
}

struct PackageData<'a> {
	deps:   Option<&'a Path>,
	world:  Option<&'a Path>,
	loaded: bool,
}
impl<'a> PackageData<'a> {
	pub fn new() -> PackageData<'a> {
		PackageData { deps: None, world: None, loaded: false }
	}
}

#[derive(Debug)]
pub enum Error {
	Todo,
	UnknownProp(String, String),            // (property name, container name)
	WrongType(Val, Val),                    // (is, should be)
	NotPackage(String),                     // (dependency name)
	ContainerLacksType(String),             // (container name)
	InvalidGridSize(String),                // (container name)
	InvalidContainerType(String, String),   // (type name, container name)
	InvalidLiteral(String, String, String), // (literal, property name, base name)
	PropertyLacksName(String),              // (container name)
	PropertyLacksType(String, String),      // (property name, container name)
	InvalidType(String, String, String),    // (type name, property name, container name)
	InvalidDefault(&'static str, String),   // (type, property name)

	Io(io::Error),
	Utf8(FromUtf8Error),
	Res(resource::Error),
	Lua(lua::Error),
}
impl From<io::Error> for Error {
	fn from(err: io::Error) -> Error {
		Error::Io(err)
	}
}
impl From<FromUtf8Error> for Error {
	fn from(err: FromUtf8Error) -> Error {
		Error::Utf8(err)
	}
}
impl From<resource::Error> for Error {
	fn from(err: resource::Error) -> Error {
		Error::Res(err)
	}
}
impl From<lua::Error> for Error {
	fn from(err: lua::Error) -> Error {
		Error::Lua(err)
	}
}

pub type Result<T> = result::Result<T, Error>;
