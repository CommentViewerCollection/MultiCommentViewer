# MultiCommentViewer (mcv) 配布APIサーバー仕様書

## 概要

MultiCommentViewerのインストーラおよびmcv本体が利用する配布APIサーバーの仕様を定義します。

### 目的

- インストーラ自身の更新配信
- mcv本体の更新配信
- プラグインの配布・更新配信

### ベースURL

```
https://api.example.com
```

**注意**: 本番環境では適切なドメインに変更してください。

---

## エンドポイント一覧

| エンドポイント | メソッド | 説明 |
|---------------|---------|------|
| `/installer/version` | GET | インストーラのバージョン情報取得 |
| `/mcv/version` | GET | mcv本体のバージョン情報取得 |
| `/plugins/list` | GET | 利用可能なプラグイン一覧取得 |

---

## 1. インストーラバージョン情報API

### エンドポイント

```
GET /installer/version
```

### 目的

インストーラ自身の最新バージョン情報を提供します。

### リクエスト

#### ヘッダー

```http
GET /installer/version HTTP/1.1
Host: api.example.com
User-Agent: McvInstaller/1.0.0
Accept: application/json
```

#### パラメータ

なし

### レスポンス

#### 成功時（200 OK）

```json
{
  "version": "1.2.0",
  "required": true,
  "download_url": "https://cdn.example.com/installer/installer-1.2.0.exe",
  "sha256": "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855",
  "release_notes": "バグ修正とパフォーマンス改善\n- プラグインインストール時のクラッシュを修正\n- ダウンロード速度の向上",
  "released_at": "2026-01-10T00:00:00Z"
}
```

#### フィールド説明

| フィールド | 型 | 必須 | 説明 |
|-----------|-----|------|------|
| `version` | string | ✓ | 最新バージョン番号（セマンティックバージョニング） |
| `required` | boolean | ✓ | 必須更新フラグ（常に`true`、更新しないと先に進めない） |
| `download_url` | string | ✓ | ダウンロードURL（HTTPS必須） |
| `sha256` | string | ✓ | SHA-256チェックサム（小文字16進数64文字） |
| `release_notes` | string | ✓ | リリースノート（改行は`\n`、Markdown不可） |
| `released_at` | string | ✓ | リリース日時（ISO 8601形式、UTC） |

#### エラーレスポンス

##### サーバーエラー（500 Internal Server Error）

```json
{
  "error": "Internal server error",
  "message": "Failed to fetch version information"
}
```

##### メンテナンス中（503 Service Unavailable）

```json
{
  "error": "Service unavailable",
  "message": "API is currently under maintenance"
}
```

---

## 2. mcv本体バージョン情報API

### エンドポイント

```
GET /mcv/version
```

### 目的

mcv本体の最新バージョン情報を提供します。

### リクエスト

#### ヘッダー

```http
GET /mcv/version HTTP/1.1
Host: api.example.com
User-Agent: McvInstaller/1.0.0
Current-Version: 0.1.0
Accept: application/json
```

#### カスタムヘッダー

| ヘッダー | 型 | 必須 | 説明 |
|---------|-----|------|------|
| `Current-Version` | string | × | 現在インストールされているmcvのバージョン（統計用） |

### レスポンス

#### 成功時（200 OK）

```json
{
  "version": "0.2.0",
  "download_url": "https://cdn.example.com/mcv/mcv-0.2.0.zip",
  "sha256": "a7f8b3e2c1d4f6e8a9b0c2d3e4f5a6b7c8d9e0f1a2b3c4d5e6f7a8b9c0d1e2f3",
  "release_notes": "新機能: プラグイン動的ロード対応\n\n変更点:\n- DLL形式のプラグインをサポート\n- プラグインの動的ロード/アンロード機能\n- 更新確認機能の追加\n\nバグ修正:\n- コメント表示の安定性向上",
  "released_at": "2026-01-12T00:00:00Z",
  "min_installer_version": "1.0.0"
}
```

#### フィールド説明

