/// ログクエリパラメータ
#[derive(serde::Deserialize)]
pub(crate) struct LogQueryParams {
    pub(crate) levels: Option<Vec<String>>,
    pub(crate) search: Option<String>,
    pub(crate) from: Option<i64>,
    pub(crate) to: Option<i64>,
    pub(crate) limit: Option<usize>,
    pub(crate) offset: Option<usize>,
}

/// ビルドプロファイル情報
#[derive(serde::Serialize)]
pub(crate) struct BuildProfileInfo {
    pub(crate) profile: String,
}

/// ログを取得
#[tauri::command]
pub(crate) async fn get_logs(
    params: LogQueryParams,
) -> Result<Vec<mcv_log_core::LogEntry>, String> {
    let storage = mcv_log_core::get_storage();

    // チャンネルに応じたレベルフィルタを適用
    let build_profile = crate::get_build_profile();
    let effective_levels = match build_profile {
        "stable" | "beta" => Some(vec![mcv_log_core::LogLevel::Error]),
        "alpha" => params.levels.as_ref().map(|levels| {
            levels
                .iter()
                .filter_map(|s| match s.to_lowercase().as_str() {
                    "trace" => Some(mcv_log_core::LogLevel::Trace),
                    "debug" => Some(mcv_log_core::LogLevel::Debug),
                    "info" => Some(mcv_log_core::LogLevel::Info),
                    "warn" => Some(mcv_log_core::LogLevel::Warn),
                    "error" => Some(mcv_log_core::LogLevel::Error),
                    _ => None,
                })
                .collect()
        }),
        _ => None,
    };

    let filters = mcv_log_core::storage::LogQueryFilters {
        levels: effective_levels,
        search: params.search,
        from: params.from,
        to: params.to,
        limit: params.limit,
        offset: params.offset,
    };

    let storage_guard = storage
        .lock()
        .map_err(|e| format!("Failed to lock storage: {}", e))?;

    storage_guard
        .query_logs(filters)
        .map_err(|e| format!("Failed to query logs: {}", e))
}

/// ビルドプロファイル情報を取得
#[tauri::command]
pub(crate) async fn get_build_profile_info() -> Result<BuildProfileInfo, String> {
    Ok(BuildProfileInfo {
        profile: crate::get_build_profile().to_string(),
    })
}
