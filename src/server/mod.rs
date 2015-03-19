
use std::thread;
use std::path::Path;
use std::sync::Arc;
use std::net::{UdpSocket, SocketAddr};
use wire;
use wire::{InTcpStream, OutTcpStream};

use network_shared::{SMessage, CMessage, Result, Error};
use resource::ResourceManager;
use stuff::Stuff;
use world::World;


pub fn listen(port: u16) {
	let server = Server::new();
	server.listen(port);
}

#[derive(Clone)]
struct Server {
	rm: Arc<ResourceManager>,
	packages: Arc<Vec<u32>>,
	
	//world: Arc<World<'a>>,
	//stuff: Arc<Stuff>,
	//request_buffer: RWArc<HashMap<SockAddr, Vec<Request>>,
}
impl Server {
	fn new() -> Server {

		// load data
		let mut rm = ResourceManager::new(&Path::new("gamedata")).unwrap();
		let packages = rm.packages().iter().map(|p| -> u32 { p.idx }).collect();
		for &package in &packages {
			info!("Loading package {}...", rm.get_package(package).name);
			match rm.load_package(package) {
				Ok(_)  => (),
				Err(e) => error!("Problem loading package: {:?}", e),
			}
		}

		// set up world
		let mut world = World::new();
		let mut stuff = Stuff::new();
		world.load(&rm, &mut stuff);

		Server {
			rm:         Arc::new(rm),
			packages:    Arc::new(packages),
			//world:        Arc::new(world),
			//stuff:         Arc::new(stuff),
			//request_buffer: Arc::new(HashMap::new()),
		}
	}
	fn listen(&self, port: u16) {
		let (listener, _) = wire::listen_tcp(("localhost", port)).unwrap();
		let udp_sock      = UdpSocket::bind(&("localhost", port)).unwrap();

		info!("Now listening for connections...");
		for (conn, addr) in listener.into_blocking_iter() {
			let new_server = self.clone();
			let udp = match udp_sock.try_clone() {
				Ok(udp) => udp,
				Err(_)  => return,
			};
			thread::spawn(move || {
				let _ = match wire::upgrade_tcp(conn, CMessage::limit(), SMessage::limit()) {
					Ok((i, o)) => new_server.new_client(addr, udp, i, o),
					Err(_)     => return,
				};
			});
		}
	}

	fn new_client(self, sock: SocketAddr, udp: UdpSocket,
	              i: InTcpStream<CMessage>, mut o: OutTcpStream<SMessage>) -> Result<()> {

		info!("New client! Sending welcome message.");
		let message = SMessage::Welcome(true, "howdy".to_string());
		try!(o.send(&message));

		let mut required_resources = Vec::new();
		for resource in self.rm.resources() {
			required_resources.push((
				self.rm.get_package(resource.get_name().package).name.clone(),
				resource.get_name().id.clone(),
				resource.get_hash(),
				resource.get_session_id(),
			));
		}
		let message = SMessage::CheckResources(required_resources);
		try!(o.send(&message));

		let message = try!(i.recv_block().ok_or(Error::Rejected));
		let request = try!(message.as_request_resources().ok_or(Error::WrongMessage));
		for id in request {
			let res = try!(self.rm.get_resource(id).ok_or(Error::WrongMessage));
			let message = SMessage::Resource(id, res.get_type(), res.get_raw_data().clone()); // expensive clone
			debug!("Sending {:?}...", res.get_name());
			try!(o.send(&message));
		}

		let message = try!(i.recv_block().ok_or(Error::Rejected));
		let ready = message.is_ready();
		if !ready {
			return Err(Error::Rejected);
		}

		self.send_loop(sock, udp, i, o);
		Ok(())
	}

	fn recv_loop(self, sock: SocketAddr, udp: UdpSocket) -> Result<()> {
		/*
		loop {
			let (amt, src) = try!(socket.recv_from(&mut buf));
			self.requests.write()
		}
		*/
		Ok(())
	}

	fn send_loop(self, sock: SocketAddr, udp: UdpSocket,
	             i: InTcpStream<CMessage>, mut o: OutTcpStream<SMessage>) -> Result<()> {
		//loop {
		//	try!(socket.send_to(buf, &sock));
		//}
		Ok(())
	}
}
