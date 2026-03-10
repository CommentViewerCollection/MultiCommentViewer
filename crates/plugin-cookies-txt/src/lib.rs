//! plugin-cookies-txt
//!
//! Netscape cookies.txt 形式のファイルを「ブラウザ」として登録し、
//! AddBrowser でCoreに通知する。GetCookie 要求に対しファイルからCookieを返す。

use mcv_messages::{
    AddBrowserAckPayload, AddBrowserPayload, BrowserId, Cookie as McvCookie, GetCookieAckPayload,
    GetCookiePayload, Message as McvMessage, MessageDestination, MessageSource, MessageType,
    PluginHelloPayload, RemoveBrowserPayload, SettingsDataPayload, SettingsSchemaPayload,
    UpdateSettingsPayload,
};
use plugin_abi_helper::v3::prelude::*;
use serde::{Deserialize, Serialize};
use std::path::PathBuf;
use std::time::Duration;
use uuid::Uuid;

// ============================================================================
// 定数・型定義
// ============================================================================

/// BrowserId 生成用の固定 UUID v5 名前空間
const COOKIES_TXT_NS: &str = "c7e9d3b1-a5f8-4c2e-b0d4-e1f2a3b4c5d6";

/// 登録済み cookies.txt ファイルのエントリ
#[derive(Debug, Clone, Serialize, Deserialize)]
struct CookiesTxtEntry {
    /// ユーザーが付けた名前（空の場合はパスを表示名として使用）
    name: String,
    /// cookies.txt ファイルのフルパス
    path: String,
    /// 決定論的に生成した BrowserId: "{ファイル名}_{フルパスのハッシュ}"
    browser_id: BrowserId,
}

/// プラグイン本体
#[derive(Default)]
struct CookiesTxtPlugin {
    logical_plugin_id: Uuid,
    entries: Vec<CookiesTxtEntry>,
}

// ============================================================================
// ヘルパー関数
// ============================================================================

/// ファイルパスから決定論的な BrowserId を生成する
/// 形式: "{ファイル名（拡張子なし）}_{フルパスのUUID v5ハッシュ}"
fn file_browser_id(path: &str) -> BrowserId {
    let ns = Uuid::parse_str(COOKIES_TXT_NS).unwrap();
    let hash = Uuid::new_v5(&ns, path.as_bytes());
    let filename = std::path::Path::new(path)
        .file_stem()
        .and_then(|s| s.to_str())
        .unwrap_or("cookies");
    BrowserId::new(filename, &hash.to_string())
}

/// 設定ファイルのパスを返す
/// %LOCALAPPDATA%\MultiCommentViewer\settings\cookies-txt-browsers.json
fn config_path() -> Option<PathBuf> {
    let base = dirs::data_local_dir()?;
    Some(
        base.join("MultiCommentViewer")
            .join("settings")
            .join("cookies-txt-browsers.json"),
    )
}

/// 設定ファイルからエントリを読み込む（失敗時は空）
fn load_entries() -> Vec<CookiesTxtEntry> {
    let path = match config_path() {
        Some(p) => p,
        None => return vec![],
    };
    let content = match std::fs::read_to_string(&path) {
        Ok(c) => c,
        Err(_) => return vec![],
    };
    serde_json::from_str(&content).unwrap_or_default()
}

/// エントリを設定ファイルに保存する
fn save_entries(entries: &[CookiesTxtEntry]) {
    let path = match config_path() {
        Some(p) => p,
        None => return,
    };
    if let Some(parent) = path.parent() {
        let _ = std::fs::create_dir_all(parent);
    }
    if let Ok(content) = serde_json::to_string_pretty(entries) {
        let _ = std::fs::write(&path, content);
    }
}

// ============================================================================
// CookiesTxtPlugin 実装
// ============================================================================

impl CookiesTxtPlugin {
    /// 設定スキーマ（JSON Schema）を返す
    fn settings_schema(&self) -> serde_json::Value {
        serde_json::json!({
            "type": "object",
            "title": "cookie.txt読み込み",
            "properties": {
                "entries": {
                    "type": "array",
                    "title": "登録済みファイル",
                    "items": {
                        "type": "object",
                        "properties": {
                            "name":       { "type": "string" },
                            "path":       { "type": "string" },
                            "browser_id": { "type": "string" }
                        }
                    }
                }
            }
        })
    }

