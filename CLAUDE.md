# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

MultiCommentViewer (mcv) は複数の配信プラットフォームのコメントを同時に集約して表示するコメントビューアです。
Tauri + React フロントエンドと Rust バックエンドで構成され、プラグインシステムによって YouTube Live / Twitch / ニコニコ生放送 / TwitCasting / Kick / IRC 等に対応しています。

## Development Commands

### 通常の開発（xtask 使用）

```bash
# ローカルデバッグインストール（LOCALAPPDATA へ配置）
cargo xtask install --dir "C:/Users/<user>/AppData/Local/MultiCommentViewer" --channel alpha

# 特定プラグインだけインストール
cargo xtask install --dir "..." --channel alpha --plugin youtube --plugin twitch

# 配布用 ZIP 生成（全プラグイン）
cargo xtask dist --channel alpha

# 特定プラグインのみ ZIP 化
cargo xtask pack --plugin bouyomi --channel beta
```

チャンネルは `alpha`（trace ログ、全機能）・`beta`（info ログ）・`stable`（error ログ）。
**ログレベルは `--features alpha/beta/stable` で制御する。`RUST_LOG` は使わない。**

### 素の Cargo コマンド

```bash
# コード検査
cargo check

# ビルド
cargo build
cargo build --release

# テスト
cargo test --workspace

# フォーマット（コミット前に必ず実行）
cargo fmt -p <crate>
```

### フロントエンド

```bash
cd apps/mcv
npm install
npm run tauri dev   # ホットリロード開発
npm run build       # フロントエンドのみビルド
```

## Architecture Overview

### Core Concepts

- **core**: UI 管理・プラグイン管理・コメント集約
- **plugin**: 各配信プラットフォーム向けのモジュール（DLL）
- **plugin-host**: actix ベースのプラグイン分離実行環境
- **connection**: 1 つの配信 URL への接続インスタンス

### Message Flow (Actor Model)

actix のアクターモデルで core ↔ plugin が通信します:

1. **Plugin Registration**: `plugin-hello` → `plugin-added` broadcast
2. **Connection Lifecycle**: `add-connection` → `connection-added` → `connect` → `connected`
3. **Comment Streaming**: Plugins send `comment-received` messages to core
4. **Disconnection**: `disconnect` → `disconnected`
5. **Comment Posting**: `send-comment` → post to platform → result via existing messages

すべてのメッセージは kebab-case 命名で以下を含みます:
- `type`: メッセージ種別（例: "add-connection"）
- `src`: 送信元（Core または Plugin with UUID）
- `dst`: 送信先（Core または Plugin with UUID）
- `request_id`: オプションの UUID（リクエスト/レスポンス対応）
- `timestamp`: Unix タイムスタンプ
- `payload`: メッセージ固有の JSON 値

### Crate Structure

```
crates/
├── mcv-messages/              # メッセージ型定義（MessageType, payload）
├── mcv-common/                # 共通ユーティリティ・定数
├── mcv-plugin-interface/      # Plugin トレイト定義（旧 v1/v2 インターフェース）
├── mcv-plugin-loader/         # DLL プラグインローダー（v1/v2）
├── mcv-plugin-loader-v3/      # DLL プラグインローダー（v3, libloading）
├── mcv-plugin-telemetry/      # プラグインテレメトリ（tracing → Core への自動転送）
├── mcv-log-schema/            # ログ共通型（SourceLocation, StackFrame, LogLevel）
├── mcv-log-core/              # ロギングシステム（SQLite + リモート送信）
├── mcv-settings-core/         # 設定管理
├── mcv-updater/               # 自動アップデート
├── mcv-core/                  # Core ロジック（CoreActor, PluginManager, ConnectionManager）
│
├── plugin-abi-helper/         # プラグイン ABI ヘルパー（v2, v3 インターフェース）
├── export-plugin-v3-macros/   # v3 プラグインエクスポート proc-macro
│
├── plugin-dummy/              # テスト・開発用ダミープラグイン
├── plugin-sample-v2/          # v2 ABI サンプルプラグイン
├── plugin-sample-v3/          # v3 ABI サンプルプラグイン
│
├── youtube-live-lib/          # YouTube Live API クライアント
├── plugin-youtube-live/       # YouTube Live プラグイン
│
├── twitch-lib/                # Twitch API クライアント
├── plugin-twitch/             # Twitch プラグイン
│
├── nicolive-lib/              # ニコニコ生放送 API クライアント
├── plugin-nicolive/           # ニコニコ生放送プラグイン
│
├── twicas-lib/                # TwitCasting API クライアント
├── twicas-secret-extractor/   # TwitCasting 認証情報取得
├── plugin-twicas/             # TwitCasting プラグイン
│
├── kick-lib/                  # Kick.com API クライアント
├── plugin-kick/               # Kick.com プラグイン
│
├── irc-lib/                   # IRC クライアントライブラリ
├── plugin-irc/                # IRC プラグイン
│
├── plugin-bouyomi/            # 棒読みちゃん TTS プラグイン
│
├── chromium-cookie-lib/       # Chromium 系ブラウザ共通 Cookie 取得（DPAPI/AES-GCM）
├── cookies_txt_reader/        # cookies.txt 形式読み取り
├── plugin-chrome-cookie/      # Chrome Cookie プラグイン
├── plugin-edge-cookie/        # Edge Cookie プラグイン
├── plugin-firefox-cookie/     # Firefox Cookie プラグイン
├── plugin-cookies-txt/        # cookies.txt プラグイン
│
├── plugin-exe-manager/        # EXE プラグインマネージャー（旧）
└── plugin-exe-manager-v3/     # EXE プラグインマネージャー v3（WebSocket, port 28901）

apps/
├── mcv/                       # メイン Tauri アプリケーション
├── log-viewer/                # ログビューア（スタンドアロン Web アプリ）
└── exe-plugin-sample/         # EXE プラグインデバッグツール（Tauri GUI）
```

