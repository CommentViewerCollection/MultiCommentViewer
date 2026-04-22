# MCV ロギングシステム

## 概要

mcv 本体とプラグインの両方からログを収集し、ローカル SQLite に保存してAPIサーバーへ送信する構造化ロギングシステム。スタンドアロンのログビューアアプリで閲覧・分析できる。

### 主な機能

- **構造化ログ**: ファイル名・関数名・行番号・スタックトレース・コンテキスト情報を自動記録
- **プラグイン対応**: プラグインから `log-entry` メッセージでログを送信可能
- **バックグラウンド送信**: 5 分ごとのバッチ送信、エラー時は即時送信
- **panic 復旧**: 前回クラッシュ時の `panic.log` を起動時に SQLite へ自動インポート
- **ログビューア**: ローカル DB またはAPIサーバーからログを取得・表示・削除できる Tauri アプリ
- **ビルドプロファイル対応**: alpha / beta / stable でログレベルを制御

---

## アーキテクチャ

### コンポーネント

```
┌──────────────────────────────────────────────────────┐
│                    mcv Application                   │
│  ┌────────────┐        ┌────────────────────────┐    │
│  │ CoreActor  │◄──────►│   Plugin (DLL)         │    │
│  │  (actix)   │        │  mcv-plugin-telemetry  │    │
│  └─────┬──────┘        └──────────┬─────────────┘    │
│        │ tracing                  │ log-entry msg     │
│        ▼                          ▼                   │
│  ┌─────────────────────────────────────────────┐      │
│  │              mcv-log-core                   │      │
│  │  ┌──────────────┐  ┌──────────────────────┐ │      │
│  │  │  Subscriber  │  │   LogSenderActor      │ │      │
│  │  │  (tracing)   │  │   (actix, 5分バッチ)  │ │      │
│  │  └──────┬───────┘  └──────────┬────────────┘ │      │
│  │         └──────────┬──────────┘              │      │
│  │                    ▼                          │      │
│  │      ┌──────────────────────────────┐        │      │
│  │      │   SQLite (logs.db)           │        │      │
│  │      │   %LOCALAPPDATA%\           │        │      │
│  │      │   MultiCommentViewer\        │        │      │
│  │      └──────────────────────────────┘        │      │
│  └──────────────────────────────────────────────┘      │
└────────────────────────┬─────────────────────────────┘
                         │ POST /api/mcv/logs (batch)
                         ▼
           ┌──────────────────────────────┐
           │    API Server                │
           │    (https://int-main.net)    │
           └────────────┬─────────────────┘
                        │ GET /api/mcv/logs
                        ▼
           ┌──────────────────────────────┐
           │    Log Viewer App (Tauri)    │
           │  apps/log-viewer             │
           └──────────────────────────────┘
                   ▲
                   │ get_local_logs (フォールバック)
           ┌───────┴──────────┐
           │  SQLite (local)  │
           └──────────────────┘
```

### ログの流れ

1. **mcv 本体**: `tracing` マクロでログ出力 → mcv-log-core subscriber → SQLite → API
2. **プラグイン**: `mcv-plugin-telemetry` が tracing を初期化し、ログを `log-entry` メッセージで core へ転送 → tracing → SQLite → API
3. **ログビューア**: API（優先）またはローカル SQLite（フォールバック）からログ取得

---

## ログレベルと Feature フラグ

mcv 本体とプラグインは個別にビルドされるため、それぞれ独立したログレベルを持つ。

| Feature Flag | ログレベル | 用途 |
|---|---|---|
| `alpha` | trace 以上すべて | 開発版、詳細なデバッグ情報 |
| `beta` | info 以上（info / warn / error） | ベータテスト版 |
| `stable` | error のみ | 安定版 |

**`RUST_LOG` 環境変数は使用しない。** ビルドは `cargo xtask` 経由で行う:

```bash
# alpha ビルドでローカルインストール
cargo xtask install --dir "C:/Users/<user>/AppData/Local/MultiCommentViewer" --channel alpha
```

---

## プラグインからのロギング

### mcv-plugin-telemetry を使う方法（推奨）

```rust
use mcv_tracing;

// on_loaded で初期化
mcv_tracing::init_tracing(
    self.plugin_id,
    Arc::clone(&host),
    env!("CARGO_PKG_VERSION"),
    "info",
)?;

// 以降は通常の tracing マクロを使うだけ
tracing::debug!(connection_id = %conn_id, "Processing message");
tracing::error!(error = %e, "Failed to connect");
```

プラグインからの `error` ログはスタックトレースが自動取得される。

### log-entry メッセージを直接送る方法

`LogEntryPayload` を `log-entry` メッセージタイプで送信する。

