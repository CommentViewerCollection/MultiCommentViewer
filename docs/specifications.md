# MultiCommentViewer 仕様書

## 名称

正式名称: MultiCommentViewer
略称: mcv

## 概要

複数の配信プラットフォームのコメントを同時に集約して表示するコメントビューア。
一人の配信者が複数サイトで同時配信する場合や、複数配信者が参加する企画視聴時に、コメントを1か所でまとめて確認できる。棒読みちゃんとの連携による読み上げにも対応。

## 対応プラットフォーム

- YouTube Live
- Twitch
- ニコニコ生放送
- TwitCasting（通常配信・プライベート配信）
- Kick.com
- IRC

## 技術スタック

| レイヤー | 技術 |
|---------|------|
| UI フレームワーク | Tauri 2.x |
| フロントエンド | React + TypeScript + Tailwind CSS |
| バックエンド | Rust |
| Core-Plugin 間通信 | actix (Actor model) |
| フロントエンドコンポーネント | `my-dataview`（仮想スクロール付き高パフォーマンス表示） |

## 用語

| 用語 | 説明 |
|------|------|
| core | mcv 本体。UI 管理・プラグイン管理・コメント集約を担当 |
| plugin | 特定の配信サイトや機能に対応するモジュール（DLL） |
| plugin-host | plugin を隔離して実行する実行環境（actix Actor） |
| connection | 1 つの配信 URL に対する接続インスタンス |
| site | 配信サイト（例: YouTubeLive、ツイキャス）。プラグインが登録する |
| browser | cookie の取得元ブラウザまたは cookie ファイル |

## 基本的な使い方

1. 「接続を追加」ボタンを押す
2. URL 入力欄に配信 URL を入力する
3. コメント投稿やログインが必要な場合は適切なブラウザを選択する
4. 「接続」ボタンを押す
5. コメントがリアルタイムに表示される

---

## アーキテクチャ

### 構成

```
┌─────────────────────────────────────────┐
│              mcv (Tauri App)            │
│  ┌──────────┐      ┌──────────────────┐ │
│  │ Frontend │◄────►│   CoreActor      │ │
│  │  (React) │      │  (actix Actor)   │ │
│  └──────────┘      └────────┬─────────┘ │
│                             │           │
│                    ┌────────┴─────────┐ │
│                    │  PluginManager   │ │
│                    └────────┬─────────┘ │
└─────────────────────────────│───────────┘
                              │ Actor メッセージ
          ┌───────────────────┼───────────────────┐
          │                   │                   │
   ┌──────▼──────┐    ┌──────▼──────┐    ┌──────▼──────┐
   │ plugin-host │    │ plugin-host │    │ plugin-host │
   │  (actix)    │    │  (actix)    │    │  (actix)    │
   └──────┬──────┘    └──────┬──────┘    └──────┬──────┘
          │                   │                   │
   ┌──────▼──────┐    ┌──────▼──────┐    ┌──────▼──────┐
   │  Plugin DLL │    │  Plugin DLL │    │  Plugin DLL │
   └─────────────┘    └─────────────┘    └─────────────┘
```

- 1 つのプラグインに 1 つの plugin-host が割り当てられる
- core ↔ plugin-host 間は actix の Actor メッセージで通信
- plugin-host ↔ plugin 間はプラグイン ABI インターフェースで通信

### Plugin ABI バージョン

#### v3（現在の標準）

`PluginImplV3Async` トレイトを実装し、`export_plugin_v3_async!` マクロでエクスポート。

```rust
use plugin_abi_helper::v3::prelude::*;
use mcv_messages::{Message as McvMessage, MessageType};

#[derive(Default)]
struct MyPlugin { plugin_id: PluginId }

#[async_trait]
impl PluginImplV3Async for MyPlugin {
    async fn on_loaded(&mut self, ctx: PluginContext) {
        // plugin-hello を送信して core に登録
    }
    async fn on_message(&mut self, ctx: PluginContext, msg: &[u8]) {
        let incoming: McvMessage = serde_json::from_slice(msg).unwrap();
        match incoming.message_type {
            MessageType::Connect => { /* ... */ }
            _ => {}
        }
    }
    async fn on_shutdown(&mut self, ctx: PluginContext) {}
}

export_plugin_v3_async!(MyPlugin);
```

