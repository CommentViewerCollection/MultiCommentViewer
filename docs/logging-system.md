# MCV エラーロギングシステム

## 概要

MCVアプリケーションの開発・運用を支援するための構造化エラーロギングシステムです。mcv本体とプラグインの両方からエラーログを収集し、専用のログビューアーアプリで閲覧・分析できます。

### 主な機能

- **構造化ログ**: ファイル名、関数名、行番号、スタックトレース、コンテキスト情報を自動記録
- **プラグイン対応**: プラグインから`log-entry`メッセージでログ送信可能
- **バックグラウンド送信**: エラー発生時に自動的にAPIサーバーへ送信、失敗時はローカルに保存
- **ログビューアー**: サーバーまたはローカルDBからログを取得・表示・削除できるTauriアプリ
- **ビルドプロファイル対応**: alpha/beta/stable でログレベルを動的に制御

---

## アーキテクチャ

### コンポーネント図

```
┌─────────────────────────────────────────────────────────────┐
│                        MCV Application                       │
│  ┌────────────┐        ┌──────────────┐                     │
│  │   Core     │◄──────►│   Plugin     │                     │
│  │   Actor    │        │ (DLL/static) │                     │
│  └─────┬──────┘        └──────┬───────┘                     │
│        │ tracing              │ log-entry message           │
│        ▼                      ▼                              │
│  ┌──────────────────────────────────────┐                   │
│  │        mcv-logger                    │                   │
│  │  ┌──────────────┐  ┌──────────────┐ │                   │
│  │  │  Subscriber  │  │ LogSenderActor│ │                   │
│  │  │  (tracing)   │  │   (actix)     │ │                   │
│  │  └──────┬───────┘  └───────┬───────┘ │                   │
│  │         │                  │          │                   │
│  │         ▼                  ▼          │                   │
│  │  ┌──────────────────────────────┐    │                   │
│  │  │   SQLite (logs.db)           │    │                   │
│  │  │   %LOCALAPPDATA%\           │    │                   │
│  │  │   MultiCommentViewer\        │    │                   │
│  │  └──────────────────────────────┘    │                   │
│  └───────────────────────────────────────┘                   │
└────────────────────────┬────────────────────────────────────┘
                         │ POST /api/mcv/logs (batch)
                         ▼
           ┌──────────────────────────────┐
           │    API Server (Node.js)      │
           │    ┌──────────────────────┐  │
           │    │   PostgreSQL         │  │
           │    │   (LogEntry table)   │  │
           │    └──────────────────────┘  │
           └────────────┬─────────────────┘
                        │ GET /api/mcv/logs
                        ▼
           ┌──────────────────────────────┐
           │    Log Viewer App (Tauri)    │
           │  ┌────────────────────────┐  │
           │  │  React UI              │  │
           │  │  - LogTable            │  │
           │  │  - FilterBar           │  │
           │  │  - LogDetail           │  │
           │  │  - StackTrace          │  │
           │  └────────────────────────┘  │
           └──────────────────────────────┘
                   ▲
                   │ get_local_logs (fallback)
                   │
           ┌───────┴──────────┐
           │  SQLite (local)  │
           └──────────────────┘
```

### ログの流れ

1. **mcv本体**: `tracing`マクロでログ出力 → mcv-logger subscriber → SQLite → API
2. **プラグイン**: `log-entry`メッセージ送信 → CoreActor → `tracing`ログ出力 → SQLite → API
3. **ログビューアー**: API (優先) またはローカルSQLite (フォールバック) からログ取得

---

## ログレベルとfeature フラグ

### ビルドプロファイル

mcvとプラグインは個別にビルドされるため、それぞれ独立したログレベルを持ちます。

| Feature Flag | ログレベル | 用途 |
|-------------|----------|------|
| `alpha` | trace以上すべて | 開発版、詳細なデバッグ情報 |
| `beta` | info以上 (info/warn/error) | ベータテスト版、一般的な情報 |
| `stable` | error のみ | 安定版、エラーのみ記録 |

