
#![feature(core, fs_walk, collections, custom_derive)]
#![cfg_attr(test, allow(dead_code))]

extern crate anymap;
extern crate ansi_term;
extern crate num;
extern crate bincode;
extern crate wire;
#[macro_use] extern crate log;
extern crate rustc_serialize;

extern crate luajit_rs;

#[macro_use] pub mod util;
pub mod logger;
pub mod property_system;
pub mod resource;
// pub mod world;
// pub mod stuff;

pub mod network_shared;
pub mod client;

use std::env;

fn main() {
	logger::init().unwrap_or_else(|_| println!("Logger failed to initalize!"));
	info!("This is client!");
	let mut args = env::args();
	args.next();
	let server: String = args.next().expect("Please supply a server.");
	match client::connect(&server[..]) {
		Ok(_)  => (),
		Err(e) => error!("{:?}", e),
	}
}