### Plugin ABI Versions

#### v3（現在の標準）

`PluginImplV3Async` トレイトを実装し、`export_plugin_v3_async!` マクロでエクスポートします:

```rust
use plugin_abi_helper::v3::prelude::*;
use mcv_messages::{Message as McvMessage, MessageType, /* ... */};

#[derive(Default)]
struct MyPlugin {
    plugin_id: PluginId,
}

#[async_trait]
impl PluginImplV3Async for MyPlugin {
    async fn on_loaded(&mut self, ctx: PluginContext) {
        // plugin-hello を送信して core に登録
    }

    async fn on_message(&mut self, ctx: PluginContext, msg: &[u8]) {
        // 必ず McvMessage としてデシリアライズし MessageType でマッチ
        let incoming: McvMessage = match serde_json::from_slice(msg) {
            Ok(m) => m,
            Err(e) => { tracing::error!(%e, "parse error"); return; }
        };
        match incoming.message_type {
            MessageType::Connect => { /* ... */ }
            MessageType::Disconnect => { /* ... */ }
            _ => {}
        }
    }

    async fn on_shutdown(&mut self, ctx: PluginContext) {
        // クリーンアップ
    }
}

export_plugin_v3_async!(MyPlugin);
```

**NG パターン（`serde_json::Value` の文字列マッチ）:**
```rust
// NG: シリアライズ形式の不一致でサイレントに失敗する
let value: serde_json::Value = serde_json::from_slice(msg)?;
match value["message_type"].as_str() {
    Some("get-cookie") => { /* ... */ }  // ← 危険
}
```

#### v2（旧スタイル、既存プラグインに残存）

`PluginImplV2` トレイトを実装します。新規プラグインは v3 を使うこと。

### Cargo Features（ログレベル・機能フラグ）

```toml
# apps/mcv/src-tauri/Cargo.toml
[features]
alpha  = ["comment-search", "mcv-log-core/alpha", ...]  # trace ログ・全機能有効
beta   = ["mcv-log-core/beta", ...]                     # info ログ
stable = ["mcv-log-core/stable", ...]                   # error ログのみ
```

- `cargo xtask install --channel alpha` で自動的に `--features alpha` が付く
- **`RUST_LOG=trace` 等の環境変数は使わない**
- alpha-only 機能を追加するには `Cargo.toml` の `alpha = [...]` に feature 名を追加するだけ

フロントエンド feature 連動:
- xtask がチャンネルに対応する `CARGO_FEATURE_<NAME>=1` を npm ビルドへ自動的に渡す
- `vite.config.ts` がこれを読み取りビルド時定数を注入（例: `__IS_SEARCH_ENABLED__`）

### Actix Message Naming Conflict

actix の `Message` トレイトと `mcv_messages::Message` 構造体が名前衝突します:
```rust
use actix::prelude::*;
use mcv_messages::{Message as McvMessage, MessageSource, MessageDestination, MessageType, *};
```

### Plugin Logging System (mcv-plugin-telemetry)

```rust
use mcv_tracing;

// on_loaded で初期化
mcv_tracing::init_tracing(
    self.plugin_id,
    Arc::clone(&host),
    env!("CARGO_PKG_VERSION"),
    "info",
)?;

// 構造化ログ
tracing::debug!(connection_id = %conn_id, "Processing message");
tracing::error!(error = %e, "Failed");
```