#### v2（旧スタイル）

`PluginImplV2` トレイトを実装。既存プラグインに残存。新規プラグインは v3 を使うこと。

### EXE プラグインシステム

DLL ではなく独立プロセスとして動作するプラグイン。`plugin-exe-manager-v3` が仲介する。

```
CoreActor
  ↕
plugin-exe-manager-v3 (DLL)
  ↕ WebSocket (JSON, port 28901+)
EXE plugin (独立プロセス)
```

- ポート 28901 が使用中なら 28902+ を自動選択
- `%APPDATA%\MultiCommentViewer\plugins\` の `plugin.json` をスキャンして自動起動
- 環境変数: `MCV_WEBSOCKET_PORT`, `MCV_WEBSOCKET_URL`

### ディレクトリ構造

```
/                          # リポジトリルート
├── Cargo.toml             # Workspace 管理
├── apps/
│   ├── mcv/               # メイン Tauri アプリ
│   ├── log-viewer/        # ログビューア（スタンドアロン）
│   └── exe-plugin-sample/ # EXE プラグインデバッグツール
├── crates/                # Rust crate 置き場
├── packages/              # React パッケージ置き場
└── docs/                  # ドキュメント置き場
```

インストール先（実行時）:
```
%LOCALAPPDATA%\MultiCommentViewer\
├── mcv.exe
└── plugins/
    ├── plugin-youtube-live.dll
    ├── plugin-twitch.dll
    └── ...

%APPDATA%\MultiCommentViewer\
├── settings.json
├── logs.db
└── plugins/               # EXE プラグイン置き場
    └── my-exe-plugin/
        ├── plugin.json
        └── plugin.exe
```

---

## connection

1 つの配信 URL に対する接続インスタンス。以下のコンテキストを持つ。

### 配信サイト（site）

プラグインが起動時に `add-site` メッセージで core に登録する。
例: "YouTubeLive"、"Twitch"、"ツイキャスプライベート配信"

### input

接続先の入力情報。URL（または配信 ID）をベースとし、サイトによっては追加情報が必要。
`get-connection-input-schema` / `connection-input-schema` で JSON Schema ベースの動的 UI を生成する。

| input_type | 説明 |
|-----------|------|
| normal | URL または配信 ID のみ |
| twicas_private | URL + 合言葉（パスワード） |

### ブラウザ（browser）

cookie の取得元。起動時に cookie プラグインが `add-browser` でインストール済みブラウザを登録する。
プロファイルが複数ある場合はそれぞれを別のブラウザとして扱う。
ユーザーが cookies.txt ファイルを追加することも可能。

### 接続フロー

```
ユーザー「接続を追加」
    → core が ConnectionManager に接続を作成
    → connection-added をブロードキャスト（デフォルト名: #1, #2, ...）

ユーザー「接続」
    → core がプラグインへ connect を送信
    → プラグインがコメント取得を開始
    → プラグインが connected を core へ送信
    → core が Tauri イベント "connected" をフロントエンドへ発火

ユーザー「切断」
    → core がプラグインへ disconnect を送信
    → プラグインが停止
    → プラグインが disconnected を core へ送信
    → core が Tauri イベント "disconnected" をフロントエンドへ発火
