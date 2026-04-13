# MultiCommentViewer 配布 API サーバー仕様書

## 概要

インストーラおよび mcv 本体が利用する配布 API サーバーの仕様を定義します。

### 目的

- mcv 本体の更新配信
- プラグインの配布・更新配信
- エラーログの収集（→ [ログシステム](./logging-system.md) 参照）

### ベース URL

```
https://int-main.net
```

---

## エンドポイント一覧

| エンドポイント | メソッド | 説明 |
| --- | --- | --- |
| `/mcv/version` | GET | mcv 本体の最新バージョン情報取得 |
| `/plugins/list` | GET | 利用可能なプラグイン一覧取得 |
| `/api/mcv/logs` | POST | エラーログの送信 |
| `/api/mcv/logs` | GET | ログの取得（ログビューア用） |
| `/api/mcv/logs` | DELETE | ログの削除 |
| `/api/mcv/health` | GET | ヘルスチェック |

---

## 1. mcv 本体バージョン情報 API

```
GET /mcv/version
```

mcv 本体の最新バージョン情報を提供します。mcv-updater クレートが起動時に呼び出します。

### リクエストヘッダー

```http
User-Agent: McvInstaller/1.0.0
Current-Version: 0.8.1
Accept: application/json
```

| ヘッダー | 説明 |
| --- | --- |
| `Current-Version` | 現在インストール済みバージョン（統計用） |

### 成功レスポンス（200 OK）

```json
{
  "version": "0.9.0",
  "download_url": "https://cdn.example.com/mcv/mcv-0.9.0.zip",
  "sha256": "a7f8b3e2c1d4f6e8a9b0c2d3e4f5a6b7c8d9e0f1a2b3c4d5e6f7a8b9c0d1e2f3",
  "release_notes": "新機能: ...",
  "released_at": "2026-01-12T00:00:00Z",
  "min_installer_version": "1.0.0"
}
```

| フィールド | 型 | 説明 |
| --- | --- | --- |
| `version` | string | 最新バージョン番号（セマンティックバージョニング） |
| `download_url` | string | ZIP ファイルのダウンロード URL（HTTPS 必須） |
| `sha256` | string | SHA-256 チェックサム（小文字16進数64文字） |
| `release_notes` | string | リリースノート（改行は `\n`） |
| `released_at` | string | リリース日時（ISO 8601 UTC） |
| `min_installer_version` | string | 必要なインストーラの最小バージョン |

#### ZIP ファイルの構造

```
mcv-0.9.0.zip
└── mcv.exe    # mcv 本体（Tauri アプリ）
```

---

## 2. プラグイン一覧 API

```
GET /plugins/list
```

### レスポンス（200 OK）

```json
{
  "plugins": [
    {
      "id": "plugin-youtube-live",
      "name": "YouTube Live プラグイン",
      "description": "YouTube Live 配信のコメントを取得します",
      "version": "1.0.0",
      "download_url": "https://cdn.example.com/plugins/plugin-youtube-live-1.0.0.dll",
      "sha256": "b4c8d9e0f1a2b3c4d5e6f7a8b9c0d1e2f3a4b5c6d7e8f9a0b1c2d3e4f5a6b7c8",
      "file_size": 524288,
      "author": "mcv-dev",
      "license": "MIT",
      "released_at": "2026-01-05T00:00:00Z",
      "min_mcv_version": "0.8.0"
    }
  ]
}
```

| フィールド | 型 | 説明 |
| --- | --- | --- |
| `id` | string | プラグイン ID（一意識別子、変更不可） |
| `name` | string | プラグイン名（UI 表示用） |
| `description` | string | 説明文 |
| `version` | string | バージョン番号 |
| `download_url` | string | DLL ファイルのダウンロード URL |
| `sha256` | string | SHA-256 チェックサム |
| `file_size` | number | ファイルサイズ（バイト） |
| `author` | string | 作者名 |
| `license` | string | ライセンス（例: "MIT"） |
| `released_at` | string | リリース日時（ISO 8601 UTC） |
| `min_mcv_version` | string | 必要な mcv の最小バージョン |

---

## 3. ログ API

ログ送信・取得・削除の仕様は [ログシステム](./logging-system.md) を参照してください。

---

## データ形式仕様

### バージョン番号

セマンティックバージョニング（`MAJOR.MINOR.PATCH`）に準拠。

### SHA-256 チェックサム

小文字16進数64文字。クライアント側でダウンロードしたファイルと必ず照合する。

```bash
# Linux / macOS
sha256sum file.exe

# Windows (PowerShell)
Get-FileHash -Algorithm SHA256 file.exe | Select-Object -ExpandProperty Hash | ForEach-Object { $_.ToLower() }
```

### 日時形式

ISO 8601 UTC: `YYYY-MM-DDTHH:MM:SSZ`（例: `2026-01-12T00:00:00Z`）

---

## セキュリティ要件

- すべての通信は HTTPS 必須
- すべてのダウンロードファイルは SHA-256 で検証
- チェックサム不一致の場合はインストールを中止

---

## エラーレスポンス形式

```json
{
  "error": "error_code",
  "message": "Human readable error message"
}
```

| ステータスコード | 説明 |
| --- | --- |
| 200 | 成功 |
| 400 | 不正なリクエスト |
| 404 | リソースが見つからない |
| 429 | レート制限超過 |
| 500 | サーバーエラー |
| 503 | メンテナンス中 |
