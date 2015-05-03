
use std::{io, result};
use std::string::FromUtf8Error;
use bincode;
use wire::SizeLimit;

use resource::SessionID;
use resource;

pub const LOGICAL_FPS: i32 = 60;
pub const NETWORK_FPS: i32 = 20;

/// Messages the Server can send to the Client over TCP.
#[derive(Debug, PartialEq, Eq, PartialOrd, Ord, Hash, Clone, RustcDecodable, RustcEncodable)]
pub enum SMessage {

	/// First message. Tells the Client if they are rejected or not and a reason why.
	/// To be used for banlists or when Server is full.
	Welcome(bool, String),

	/// A list of all the required resources as (package, name id, version id, session id)
	CheckResources(Vec<(String, String, u64, SessionID)>),

	/// The data for a particular resource.
	Resource(SessionID, resource::Type, Vec<u8>),

	Chat(String),
	Goodbye(String),
}
impl SMessage {
	/// The maximum size of a serialized SMessage.
	pub fn limit() -> SizeLimit { SizeLimit::Infinite }

	// i cant believe enums dont already come with these methods
	pub fn as_welcome(self)         -> Option<(bool, String)> {
		match self { SMessage::Welcome(b, s)     => Some((b, s)),    _ => None }
	}
	pub fn as_check_resources(self) -> Option<Vec<(String, String, u64, SessionID)>> {
		match self { SMessage::CheckResources(v) => Some(v),         _ => None }
	}
	pub fn as_resource(self)        -> Option<(SessionID, resource::Type, Vec<u8>)> {
		match self { SMessage::Resource(s, t, v) => Some((s, t, v)), _ => None }
	}
}

/// Messages the Client can send to the Server over TCP.
#[derive(Debug, PartialEq, Eq, PartialOrd, Ord, Hash, Clone, RustcDecodable, RustcEncodable)]
pub enum CMessage {
	/// A list of all the resources among those mentioned in CheckResources
	/// that the Client does not have and needs the Server to send to them
	RequestResources(Vec<SessionID>),

	/// Client has all resources and is ready to start game.
	Ready,

	Chat(String, String),
	Goodbye(String),
}
impl CMessage {
	/// The maximum size of a serialized CMessage.
	pub fn limit() -> SizeLimit { SizeLimit::Bounded(1024) }

	pub fn as_request_resources<'a>(self)->Option<Vec<SessionID>> {
		match self { CMessage::RequestResources(v) => Some(v),      _ => None }
	}
	pub fn is_ready(self) -> bool {
		match self { CMessage::Ready => true, _ => false }
	}
}

pub type Result<T> = result::Result<T, Error>;

#[derive(Debug)]
pub enum Error {
	Io(io::Error),
	Utf8(FromUtf8Error),
	Res(resource::Error),
	Bincode,
	WrongMessage,
	Rejected,
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
impl From<bincode::DecodingError> for Error {
	fn from(_: bincode::DecodingError) -> Error {
		Error::Bincode
	}
}
impl From<bincode::EncodingError> for Error {
	fn from(_: bincode::EncodingError) -> Error {
		Error::Bincode
	}
}