```

---

## core-plugin 間メッセージ

### 命名規則

- すべて kebab-case
- 要求系: 動詞から始める（`add-connection`、`connect`、`get-*` 等）
- 応答・通知系: 過去形または状態を表す名前（`connected`、`plugin-added` 等）
- 失敗通知: `-failed` サフィックス（`connect-failed` 等）

### メッセージ構造

```json
{
  "type": "connect",
  "src": "core | <plugin_uuid>",
  "dst": "core | <plugin_uuid> | broadcast",
  "request_id": "<uuid> | null",
  "timestamp": 1700000000,
  "payload": { ... }
}
```

- `src` / `dst`: `"core"` またはプラグインの UUID 文字列
- `dst`: `"broadcast"` を指定すると全プラグインに配信
- `request_id`: リクエスト/レスポンス対応時に設定する（応答側は同じ値を返す）
- `timestamp`: Unix タイムスタンプ（秒）

### メッセージ一覧

#### Plugin ライフサイクル

| メッセージ | 方向 | 説明 |
|-----------|------|------|
| `plugin-hello` | plugin→core | プラグイン登録要求 |
| `plugin-hello-ack` | core→plugin | 登録確認応答 |
| `plugin-added` | core→broadcast | プラグインが追加されたことの通知 |
| `plugin-removed` | core→broadcast | プラグインが削除・無効化されたことの通知 |
| `plugin-error` | plugin→core | プラグイン内エラーの通知 |
| `get-plugins` | plugin→core | 登録済みプラグイン一覧の取得 |

#### Cookie・ブラウザ管理

| メッセージ | 方向 | 説明 |
|-----------|------|------|
| `add-browser` | plugin→core | ブラウザを選択肢として登録 |
| `add-browser-ack` | core→plugin | ブラウザ登録確認 |
| `remove-browser` | plugin→core | ブラウザ登録解除 |
| `get-browser-plugin` | core→plugin | 指定 browser_id を担当するプラグインを照会 |
| `get-browser-plugin-ack` | plugin→core | 担当プラグイン ID を返す |
| `get-cookie` | core→plugin | 指定ドメインの cookie を要求 |
| `get-cookie-ack` | plugin→core | cookie 一覧を返す |

Cookie 取得フロー:
1. cookie プラグインが起動時に `add-browser` を送信
2. 接続時にプラットフォームプラグインが `get-browser-plugin` で担当プラグインを確認
3. `get-cookie` で cookie を取得し `get-cookie-ack` で受け取る

#### 配信サイト管理

| メッセージ | 方向 | 説明 |
|-----------|------|------|
| `add-site` | plugin→core | 配信サイトをUIに登録 |
| `add-site-ack` | core→plugin | サイト登録確認 |
| `set-connection-site` | core→plugin | 接続にサイトを割り当て |
| `discard-connection-site` | core→plugin | 接続からサイトの割り当てを解除 |

#### 接続管理

| メッセージ | 方向 | 説明 |
|-----------|------|------|
| `add-connection` | core→plugin | 接続の追加通知（UI 操作で core が生成） |
| `connection-added` | core→broadcast | 接続が追加されたことの通知 |
| `connection-add-failed` | core→plugin | 接続の追加に失敗 |
| `remove-connection` | core→plugin | 接続の削除通知 |
| `connection-removed` | core→broadcast | 接続が削除されたことの通知 |
| `connection-remove-failed` | core→plugin | 接続の削除に失敗 |
| `get-connections` | plugin→core | 接続一覧の取得 |
| `get-connection-status` | plugin→core | 指定接続のステータス取得 |
| `get-connection-input-schema` | core→plugin | 接続入力フォームの JSON Schema を要求 |
| `connection-input-schema` | plugin→core | JSON Schema とオプション UI Schema を返す |
| `update-connection-settings` | core→plugin | 接続設定の更新（URL・ブラウザ・詳細設定） |

#### 接続・切断

| メッセージ | 方向 | 説明 |
|-----------|------|------|
| `connect` | core→plugin | 接続開始要求 |
| `connected` | plugin→core | 接続確立通知 |
| `connect-failed` | plugin→core | 接続失敗通知 |
| `disconnect` | core→plugin | 切断要求 |
| `disconnected` | plugin→core | 切断完了通知 |
| `disconnect-failed` | plugin→core | 切断失敗通知 |

`connect` payload:
```json
{
  "connection_id": "<uuid>",
  "site": { "name": "YouTubeLive", "id": "<site_id>" },
  "input": { "input_type": "normal", "url": "https://..." },
  "browser": { "name": "Chrome", "id": "<browser_id>" }
}
```

#### コメント・メタデータ

| メッセージ | 方向 | 説明 |
|-----------|------|------|
| `comment-received` | plugin→core | コメント受信通知 |
| `send-comment` | core→plugin | コメント投稿要求 |
| `get-send-comment-schema` | core→plugin | コメント投稿フォームの JSON Schema を要求 |
| `send-comment-schema` | plugin→core | コメント投稿フォームの JSON Schema を返す |
| `stream-metadata` | plugin→core | 配信メタデータの更新（タイトル・視聴者数・配信開始時刻等） |

`comment-received` payload は `McvEnvelope` 構造（後述）。

#### アカウント管理

| メッセージ | 方向 | 説明 |
|-----------|------|------|
| `fetch-account-info` | core→plugin | ブラウザ cookie からアカウント情報を取得要求 |
| `update-connection-account` | plugin→core | アカウント情報を core に通知（None でクリア） |

#### URL 自動検出

| メッセージ | 方向 | 説明 |
|-----------|------|------|
| `can-handle-url` | core→plugin | 指定 URL を処理できるか問い合わせ |
| `can-handle-url-result` | plugin→core | 対応可否と対応するサイト ID を返す |

URL を入力すると core が全プラグインに `can-handle-url` を送信し、対応するプラグインが自動的にサイトを決定する。

#### Settings

| メッセージ | 方向 | 説明 |
|-----------|------|------|
| `get-settings-schema` | core→plugin | プラグイン設定の JSON Schema を要求 |
| `settings-schema` | plugin→core | JSON Schema を返す |
| `get-settings` | core→plugin | プラグイン設定の現在値を要求 |
| `settings-data` | plugin→core | 設定値を返す |
| `update-settings` | core→plugin | 設定値の更新 |

#### サイト NG 管理

| メッセージ | 方向 | 説明 |
|-----------|------|------|
| `set-site-ng-users` | plugin→core | ログイン中アカウントのブロックユーザーリストを通知 |

#### ディレクトリ情報

| メッセージ | 方向 | 説明 |
|-----------|------|------|
| `get-logs-dir` | plugin→core | ログディレクトリのパスを要求 |
| `logs-dir-ack` | core→plugin | ログディレクトリのパスを返す |
| `get-plugins-dir` | plugin→core | プラグインディレクトリのパスを要求 |
| `plugins-dir-ack` | core→plugin | プラグインディレクトリのパスを返す |
| `get-settings-dir` | plugin→core | 設定ディレクトリのパスを要求 |
| `settings-dir-ack` | core→plugin | 設定ディレクトリのパスを返す |

#### ロギング

| メッセージ | 方向 | 説明 |
|-----------|------|------|
| `log-entry` | plugin→core | プラグインからのログ送信 |

#### アプリ情報

| メッセージ | 方向 | 説明 |
|-----------|------|------|
| `get-app-name` | plugin→core | アプリ名の取得 |
| `get-app-version` | plugin→core | アプリバージョンの取得 |

---

## コメントデータモデル（McvEnvelope）

コメントは `CommentReceived` メッセージの payload として `McvEnvelope` 形式で送信される。

```
McvEnvelope
├── event_id: Uuid              # イベント一意ID
├── connection_id: Uuid         # 受信した接続のID
├── received_at: i64            # Core の受信時刻（Unix 秒）
├── raw_message: Option<String> # 生データ（デバッグ用）
└── messages: Vec<ProviderMessage>
    └── ProviderMessage
        ├── id: String                        # 内部一意ID
        ├── platform_message_id: Option<String> # プラットフォーム固有ID
        ├── service: ServiceId                # サービス識別子（例: "twitch"）
        ├── channel: ChannelId                # チャンネル識別子
        ├── sender: ProviderSender
        │   ├── id: String                   # プラットフォーム上のユーザーID
        │   ├── display_name: Vec<MessagePart> # リッチ表示名
        │   ├── badges: Vec<ProviderBadge>
        │   ├── role: Option<UserRole>        # streamer/moderator/vip/subscriber/member/viewer/staff
        │   └── avatar_url: Option<String>
        ├── timestamp: i64                    # 投稿時刻（Unix 秒）
        ├── kind: ProviderMessageKind
        │   ├── Chat                          # 通常チャット
        │   ├── HistoryChat                   # バックログ（接続直後の過去コメント）
        │   ├── Monetary(MonetaryInfo)        # スパチャ・サブスク等
        │   ├── Moderation(ModerationAction)  # delete/timeout/ban
        │   └── System(SystemKind)            # notice/subscription/message_delete/...
        ├── content: ProviderContent
        │   ├── Empty
        │   └── Text { text: Vec<MessagePart> }
        │       ├── MessagePart::Text { text }
        │       └── MessagePart::Image { url, width, height, alt }
        ├── reply_to: Option<String>          # 返信先の platform_message_id
        └── metadata: serde_json::Value       # プラットフォーム固有の追加データ