| フィールド | 型 | 必須 | 説明 |
|-----------|-----|------|------|
| `version` | string | ✓ | 最新バージョン番号（セマンティックバージョニング） |
| `download_url` | string | ✓ | ダウンロードURL（ZIP形式、HTTPS必須） |
| `sha256` | string | ✓ | SHA-256チェックサム（小文字16進数64文字） |
| `release_notes` | string | ✓ | リリースノート（改行は`\n`、Markdown不可） |
| `released_at` | string | ✓ | リリース日時（ISO 8601形式、UTC） |
| `min_installer_version` | string | ✓ | 必要なインストーラの最小バージョン |

#### ZIPファイルの構造

```
mcv-0.2.0.zip
├── mcv.exe              # mcv本体（Tauriアプリ）
└── （その他必要なファイル）
```

**重要**: ZIPファイルのルートに`mcv.exe`が配置されている必要があります。

---

## 3. プラグイン一覧API

### エンドポイント

```
GET /plugins/list
```

### 目的

利用可能なプラグインの一覧を提供します。

### リクエスト

#### ヘッダー

```http
GET /plugins/list HTTP/1.1
Host: api.example.com
User-Agent: McvInstaller/1.0.0
Accept: application/json
```

#### パラメータ

なし

### レスポンス

#### 成功時（200 OK）

```json
{
  "plugins": [
    {
      "id": "plugin-youtube",
      "name": "YouTube Live Plugin",
      "description": "YouTube Live配信のコメントを取得します",
      "version": "1.0.0",
      "download_url": "https://cdn.example.com/plugins/plugin-youtube-1.0.0.dll",
      "sha256": "b4c8d9e0f1a2b3c4d5e6f7a8b9c0d1e2f3a4b5c6d7e8f9a0b1c2d3e4f5a6b7c8",
      "file_size": 524288,
      "author": "mcv-dev",
      "license": "MIT",
      "released_at": "2026-01-05T00:00:00Z",
      "min_mcv_version": "0.2.0"
    },
    {
      "id": "plugin-twicas",
      "name": "ツイキャスプラグイン",
      "description": "ツイキャス（通常配信・プライベート配信）のコメントを取得します",
      "version": "1.1.0",
      "download_url": "https://cdn.example.com/plugins/plugin-twicas-1.1.0.dll",
      "sha256": "c5d6e7f8a9b0c1d2e3f4a5b6c7d8e9f0a1b2c3d4e5f6a7b8c9d0e1f2a3b4c5d6",
      "file_size": 614400,
      "author": "mcv-dev",
      "license": "MIT",
      "released_at": "2026-01-08T00:00:00Z",
      "min_mcv_version": "0.2.0"
    },
    {
      "id": "plugin-nicolive",
      "name": "ニコ生プラグイン",
      "description": "ニコニコ生放送のコメントを取得します",
      "version": "1.0.0",
      "download_url": "https://cdn.example.com/plugins/plugin-nicolive-1.0.0.dll",
      "sha256": "d7e8f9a0b1c2d3e4f5a6b7c8d9e0f1a2b3c4d5e6f7a8b9c0d1e2f3a4b5c6d7e8",
      "file_size": 1048576,
      "author": "mcv-dev",
      "license": "MIT",
      "released_at": "2026-01-10T00:00:00Z",
      "min_mcv_version": "0.2.0"
    }
  ]
}
```

#### フィールド説明（plugins配列の各要素）

| フィールド | 型 | 必須 | 説明 |
|-----------|-----|------|------|
| `id` | string | ✓ | プラグインID（一意識別子、ファイル名のベース、`plugin-*`形式推奨） |
| `name` | string | ✓ | プラグイン名（UI表示用、日本語可） |
| `description` | string | ✓ | 説明文（UI表示用、日本語可） |
| `version` | string | ✓ | バージョン番号（セマンティックバージョニング） |
| `download_url` | string | ✓ | ダウンロードURL（DLLファイル、HTTPS必須） |
| `sha256` | string | ✓ | SHA-256チェックサム（小文字16進数64文字） |
| `file_size` | number | ✓ | ファイルサイズ（バイト単位） |
| `author` | string | ✓ | 作者名（将来的にUI表示） |
| `license` | string | ✓ | ライセンス（例: "MIT", "Apache-2.0"） |
| `released_at` | string | ✓ | リリース日時（ISO 8601形式、UTC） |
| `min_mcv_version` | string | ✓ | 必要なmcvの最小バージョン |

