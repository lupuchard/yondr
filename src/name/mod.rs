
use util::simplify_str;

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