    /// 現在の設定データを返す
    fn settings_data(&self) -> serde_json::Value {
        serde_json::json!({ "entries": self.entries })
    }

    /// AddBrowser メッセージを Core に送信する
    async fn send_add_browser(&self, ctx: PluginContext, entry: &CookiesTxtEntry) {
        let display_name = if entry.name.is_empty() {
            entry.path.clone()
        } else {
            entry.name.clone()
        };
        let payload = AddBrowserPayload {
            browser_id: entry.browser_id.clone(),
            browser_name: "cookies.txt".to_string(),
            display_name,
        };
        let message = McvMessage::new_request(
            MessageType::AddBrowser,
            MessageSource::Plugin {
                plugin_id: self.logical_plugin_id,
            },
            MessageDestination::Core,
            serde_json::to_value(&payload).unwrap(),
        );
        let result = ctx.send_request(message, Duration::from_secs(10)).await;
        if let Ok(response) = result {
            if response.message_type == MessageType::AddBrowserAck {
                if let Ok(_ack) = serde_json::from_value::<AddBrowserAckPayload>(response.payload) {
                    tracing::info!(
                        target: "mcv::plugin-cookies-txt",
                        browser_id = %entry.browser_id,
                        "AddBrowser Ack 受信"
                    );
                }
            }
        }
    }

    /// GetCookie リクエストに対し、該当ファイルから Cookie を読み込んで返す
    fn load_cookies_for(&self, browser_id: &BrowserId, domain: &str) -> Vec<McvCookie> {
        let entry = match self.entries.iter().find(|e| &e.browser_id == browser_id) {
            Some(e) => e,
            None => {
                tracing::warn!(
                    target: "mcv::plugin-cookies-txt",
                    browser_id = %browser_id,
                    "BrowserId に対応するエントリが見つかりません"
                );
                return vec![];
            }
        };

        let store = match cookies_txt::load_from_file(&entry.path) {
            Ok(s) => s,
            Err(e) => {
                tracing::warn!(
                    target: "mcv::plugin-cookies-txt",
                    path = %entry.path,
                    error = %e,
                    "cookies.txt の読み込みに失敗しました"
                );
                return vec![];
            }
        };

        // domain フィルタリング
        // ".example.com" でも "example.com" でも一致するよう正規化
        // 判定: クッキーのドメインが要求ドメインと一致、または要求ドメインがクッキードメインのサブドメインである
        //   OK: request=www.youtube.com, cookie=.youtube.com → "www.youtube.com".ends_with(".youtube.com")
        //   OK: request=youtube.com,     cookie=.youtube.com → exact match after strip
        //   NG: request=youtube.com,     cookie=.evil.com    → neither
        let normalized = domain.trim_start_matches('.').to_lowercase();
        store
            .cookies
            .into_iter()
            .filter(|c| {
                let d = c.domain.trim_start_matches('.').to_lowercase();
                d == normalized || normalized.ends_with(&format!(".{}", d))
            })
            .map(|c| McvCookie {
                name: c.name,
                value: c.value,
                domain: c.domain,
                path: c.path,
            })
            .collect()
    }
}

// ============================================================================
// PluginImplV3Async 実装
// ============================================================================