#### 注意事項

- `plugins`配列は空でも構いません（プラグインが1つもない場合）
- プラグインの順序は任意（クライアント側でソート可能）
- プラグインIDは変更不可（バージョンアップ時も同じIDを使用）

---

## データ形式仕様

### バージョン番号

セマンティックバージョニング（`MAJOR.MINOR.PATCH`）に準拠します。

**例**:
- `1.0.0`: メジャーバージョン
- `1.2.0`: マイナーバージョン
- `1.2.3`: パッチバージョン

**比較規則**:
- `1.2.1` > `1.2.0`
- `1.3.0` > `1.2.9`
- `2.0.0` > `1.99.99`

### SHA-256チェックサム

**形式**: 小文字16進数、64文字

**例**: `e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855`

**生成方法**:
```bash
# Linux/macOS
sha256sum file.exe

# Windows (PowerShell)
Get-FileHash -Algorithm SHA256 file.exe | Select-Object -ExpandProperty Hash | ForEach-Object { $_.ToLower() }
```

**検証**:
クライアント側でダウンロードしたファイルのSHA-256ハッシュを計算し、APIレスポンスの値と一致することを確認します。

### 日時形式

ISO 8601形式（UTC）を使用します。

**形式**: `YYYY-MM-DDTHH:MM:SSZ`

**例**: `2026-01-12T00:00:00Z`

**注意**:
- タイムゾーンは常に`Z`（UTC）
- ミリ秒は省略可能

---

## セキュリティ要件

### 1. HTTPS必須

すべてのエンドポイントはHTTPS経由でのみアクセス可能にしてください。

### 2. CORS設定

APIサーバーは適切なCORSヘッダーを返す必要があります。

```http
Access-Control-Allow-Origin: *
Access-Control-Allow-Methods: GET, OPTIONS
Access-Control-Allow-Headers: Content-Type, Current-Version
```

### 3. レート制限

DoS攻撃を防ぐため、適切なレート制限を設定してください。

**推奨値**:
- 同一IPアドレスから: 100リクエスト/分
- グローバル: 10,000リクエスト/分

### 4. ファイル配信のセキュリティ

#### ダウンロードURL

- 直接アクセス可能なHTTPS URL
- CDNの使用を推奨
- 署名付きURL（オプション）

#### チェックサム検証

- クライアント側で必ずSHA-256検証を実施
- 不一致の場合はインストールを中止

### 5. コンテンツタイプ

適切なContent-Typeヘッダーを返してください。

```http
Content-Type: application/json; charset=utf-8
```

---

## エラーハンドリング

### HTTPステータスコード

| ステータスコード | 説明 | 使用ケース |
|-----------------|------|-----------|
| 200 OK | 成功 | 正常なレスポンス |
| 400 Bad Request | 不正なリクエスト | パラメータエラー |
| 404 Not Found | リソースが見つからない | 存在しないエンドポイント |
| 429 Too Many Requests | レート制限超過 | 過度なリクエスト |
| 500 Internal Server Error | サーバーエラー | 内部エラー |
| 503 Service Unavailable | サービス利用不可 | メンテナンス中 |

### エラーレスポンス形式

```json
{
  "error": "error_code",
  "message": "Human readable error message"
}
```

**例**:
```json
{
  "error": "rate_limit_exceeded",
  "message": "Too many requests. Please try again later."
}
```

---

## 配信ファイルの要件

### インストーラ（.exe）

- **ファイル形式**: Windows実行可能ファイル（.exe）
- **ファイル名**: `installer-{version}.exe`（例: `installer-1.2.0.exe`）
- **推奨サイズ**: 50MB以下
- **デジタル署名**: 推奨（将来実装）

### mcv本体（.zip）

- **ファイル形式**: ZIP圧縮
- **ファイル名**: `mcv-{version}.zip`（例: `mcv-0.2.0.zip`）
- **推奨サイズ**: 100MB以下
- **内容**: ZIPのルートに`mcv.exe`が必須