```

### SystemKind 一覧

| variant | 説明 |
|---------|------|
| `Notice` | システム通知 |
| `Subscription` | サブスクリプション |
| `Membership` | メンバーシップ |
| `MessageUpdate { target_message_id }` | メッセージ更新 |
| `MessageDelete { target_message_id }` | 特定メッセージ削除 |
| `ChannelEvent` | チャンネルイベント |
| `Placeholder` | 承認待ちコメント |
| `MessageDeleteAll { user_id }` | 指定ユーザーのコメントを全削除（BAN 等） |
| `GiftAnnouncement` | ギフトメンバーシップ通知等 |

### 設計原則

**`mcv-messages` クレートはプラットフォーム非依存の型のみ定義する。**
特定プラットフォーム固有の概念は追加してはならない。

判断基準: 「YouTube 以外（Twitch、ニコ生等）でも意味をなすか？」
- YES → `mcv-messages` に追加
- NO → プラグイン側で吸収し、汎用的な表現に変換して送出

---

## `send-comment` の動作

### 配信サイトプラグイン

受信したテキストを配信サイトにコメントとして投稿する。

`send-comment` payload:
```json
{
  "connection_id": "<uuid>",
  "text": "投稿するコメント",
  "extra": {}
}
```

`extra` フィールドにはプラットフォーム固有の追加入力（絵文字・表示設定等）を格納できる。フォームスキーマは `get-send-comment-schema` / `send-comment-schema` で取得する。

### DummyPlugin コマンド

DummyPlugin では `send-comment` のテキストをコマンドとして扱う:

| コマンド | 説明 |
|---------|------|
| `disconnect` | 配信サイト側からの切断をシミュレート |
| `connect` | 再接続（UI から接続する必要がある） |
| `pause` | コメント生成を一時停止 |
| `resume` | コメント生成を再開 |
| `rate <seconds>` | コメント生成間隔を設定（0 でランダム） |
| `comment <user> <text>` | 手動でコメントを生成 |
| `log-error <message>` | エラーログを送信 |
| `log-warn <message>` | 警告ログを送信 |
| `log-info <message>` | 情報ログを送信 |
| `log-debug <message>` | デバッグログを送信 |
| `help` | ヘルプメッセージを表示 |
| `status` | 接続ステータスを表示 |

---

## `plugin-hello` の仕様

プラグイン起動時に core に自身を登録するメッセージ。

```json
{
  "name": "YouTube Live",
  "plugin_id": "<uuid>",
  "role": ["youtubelive"],
  "api_version": "v3",
  "send_comment_schema": null
}
```

- `role`: プラグインの機能を表す文字列の配列（任意の値）
- `api_version`: メッセージ形式のバージョン（`"v3"` を推奨）
- `send_comment_schema`: コメント投稿フォームのカスタム JSON Schema（省略時は core のデフォルトを使用）

core は `plugin-hello-ack` で登録確認を返し、`plugin-added` を全プラグインにブロードキャストする。

---

## Cargo Features（ログレベル制御）

| Feature | ログレベル | 用途 |
|---------|----------|------|
| `alpha` | trace 以上すべて | 開発版・全機能有効（comment-search 等） |
| `beta` | info 以上 | ベータテスト版 |
| `stable` | error のみ | 安定版 |

ビルドは `cargo xtask` 経由で行う（`RUST_LOG` 環境変数は使用しない）:

```bash
cargo xtask install --dir "C:/Users/<user>/AppData/Local/MultiCommentViewer" --channel alpha
```