#[async_trait]
impl PluginImplV3Async for CookiesTxtPlugin {
    async fn on_loaded(&mut self, ctx: PluginContext) {
        self.logical_plugin_id = Uuid::new_v4();

        tracing::info!(
            target: "mcv::plugin-cookies-txt",
            plugin_id = %self.logical_plugin_id,
            "plugin-cookies-txt loaded"
        );

        // plugin-hello を Core に送信
        let hello_payload = PluginHelloPayload {
            name: "cookie.txt読み込み".to_string(),
            plugin_id: self.logical_plugin_id,
            role: vec!["browser-cookie".to_string()],
            api_version: "v3".to_string(),
            send_comment_schema: None,
        };
        let message = McvMessage::new_request(
            MessageType::PluginHello,
            MessageSource::Plugin {
                plugin_id: self.logical_plugin_id,
            },
            MessageDestination::Core,
            serde_json::to_value(&hello_payload).unwrap(),
        );
        let _ = ctx.send_request(message, Duration::from_secs(10)).await;

        tracing::info!(
            target: "mcv::plugin-cookies-txt",
            "plugin-hello 送信完了（role: browser-cookie）"
        );

        // 設定ファイルからエントリを読み込む
        self.entries = load_entries();

        tracing::info!(
            target: "mcv::plugin-cookies-txt",
            count = self.entries.len(),
            "登録済み cookies.txt ファイルを読み込みました"
        );

        // 設定スキーマ・データを Core にキャッシュ登録
        let src = MessageSource::Plugin {
            plugin_id: self.logical_plugin_id,
        };

        let schema_msg = McvMessage::new_notification(
            MessageType::SettingsSchema,
            src.clone(),
            MessageDestination::Core,
            serde_json::to_value(SettingsSchemaPayload {
                target: self.logical_plugin_id.to_string(),
                schema: self.settings_schema(),
            })
            .unwrap(),
        );
        let _ = ctx.send_notification(schema_msg).await;

        let data_msg = McvMessage::new_notification(
            MessageType::SettingsData,
            src,
            MessageDestination::Core,
            serde_json::to_value(SettingsDataPayload {
                target: self.logical_plugin_id.to_string(),
                data: self.settings_data(),
            })
            .unwrap(),
        );
        let _ = ctx.send_notification(data_msg).await;

        // 登録済みエントリを AddBrowser で通知
        for entry in self.entries.clone() {
            self.send_add_browser(ctx.clone(), &entry).await;
        }
    }