### ビルド方法

```bash
# mcv本体
cd apps/mcv
cargo build --features alpha      # Alpha build
cargo build --features beta       # Beta build
cargo build --features stable     # Stable build (default)

# プラグイン
cd crates/plugin-dummy
cargo build --features alpha      # Plugin alpha build
```

### 実装詳細

**mcv-logger/src/subscriber.rs**:
```rust
fn get_build_profile() -> String {
    #[cfg(feature = "alpha")]
    return "alpha".to_string();
    #[cfg(all(feature = "beta", not(feature = "alpha")))]
    return "beta".to_string();
    #[cfg(all(not(feature = "alpha"), not(feature = "beta"), feature = "stable"))]
    return "stable".to_string();
    #[cfg(all(not(feature = "alpha"), not(feature = "beta"), not(feature = "stable")))]
    return "unknown".to_string();
}

// ログレベル設定
let log_level = match get_build_profile().as_str() {
    "alpha" => "trace",
    "beta" => "info",
    "stable" | _ => "error",
};
```

---

## プラグインからのロギング

プラグインは別途ビルドされるため、mcv本体とは独立したログレベルを持ちます。プラグインからログを送信する場合、`log-entry`メッセージタイプを使用します。

### メッセージ定義

**mcv-messages/src/lib.rs**:
```rust
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct LogEntryPayload {
    pub level: String,           // "error", "warn", "info", "debug", "trace"
    pub message: String,
    pub context: Option<serde_json::Value>,
    pub connection_id: Option<Uuid>,
    pub plugin_version: Option<String>,
    pub plugin_build_profile: Option<String>,
}
```

### 使用例（プラグイン側）

```rust
// エラーログ送信
let message = Message::new_notification(
    MessageType::LogEntry,
    MessageSource::Plugin { plugin_id: self.plugin_id },
    MessageDestination::Core,
    serde_json::to_value(LogEntryPayload {
        level: "error".to_string(),
        message: "Failed to connect to stream".to_string(),
        context: Some(serde_json::json!({
            "url": "https://...",
            "error_detail": "Connection timeout"
        })),
        connection_id: Some(connection_id),
        plugin_version: Some(env!("CARGO_PKG_VERSION").to_string()),
        plugin_build_profile: get_build_profile(),
    }).unwrap(),
);
host.send_message(message).await?;
```

### CoreActorでの処理

**mcv-core/src/core_actor.rs**:
```rust
fn handle_log_entry(&mut self, message: McvMessage, _ctx: &mut Context<Self>) {
    let payload: LogEntryPayload = /* parse */;
    let plugin_id = /* extract */;

    // プラグインからのログをmcv本体のログとして記録
    match payload.level.as_str() {
        "error" => tracing::error!(
            plugin_id = %plugin_id,
            plugin_version = ?payload.plugin_version,
            plugin_build_profile = ?payload.plugin_build_profile,
            connection_id = ?payload.connection_id,
            context = ?payload.context,
            "Plugin error: {}",
            payload.message
        ),
        // ... 他のレベル
    }
}
```

### plugin-dummyのログコマンド

テスト用のログコマンドを実装済み:

```
log-error <message>   # エラーログ送信
log-warn <message>    # 警告ログ送信
log-info <message>    # 情報ログ送信
log-debug <message>   # デバッグログ送信
```

**使用例**:
```
log-error Database connection failed
log-warn Stream quality degraded
```

---

## ローカルストレージ

### データベース

- **場所**: `%LOCALAPPDATA%\MultiCommentViewer\logs.db`
- **DB**: SQLite
- **保存期間**: 最大1000件（古いものから自動削除）

### スキーマ

