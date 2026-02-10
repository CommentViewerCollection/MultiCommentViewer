//! mcv-log-schema
//!
//! ロギング/トレーシングに関する共通型定義とユーティリティ

mod types;
mod stacktrace;
mod visitor;
mod profile;

pub use types::{SourceLocation, StackFrame, LogLevel};
pub use stacktrace::capture_stacktrace;
pub use visitor::MessageVisitor;
pub use profile::get_build_profile;