### プラグイン（.dll）

- **ファイル形式**: Windows DLL
- **ファイル名**: `{plugin-id}-{version}.dll`（例: `plugin-youtube-1.0.0.dll`）
- **推奨サイズ**: 10MB以下
- **C ABI**: プラグインインターフェース準拠

---

## バージョン管理戦略

### インストーラ

- セマンティックバージョニング準拠
- 必須更新（`required: true`）のみサポート
- 後方互換性は考慮不要（常に最新版を要求）

### mcv本体

- セマンティックバージョニング準拠
- `min_installer_version`で互換性管理
- 破壊的変更時はメジャーバージョンをインクリメント

### プラグイン

- セマンティックバージョニング準拠
- `min_mcv_version`で互換性管理
- 複数バージョンの同時配布は不可（常に最新版のみ）

---

## 実装例

### Node.js + Express

```javascript
const express = require('express');
const app = express();

// インストーラバージョン情報
app.get('/installer/version', (req, res) => {
  res.json({
    version: '1.2.0',
    required: true,
    download_url: 'https://cdn.example.com/installer/installer-1.2.0.exe',
    sha256: 'e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855',
    release_notes: 'バグ修正とパフォーマンス改善',
    released_at: new Date().toISOString()
  });
});

// mcvバージョン情報
app.get('/mcv/version', (req, res) => {
  const currentVersion = req.headers['current-version'];
  console.log(`Client version: ${currentVersion}`);

  res.json({
    version: '0.2.0',
    download_url: 'https://cdn.example.com/mcv/mcv-0.2.0.zip',
    sha256: 'a7f8b3e2c1d4f6e8a9b0c2d3e4f5a6b7c8d9e0f1a2b3c4d5e6f7a8b9c0d1e2f3',
    release_notes: '新機能: プラグイン動的ロード対応',
    released_at: new Date().toISOString(),
    min_installer_version: '1.0.0'
  });
});

// プラグイン一覧
app.get('/plugins/list', (req, res) => {
  res.json({
    plugins: [
      {
        id: 'plugin-youtube',
        name: 'YouTube Live Plugin',
        description: 'YouTube Live配信のコメントを取得します',
        version: '1.0.0',
        download_url: 'https://cdn.example.com/plugins/plugin-youtube-1.0.0.dll',
        sha256: 'b4c8d9e0f1a2b3c4d5e6f7a8b9c0d1e2f3a4b5c6d7e8f9a0b1c2d3e4f5a6b7c8',
        file_size: 524288,
        author: 'mcv-dev',
        license: 'MIT',
        released_at: new Date().toISOString(),
        min_mcv_version: '0.2.0'
      }
    ]
  });
});

app.listen(3000, () => {
  console.log('API server running on port 3000');
});
```

### Python + Flask

```python
from flask import Flask, jsonify, request
from datetime import datetime

app = Flask(__name__)

@app.route('/installer/version')
def installer_version():
    return jsonify({
        'version': '1.2.0',
        'required': True,
        'download_url': 'https://cdn.example.com/installer/installer-1.2.0.exe',
        'sha256': 'e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855',
        'release_notes': 'バグ修正とパフォーマンス改善',
        'released_at': datetime.utcnow().isoformat() + 'Z'
    })

@app.route('/mcv/version')
def mcv_version():
    current_version = request.headers.get('Current-Version')
    print(f'Client version: {current_version}')

    return jsonify({
        'version': '0.2.0',
        'download_url': 'https://cdn.example.com/mcv/mcv-0.2.0.zip',
        'sha256': 'a7f8b3e2c1d4f6e8a9b0c2d3e4f5a6b7c8d9e0f1a2b3c4d5e6f7a8b9c0d1e2f3',
        'release_notes': '新機能: プラグイン動的ロード対応',
        'released_at': datetime.utcnow().isoformat() + 'Z',
        'min_installer_version': '1.0.0'
    })

@app.route('/plugins/list')
def plugins_list():
    return jsonify({
        'plugins': [
            {
                'id': 'plugin-youtube',
                'name': 'YouTube Live Plugin',
                'description': 'YouTube Live配信のコメントを取得します',
                'version': '1.0.0',
                'download_url': 'https://cdn.example.com/plugins/plugin-youtube-1.0.0.dll',
                'sha256': 'b4c8d9e0f1a2b3c4d5e6f7a8b9c0d1e2f3a4b5c6d7e8f9a0b1c2d3e4f5a6b7c8',
                'file_size': 524288,
                'author': 'mcv-dev',
                'license': 'MIT',
                'released_at': datetime.utcnow().isoformat() + 'Z',
                'min_mcv_version': '0.2.0'
            }
        ]
    })

if __name__ == '__main__':
    app.run(port=3000)
```