```sql
CREATE TABLE logs (
    id TEXT PRIMARY KEY,          -- UUID
    level TEXT NOT NULL,          -- trace/debug/info/warn/error
    timestamp INTEGER NOT NULL,   -- Unix timestamp (ミリ秒)
    message TEXT NOT NULL,

    -- SourceLocation
    file TEXT NOT NULL,
    line INTEGER NOT NULL,
    column INTEGER,
    module_path TEXT NOT NULL,

    -- Optional fields
    stacktrace TEXT,              -- JSON形式
    context TEXT,                 -- JSON形式

    -- SystemInfo
    mcv_version TEXT NOT NULL,
    platform TEXT NOT NULL,
    arch TEXT NOT NULL,
    build_profile TEXT NOT NULL,

    -- Metadata
    sent INTEGER DEFAULT 0,       -- 0: 未送信, 1: 送信済み
    created_at INTEGER DEFAULT (strftime('%s','now'))
);

CREATE INDEX idx_timestamp ON logs(timestamp DESC);
CREATE INDEX idx_sent ON logs(sent);
CREATE INDEX idx_level ON logs(level);
```

### 自動削除ポリシー

- ログ送信成功後、1000件を超えた場合に古いログから削除
- `LogStorage::cleanup_old_logs(1000)` で実行

---

## API仕様

APIサーバーは `mcv-plugin-registry` リポジトリで実装されています。

### 1. POST /api/mcv/logs（ログ送信）

mcvアプリから5分ごと、またはエラー発生時に即座にバッチ送信されます。

**リクエスト**:
```json
[
  {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "level": "error",
    "timestamp": 1705766400000,
    "message": "Failed to connect to streaming server",
    "source": {
      "file": "crates\\mcv-core\\src\\core_actor.rs",
      "line": 142,
      "column": 20,
      "module_path": "mcv_core::core_actor"
    },
    "stacktrace": [
      {
        "symbol": "mcv_core::connect",
        "filename": "crates\\mcv-core\\src\\connection.rs",
        "lineno": 45,
        "addr": "0x7ff7ec839c5e"
      }
    ],
    "context": {
      "connection_id": "abc-123",
      "url": "https://example.com"
    },
    "system_info": {
      "mcv_version": "0.1.0",
      "platform": "windows",
      "arch": "x86_64",
      "build_profile": "alpha"
    }
  }
]
```

**レスポンス**:
```json
{
  "accepted": 1,
  "rejected": 0
}
```

### 2. GET /api/mcv/logs（ログ取得）

ログビューアーからフィルタリング・ページネーション付きで取得されます。

**クエリパラメータ**:
- `level`: `error`/`warn`/`info`/`debug`/`trace`
- `from`: タイムスタンプ（ミリ秒、開始日時）
- `to`: タイムスタンプ（ミリ秒、終了日時）
- `search`: キーワード検索（message/file/module_path）
- `limit`: 取得件数（デフォルト: 100）
- `offset`: オフセット（デフォルト: 0）

**リクエスト例**:
```
GET /api/mcv/logs?level=error&from=1705766400000&limit=10&offset=0
```

**レスポンス**:
```json
{
  "logs": [ /* LogEntry配列 */ ],
  "total": 1523,
  "limit": 10,
  "offset": 0
}
```

### 3. DELETE /api/mcv/logs（ログ削除）

ログビューアーから不要なログを削除します。

**リクエスト**:
```json
{
  "ids": ["550e8400-...", "660e8400-..."]
}
```

**レスポンス**:
```json
{
  "deleted": 2
}
```

---

## ログビューアーアプリ

### 起動方法

```bash
cd apps/log-viewer
npm install  # 初回のみ
npm run tauri dev
```

### UI構成

