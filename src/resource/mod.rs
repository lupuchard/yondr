
use std::collections::{HashMap, VecMap};
use std::path::{Path, PathBuf};
use std::io::{Read, Write};
use std::ffi::OsStr;
use std::{result, fs, io};
use rustc_serialize::json;

use util::{simplify_str, hash};

pub type SessionID = u16;

#[derive(PartialEq, Eq, PartialOrd, Ord, Debug, Hash, Clone)]
pub struct Name {
	pub id: String,
	pub package: u32,
}
impl Name {
	pub fn new(id: &str, package: u32) -> Name {
		Name { id: simplify_str(id), package: package }
	}
}

#[derive(Debug, PartialEq, Eq, PartialOrd, Ord, Hash, Copy, Clone, RustcDecodable, RustcEncodable)]
pub enum Type { Unknown, Lua }
impl Type {
	pub fn get_extension(&self) -> &'static str {
		match *self {
			Type::Unknown => "what",
			Type::Lua     => "lua",
		}
	}
	pub fn from_extension(extension: &str) -> Type {
		match extension {
			"lua" => Type::Lua,
			_     => Type::Unknown,
		}
	}
}

/// A resource yes.
pub struct Resource {
	ty: Type,
	path: PathBuf,
	name: Name,
	session_id: SessionID,
	raw_data: Vec<u8>,
	hash: u64,
}
impl Resource {
	pub fn new(name: Name, ty: Type, path: PathBuf, hash: u64, id: SessionID,
	           mut raw: Vec<u8>) -> Resource {
		raw.shrink_to_fit();
		Resource {
			path: path,
			name: name,
			session_id: id,
			ty: ty,
			raw_data: raw,
			hash: hash,
		}
	}

	/// The raw bytes that this data is for.
	pub fn get_raw_data<'a>(&'a self) -> &'a Vec<u8> {
		&self.raw_data
	}

	/// The type of resource.
	pub fn get_type(&self) -> Type {
		self.ty
	}

	/// Where this resource is located in them files.
	pub fn get_path(&self) -> &Path {
		&self.path
	}

	/// The simplified name and package of the resource.
	pub fn get_name(&self) -> &Name {
		&self.name
	}

	/// Used as version id.
	pub fn get_hash(&self) -> u64 {
		self.hash
	}

	/// The ID this resource was given for this session.
	pub fn get_session_id(&self) -> SessionID {
		self.session_id
	}
}

pub struct Package {
	pub name: String,
	pub path: PathBuf,
	pub idx: u32,
	pub metadata: json::Json,
	pub resources: HashMap<String, PathBuf>,
}
pub struct Manager {
	directory: &'static Path,

	packages: Vec<Package>,
	package_name_map: HashMap<String, u32>,

	resources: Vec<Resource>,
	name_map:       HashMap<Name, usize>,
	session_id_map: VecMap<usize>,
	next_session_id: SessionID,
}
impl Manager {
	/// Creates a new resource manager for the given directory.
	/// May return an error if the resource manager has trouble scanning the given directory.
	pub fn new(directory: &'static Path) -> Result<Manager> {
		let mut rm = Manager {
			directory:        directory,
			packages:         Vec::new(),
			package_name_map: HashMap::new(),
			resources:        Vec::new(),
			session_id_map:   VecMap::new(),
			next_session_id:  1,
			name_map:         HashMap::new(),
		};
		try!(rm.scan_packages());
		Ok(rm)
	}
	fn scan_packages(&mut self) -> Result<()> {
		let _ = fs::create_dir(self.directory);
		for entry in try!(fs::read_dir(self.directory)) {
			let entry_path = try!(entry).path();
			let name = {
				let filename = entry_path.file_name().unwrap();
				simplify_str(try!(filename.to_str().ok_or(Error::Other("Bad directory name."))))
			};
			let package = Package {
				name:      name,
				path:      entry_path,
				idx:       self.packages.len() as u32,
				metadata:  json::Json::Null,
				resources: HashMap::new(),
			};
			self.package_name_map.insert(package.name.clone(), package.idx);
			self.packages.push(package);
		}
		Ok(())
	}

