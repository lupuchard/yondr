
use std::{io, error, result};
use std::string::FromUtf8Error;
use bincode;
use wire::SizeLimit;

use resource::{SessionID, ResourceType};
use resource;

/// Messages the Server can send to the Client over TCP.
#[derive(Debug, PartialEq, Eq, PartialOrd, Ord, Hash, Clone, RustcDecodable, RustcEncodable)]
pub enum SMessage {

	/// First message. Tells the Client if they are rejected or not and a reason why.
	/// To be used for banlists or when Server is full.
	Welcome(bool, String),

	/// A list of all the required resources as (package, name id, version id, session id)
	CheckResources(Vec<(String, String, u64, SessionID)>),

	/// The data for a particular resource.
	Resource(SessionID, ResourceType, Vec<u8>),

	Chat(String),
	Goodbye(String),
}
impl SMessage {
	pub fn limit() -> SizeLimit { SizeLimit::Infinite }

	// this is so fucking dumb
	// i cant believe enums dont already come with these methods
	pub fn as_welcome(self)         -> Option<(bool, String)> {
		match self { SMessage::Welcome(b, s)     => Some((b, s)),    _ => None }
	}
	pub fn as_check_resources(self) -> Option<Vec<(String, String, u64, SessionID)>> {
		match self { SMessage::CheckResources(v) => Some(v),         _ => None }
	}
	pub fn as_resource(self)        -> Option<(SessionID, ResourceType, Vec<u8>)> {
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
	pub fn limit() -> SizeLimit { SizeLimit::Bounded(1024) }

	pub fn as_request_resources<'a>(self)->Option<Vec<SessionID>> {
		match self { CMessage::RequestResources(v) => Some(v),      _ => None }
	}
	pub fn is_ready(self) -> bool {
		match self { CMessage::Ready => true, _ => false }
	}
}
/*impl<'a> SMessage<'a> {
	pub fn parse(message: &'a str) -> Result<Message<'a>> {
		let v: Vec<&str> = message.splitn(2, ':').collect();
		if v.len() < 2 { return Err(Error::ParseError("Missing colon.")); }

		match v[0] {
			"WELCOME" => Message::parse_welcome(v[1]),
			_ => Err(Error::ParseError("Unknown message type.")),
		}
	}
	fn parse_welcome(message: &'a str) -> Result<Message<'a>> {
		let v: Vec<&str> = message.splitn(2, '(').collect();
		if v.len() < 1 { return Err(Error::ParseError("Empty message.")); }

		let status = match v[0].trim_matches(' ') {
			"yes" => true,
			"no"  => false,
			_ => return Err(Error::ParseError("Unknown welcome status.")),
		};
		match v.len() {
			1 => Ok(Message::Welcome(status, "")),
			_ => Ok(Message::Welcome(status, v[1].trim_right_matches(')'))),
		}
	}

	pub fn as_welcome(self) -> Result<(bool, &'a str)> {
		match self {
			Message::Welcome(b, s) => Ok((b, s)),
			//_ => Err(Error::UnexpectedMessageType),
		}
	}
}*/

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
impl error::FromError<io::Error> for Error {
	fn from_error(err: io::Error) -> Error {
		Error::Io(err)
	}
}
impl error::FromError<FromUtf8Error> for Error {
	fn from_error(err: FromUtf8Error) -> Error {
		Error::Utf8(err)
	}
}
impl error::FromError<resource::Error> for Error {
	fn from_error(err: resource::Error) -> Error {
		Error::Res(err)
	}
}
impl error::FromError<bincode::DecodingError> for Error {
	fn from_error(_: bincode::DecodingError) -> Error {
		Error::Bincode
	}
}
impl error::FromError<bincode::EncodingError> for Error {
	fn from_error(_: bincode::EncodingError) -> Error {
		Error::Bincode
	}
}