```rust
use mcv_messages::{Message, MessageType, MessageSource, MessageDestination, LogEntryPayload};

let payload = serde_json::to_value(LogEntryPayload {
    level: "error".to_string(),
    message: "Failed to connect".to_string(),
    context: Some(serde_json::json!({ "url": "https://..." })),
    connection_id: Some(connection_id),
    plugin_version: Some(env!("CARGO_PKG_VERSION").to_string()),
    plugin_build_profile: Some("alpha".to_string()),
}).unwrap();
```

---

## ローカルストレージ

- **場所**: `%LOCALAPPDATA%\MultiCommentViewer\logs.db`
- **DB**: SQLite
- **保存件数**: 最大 1000 件（送信済みの古いものから自動削除）

### スキーマ

```sql
CREATE TABLE logs (
    id TEXT PRIMARY KEY,          -- UUID
    level TEXT NOT NULL,          -- trace/debug/info/warn/error
    timestamp INTEGER NOT NULL,   -- Unix タイムスタンプ（ミリ秒）
    message TEXT NOT NULL,

    -- SourceLocation
    file TEXT NOT NULL,
    line INTEGER NOT NULL,
    column INTEGER,
    module_path TEXT NOT NULL,

    -- Optional
    stacktrace TEXT,              -- JSON 形式
    context TEXT,                 -- JSON 形式

    -- SystemInfo
    mcv_version TEXT NOT NULL,
    platform TEXT NOT NULL,
    arch TEXT NOT NULL,
    build_profile TEXT NOT NULL,

    -- Metadata
    sent INTEGER DEFAULT 0,       -- 0: 未送信, 1: 送信済み
    created_at INTEGER DEFAULT (strftime('%s','now'))
);
```

---

## API 仕様

APIサーバーは `mcv-plugin-registry` リポジトリで実装されている。
ベース URL: `https://int-main.net`

### POST /api/mcv/logs（ログ送信）

5 分ごと、またはエラー発生時に即時バッチ送信される。

**リクエスト**:
```json
[
  {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "level": "error",
    "timestamp": 1705766400000,
    "message": "Failed to connect to streaming server",
    "source": {
      "file": "crates/mcv-core/src/core_actor.rs",
      "line": 142,
      "column": 20,
      "module_path": "mcv_core::core_actor"
    },
    "stacktrace": [ { "symbol": "...", "filename": "...", "lineno": 45 } ],
    "context": { "connection_id": "abc-123" },
    "system_info": {
      "mcv_version": "0.8.1",
      "platform": "windows",
      "arch": "x86_64",
      "build_profile": "alpha"
    }
  }
]
```

**レスポンス**:
```json
{ "accepted": 1, "rejected": 0 }
```

### GET /api/mcv/logs（ログ取得）

**クエリパラメータ**:

| パラメータ | 説明 |
|---|---|
| `level` | `error` / `warn` / `info` / `debug` / `trace` |
| `from` | タイムスタンプ（ミリ秒、開始） |
| `to` | タイムスタンプ（ミリ秒、終了） |
| `search` | キーワード検索（message / file / module_path） |
| `limit` | 取得件数（デフォルト: 100） |
| `offset` | オフセット（デフォルト: 0） |

**レスポンス**:
```json
{ "logs": [ /* LogEntry 配列 */ ], "total": 1523, "limit": 100, "offset": 0 }
```

### DELETE /api/mcv/logs（ログ削除）

```json
{ "ids": ["550e8400-...", "660e8400-..."] }
```

### GET /api/mcv/health（ヘルスチェック）

```json
{ "status": "ok" }
```

---

## ログビューアアプリ

### 起動方法

```bash
cd apps/log-viewer
npm install  # 初回のみ
npm run tauri dev
```

### 機能

- **データソース切り替え**: Local DB / Server API
- **フィルタリング**: ログレベル・日時範囲・キーワード検索
- **ログ一覧**: 仮想スクロール、レベル別色分け、タイムスタンプ・ソース位置表示
- **ログ詳細**: メッセージ全文・ソース位置・コンテキスト（JSON）・スタックトレース・システム情報
- **ログ削除**: 個別削除（ローカル DB または API 経由）

---

## デバッグ方法

### DummyPlugin でログをテスト

1. mcv を alpha ビルドで起動
2. DummyPlugin の接続を追加・接続
3. コメント投稿欄に以下を入力:

```
log-error Database connection failed
log-warn Stream quality degraded
log-info Connected successfully
log-debug Processing message
```

### ローカル DB を直接確認

```bash
sqlite3 "%LOCALAPPDATA%\MultiCommentViewer\logs.db"

-- 最新10件
SELECT level, timestamp, message, file, line
FROM logs ORDER BY timestamp DESC LIMIT 10;

-- 未送信件数
SELECT COUNT(*) FROM logs WHERE sent = 0;
```

### API での確認

```bash
# ヘルスチェック
curl https://int-main.net/api/mcv/health

# エラーログのみ取得
curl "https://int-main.net/api/mcv/logs?level=error&limit=10"
```
