# mcv-tracing 使用例

## 基本的なエラーコンテキストのキャプチャ

```rust
use mcv_tracing::capture_context;

fn process_data(value: i32) -> Result<String, mcv_tracing::TracingError> {
    if value == 0 {
        let ctx = capture_context!("Invalid value: cannot be zero");
        return Err(ctx.into());
    }
    Ok(format!("Processed: {}", value))
}
```

## 構造化フィールド付きのエラーコンテキスト

```rust
use mcv_tracing::capture_context;

async fn fetch_comments(video_id: &str) -> Result<Vec<Comment>, mcv_tracing::TracingError> {
    match api_call(video_id).await {
        Ok(data) => Ok(data),
        Err(e) => {
            let ctx = capture_context!(
                "Failed to fetch comments",
                video_id = video_id,
                error = e.to_string(),
                retry_count = 3,
            );
            Err(ctx.into())
        }
    }
}
```

## 入れ子のエラーコンテキスト（InnerException パターン）

### シンプルな入れ子

```rust
use mcv_tracing::capture_context;

async fn parse_data() -> Result<Data, mcv_tracing::TracingError> {
    // 最下層でのエラー
    let inner_ctx = capture_context!(
        "Network connection failed",
        error_code = "ETIMEDOUT",
        host = "api.example.com",
    );

    // 上位層でのエラーに inner_ctx を追加
    let mut outer_ctx = capture_context!(
        "Failed to fetch data from API",
        endpoint = "/api/v1/comments",
    );
    outer_ctx.add_inner_error(inner_ctx);

    Err(outer_ctx.into())
}
```

### 複数階層の入れ子（3層）

```rust
use mcv_tracing::capture_context;

async fn process_comments() -> Result<String, mcv_tracing::TracingError> {
    // レイヤー1: ネットワークエラー（最下層）
    let network_ctx = capture_context!(
        "Network connection failed",
        error_code = "ETIMEDOUT",
        host = "api.example.com",
    );

    // レイヤー2: APIエラー（中間層）
    let mut api_ctx = capture_context!(
        "Failed to fetch data from API",
        endpoint = "/api/v1/comments",
    );
    api_ctx.add_inner_error(network_ctx);

    // レイヤー3: ビジネスロジックエラー（最上層）
    let mut process_ctx = capture_context!(
        "Failed to process comments",
        operation = "process_comments",
        retry_count = 3,
    );
    process_ctx.add_inner_error(api_ctx.clone());

    Err(process_ctx.into())
}
```

## プラグインでのLogEntry送信

```rust
use mcv_tracing::capture_context;
use mcv_messages::{Message, MessageType, MessageSource, MessageDestination};

async fn on_message(
    &mut self,
    message: Message,
    host: Arc<dyn PluginHost>,
) -> Result<(), PluginError> {
    match process_message(&message).await {
        Ok(_) => Ok(()),
        Err(tracing_error) => {
            // ErrorContext から LogEntryPayload を作成
            let error_context = tracing_error.context();
            let mut payload = error_context.to_log_entry_payload();
            payload.plugin_version = Some(env!("CARGO_PKG_VERSION").to_string());
            payload.plugin_build_profile = Some("stable".to_string());

            // Core にログ送信
            let log_message = Message::new_notification(
                MessageType::LogEntry,
                MessageSource::Plugin { plugin_id: self.plugin_id },
                MessageDestination::Core,
                serde_json::to_value(payload)?,
            );
            host.send_message(log_message).await?;

            // エラーを手動で変換して伝播
            Err(PluginError::Other(tracing_error.to_string()))
        }
    }
}
```

## ErrorContext の構造

ErrorContext は以下の情報を自動的にキャプチャします：

- **message**: エラーメッセージ
- **source**: ソース位置情報
  - file: ファイルパス
  - line: 行番号
  - module_path: モジュールパス
- **stacktrace**: スタックトレース（最大10フレーム）
  - symbol: シンボル名
  - filename: ファイル名
  - lineno: 行番号
- **fields**: カスタムフィールド（HashMap）
- **timestamp**: タイムスタンプ（ミリ秒）

入れ子エラーを追加した場合、`fields.inner_error` に内部エラーコンテキスト全体が格納されます。

## plugin-dummy での実行例

### 基本的なエラーコンテキスト

```
error-context-test 0
```

### 入れ子のエラーコンテキスト

```
# ネットワークエラー → APIエラー
error-context-nested fetch

# ネットワークエラー → APIエラー → パースエラー
error-context-nested parse

# ネットワークエラー → APIエラー → パースエラー → ビジネスロジックエラー
error-context-nested process
```

これらのコマンドを実行すると、LogEntryメッセージが送信され、Core側で詳細なエラーコンテキストが記録されます。
