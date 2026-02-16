//! mcv-log-schema
//!
//! ロギング/トレーシングに関する共通型定義とユーティリティ

mod profile;
mod stacktrace;
mod types;
mod visitor;

pub use profile::get_build_profile;
pub use stacktrace::capture_stacktrace;
pub use types::{LogLevel, SourceLocation, StackFrame};
pub use visitor::MessageVisitor;