	pub fn packages(&self) -> &Vec<Package> {
		&self.packages
	}
	pub fn get_package(&self, package: u32) -> &Package {
		&self.packages[package as usize]
	}
	pub fn package_name_to_idx(&self, package: &str) -> Option<u32> {
		match self.package_name_map.get(&simplify_str(package)[..]) {
			Some(v) => Some(*v),
			None    => None,
		}
	}
	pub fn create_package(&mut self, name: String) -> Result<u32> {
		let mut dir = self.directory.to_path_buf();
		dir.push(&name[..]);
		try!(fs::create_dir(&dir));

		let pidx = self.packages.len() as u32;
		let package = Package {
			name: name.clone(),
			path: dir,
			idx: pidx,
			metadata: json::Json::Object(json::Object::new()),
			resources: HashMap::new(),
		};
		self.package_name_map.insert(name, package.idx);
		self.packages.push(package);
		try!(self.store_metadata(pidx));
		Ok(pidx)
	}

	/// Loads all the resources from the given package into the Manager.
	/// This generates both their session id and version id.
	/// (server function)
	pub fn load_package(&mut self, package: u32) -> Result<()> {
		if package as usize >= self.packages.len() {
			return Err(Error::InvalidPackage);
		}

		for entry in try!(fs::walk_dir(&self.packages[package as usize].path)) {
			let entry_path = match entry {
				Ok(e)  => e,
				Err(_) => continue,
			}.path();
			let session_id = self.next_session_id;

			let mut data = Vec::new();
			let mut file = try!(fs::File::open(&entry_path));
			try!(file.read_to_end(&mut data));

			match self.new_resource(entry_path, package, hash(&data), session_id, data) {
				Ok(_)  => (),
				Err(e) => match e {
					Error::Ignored => continue,
					_ => return Err(e),
				},
			};
		}
		Ok(())
	}

	/// Tells the resource manager to attempt to load the resource with the given name.
	/// Possible errors:
	///   Error::ResourceDoesNotExist if there is no resource with the given name.
	///   Error::ResourceDoesNotMatchChecksum if the resource exists but does not match the given version_id
	///   Any other error if there is a problem loading the resource.
	/// If the resource exists, successfully loads, and matches the given version_id, then it
	/// is added to the resource manager and given the provided session id.
	/// (client function)
	pub fn check_resource(&mut self, res: Name, version_id: u64, id: SessionID) -> Result<()> {
		let package = res.package as usize;
		if package as usize >= self.packages.len() {
			return Err(Error::InvalidPackage);
		}

		if self.packages[package].resources.is_empty() {
			try!(self.scan_resources(package as u32));
		}

		if (match self.packages[package].metadata.as_object() {
			Some(o) => match o.get(&res.id) {
				Some(j) => match j.as_u64() {
					Some(v) => v,
					None => return Err(Error::IncorrectVersion),
				},	None => return Err(Error::IncorrectVersion),
			},		None => return Err(Error::IncorrectVersion),
		}) != version_id  { return Err(Error::IncorrectVersion); }

		let path = match self.packages[package].resources.get(&res.id) {
			Some(p) => p.clone(),
			None => return Err(Error::ResourceDoesNotExist),
		};

		let mut data = Vec::new();
		let mut file = try!(fs::File::open(&path));
		try!(file.read_to_end(&mut data));

		try!(self.new_resource(path, res.package, version_id, id, data));
		Ok(())
	}
	fn scan_resources(&mut self, package: u32) -> Result<()> {
		try!(self.load_metadata(package));

		for entry in try!(fs::walk_dir(&self.packages[package as usize].path)) {
			let entry_path = match entry {
				Ok(e)  => e,
				Err(_) => continue,
			}.path();
			let goddammit_rust = entry_path.clone();
			let stem = goddammit_rust.file_stem().unwrap();
			let name = try!(stem.to_str().ok_or(Error::Other("Bad filename.")));
			self.packages[package as usize].resources.insert(simplify_str(name), entry_path);
		}
		Ok(())
	}