    async fn on_message(&mut self, ctx: PluginContext, msg: &[u8]) {
        let incoming: McvMessage = match serde_json::from_slice(msg) {
            Ok(m) => m,
            Err(e) => {
                tracing::error!(
                    target: "mcv::plugin-cookies-txt",
                    error = %e,
                    "メッセージのパースに失敗しました"
                );
                return;
            }
        };

        match incoming.message_type {
            // Cookie 取得リクエスト
            MessageType::GetCookie => {
                let payload: GetCookiePayload =
                    match serde_json::from_value(incoming.payload.clone()) {
                        Ok(p) => p,
                        Err(e) => {
                            tracing::warn!(
                                target: "mcv::plugin-cookies-txt",
                                error = %e,
                                "GetCookiePayload のパースに失敗しました"
                            );
                            return;
                        }
                    };

                tracing::debug!(
                    target: "mcv::plugin-cookies-txt",
                    browser_id = %payload.browser_id,
                    domain = %payload.domain,
                    "GetCookie リクエスト受信"
                );

                let cookies = self.load_cookies_for(&payload.browser_id, &payload.domain);

                tracing::debug!(
                    target: "mcv::plugin-cookies-txt",
                    cookie_count = cookies.len(),
                    "Cookie を返します"
                );

                let response = incoming.create_response(
                    MessageType::GetCookieAck,
                    serde_json::to_value(GetCookieAckPayload { cookies }).unwrap(),
                );
                let _ = ctx.send_notification(response).await;
            }

            // 設定スキーマ取得
            MessageType::GetSettingsSchema => {
                let response = incoming.create_response(
                    MessageType::SettingsSchema,
                    serde_json::to_value(SettingsSchemaPayload {
                        target: self.logical_plugin_id.to_string(),
                        schema: self.settings_schema(),
                    })
                    .unwrap(),
                );
                let _ = ctx.send_notification(response).await;
            }

            // 設定データ取得
            MessageType::GetSettings => {
                let response = incoming.create_response(
                    MessageType::SettingsData,
                    serde_json::to_value(SettingsDataPayload {
                        target: self.logical_plugin_id.to_string(),
                        data: self.settings_data(),
                    })
                    .unwrap(),
                );
                let _ = ctx.send_notification(response).await;
            }

            // 設定更新（add / remove アクション）
            MessageType::UpdateSettings => {
                let payload: UpdateSettingsPayload =
                    match serde_json::from_value(incoming.payload.clone()) {
                        Ok(p) => p,
                        Err(e) => {
                            tracing::warn!(
                                target: "mcv::plugin-cookies-txt",
                                error = %e,
                                "UpdateSettingsPayload のパースに失敗しました"
                            );
                            return;
                        }
                    };

                let action = payload.data["action"].as_str().unwrap_or("");

                match action {
                    "add" => {
                        let name = payload.data["name"].as_str().unwrap_or("").to_string();
                        let path = payload.data["path"].as_str().unwrap_or("").to_string();

                        if path.is_empty() {
                            tracing::warn!(
                                target: "mcv::plugin-cookies-txt",
                                "add アクション: path が空のため無視します"
                            );
                            return;
                        }

                        let browser_id = file_browser_id(&path);

                        // 重複チェック
                        if self.entries.iter().any(|e| e.browser_id == browser_id) {
                            tracing::info!(
                                target: "mcv::plugin-cookies-txt",
                                browser_id = %browser_id,
                                "既に登録済みのファイルです"
                            );
                            return;
                        }

                        let entry = CookiesTxtEntry {
                            name,
                            path,
                            browser_id,
                        };

                        tracing::info!(
                            target: "mcv::plugin-cookies-txt",
                            browser_id = %entry.browser_id,
                            path = %entry.path,
                            "新しい cookies.txt を登録します"
                        );

                        self.entries.push(entry.clone());
                        save_entries(&self.entries);

                        // Core のキャッシュをアクションJSONではなく正しい entries データで更新する
                        // （Core の handle_update_settings はアクションJSONをキャッシュに保存してしまうため）
                        let src = MessageSource::Plugin {
                            plugin_id: self.logical_plugin_id,
                        };
                        let data_msg = McvMessage::new_notification(
                            MessageType::SettingsData,
                            src,
                            MessageDestination::Core,
                            serde_json::to_value(SettingsDataPayload {
                                target: self.logical_plugin_id.to_string(),
                                data: self.settings_data(),
                            })
                            .unwrap(),
                        );
                        let _ = ctx.send_notification(data_msg).await;

                        // AddBrowser を Core に送信
                        self.send_add_browser(ctx, &entry).await;
                    }

                    "remove" => {
                        let browser_id_str = payload.data["browser_id"].as_str().unwrap_or("");

                        let before = self.entries.len();
                        self.entries
                            .retain(|e| e.browser_id.as_str() != browser_id_str);
                        let after = self.entries.len();

                        if before != after {
                            tracing::info!(
                                target: "mcv::plugin-cookies-txt",
                                browser_id = %browser_id_str,
                                "エントリを削除しました"
                            );
                            save_entries(&self.entries);

                            // Core のキャッシュを更新
                            let src = MessageSource::Plugin {
                                plugin_id: self.logical_plugin_id,
                            };
                            let data_msg = McvMessage::new_notification(
                                MessageType::SettingsData,
                                src.clone(),
                                MessageDestination::Core,
                                serde_json::to_value(SettingsDataPayload {
                                    target: self.logical_plugin_id.to_string(),
                                    data: self.settings_data(),
                                })
                                .unwrap(),
                            );
                            let _ = ctx.send_notification(data_msg).await;

                            // RemoveBrowser を Core に送信してブラウザ一覧を更新する
                            let browser_id = BrowserId::from_string(browser_id_str.to_string());
                            let remove_msg = McvMessage::new_notification(
                                MessageType::RemoveBrowser,
                                src,
                                MessageDestination::Core,
                                serde_json::to_value(RemoveBrowserPayload { browser_id }).unwrap(),
                            );
                            let _ = ctx.send_notification(remove_msg).await;
                        } else {
                            tracing::warn!(
                                target: "mcv::plugin-cookies-txt",
                                browser_id = %browser_id_str,
                                "削除対象のエントリが見つかりません"
                            );
                        }
                    }

                    other => {
                        tracing::warn!(
                            target: "mcv::plugin-cookies-txt",
                            action = %other,
                            "不明なアクションです"
                        );
                    }
                }
            }

            _ => {
                tracing::debug!(
                    target: "mcv::plugin-cookies-txt",
                    msg_type = ?incoming.message_type,
                    "未処理のメッセージ"
                );
            }
        }
    }

    async fn on_shutdown(&mut self, _ctx: PluginContext) {
        tracing::info!(
            target: "mcv::plugin-cookies-txt",
            "plugin-cookies-txt shutting down"
        );
    }
}

export_plugin_v3_async!(CookiesTxtPlugin);
