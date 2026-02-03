# manifest.json スキーマ

EXEプラグインの設定ファイル `manifest.json` のスキーマ仕様です。

## 配置場所

```
%LOCALAPPDATA%\MultiCommentViewer\plugins\
└── your-plugin-name/
    ├── manifest.json    ← このファイル
    └── your-plugin.exe
```

## スキーマ

現在のバージョンでは、シンプルなスキーマを採用しています。

```json
{
  "path": "your-plugin.exe"
}
```

### フィールド

#### `path` (必須)

- **型**: `string`
- **説明**: 実行ファイルへの相対パス（manifest.jsonからの相対パス）
- **例**:
  - `"your-plugin.exe"` - 同じディレクトリ内
  - `"bin/your-plugin.exe"` - binサブディレクトリ内

## プラグインIDの決定

プラグインIDは、manifest.jsonが配置されているディレクトリ名から自動的に生成されます。

**例:**
```
%LOCALAPPDATA%\MultiCommentViewer\plugins\my-awesome-plugin\
└── manifest.json
```

この場合、プラグインIDは `"my-awesome-plugin"` になります。

## プラグイン名の決定

プラグイン名は、実行ファイル名（拡張子なし）から自動的に生成されます。

**例:**
- `"path": "my-plugin.exe"` → プラグイン名: `"my-plugin"`
- `"path": "bin/awesome.exe"` → プラグイン名: `"awesome"`

## バリデーション

manifest.json読み込み時に以下のバリデーションが行われます：

1. `path` フィールドが存在すること
2. `path` が空文字列でないこと
3. 指定された実行ファイルが実際に存在すること（起動時）

バリデーションエラーが発生した場合、そのプラグインはスキップされ、ログにエラーが記録されます。

## 環境変数

EXEプラグインには以下の環境変数が自動的に設定されます：

- `MCV_WEBSOCKET_PORT`: WebSocketサーバーのポート番号（例: "28901"）
- `MCV_WEBSOCKET_URL`: WebSocketサーバーの完全なURL（例: "ws://127.0.0.1:28901"）

## 例

### 基本的な例

```json
{
  "path": "my-plugin.exe"
}
```

### サブディレクトリを使用する例

```json
{
  "path": "bin/my-plugin.exe"
}
```

## 将来の拡張

将来のバージョンでは、以下のフィールドが追加される可能性があります：

```json
{
  "schema_version": "1.0",
  "plugin": {
    "id": "com.example.my-plugin",
    "name": "My Plugin",
    "version": "1.0.0",
    "api_version": "v2",
    "description": "プラグインの説明",
    "author": "作者名",
    "roles": ["comment-provider"]
  },
  "executable": {
    "path": "bin/my-plugin.exe",
    "args": ["--debug"],
    "working_directory": "."
  },
  "websocket": {
    "auto_reconnect": true,
    "reconnect_interval_ms": 5000,
    "timeout_ms": 30000
  }
}
```

現時点では、この拡張スキーマは実装されていません。
