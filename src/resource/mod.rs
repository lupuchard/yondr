
use std::collections::{HashMap, VecMap};
use std::path::{Path, PathBuf};
use std::io::{Read, Write};
use std::ffi::OsStr;
use std::{error, result, fs, io};
use std::str::from_utf8;
use rustc_serialize::json;

use util::{simplify_str, hash};
use name::Name;

pub type SessionID = u16;

#[derive(Clone, Debug, PartialEq, PartialOrd)]
pub enum ResourceData { Unknown(Vec<u8>), Json(json::Json) }

#[derive(Debug, PartialEq, Eq, PartialOrd, Ord, Hash, Clone, RustcDecodable, RustcEncodable)]
pub enum ResourceType { Unknown         , Json }

/// A resource yes.
pub struct Resource {
	path: PathBuf,
	name: Name,
	session_id: SessionID,
	data: ResourceData,
	raw_data: Option<Vec<u8>>,
	hash: u64,
}
impl Resource {

	/// The data & type of resource.
	pub fn get_data(&self) -> &ResourceData {
		&self.data
	}

	pub fn get_raw_data<'a>(&'a self) -> &'a Vec<u8> {
		match self.data {
			ResourceData::Unknown(ref d) => return d,
			_ => (),
		}
		self.raw_data.as_ref().unwrap()
	}

	pub fn get_type(&self) -> ResourceType {
		match self.data {
			ResourceData::Unknown(_) => ResourceType::Unknown,
			ResourceData::Json(_)    => ResourceType::Json,
		}
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

	pub fn new(name: Name, path: PathBuf, hash: u64, id: SessionID,
	           raw: Option<Vec<u8>>, data: ResourceData) -> Resource {
		Resource {
			path: path,
			name: name,
			session_id: id,
			data: data,
			raw_data: raw,
			hash: hash,
		}
	}

	pub fn new_unknown(name: Name, path: PathBuf, hash: u64, id: SessionID,
	                   data: Vec<u8>) -> Resource {
		Resource::new(name, path, hash, id, None, ResourceData::Unknown(data))
	}
	pub fn new_json(name: Name, path: PathBuf, hash: u64, id: SessionID,
	                data: Vec<u8>) -> Result<Resource> {
		let json = try!(json::Json::from_str(match from_utf8(&data) {
			Ok(k) => k,
			Err(_) => return Err(Error::Other("Json file not utf8.")),
		}));
		Ok(Resource::new(name, path, hash, id, Some(data), ResourceData::Json(json)))
	}
}

pub struct Package {
	pub name: String,
	pub path: PathBuf,
	pub idx: u32,
	pub metadata: json::Json,
	pub resources: HashMap<String, PathBuf>,
}
pub struct ResourceManager {
	directory: &'static Path,

	packages: Vec<Package>,
	package_name_map: HashMap<String, u32>,
	
	resources: Vec<Resource>,
	name_map:       HashMap<Name, usize>,
	session_id_map: VecMap<usize>,
	next_session_id: SessionID,
}
impl ResourceManager {
	/// Creates a new resource manager for the given directory.
	/// May return an error if the resource manager has trouble scanning the given directory.
	pub fn new(directory: &'static Path) -> Result<ResourceManager> {
		let mut rm = ResourceManager {
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

	/// Returns an iterator through all the packages in this ResourceManager.
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

	/// Loads all the resources from the given package into the ResourceManager.
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

	pub fn create_resource(&mut self, res: Name, res_type: ResourceType,
	                       version_id: u64, id: SessionID, data: Vec<u8>) -> Result<()> {
		let pidx = res.package as usize;

		let mut filepath = self.packages[pidx].path.clone();
		filepath.push(&res.id[..]);
		filepath.set_extension(match res_type {
			ResourceType::Unknown => "what",
			ResourceType::Json    => "json",
		});

		let mut file = try!(fs::File::create(&filepath));
		try!(file.write_all(&data));

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
	pub fn get_resource_by_name(&self, name: &Name) -> Option<&Resource> {
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

		// load resource
		let goddammit_rust = path.clone();
		let extension = goddammit_rust.extension().unwrap_or(OsStr::from_str("")).to_str().unwrap();
		
		let res = match extension {
			"json" => try!(Resource::new_json(   name.clone(), path, ver, id, data)),
			_      =>      Resource::new_unknown(name.clone(), path, ver, id, data),
		};

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
	Parser(json::ParserError),
	IsShallowResource,
	InvalidPackage,
	ResourceDoesNotExist,
	IncorrectVersion,
	ResourceWithNameAlreadyExists,
	Ignored,
}
/*impl error::Error for Error {
	fn description(&self) -> &str {
		match self {
			Error::Unknown              => "how hapen",
			Error::Io(err)              => err.description(),
			Error::Parser(err)          => err.description(),
			Error::IsShallowResource    => "why u call from_data?", 
			Error::InvalidPackage       => "invalid package",
			Error::ResourceDoesNotExist => "resource does not exist",
			Error::IncorrectVersion     => "incorrect version",
			Error::ResourceWithNameAlreadyExists => "resource with name already exists",
		}
	}
	fn cause(&self) -> Option<&Error> {
		match self.cause {
			Error::Io(err)     => err.cause(),
			Error::Parser(err) => err.cause(),
			_ => None,
		}
	}
}*/ // TODO implement Error
impl error::FromError<io::Error> for Error {
	fn from_error(err: io::Error) -> Error {
		Error::Io(err)
	}
}
impl error::FromError<json::ParserError> for Error {
	fn from_error(err: json::ParserError) -> Error {
		Error::Parser(err)
	}
}
/*impl fmt::Display for Error {
	fn fmt(&self, fmt: &mut fmt::Formatter) -> result::Result<(), fmt::Error> {
		fmt.write_fmt("{:?}", self);
	}
}*/

pub type Result<T> = result::Result<T, Error>;