```
┌──────────────────────────────────────────────────────────────┐
│ MCV Log Viewer          [Data Source: ▼] [API URL: ______]  │
│                                              [Refresh]        │
├──────────────────────────────────────────────────────────────┤
│ Filter: [Level: ▼] [Search: _______] [From: ___] [To: ___] │
│         [Apply] [Reset]                                      │
├────────────────────────────────┬─────────────────────────────┤
│ Log Table (2/3)                │ Log Detail (1/3)            │
│ ┌────────────────────────────┐ │ ┌─────────────────────────┐ │
│ │ [ERROR] 2025-01-19 10:30:45│ │ │ Level: ERROR            │ │
│ │ Failed to connect...       │ │ │ Timestamp: ...          │ │
│ │ core_actor.rs:142          │ │ │ Message: ...            │ │
│ │                            │ │ │ Source Location:        │ │
│ │ [WARN] 2025-01-19 10:25:12 │ │ │   File: ...             │ │
│ │ Stream quality degraded... │ │ │   Line: ...             │ │
│ │ plugin.rs:78               │ │ │ Context: {...}          │ │
│ │                            │ │ │ Stack Trace:            │ │
│ │ ...                        │ │ │   frame 1               │ │
│ │                            │ │ │   frame 2               │ │
│ └────────────────────────────┘ │ │ System Info:            │ │
│ Showing 1-100 of 1523 logs     │ │   Version: 0.1.0        │ │
│ [Load More]                    │ │   Platform: windows     │ │
│                                │ │ [Delete]                │ │
│                                │ └─────────────────────────┘ │
└────────────────────────────────┴─────────────────────────────┘
```

### 機能

1. **データソース切り替え**
   - Local DB: ローカルSQLiteから直接取得
   - Server API: APIサーバーから取得（推奨）

2. **フィルタリング**
   - ログレベル: trace/debug/info/warn/error
   - 日時範囲: 開始日時〜終了日時
   - キーワード検索: message/file/module_path内を検索

3. **ログ一覧**
   - 仮想スクロール対応（react-virtuoso）
   - レベル別色分け表示
   - タイムスタンプ、メッセージ、ソース位置を一覧表示
   - クリックで詳細表示

4. **ログ詳細**
   - メッセージ全文
   - ソースコード位置（ファイル、行、列、モジュールパス）
   - コンテキスト情報（JSON）
   - スタックトレース（整形表示）
   - システム情報（mcvバージョン、プラットフォーム、アーキテクチャ、ビルドプロファイル）

5. **ログ削除**
   - 個別削除: 詳細ビューまたは一覧の「Delete」ボタン
   - データソースに応じて自動的にローカルDB/API経由で削除

---

## デバッグ方法

### 1. plugin-dummyのログコマンド

最も簡単なテスト方法です。

```bash
# mcvを起動
cd apps/mcv
npm run tauri dev
```

mcv UI上で:
1. 「接続を追加」ボタンで接続作成
2. コメント入力欄に以下を入力:
```
log-error Test error from plugin
log-warn Test warning message
log-info Test info message
```

### 2. ローカルDBの直接確認

```bash
# SQLite CLIでログDB を開く
sqlite3 "%LOCALAPPDATA%\MultiCommentViewer\logs.db"

# ログ件数確認
SELECT COUNT(*) FROM logs;

# 最新10件のログを表示
SELECT level, timestamp, message, file, line
FROM logs
ORDER BY timestamp DESC
LIMIT 10;

# 未送信ログの確認
SELECT COUNT(*) FROM logs WHERE sent = 0;
```

### 3. APIサーバーでの確認

```bash
# ログ件数確認
curl "https://int-main.net/api/mcv/logs?limit=1" | jq '.total'

# エラーログのみ取得
curl "https://int-main.net/api/mcv/logs?level=error&limit=10" | jq '.logs[] | {level, message, timestamp}'

# 特定キーワードで検索
curl "https://int-main.net/api/mcv/logs?search=plugin&limit=5" | jq
```

### 4. ログ送信の動作確認

mcvを起動して意図的にエラーを発生させ、以下を確認:

1. **即時送信**: エラー発生直後にAPIサーバーのログで `POST /api/mcv/logs` を確認
2. **定期送信**: 5分後に未送信ログがバッチ送信される
3. **送信失敗時**: ネットワーク切断時、ローカルDBに `sent=0` で保存される

---

