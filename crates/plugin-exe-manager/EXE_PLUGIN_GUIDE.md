# EXEプラグイン開発ガイド

MultiCommentViewerのEXEプラグインを開発するためのガイドです。

## 目次

1. [概要](#概要)
2. [アーキテクチャ](#アーキテクチャ)
3. [開発環境のセットアップ](#開発環境のセットアップ)
4. [基本的なプラグインの作成](#基本的なプラグインの作成)
5. [メッセージハンドリング](#メッセージハンドリング)
6. [デバッグとテスト](#デバッグとテスト)
7. [配布とインストール](#配布とインストール)

## 概要

EXEプラグインは、独立したプロセスとして動作し、WebSocketを介してMultiCommentViewerのコアと通信します。

### 利点

- **言語の自由**: Rust以外の言語（C#、Python、Go、Node.jsなど）でプラグインを作成可能
- **独立性**: クラッシュしてもコアに影響を与えない
- **開発の容易さ**: DLLプラグインよりシンプルなAPI

### 制約

- **パフォーマンス**: WebSocket通信のオーバーヘッド
- **起動時間**: プロセス起動の遅延

## アーキテクチャ

```
MultiCommentViewer Core
  ↕
plugin-exe-manager (DLL plugin)
  ↕ WebSocket (JSON, port 28901)
Your EXE Plugin (independent process)
```

### メッセージフロー

1. **起動時**:
   - plugin-exe-managerがplugin.jsonをスキャン
   - EXEプラグインを起動（環境変数 `MCV_WEBSOCKET_URL` を設定）
   - EXEプラグインがWebSocketに接続
   - `plugin-hello` メッセージを送信

2. **実行中**:
   - コア → プラグイン: コマンド、イベント通知
   - プラグイン → コア: コメント、ステータス更新

3. **終了時**:
   - プラグインがWebSocket切断
   - プロセス終了

## 開発環境のセットアップ

### Rustの場合

#### 1. Cargo.tomlに依存関係を追加

```toml
[dependencies]
mcv-plugin-exe-interface = { path = "../../crates/mcv-plugin-exe-interface" }
mcv-messages = { path = "../../crates/mcv-messages" }
tokio = { version = "1.0", features = ["full"] }
tracing = "0.1"
tracing-subscriber = "0.3"
```

#### 2. main.rsの基本構造

```rust
use mcv_plugin_exe_interface::ExePluginClient;
use mcv_messages::{Message as McvMessage, MessageType};

#[tokio::main]
async fn main() -> Result<(), Box<dyn std::error::Error>> {
    // ロギングの初期化
    tracing_subscriber::fmt::init();

    // WebSocket URLを環境変数から取得
    let url = std::env::var("MCV_WEBSOCKET_URL")
        .unwrap_or_else(|_| "ws://127.0.0.1:28901".to_string());

    // WebSocketに接続
    let mut client = ExePluginClient::connect(&url).await?;

    // plugin-helloを送信
    client.send_plugin_hello("My Plugin", vec!["comment-provider"]).await?;

    // メッセージハンドラーを登録
    client.on_message(|msg| {
        handle_message(msg);
    });

    // メッセージループを実行
    client.run().await?;

    Ok(())
}

fn handle_message(msg: McvMessage) {
    match msg.message_type {
        MessageType::Connect => {
            // 接続要求の処理
            tracing::info!("Received connect request");
        }
        MessageType::Disconnect => {
            // 切断要求の処理
            tracing::info!("Received disconnect request");
        }
        MessageType::SendComment => {
            // コメント送信要求の処理
            tracing::info!("Received send-comment request");
        }
        _ => {
            tracing::debug!("Received message: {:?}", msg.message_type);
        }
    }
}
```

### 他の言語の場合

WebSocketクライアントライブラリとJSON パーサーがあれば、任意の言語で実装可能です。

#### Python の例

```python
import asyncio
import websockets
import json
import os
import uuid

async def main():
    url = os.environ.get('MCV_WEBSOCKET_URL', 'ws://127.0.0.1:28901')
    plugin_id = str(uuid.uuid4())

    async with websockets.connect(url) as websocket:
        # Send plugin-hello
        hello_msg = {
            "type": "plugin-hello",
            "src": {"Plugin": {"plugin_id": plugin_id}},
            "dst": "Core",
            "payload": {
                "name": "Python Plugin",
                "plugin_id": plugin_id,
                "role": ["comment-provider"],
                "api_version": "v2"
            },
            "timestamp": int(time.time())
        }
        await websocket.send(json.dumps(hello_msg))

        # Message loop
        async for message in websocket:
            msg = json.loads(message)
            print(f"Received: {msg['type']}")
            # Handle message...

if __name__ == "__main__":
    asyncio.run(main())
```

## 基本的なプラグインの作成

### 1. ディレクトリ構造

```
%LOCALAPPDATA%\MultiCommentViewer\plugins\
└── my-plugin/
    ├── plugin.json
    └── my-plugin.exe
```

### 2. plugin.json の作成

```json
{
  "path": "my-plugin.exe"
}
```

### 3. プラグインのビルドと配置

```bash
# Rustの場合
cargo build --release

# バイナリをコピー
copy target\release\my-plugin.exe "%LOCALAPPDATA%\MultiCommentViewer\plugins\my-plugin\"
copy plugin.json "%LOCALAPPDATA%\MultiCommentViewer\plugins\my-plugin\"
```

## メッセージハンドリング

### 主要なメッセージタイプ

#### Core → Plugin

| メッセージ | 説明 | ペイロード |
|----------|------|-----------|
| `connect` | 配信サイトへの接続要求 | `ConnectPayload` |
| `disconnect` | 切断要求 | `DisconnectPayload` |
| `send-comment` | コメント送信要求 | `SendCommentPayload` |

#### Plugin → Core

| メッセージ | 説明 | ペイロード |
|----------|------|-----------|
| `plugin-hello` | プラグイン登録 | `PluginHelloPayload` |
| `add-connection` | 新規接続の追加 | `AddConnectionPayload` |
| `connected` | 接続成功通知 | `ConnectedPayload` |
| `disconnected` | 切断通知 | `DisconnectedPayload` |
| `comment-received` | コメント受信通知 | `CommentReceivedPayload` |

### メッセージ送信の例

```rust
use mcv_messages::*;

// add-connectionメッセージを送信
let payload = AddConnectionPayload {
    site_id: "youtube-live".to_string(),
};

let message = Message::new(
    MessageType::AddConnection,
    MessageSource::Plugin { plugin_id: client.plugin_id() },
    MessageDestination::Core,
    serde_json::to_value(&payload).unwrap(),
);

client.send_message(message).await?;
```

### ブロードキャストメッセージ

以下のメッセージは全EXEプラグインにブロードキャストされます：

- `plugin-added`, `plugin-removed`
- `connection-added`, `connection-removed`
- `connected`, `disconnected`
- `comment-received`

## デバッグとテスト

### ログの確認

```rust
// tracing マクロを使用
tracing::info!("Plugin started");
tracing::debug!("Processing message: {:?}", msg);
tracing::error!("Failed to connect: {}", err);
```

### exe-plugin-sample デバッグツールの使用

```bash
cd apps/exe-plugin-sample
npm install
npm run tauri dev
```

デバッグツールでは以下が可能：
- 送受信メッセージのリアルタイム監視
- 任意のメッセージの手動送信
- プラグイン、接続、コメントの状態確認

## 配布とインストール

### インストール手順（エンドユーザー向け）

1. プラグインのZIPファイルをダウンロード
2. `%LOCALAPPDATA%\MultiCommentViewer\plugins\` に解凍
3. MultiCommentViewerを再起動

### 配布パッケージの構成

```
my-plugin.zip
└── my-plugin/
    ├── plugin.json
    ├── my-plugin.exe
    └── README.md (オプション)
```

## トラブルシューティング

### プラグインが起動しない

1. plugin.jsonの `path` が正しいか確認
2. 実行ファイルに実行権限があるか確認
3. MultiCommentViewerのログを確認

### WebSocketに接続できない

1. 環境変数 `MCV_WEBSOCKET_URL` が設定されているか確認
2. ポート28901が使用可能か確認
3. ファイアウォールの設定を確認

### メッセージが受信できない

1. `plugin-hello` を正しく送信しているか確認
2. メッセージハンドラーが登録されているか確認
3. デバッグツール（exe-plugin-sample）で動作確認

## 参考リソース

- [MANIFEST_SCHEMA.md](./MANIFEST_SCHEMA.md) - plugin.jsonスキーマ
- [mcv-messages](../mcv-messages/) - メッセージ型定義
- [mcv-plugin-exe-interface](../mcv-plugin-exe-interface/) - Rust用クライアントライブラリ
- [exe-plugin-sample](../../apps/exe-plugin-sample/) - デバッグツール

## よくある質問

### Q: 複数の接続を管理できますか？

A: はい。`connection_id` を使って複数の接続を管理できます。

### Q: 認証が必要な配信サイトに対応できますか？

A: はい。ブラウザのCookieを取得するか、独自の認証UIを実装できます。

### Q: プラグインがクラッシュしたらどうなりますか？

A: plugin-exe-managerが検出し、最大3回まで自動再起動を試みます。

### Q: プラグインを更新するには？

A: MultiCommentViewerを終了し、実行ファイルを上書きして再起動してください。
