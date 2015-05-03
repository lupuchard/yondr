
use std::collections::HashMap;
use std::path::Path;
use wire;
use wire::{InTcpStream, OutTcpStream};

use network_shared::{SMessage, CMessage, Result, Error};
use resource::{Name, SessionID};
use resource;

pub fn connect(server: &str) -> Result<()> {
	let (i, o) = wire::connect_tcp(server, SMessage::limit(), CMessage::limit()).unwrap();
	let mut client = Client::new(i, o);
	try!(client.begin());
	Ok(())
}

struct Client {
	i: InTcpStream<SMessage>,
	o: OutTcpStream<CMessage>,
	rm: resource::Manager,
}
impl Client {
	fn new(i: InTcpStream<SMessage>, o: OutTcpStream<CMessage>) -> Client {
		Client { i: i, o: o, rm: resource::Manager::new(&Path::new("cache")).unwrap() }
	}
	fn begin(&mut self) -> Result<()> {

		// get welcome message
		let message = try!(self.i.recv_block().ok_or(Error::Rejected));
		let (welcome, welcome_message) = try!(message.as_welcome().ok_or(Error::WrongMessage));
		if welcome {
			info!("The server has accepted you with the message: {}", welcome_message);
		} else if !welcome {
			info!("The server has rejected you with the message: {}", welcome_message);
			return Err(Error::Rejected);
		}

		debug!("Requesting missing resources...");
		let message   = try!(self.i.recv_block().ok_or(Error::Rejected));
		let resources = try!(message.as_check_resources().ok_or(Error::WrongMessage));
		let mut resource_request = Vec::new();
		let mut  missing_resources: HashMap<SessionID, (String, String, u64)> = HashMap::new();
		for (package, name_id, v_id, session_id) in resources.into_iter() {
			let has_resource = match self.rm.package_name_to_idx(&package) {
				Some(idx) => {
					let name = Name::new(&name_id[..], idx);
					self.rm.check_resource(name, v_id, session_id).is_ok()
				},
				None => false,
			};
			if !has_resource {
				resource_request.push(session_id);
				missing_resources.insert(session_id, (package, name_id, v_id));
			}
		}

		debug!("Receiving missing resources...");
		let message = CMessage::RequestResources(resource_request);
		try!(self.o.send(&message));
		while !missing_resources.is_empty() {
			let message  = try!(self.i.recv_block().ok_or(Error::Rejected));
			let (sesh, res_type, data) = try!(message.as_resource().ok_or(Error::WrongMessage));
			let (package, name_id, v_id) = missing_resources.remove(&sesh).unwrap();
			debug!("Got {}:{}", package, name_id);
			let pidx = match self.rm.package_name_to_idx(&package) {
				Some(idx) => idx,
				None => self.rm.create_package(package).unwrap(),
			};
			try!(self.rm.create_resource(Name::new(&name_id, pidx), res_type, v_id, sesh, data));
		}

		let message = CMessage::Ready;
		try!(self.o.send(&message));

		let message = CMessage::Goodbye("thx".to_string());
		try!(self.o.send(&message));

		Ok(())
	}
}