## トラブルシューティング

### ログが記録されない

**症状**: mcvを実行してもログが記録されない

**確認事項**:
1. ビルドプロファイルが適切か確認
   ```bash
   # alphaビルドでない場合、debugログは記録されない
   cargo build --features alpha
   ```

2. ログDBが作成されているか確認
   ```bash
   ls "%LOCALAPPDATA%\MultiCommentViewer\logs.db"
   ```

3. mcv起動時のログ出力を確認
   ```
   INFO mcv started, log_db_path=C:\Users\...\logs.db
   INFO LogSenderActor started, api_base_url=https://int-main.net
   ```

### 送信が失敗する

**症状**: ログがローカルに保存されるがAPIサーバーに送信されない

**確認事項**:
1. APIサーバーが起動しているか確認
   ```bash
   curl https://int-main.net/api/mcv/health
   # {"status":"ok"}
   ```

2. API URLが正しいか確認（mcv/src-tauri/src/main.rs:323）
   ```rust
   const API_BASE_URL: &str = "https://int-main.net";
   ```

3. 未送信ログの件数確認
   ```sql
   SELECT COUNT(*) FROM logs WHERE sent = 0;
   ```

4. LogSenderActorのログを確認
   ```
   ERROR Failed to send logs
   ```

### ログビューアーで表示されない

**症状**: ログビューアーを開いてもログが表示されない

**Server API モード**:
1. API URLが正しいか確認（デフォルト: `https://int-main.net`）
2. curlでAPIレスポンスを確認
   ```bash
   curl "https://int-main.net/api/mcv/logs?limit=1"
   ```
3. ブラウザのDevToolsでエラーを確認

**Local DB モード**:
1. ログDBパスが正しいか確認
   ```bash
   # Tauriコマンドを実行
   # get_log_db_path()の出力を確認
   ```
2. DBファイルが存在するか確認
3. SQLiteで直接読み込んで確認

### プラグインログが記録されない

**症状**: plugin-dummyの`log-error`コマンドを実行してもログが記録されない

**確認事項**:
1. CoreActorで`handle_log_entry`が呼ばれているか確認（デバッグログ）
2. mcv-messagesに`MessageType::LogEntry`が定義されているか確認
3. プラグインが正しくメッセージを送信しているか確認
   ```rust
   tracing::debug!("Sending log-entry message");
   host.send_message(message).await?;
   ```

---

## セキュリティ考慮事項

### 現在の実装（開発者専用ツール）

- **機密情報フィルタリング**: 未実装（開発者専用ツールのため）
- **認証**: 未実装
- **アクセス制御**: なし（ローカルファイルシステムとローカルAPIサーバー）

### 将来の拡張

本番環境で使用する場合、以下の対策が必要:

1. **機密情報の自動フィルタリング**
   - パスワード、APIキー、トークンを自動検出・マスク
   - 正規表現ベースのフィルタ実装

2. **認証・認可**
   - Bearer Token認証の追加
   - ユーザーロール管理（admin/developer）

3. **アクセスログ**
   - APIアクセス履歴の記録
   - 異常なアクセスパターンの検出

4. **データ保持期間**
   - 自動削除ポリシーの設定
   - GDPR準拠のデータ削除機能

---

## まとめ

MCVエラーロギングシステムは、開発者がエラーを迅速に発見・解析するための包括的なツールです。

**主な利点**:
- 構造化ログにより詳細なコンテキスト情報を自動記録
- プラグインからのログ送信により、分散システム全体を監視
- ビルドプロファイル対応により、環境ごとに適切なログレベルを設定
- ログビューアーにより、開発者はローカル/サーバーから簡単にログを閲覧・分析

**使用開始**:
1. mcvをalphaビルドで起動（`cargo build --features alpha`）
2. plugin-dummyで`log-error`コマンドを実行
3. ログビューアーを起動してログを確認

**問題報告**:
- GitHub Issues: <https://github.com/your-repo/mcv/issues>
