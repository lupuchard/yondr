
use log;
use log::{LogRecord, LogLevel, SetLoggerError, LogLevelFilter, LogMetadata};
use ansi_term::Colour;

pub fn init() -> Result<(), log::SetLoggerError> {
	log::set_logger(|max_log_level| {
		max_log_level.set(LogLevelFilter::Debug);
		Box::new(Logger)
	})
}

struct Logger;

impl Logger {

	#[cfg(ndebug)]
	fn log_debug(&self, record: &LogRecord) { }

	#[cfg(not(ndebug))]
	fn log_debug(&self, record: &LogRecord) {
		println!("[{}]", record.args());
	}

	fn log_info(&self, record: &LogRecord) {
		println!("{}", record.args());
	}

	fn log_warn(&self, record: &LogRecord) {
		println!("{} {}", Colour::Yellow.paint("Warning:"), record.args());
	}

	fn log_error(&self, record: &LogRecord) {
		println!("{} {}", Colour::Red.bold().paint("Error:"), record.args());
	}
}

impl log::Log for Logger {
	fn enabled(&self, metadata: &LogMetadata) -> bool {
		metadata.level() <= LogLevel::Debug
	}

	fn log(&self, record: &LogRecord) {
		match record.level() {
			LogLevel::Trace => (),
			LogLevel::Debug => self.log_debug(record),
			LogLevel::Info  => self.log_info(record),
			LogLevel::Warn  => self.log_warn(record),
			LogLevel::Error => self.log_error(record),
		}
	}
}