	fn load_metadata(&mut self, package: u32) -> Result<()> {
		if self.packages[package as usize].metadata != json::Json::Null {
			return Ok(());
		}

		let mut metapath = self.packages[package as usize].path.clone();
		metapath.push("_metadata");
		metapath.set_extension("json");
		self.packages[package as usize].metadata = match fs::File::open(&metapath) {
			Ok(mut f) => try!(json::Json::from_reader(&mut f)),
			Err(_)    => return Err(Error::Other("Can't open package metadata.")),
		};
		Ok(())
	}
	fn store_metadata(&mut self, package: u32) -> Result<()> {
		let mut metapath = self.packages[package as usize].path.clone();
		metapath.push("_metadata");
		metapath.set_extension("json");
		let mut file = try!(fs::File::create(&metapath));
		try!(file.write_all(&self.packages[package as usize].metadata.to_string().as_bytes()));
		Ok(())
	}

	pub fn create_resource(&mut self, res: Name, res_type: Type,
	                       version_id: u64, id: SessionID, data: Vec<u8>) -> Result<()> {
		let pidx = res.package as usize;

		// write data to file
		let mut filepath = self.packages[pidx].path.clone();
		filepath.push(&res.id[..]);
		filepath.set_extension(res_type.get_extension());
		let mut file = try!(fs::File::create(&filepath));
		try!(file.write_all(&data));

		// update metadata
		try!(self.load_metadata(res.package));
		let jsonval = json::Json::U64(version_id);
		self.packages[pidx].metadata.as_object_mut().unwrap().insert(res.id, jsonval);
		try!(self.store_metadata(res.package));

		try!(self.new_resource(filepath, res.package, version_id, id, data));
		Ok(())
	}

	pub fn resources(&self) -> &Vec<Resource> {
		&self.resources
	}

	pub fn get_resource(&self, session_id: SessionID) -> Option<&Resource> {
		match self.session_id_map.get(&(session_id as usize)) {
			Some(idx) => Some(&self.resources[*idx]),
			None => None,
		}
	}
	pub fn get_resource_with_name(&self, name: &Name) -> Option<&Resource> {
		match self.name_map.get(name) {
			Some(idx) => Some(&self.resources[*idx]),
			None => None,
		}
	}

	fn new_resource(&mut self, path: PathBuf, package: u32,
	                ver: u64, id: SessionID, data: Vec<u8>) -> Result<&Resource> {

		let name = {
			let stem = path.file_stem().unwrap();
			let name = try!(stem.to_str().ok_or(Error::Other("Bad filename.")));

			// files that start with underscore are ignored
			if name.chars().next().unwrap() == '_' {
				return Err(Error::Ignored);
			}

			Name::new(name, package)
		};

		// create resource
		let dammit_rust = path.clone();
		let extension = dammit_rust.extension().unwrap_or(OsStr::new("")).to_str().unwrap();
		let res_type = Type::from_extension(extension);
		let res      = Resource::new(name.clone(), res_type, path, ver, id, data);

		// store resource
		match self.name_map.insert(name, self.resources.len()) {
			Some(_) => return Err(Error::ResourceWithNameAlreadyExists),
			None    => (),
		};
		self.session_id_map.insert(id as usize, self.resources.len());
		self.resources.push(res);

		// increment session id
		if self.next_session_id <= id {
			self.next_session_id = id + 1;
		}

		Ok(&self.resources[self.resources.len() - 1])
	}

	/// Call when done with loading to clear unnecessary package and string mapping data.
	pub fn clean(&mut self) {
		self.packages.clear();
		self.package_name_map.clear();
		self.name_map.clear();
	}
}

#[derive(Debug)]
pub enum Error {
	Other(&'static str),
	Io(io::Error),
	Json(json::ParserError),
	IsShallowResource,
	InvalidPackage,
	ResourceDoesNotExist,
	IncorrectVersion,
	ResourceWithNameAlreadyExists,
	Ignored,
}

impl From<io::Error> for Error {
	fn from(err: io::Error) -> Error {
		Error::Io(err)
	}
}
impl From<json::ParserError> for Error {
	fn from(err: json::ParserError) -> Error {
		Error::Json(err)
	}
}

pub type Result<T> = result::Result<T, Error>;

