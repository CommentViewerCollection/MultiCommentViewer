//! chromium-cookie-lib
//!
//! Chrome・Edge など Chromium ベースのブラウザに共通するクッキー取得ロジック。
//!
//! # 提供する機能
//! - Windows DPAPI による復号 (`decrypt_dpapi`)
//! - AES-256-GCM または DPAPI によるクッキー値の復号 (`decrypt_cookie_value`)
//! - Chromium の Local State からマスターキーを取得 (`load_master_key`)
//! - クッキー DB ファイルのパス解決 (`resolve_cookie_db_path`)
//! - SQLite クッキー DB からクッキーを取得 (`query_cookies`)

pub mod profiles;

use std::fs;
use std::path::{Path, PathBuf};

use aes_gcm::aead::{Aead, KeyInit};
use aes_gcm::{Aes256Gcm, Nonce};
use base64::engine::general_purpose::STANDARD as BASE64_STANDARD;
use base64::Engine;
use mcv_messages::Cookie as McvCookie;
use rusqlite::Connection;
use uuid::Uuid;

// ============================================================================
// DPAPI 復号
// ============================================================================

/// Windows DPAPI でデータを復号する。
/// 非 Windows ビルドでは常に `None` を返す。
#[cfg(windows)]
pub fn decrypt_dpapi(data: &[u8]) -> Option<Vec<u8>> {
    use windows_sys::Win32::Foundation::LocalFree;
    use windows_sys::Win32::Security::Cryptography::{CryptUnprotectData, CRYPT_INTEGER_BLOB};

    let mut in_buf = data.to_vec();
    let in_blob = CRYPT_INTEGER_BLOB {
        cbData: in_buf.len() as u32,
        pbData: in_buf.as_mut_ptr(),
    };
    let mut out_blob = CRYPT_INTEGER_BLOB {
        cbData: 0,
        pbData: std::ptr::null_mut(),
    };

    let ok = unsafe {
        CryptUnprotectData(
            &in_blob,
            std::ptr::null_mut(),
            std::ptr::null_mut(),
            std::ptr::null_mut(),
            std::ptr::null_mut(),
            0,
            &mut out_blob,
        )
    };
    if ok == 0 || out_blob.pbData.is_null() {
        return None;
    }

    let result =
        unsafe { std::slice::from_raw_parts(out_blob.pbData, out_blob.cbData as usize).to_vec() };
    unsafe {
        LocalFree(out_blob.pbData as *mut core::ffi::c_void);
    }
    Some(result)
}

#[cfg(not(windows))]
pub fn decrypt_dpapi(_data: &[u8]) -> Option<Vec<u8>> {
    None
}

// ============================================================================
// クッキー値の復号
// ============================================================================

/// Chromium のクッキー値を復号する。
///
/// - `v10`/`v11` プレフィックスがある場合は AES-256-GCM（`master_key` 必須）
/// - それ以外は DPAPI フォールバック（古い形式）
pub fn decrypt_cookie_value(encrypted_value: &[u8], master_key: Option<&[u8]>) -> Option<String> {
    if encrypted_value.starts_with(b"v10") || encrypted_value.starts_with(b"v11") {
        let key = master_key?;
        if encrypted_value.len() < 3 + 12 + 16 {
            return None;
        }
        let nonce = Nonce::from_slice(&encrypted_value[3..15]);
        let ciphertext = &encrypted_value[15..];
        let cipher = Aes256Gcm::new_from_slice(key).ok()?;
        let plaintext = cipher.decrypt(nonce, ciphertext).ok()?;
        return String::from_utf8(plaintext).ok();
    }

    let decrypted = decrypt_dpapi(encrypted_value)?;
    String::from_utf8(decrypted).ok()
}

// ============================================================================
// マスターキー取得
// ============================================================================

/// Chromium ブラウザの `Local State` ファイルからマスターキーを読み込む。
///
/// `user_data_dir` には `Local State` が存在するディレクトリを渡す。
/// 例: `%LOCALAPPDATA%\Google\Chrome\User Data`
pub fn load_master_key(user_data_dir: &Path) -> Option<Vec<u8>> {
    let local_state_path = user_data_dir.join("Local State");
    let local_state = fs::read_to_string(local_state_path).ok()?;
    let json: serde_json::Value = serde_json::from_str(&local_state).ok()?;
    let encrypted_key_b64 = json["os_crypt"]["encrypted_key"].as_str()?;
    let mut encrypted_key = BASE64_STANDARD.decode(encrypted_key_b64).ok()?;
    if encrypted_key.starts_with(b"DPAPI") {
        encrypted_key.drain(0..5);
    }
    decrypt_dpapi(&encrypted_key)
}

// ============================================================================
// クッキー DB パス解決
// ============================================================================

/// プロファイルディレクトリ内のクッキー DB ファイルのパスを返す。
///
/// 新形式 (`Network/Cookies`) を優先し、旧形式 (`Cookies`) にフォールバックする。
pub fn resolve_cookie_db_path(profile_dir: &Path) -> Option<PathBuf> {
    let new_path = profile_dir.join("Network").join("Cookies");
    if new_path.exists() {
        return Some(new_path);
    }
    let old_path = profile_dir.join("Cookies");
    if old_path.exists() {
        return Some(old_path);
    }
    None
}

// ============================================================================
// SQLite クッキー取得
// ============================================================================

/// クッキー DB から指定ドメインのクッキーを取得・復号して返す。
///
/// DB はロックされている可能性があるため、内部で一時ファイルにコピーしてから開く。
/// `master_key` が `None` の場合は DPAPI フォールバックのみ試みる。
pub fn query_cookies(db_path: &Path, domain: &str, master_key: Option<&[u8]>) -> Vec<McvCookie> {
    let temp_db_path =
        std::env::temp_dir().join(format!("mcv_chromium_cookie_{}.db", Uuid::new_v4()));
    if fs::copy(db_path, &temp_db_path).is_err() {
        return vec![];
    }

    let conn = match Connection::open(&temp_db_path) {
        Ok(c) => c,
        Err(_) => {
            let _ = fs::remove_file(&temp_db_path);
            return vec![];
        }
    };

    let normalized_domain = domain.trim().trim_start_matches('.').to_lowercase();
    let exact = normalized_domain.clone();
    let dotted = format!(".{}", normalized_domain);
    let like = format!("%.{}", normalized_domain);

    let mut stmt = match conn.prepare(
        "SELECT host_key, name, value, encrypted_value, path
         FROM cookies
         WHERE host_key = ?1 OR host_key = ?2 OR host_key LIKE ?3",
    ) {
        Ok(s) => s,
        Err(_) => {
            let _ = fs::remove_file(&temp_db_path);
            return vec![];
        }
    };

    let rows = stmt.query_map([exact, dotted, like], |row| {
        let host_key: String = row.get(0)?;
        let name: String = row.get(1)?;
        let value: String = row.get(2)?;
        let encrypted_value: Vec<u8> = row.get(3)?;
        let path: String = row.get(4)?;
        Ok((host_key, name, value, encrypted_value, path))
    });

    let mut cookies = vec![];
    if let Ok(iter) = rows {
        for row in iter.flatten() {
            let (host_key, name, value, encrypted_value, path) = row;
            let resolved_value = if value.is_empty() {
                decrypt_cookie_value(&encrypted_value, master_key).unwrap_or_default()
            } else {
                value
            };
            if resolved_value.is_empty() {
                continue;
            }
            cookies.push(McvCookie {
                name,
                value: resolved_value,
                domain: host_key,
                path,
            });
        }
    }

    let _ = fs::remove_file(&temp_db_path);
    cookies
}
