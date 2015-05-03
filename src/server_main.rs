#![feature(core, fs_walk, collections, custom_derive)]
#![cfg_attr(test, allow(dead_code))]

extern crate anymap;
extern crate ansi_term;
extern crate num;
extern crate bincode;
extern crate wire;
extern crate time;
#[macro_use] extern crate log;
extern crate rustc_serialize;

extern crate luajit_rs;

#[macro_use] pub mod util;
pub mod logger;
pub mod property_system;
pub mod resource;
pub mod world;

pub mod network_shared;
pub mod server;

use std::env;
use std::str::FromStr;

fn main() {
	logger::init().unwrap_or_else(|_| println!("Logger failed to initalize!"));
	info!("This is server!");
	let mut args = env::args();
	args.next();
	let portstr = args.next().expect("Please supply a port.");
	let port: u16 = FromStr::from_str(&portstr).ok().expect("Port must be a 16-bit number");
	server::listen(port);
}