- Core へ LogEntry メッセージとして自動転送
- error レベルはスタックトレース自動取得
- mcv-log-core（SQLite + リモート送信）と統合

### EXE Plugin System

```
Core (CoreActor)
  ↕
plugin-exe-manager-v3 (DLL)
  ↕ WebSocket (JSON, port 28901)
EXE plugin (独立プロセス)
```

- port 28901 が使用中なら 28902+ を自動選択
- `%APPDATA%\MultiCommentViewer\plugins\` の `plugin.json` をスキャン
- 環境変数: `MCV_WEBSOCKET_PORT`, `MCV_WEBSOCKET_URL`

### Comment Posting System

`send-comment`: Core → Plugin でコメントテキストを送信。
DummyPlugin では `disconnect` / `pause` / `resume` / `rate <sec>` / `comment <user> <text>` 等のコマンドとして再利用。

### Cookie Plugins

ブラウザから Cookie を取得するプラグイン群:
- `plugin-chrome-cookie`: Chrome プロファイル検出 → AddBrowser / GetCookie 対応
- `plugin-edge-cookie`: Edge 対応（chromium-cookie-lib 共用）
- `plugin-firefox-cookie`: Firefox 対応
- `plugin-cookies-txt`: cookies.txt 形式ファイル対応

Cookie 取得フロー:
1. Cookie プラグインが起動時に `AddBrowser` を Core へ送信
2. 接続時に Core/プラットフォームプラグインから `GetCookie` が届く
3. Cookie プラグインがブラウザ DB を読み取り `GetCookieAck` で返答

### Connection Flow

1. ユーザーが "接続を追加" → `add_connection` → デフォルト名 (#1, #2, ...) で接続作成
2. ユーザーが "接続" ボタン → `connect` → プラグインがコメント取得開始
3. ユーザーが "切断" ボタン → `disconnect` → プラグインが停止
4. 接続のリネーム・削除（切断時のみ）が可能

### Tauri Integration

- `apps/mcv/src-tauri/src/main.rs`: CoreActor 初期化・プラグイン登録
- Tauri commands: `add_connection`, `remove_connection`, `rename_connection`, `connect`, `disconnect`, `get_connections`, `send_comment`
- Frontend events: `comment-received`, `connected`, `disconnected`

### Frontend Structure

React + TypeScript + Tailwind CSS:
- `src/App.tsx`: メインコンポーネント
- `@tauri-apps/api` でバックエンド通信
- `my-dataview` パッケージ（`packages/`）: 仮想スクロール付き高パフォーマンスコメント表示
- コメント投稿セクション（接続セレクタ + 入力フィールド + 送信ボタン）

## mcv-messages 設計原則（重要）

`crates/mcv-messages` はすべての配信サイト・プラグインが共通で使う型定義クレートです。
**特定プラットフォーム固有の概念を追加してはならない。**

### NG の例

```rust
// NG: YouTube 固有の概念
AuthorDelete { external_channel_id: String }
```

### OK の例

```rust
// OK: 汎用的な表現
MessageDeleteAll { user_id: String }
```

判断基準: **「YouTube 以外（Twitch, ニコ生等）でも意味をなすか？」**
- YES → `mcv-messages` に追加
- NO → プラグイン側で吸収し汎用的な表現に変換して送出

`CommentRow`（フロントエンド DTO）も同様。プラットフォーム固有概念は持ち込まない。
また `CommentRow` は「コメントまたはコメント類似のもの」のみ保持する。
制御メッセージ（全ユーザー削除等）は別の Tauri イベントとして扱う。

## xtask コマンド詳細

`cargo xtask` がビルド・パッケージング・配布の全工程を管理します（詳細は `xtask/README.md`）。
プラグインのメタデータは `tools/plugins.json` で管理されています。

| コマンド | 用途 |
| ------- | ---- |
| `cargo xtask build [--release]` | ワークスペース全体ビルド（npm 変更も自動検知） |
| `cargo xtask install --dir PATH [--channel] [--plugin ID...]` | ローカルデバッグインストール |
| `cargo xtask pack --plugin ID [--channel]` | プラグイン単体 ZIP 化 |
| `cargo xtask dist [--channel] [--plugin ID...]` | 配布用バンドル ZIP 生成 |

## Test Coverage

```bash
# 全テスト
cargo test --workspace

# クレート指定
cargo test --package mcv-messages
cargo test --package plugin-dummy
cargo test --package mcv-core

# 統合テストのみ
cargo test --test command_system_test
```

## Future Extension Points

詳細は `docs/specifications.md` を参照:
- プラグイン配布システム
- コメント遅延調整
- 各種 UI 改善