---

## モックサーバーの構成

開発・テスト用のモックサーバーを用意することを推奨します。

### ディレクトリ構造

```
mock-api-server/
├── server.js                # APIサーバー本体
├── package.json
├── data/
│   ├── installer.json       # インストーラバージョン情報
│   ├── mcv.json            # mcvバージョン情報
│   └── plugins.json        # プラグイン一覧
└── files/                  # 配信ファイル（オプション）
    ├── installer-1.2.0.exe
    ├── mcv-0.2.0.zip
    └── plugins/
        ├── plugin-youtube-1.0.0.dll
        └── plugin-twicas-1.1.0.dll
```

### データファイル例

**data/installer.json**:
```json
{
  "version": "1.2.0",
  "required": true,
  "download_url": "https://cdn.example.com/installer/installer-1.2.0.exe",
  "sha256": "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855",
  "release_notes": "バグ修正とパフォーマンス改善",
  "released_at": "2026-01-10T00:00:00Z"
}
```

---

## テスト方法

### curlでのテスト

```bash
# インストーラバージョン確認
curl -X GET https://api.example.com/installer/version \
  -H "User-Agent: McvInstaller/1.0.0" \
  -H "Accept: application/json"

# mcvバージョン確認
curl -X GET https://api.example.com/mcv/version \
  -H "User-Agent: McvInstaller/1.0.0" \
  -H "Current-Version: 0.1.0" \
  -H "Accept: application/json"

# プラグイン一覧
curl -X GET https://api.example.com/plugins/list \
  -H "User-Agent: McvInstaller/1.0.0" \
  -H "Accept: application/json"
```

### ファイルダウンロードのテスト

```bash
# ファイルをダウンロード
curl -o installer.exe https://cdn.example.com/installer/installer-1.2.0.exe

# SHA-256チェックサムを検証
sha256sum installer.exe
# 出力: e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855  installer.exe
```

---

## FAQ

### Q1. バージョン比較はどのように行うべきですか？

A1. セマンティックバージョニングの規則に従って比較します。Rust実装では`semver`クレート、JavaScriptでは`semver`パッケージを使用できます。

### Q2. プラグインを削除したい場合はどうすればいいですか？

A2. `/plugins/list`のレスポンスから該当プラグインを削除してください。過去にインストールされたプラグインは自動的に削除されません（手動アンインストールが必要）。

### Q3. ダウンロードURLの有効期限はどうすればいいですか？

A3. CDNを使用する場合、永続的なURLを推奨します。署名付きURLを使用する場合は、有効期限を24時間以上に設定してください。

### Q4. 複数のmcvバージョンを同時に配布できますか？

A4. APIは常に最新バージョンのみを返します。複数バージョンの同時配布はサポートしていません。

### Q5. プラグインのベータ版はどう配布すればいいですか？

A5. 現在の仕様ではベータ版の配布はサポートしていません。将来的には`channel`フィールドの追加を検討しています。

---

## 変更履歴

| バージョン | 日付 | 変更内容 |
|-----------|------|---------|
| 1.0.0 | 2026-01-13 | 初版作成 |

---

## 参考資料

- [mcvインストーラ仕様書](./spec-installer.md)
- [セマンティックバージョニング 2.0.0](https://semver.org/lang/ja/)
- [ISO 8601 - 日付と時刻の表記](https://ja.wikipedia.org/wiki/ISO_8601)
- [SHA-256](https://ja.wikipedia.org/wiki/SHA-2)
