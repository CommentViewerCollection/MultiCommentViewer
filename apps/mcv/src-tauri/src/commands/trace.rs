#[derive(Debug, Clone, serde::Deserialize)]
pub(crate) struct FrontendTraceSource {
    pub(crate) file: String,
    pub(crate) line: Option<u32>,
    pub(crate) column: Option<u32>,
    pub(crate) function: Option<String>,
}

/// フロントエンド診断ログを tracing に転送する
#[tauri::command]
pub(crate) async fn frontend_trace(
    level: String,
    message: String,
    fields: Option<serde_json::Value>,
    source: Option<FrontendTraceSource>,
) -> Result<(), String> {
    let source_file = source
        .as_ref()
        .map(|s| s.file.as_str())
        .unwrap_or("unknown");
    let source_line = source.as_ref().and_then(|s| s.line).unwrap_or(0) as u64;
    let source_column = source.as_ref().and_then(|s| s.column).unwrap_or(0) as u64;
    let source_module = source
        .as_ref()
        .and_then(|s| s.function.as_deref())
        .unwrap_or("frontend");

    match level.to_ascii_lowercase().as_str() {
        "trace" => {
            tracing::trace!(
                target: "mcv::frontend",
                frontend_fields = ?fields,
                frontend_source_file = source_file,
                frontend_source_line = source_line,
                frontend_source_column = source_column,
                frontend_source_module = source_module,
                "{message}"
            );
        }
        "debug" => {
            tracing::debug!(
                target: "mcv::frontend",
                frontend_fields = ?fields,
                frontend_source_file = source_file,
                frontend_source_line = source_line,
                frontend_source_column = source_column,
                frontend_source_module = source_module,
                "{message}"
            );
        }
        "warn" => {
            tracing::warn!(
                target: "mcv::frontend",
                frontend_fields = ?fields,
                frontend_source_file = source_file,
                frontend_source_line = source_line,
                frontend_source_column = source_column,
                frontend_source_module = source_module,
                "{message}"
            );
        }
        "error" => {
            tracing::error!(
                target: "mcv::frontend",
                frontend_fields = ?fields,
                frontend_source_file = source_file,
                frontend_source_line = source_line,
                frontend_source_column = source_column,
                frontend_source_module = source_module,
                "{message}"
            );
        }
        _ => {
            tracing::info!(
                target: "mcv::frontend",
                frontend_fields = ?fields,
                frontend_source_file = source_file,
                frontend_source_line = source_line,
                frontend_source_column = source_column,
                frontend_source_module = source_module,
                "{message}"
            );
        }
    }
    Ok(())
}
