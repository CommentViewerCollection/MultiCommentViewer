# MultiCommentViewer (mcv) インストーラ仕様書

**バージョン**: 1.0.0
**最終更新**: 2026-01-13
**ステータス**: 設計段階

---

## 目次

1. [概要](#概要)
2. [要件](#要件)
3. [ディレクトリ構造](#ディレクトリ構造)
4. [API仕様](#api仕様)
5. [インストーラUI仕様](#インストーラui仕様)
6. [アップデートフロー](#アップデートフロー)
7. [プラグイン動的ロード仕様](#プラグイン動的ロード仕様)
8. [セキュリティ](#セキュリティ)
9. [実装計画](#実装計画)

---

## 概要

MultiCommentViewer（mcv）のWindows向けインストーラ/アップデータは、以下の機能を提供する統合アプリケーションです:

- **新規インストール**: mcv本体とプラグインの初回インストール
- **アップデート**: mcv本体、プラグイン、インストーラ自身の更新
- **プラグイン管理**: プラグインの追加・削除・更新

### 主要な設計判断

| 項目 | 決定内容 | 理由 |
|------|----------|------|
| インストール先 | `%LOCALAPPDATA%\MultiCommentViewer\` | ユーザー毎インストール、管理者権限不要 |
| 設定保存先 | `%APPDATA%\MultiCommentViewer\` | ローミング可能、Windowsの標準的な場所 |
| プラグイン方式 | 動的ロード（DLL）、C ABI + FFI | mcv本体の更新なしでプラグイン追加可能、他言語対応可能 |
| アップデート方式 | 全体置き換え | シンプルで確実、差分アップデートは将来実装 |
| インストーラ実装 | Tauri 2.x + React + TypeScript | mcv本体と同じ技術スタック、柔軟なUI |
| プラグイン検証 | SHA-256チェックサム | 改ざん検出、コード署名は将来導入（費用面で現時点では見送り） |

---

## 要件

### 機能要件

#### FR-01: 新規インストール
- mcv本体のインストール
- プラグイン選択画面からプラグインを選択してインストール
- インストール完了後、mcvを起動可能

#### FR-02: アップデート
- mcv本体の更新確認と適用
- インストール済みプラグインの更新確認と適用
- 新規プラグインの追加

#### FR-03: インストーラ自身の更新
- インストーラ起動時に必須更新チェック
- 更新がある場合、更新しないと先に進めない（必須）
- ブートストラップ方式で自身を更新

#### FR-04: プラグイン管理
- プラグインの追加
- プラグインの削除（将来実装）
- プラグインの有効化/無効化（将来実装）

#### FR-05: エラーハンドリング
- ネットワークエラー時のリトライ（最大3回）
- ダウンロード失敗時の通知
- チェックサム不一致時の通知
- ディスク容量不足時の通知

### 非機能要件

#### NFR-01: パフォーマンス
- インストーラ起動時間: 3秒以内
- 更新確認: 5秒以内
- ダウンロード進捗表示: リアルタイム更新

#### NFR-02: セキュリティ
- すべての通信はHTTPS
- すべてのダウンロードファイルをSHA-256検証
- プラグインDLLロード前にチェックサム確認

#### NFR-03: ユーザビリティ
- 明確な進捗表示
- エラーメッセージはわかりやすく
- キャンセル不可操作は明示

---

## ディレクトリ構造

### インストール後の構成

```
%LOCALAPPDATA%\MultiCommentViewer\     (例: C:\Users\ryu\AppData\Local\MultiCommentViewer\)
├── mcv.exe                            # mcv本体（Tauriアプリ、約50MB）
├── installer.exe                      # インストーラ/アップデータ（約20MB）
├── plugins\                           # プラグインDLL置き場
│   ├── plugin_dummy.dll               # ダミープラグイン（テスト用、約2MB）
│   ├── plugin_youtube.dll             # YouTube Live プラグイン（約3MB）
│   ├── plugin_twicas.dll              # ツイキャス プラグイン（約2MB）
│   └── plugin_nicolive.dll            # ニコ生 プラグイン（約2MB）
├── cache\                             # 一時ダウンロードキャッシュ
│   ├── installer_update.exe           # インストーラ更新用
│   ├── mcv_update.zip                 # mcv本体更新用（約40MB）
│   └── plugin_*.dll                   # プラグイン更新用
└── uninstall.exe                      # アンインストーラ（将来実装）

%APPDATA%\MultiCommentViewer\          (例: C:\Users\ryu\AppData\Roaming\MultiCommentViewer\)
├── config.json                        # mcv設定ファイル
├── connections.json                   # 接続履歴
├── plugins.json                       # インストール済みプラグイン情報
├── logs\                              # ログファイル
│   ├── mcv_20260113.log              # mcv本体ログ（日次ローテーション）
│   └── installer_20260113.log        # インストーラログ
└── browser_profiles\                  # ブラウザプロファイル管理（将来実装）
```

### プラグイン情報ファイル（plugins.json）

インストール済みプラグインの情報を保存:

```json
{
  "plugins": [
    {
      "id": "plugin-dummy",
      "name": "ダミープラグイン",
      "version": "0.1.0",
      "installed_at": "2026-01-13T00:00:00Z",
      "dll_path": "C:\\Users\\ryu\\AppData\\Local\\MultiCommentViewer\\plugins\\plugin_dummy.dll",
      "enabled": true
    },
    {
      "id": "plugin-youtube",
      "name": "YouTube Live Plugin",
      "version": "1.0.0",
      "installed_at": "2026-01-13T00:10:00Z",
      "dll_path": "C:\\Users\\ryu\\AppData\\Local\\MultiCommentViewer\\plugins\\plugin_youtube.dll",
      "enabled": true
    }
  ]
}
```

---

## API仕様

インストーラは3種類の配布APIと通信します。すべての通信はHTTPSで行われます。

### 1. インストーラ配布API

インストーラ自身のバージョンチェックとダウンロード。

#### エンドポイント
```
GET https://api.example.com/installer/version
```

#### リクエストヘッダー
```http
User-Agent: McvInstaller/1.0.0
```

#### レスポンス
```json
{
  "version": "1.2.0",
  "required": true,
  "download_url": "https://cdn.example.com/installer/installer-1.2.0.exe",
  "sha256": "abc123...",
  "release_notes": "バグ修正とパフォーマンス改善",
  "released_at": "2026-01-10T00:00:00Z"
}
```

#### フィールド説明

| フィールド | 型 | 説明 |
|-----------|-----|------|
| `version` | string | 最新バージョン番号（セマンティックバージョニング） |
| `required` | boolean | 必須アップデートか（常に`true`） |
| `download_url` | string | ダウンロードURL（HTTPS必須） |
| `sha256` | string | SHA-256チェックサム（小文字16進数64文字） |
| `release_notes` | string | リリースノート（Markdown形式） |
| `released_at` | string | リリース日時（ISO 8601形式） |

#### エラーレスポンス

```json
{
  "error": "NETWORK_ERROR",
  "message": "サーバーに接続できませんでした"
}
```

---

### 2. mcv本体配布API

mcv本体のバージョンチェックとダウンロード。

#### エンドポイント
```
GET https://api.example.com/mcv/version
```

#### リクエストヘッダー
```http
User-Agent: McvInstaller/1.0.0
Current-Version: 0.1.0
```

#### レスポンス
```json
{
  "version": "0.2.0",
  "download_url": "https://cdn.example.com/mcv/mcv-0.2.0.zip",
  "sha256": "def456...",
  "release_notes": "## 新機能\n- プラグイン動的ロード対応\n\n## バグ修正\n- 接続タイムアウト問題を修正",
  "released_at": "2026-01-12T00:00:00Z",
  "min_installer_version": "1.0.0"
}
```

#### フィールド説明

| フィールド | 型 | 説明 |
|-----------|-----|------|
| `version` | string | 最新バージョン番号 |
| `download_url` | string | ダウンロードURL（ZIP形式、mcv.exeを含む） |
| `sha256` | string | SHA-256チェックサム |
| `release_notes` | string | リリースノート（Markdown形式） |
| `released_at` | string | リリース日時（ISO 8601形式） |
| `min_installer_version` | string | 必要なインストーラの最小バージョン |

#### ZIPファイル構造

```
mcv-0.2.0.zip
├── mcv.exe                 # mcv本体実行ファイル
├── README.md               # リリースノート
└── CHANGELOG.md            # 変更履歴
```

---

### 3. プラグイン配布API

利用可能なプラグイン一覧の取得。

#### エンドポイント
```
GET https://api.example.com/plugins/list
```

#### リクエストヘッダー
```http
User-Agent: McvInstaller/1.0.0
```

#### レスポンス
```json
{
  "plugins": [
    {
      "id": "plugin-youtube",
      "name": "YouTube Live Plugin",
      "description": "YouTube Live配信のコメントを取得します",
      "version": "1.0.0",
      "download_url": "https://cdn.example.com/plugins/plugin-youtube-1.0.0.dll",
      "sha256": "ghi789...",
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
      "sha256": "jkl012...",
      "file_size": 614400,
      "author": "mcv-dev",
      "license": "MIT",
      "released_at": "2026-01-08T00:00:00Z",
      "min_mcv_version": "0.2.0"
    }
  ]
}
```

#### フィールド説明

| フィールド | 型 | 説明 |
|-----------|-----|------|
| `id` | string | プラグインID（一意識別子、ファイル名のベース、小文字・ハイフン・数字のみ） |
| `name` | string | プラグイン名（UI表示用） |
| `description` | string | 説明文（100文字以内推奨） |
| `version` | string | バージョン番号（セマンティックバージョニング） |
| `download_url` | string | ダウンロードURL（DLLファイル、HTTPS必須） |
| `sha256` | string | SHA-256チェックサム |
| `file_size` | number | ファイルサイズ（バイト） |
| `author` | string | 作者名 |
| `license` | string | ライセンス（SPDX識別子推奨） |
| `released_at` | string | リリース日時（ISO 8601形式） |
| `min_mcv_version` | string | 必要なmcvの最小バージョン |

---

### 4. プラグインDLL API仕様（C ABI + FFI）

プラグインDLLはC ABIでエクスポートされた関数を提供します。これにより、Rust以外の言語でもプラグインを開発できます。

#### エクスポート関数

```c
// プラグインメタデータ取得
// 戻り値: JSON文字列の先頭ポインタ（NUL終端）
extern "C" const char* plugin_get_metadata();

// プラグイン初期化
// host_context: mcv側から渡されるコンテキスト（将来の拡張用、現在は未使用）
// 戻り値: 0=成功、非0=失敗
extern "C" int plugin_init(void* host_context);

// メッセージ送信（プラグイン→mcv）
// message_json: JSON形式のメッセージ文字列（NUL終端）
// 戻り値: 0=成功、非0=失敗
extern "C" int plugin_send_message(const char* message_json);

// メッセージ受信コールバック設定（mcv→プラグイン）
// callback: mcv側から呼ばれるコールバック関数
// 戻り値: 0=成功、非0=失敗
typedef void (*MessageCallback)(const char* message_json);
extern "C" int plugin_set_callback(MessageCallback callback);

// プラグイン終了
// 戻り値: 0=成功、非0=失敗
extern "C" int plugin_shutdown();
```

#### メタデータJSON仕様

`plugin_get_metadata()`が返すJSON:

```json
{
  "id": "plugin-youtube",
  "name": "YouTube Live Plugin",
  "version": "1.0.0",
  "api_version": "v2",
  "roles": ["youtube-live"]
}
```

| フィールド | 型 | 説明 |
|-----------|-----|------|
| `id` | string | プラグインID（配布APIのIDと一致） |
| `name` | string | プラグイン名 |
| `version` | string | バージョン番号 |
| `api_version` | string | プラグインAPIバージョン（現在は"v2"固定） |
| `roles` | array | プラグインの役割（配信サイト識別子のリスト） |

#### メッセージJSON仕様

プラグイン-mcv間のメッセージは、既存のmcv-messagesクレートの仕様に従います。詳細は`docs/specifications.md`を参照。

---

## インストーラUI仕様

インストーラはTauri + Reactで実装され、以下の7つの画面で構成されます。

### 画面フロー

```
[起動]
  ↓
[1. スプラッシュ画面] (2秒)
  ↓
[2. インストーラ更新確認画面]
  ├─更新あり→ ダウンロード→新インストーラ起動→終了
  └─更新なし→
      ↓
[3. インストール/アップデート判定画面]
  ├─新規インストール→ [4. プラグイン選択画面]
  └─アップデート→     [5. アップデート確認画面]
      ↓
[6. ダウンロード・インストール進捗画面]
  ↓
[7. 完了画面]
```

---

### 1. スプラッシュ画面（SplashScreen.tsx）

#### 目的
- アプリケーションのロゴ表示
- 初期化処理の視覚的フィードバック

#### UI要素
- **ロゴ**: mcvのアプリアイコン（中央、大サイズ）
- **テキスト**: "MultiCommentViewer Installer"（ロゴ下）
- **プログレスバー**: インデターミネート（無限ループ）
- **ステータステキスト**: "インストーラを初期化中..."

#### 画面遷移
- 自動的に次の画面へ（約2秒後）

---

### 2. インストーラ更新確認画面（InstallerUpdateScreen.tsx）

#### 目的
- インストーラ自身の更新確認
- 必須更新の適用

#### UI要素（更新なしの場合）
- **タイトル**: "更新確認中..."
- **プログレスバー**: インデターミネート
- **ステータステキスト**: "インストーラの更新を確認しています..."

#### UI要素（更新ありの場合）
- **タイトル**: "インストーラの更新があります"
- **バージョン表示**:
  ```
  現在のバージョン: v1.0.0
  最新バージョン:   v1.2.0
  ```
- **リリースノート**: スクロール可能なテキストエリア
- **更新ボタン**: "更新する"（青色、大きめ）
- **注意メッセージ**: "この更新は必須です。更新しないと続行できません。"（赤色）

#### 動作
1. 配布API `/installer/version` に接続
2. 現在のバージョンと比較
3. 更新がある場合:
   - 「更新する」ボタン押下→ダウンロード開始
   - ダウンロード進捗バー表示
   - SHA-256検証
   - `cache/installer_update.exe --replace-self` 起動
   - 自身を終了

#### 画面遷移
- 更新なし: 自動的に次の画面へ
- 更新あり: 新しいインストーラが起動

---

### 3. インストール/アップデート判定画面（InstallTypeScreen.tsx）

#### 目的
- 新規インストールかアップデートかを判定
- ユーザーに状況を明示

#### 動作
1. `%LOCALAPPDATA%\MultiCommentViewer\mcv.exe` の存在チェック
2. ファイルが存在する場合、バージョン取得（ファイルのメタデータから）
3. 判定結果に応じて画面遷移

#### UI要素（新規インストールの場合）
- **タイトル**: "ようこそ MultiCommentViewer へ"
- **メッセージ**: "初めてのインストールです。プラグインを選択してインストールします。"
- **次へボタン**: "次へ"

#### UI要素（アップデートの場合）
- **タイトル**: "アップデート"
- **メッセージ**: "既存のインストールが検出されました。"
- **バージョン表示**: "現在のバージョン: v0.1.0"
- **次へボタン**: "次へ"

#### 画面遷移
- 新規インストール: プラグイン選択画面へ
- アップデート: アップデート確認画面へ

---

### 4. プラグイン選択画面（PluginSelectScreen.tsx）

#### 目的
- 新規インストール時にプラグインを選択

#### UI要素
- **タイトル**: "プラグインを選択"
- **説明**: "インストールするプラグインを選択してください。後から追加することもできます。"
- **プラグインリスト**: チェックボックス付きリスト
  ```
  □ YouTube Live Plugin (v1.0.0)
    YouTube Live配信のコメントを取得します
    サイズ: 512 KB

  ☑ ツイキャスプラグイン (v1.1.0)
    ツイキャス（通常配信・プライベート配信）のコメントを取得します
    サイズ: 600 KB

  □ ニコ生プラグイン (v1.0.0)
    ニコニコ生放送のコメントを取得します
    サイズ: 550 KB
  ```
- **選択数表示**: "3個のプラグインのうち1個を選択"
- **戻るボタン**: "戻る"（グレー）
- **次へボタン**: "次へ"（青色）

#### 動作
1. 配布API `/plugins/list` からプラグイン一覧取得
2. プラグインをリスト表示（デフォルトは全てチェック済み）
3. ユーザーがチェックボックスで選択
4. 「次へ」押下で選択されたプラグインをダウンロード画面へ

#### 画面遷移
- 戻る: 前の画面へ
- 次へ: ダウンロード画面へ

---

### 5. アップデート確認画面（UpdateCheckScreen.tsx）

#### 目的
- mcv本体とプラグインの更新内容を表示
- 新規プラグイン追加の提案

#### UI要素
- **タイトル**: "アップデート内容"
- **説明**: "以下の更新が利用可能です。"
- **更新リスト**:
  ```
  ■ mcv本体
    現在: v0.1.0 → 最新: v0.2.0
    リリースノート:
    - プラグイン動的ロード対応
    - バグ修正

  ■ ツイキャスプラグイン
    現在: v1.0.0 → 最新: v1.1.0
    リリースノート:
    - プライベート配信対応

  ■ 新規プラグイン
    ☑ YouTube Live Plugin (v1.0.0) - 追加する
      YouTube Live配信のコメントを取得します
  ```
- **合計ダウンロードサイズ**: "合計: 約 42 MB"
- **戻るボタン**: "戻る"（グレー）
- **アップデート開始ボタン**: "アップデート開始"（青色、大きめ）

#### 動作
1. mcv本体の更新確認（配布API `/mcv/version`）
2. `%APPDATA%\MultiCommentViewer\plugins.json` からインストール済みプラグイン取得
3. 各プラグインの更新確認（配布API `/plugins/list`と比較）
4. 新規プラグインの提案（配布APIにあるがローカルにないプラグイン）

#### 画面遷移
- 戻る: 前の画面へ（判定画面へ）
- アップデート開始: ダウンロード画面へ

---

### 6. ダウンロード・インストール進捗画面（DownloadScreen.tsx）

#### 目的
- ダウンロードとインストールの進捗表示

#### UI要素
- **タイトル**: "ダウンロード中..."（ダウンロード時）/ "インストール中..."（インストール時）
- **全体進捗バー**: `[=========>    ] 60%`
- **進捗テキスト**: "3/5 完了"
- **個別ダウンロードリスト**:
  ```
  ✓ mcv本体 (mcv-0.2.0.zip) - 完了 (40.2 MB)
  ✓ ツイキャスプラグイン (plugin-twicas-1.1.0.dll) - 完了 (600 KB)
  → YouTube Live Plugin (plugin-youtube-1.0.0.dll) - ダウンロード中 40% (205/512 KB)
  □ ニコ生プラグイン (plugin-nicolive-1.0.0.dll) - 待機中 (550 KB)
  □ インストール処理 - 待機中
  ```
- **キャンセルボタン**: なし（キャンセル不可）

#### 動作フロー
1. **ダウンロード**:
   - mcv本体（ZIP）
   - 選択されたプラグイン（DLL）
   - 各ファイルのダウンロード進捗をリアルタイム表示
2. **SHA-256検証**:
   - ダウンロード完了後、チェックサムを検証
   - 不一致の場合、エラー表示してリトライ（最大3回）
3. **インストール**:
   - mcv本体: ZIPを展開して `%LOCALAPPDATA%\MultiCommentViewer\mcv.exe` に配置
   - プラグイン: DLLを `%LOCALAPPDATA%\MultiCommentViewer\plugins\` にコピー
   - `%APPDATA%\MultiCommentViewer\plugins.json` を更新

#### エラーハンドリング
- **ネットワークエラー**: "ダウンロード失敗: ネットワークエラー（リトライ 1/3）"
- **チェックサム不一致**: "ファイル検証失敗: ファイルが破損しています（リトライ 1/3）"
- **ディスク容量不足**: "エラー: ディスク容量が不足しています（必要: 50 MB、空き: 10 MB）"

#### 画面遷移
- 完了: 自動的に完了画面へ
- エラー: エラーダイアログ表示→リトライまたはキャンセル

---

### 7. 完了画面（CompleteScreen.tsx）

#### 目的
- インストール/アップデート完了の通知
- mcv起動またはインストーラ終了

#### UI要素
- **タイトル**: "インストール完了" / "アップデート完了"
- **メッセージ**: "MultiCommentViewerのインストールが完了しました。"
- **インストール内容サマリ**:
  ```
  インストール済み:
  - mcv本体 v0.2.0
  - YouTube Live Plugin v1.0.0
  - ツイキャスプラグイン v1.1.0
  - ニコ生プラグイン v1.0.0
  ```
- **mcvを起動ボタン**: "mcvを起動する"（青色、大きめ）
- **閉じるボタン**: "閉じる"（グレー）

#### 動作
- 「mcvを起動」押下: `%LOCALAPPDATA%\MultiCommentViewer\mcv.exe` を起動してインストーラ終了
- 「閉じる」押下: インストーラ終了

---

## アップデートフロー

### インストーラ自身のアップデート（ブートストラップ方式）

インストーラ自身を更新する仕組み。

#### フロー

```
[インストーラ起動]
       ↓
[バージョンチェック]
 API: /installer/version
       ↓
    更新あり?
    ├─No→ [次の画面へ]
    └─Yes→ [更新画面表示]
             ↓
        [ダウンロード]
        cache/installer_update.exe
             ↓
        [SHA-256検証]
             ↓
        [新インストーラ起動]
        --replace-self フラグ付き
             ↓
        [旧インストーラ終了]
             ↓
    [新インストーラが起動]
             ↓
        [旧installer.exeを上書き]
             ↓
        [cache/installer_update.exeを削除]
             ↓
        [通常フローに進む]
```

#### 実装詳細

**旧インストーラ側（更新前）**:
```rust
// 新しいインストーラをダウンロード
download("https://.../installer-1.2.0.exe", "cache/installer_update.exe");

// SHA-256検証
verify_checksum("cache/installer_update.exe", expected_sha256);

// 新しいインストーラを起動（--replace-selfフラグ付き）
std::process::Command::new("cache/installer_update.exe")
    .arg("--replace-self")
    .spawn()
    .expect("Failed to launch new installer");

// 旧インストーラを終了
std::process::exit(0);
```

**新インストーラ側（更新後）**:
```rust
// --replace-selfフラグの確認
if args.contains(&"--replace-self".to_string()) {
    // 旧installer.exeを上書き
    let old_installer_path = std::env::current_exe()
        .unwrap()
        .parent()
        .unwrap()
        .join("installer.exe");

    let new_installer_path = std::env::current_exe().unwrap();

    // ファイルコピー（上書き）
    std::fs::copy(&new_installer_path, &old_installer_path)?;

    // cache/installer_update.exeを削除
    std::fs::remove_file(&new_installer_path)?;

    // 通常フローに進む（フラグを除去）
}
```

---

### mcv本体のアップデート

#### A. mcv内部からの更新確認（ユーザー主導）

mcv起動時またはユーザー操作でアップデート確認。

##### フロー

```
[mcv起動]
       ↓
[更新確認]
 API: /mcv/version
       ↓
    更新あり?
    ├─No→ [通常起動]
    └─Yes→ [プロンプト表示]
             "新しいバージョン(v0.2.0)が利用可能です。
              今すぐ更新しますか？"
             [今すぐ更新] [後で]
             ↓
        [今すぐ更新]選択
             ↓
        [インストーラ起動]
        installer.exe --update-mcv
             ↓
        [mcv終了]
             ↓
    [インストーラが起動]
             ↓
        [mcv本体ダウンロード]
        cache/mcv_update.zip
             ↓
        [SHA-256検証]
             ↓
        [ZIP展開]
             ↓
        [mcv.exeを上書き]
             ↓
        [完了画面]
```

##### 実装詳細（mcv側）

**Tauriコマンド追加**:
```rust
#[tauri::command]
async fn check_for_updates() -> Result<Option<UpdateInfo>, String> {
    // mcv-updaterクレート使用
    let checker = UpdateChecker::new("https://api.example.com");
    checker.check_for_updates().await
}

#[tauri::command]
async fn launch_installer_for_update() -> Result<(), String> {
    // インストーラを起動
    let installer_path = std::env::current_exe()
        .unwrap()
        .parent()
        .unwrap()
        .join("installer.exe");

    std::process::Command::new(installer_path)
        .arg("--update-mcv")
        .spawn()
        .map_err(|e| e.to_string())?;

    // mcvを終了
    std::process::exit(0);
}
```

**Reactフロントエンド**:
```typescript
// 起動時に更新確認
useEffect(() => {
  const checkUpdates = async () => {
    const updateInfo = await invoke<UpdateInfo | null>('check_for_updates');
    if (updateInfo) {
      // プロンプト表示
      const result = await confirm(
        `新しいバージョン(${updateInfo.version})が利用可能です。今すぐ更新しますか？`
      );
      if (result) {
        await invoke('launch_installer_for_update');
      }
    }
  };

  checkUpdates();
}, []);
```

#### B. インストーラから直接アップデート

ユーザーがインストーラを直接起動した場合。

##### フロー

```
[installer.exe起動]
       ↓
[インストーラ更新確認] (必須)
       ↓
[既存インストール検出]
 %LOCALAPPDATA%\MultiCommentViewer\mcv.exe
       ↓
[アップデート確認画面]
 - mcv本体の更新確認
 - プラグインの更新確認
       ↓
[アップデート開始]
       ↓
[ダウンロード・インストール]
       ↓
[完了]
```

---

### プラグインのアップデート

インストーラのアップデート確認画面でプラグインの更新をチェック。

#### フロー

```
[アップデート確認画面]
       ↓
[plugins.json読み込み]
 インストール済みプラグイン一覧
       ↓
[API: /plugins/list]
 利用可能なプラグイン一覧
       ↓
[バージョン比較]
 インストール済み vs 最新
       ↓
    更新あり?
    ├─No→ [更新なしと表示]
    └─Yes→ [更新リストに追加]
             ↓
        [ダウンロード]
             ↓
        [SHA-256検証]
             ↓
        [plugins/にコピー]
             ↓
        [plugins.json更新]
```

#### プラグイン削除（将来実装）

将来的にプラグインの削除機能を追加:
- インストーラUIにプラグイン管理画面を追加
- 「削除」ボタンでDLLファイルを削除
- plugins.jsonから該当エントリを削除

---

## プラグイン動的ロード仕様

### 概要

mcv本体は起動時に`%LOCALAPPDATA%\MultiCommentViewer\plugins\`ディレクトリをスキャンし、すべての`.dll`ファイルを動的にロードします。

### mcv-plugin-loaderクレート

プラグインDLLの動的ロード、メタデータ取得、メッセージ送受信を担当。

#### 主要型

```rust
use libloading::{Library, Symbol};
use std::path::Path;
use std::ffi::{CStr, CString, c_void};

pub struct PluginLoader {
    library: Library,
    metadata: PluginMetadata,
}

#[derive(Debug, Clone)]
pub struct PluginMetadata {
    pub id: String,
    pub name: String,
    pub version: String,
    pub api_version: String,
    pub roles: Vec<String>,
}

pub enum PluginLoaderError {
    LoadFailed(String),
    MetadataError(String),
    InitFailed(String),
    MessageSendFailed(String),
}
```

#### 実装

```rust
impl PluginLoader {
    /// DLLをロードしてメタデータを取得
    pub fn load(path: &Path) -> Result<Self, PluginLoaderError> {
        // libloadingでDLLをロード
        let library = unsafe {
            Library::new(path)
                .map_err(|e| PluginLoaderError::LoadFailed(e.to_string()))?
        };

        // plugin_get_metadata()を呼び出し
        let get_metadata: Symbol<unsafe extern "C" fn() -> *const c_char> = unsafe {
            library.get(b"plugin_get_metadata\0")
                .map_err(|e| PluginLoaderError::MetadataError(e.to_string()))?
        };

        let metadata_json_ptr = unsafe { get_metadata() };
        let metadata_json_cstr = unsafe { CStr::from_ptr(metadata_json_ptr) };
        let metadata_json = metadata_json_cstr.to_str()
            .map_err(|e| PluginLoaderError::MetadataError(e.to_string()))?;

        // JSONをパース
        let metadata: PluginMetadata = serde_json::from_str(metadata_json)
            .map_err(|e| PluginLoaderError::MetadataError(e.to_string()))?;

        Ok(Self { library, metadata })
    }

    /// プラグイン初期化
    pub fn init(&self, host_context: *mut c_void) -> Result<(), PluginLoaderError> {
        let init_fn: Symbol<unsafe extern "C" fn(*mut c_void) -> i32> = unsafe {
            self.library.get(b"plugin_init\0")
                .map_err(|e| PluginLoaderError::InitFailed(e.to_string()))?
        };

        let result = unsafe { init_fn(host_context) };

        if result == 0 {
            Ok(())
        } else {
            Err(PluginLoaderError::InitFailed(format!("plugin_init returned {}", result)))
        }
    }

    /// メッセージ送信（mcv→プラグイン）
    pub fn send_message(&self, message: &str) -> Result<(), PluginLoaderError> {
        let send_fn: Symbol<unsafe extern "C" fn(*const c_char) -> i32> = unsafe {
            self.library.get(b"plugin_send_message\0")
                .map_err(|e| PluginLoaderError::MessageSendFailed(e.to_string()))?
        };

        let message_cstr = CString::new(message)
            .map_err(|e| PluginLoaderError::MessageSendFailed(e.to_string()))?;

        let result = unsafe { send_fn(message_cstr.as_ptr()) };

        if result == 0 {
            Ok(())
        } else {
            Err(PluginLoaderError::MessageSendFailed(format!("plugin_send_message returned {}", result)))
        }
    }

    /// コールバック設定（プラグイン→mcv）
    pub fn set_callback(&self, callback: extern "C" fn(*const c_char)) -> Result<(), PluginLoaderError> {
        let set_callback_fn: Symbol<unsafe extern "C" fn(extern "C" fn(*const c_char)) -> i32> = unsafe {
            self.library.get(b"plugin_set_callback\0")
                .map_err(|e| PluginLoaderError::InitFailed(e.to_string()))?
        };

        let result = unsafe { set_callback_fn(callback) };

        if result == 0 {
            Ok(())
        } else {
            Err(PluginLoaderError::InitFailed(format!("plugin_set_callback returned {}", result)))
        }
    }

    /// プラグイン終了
    pub fn shutdown(&self) -> Result<(), PluginLoaderError> {
        let shutdown_fn: Symbol<unsafe extern "C" fn() -> i32> = unsafe {
            self.library.get(b"plugin_shutdown\0")
                .map_err(|e| PluginLoaderError::InitFailed(e.to_string()))?
        };

        let result = unsafe { shutdown_fn() };

        if result == 0 {
            Ok(())
        } else {
            Err(PluginLoaderError::InitFailed(format!("plugin_shutdown returned {}", result)))
        }
    }
}
```

---

### mcv-core修正

#### plugin_manager.rs

DLLパスからプラグインをロードするように修正。

```rust
// 修正前
pub fn register_plugin(&mut self, plugin: Box<dyn Plugin>) -> Result<(Uuid, Addr<PluginHostActor>), String> {
    // ...
}

// 修正後
pub fn register_plugin(&mut self, dll_path: &Path) -> Result<(Uuid, Addr<PluginHostActor>), String> {
    // PluginLoaderでDLLをロード
    let loader = PluginLoader::load(dll_path)
        .map_err(|e| format!("Failed to load plugin DLL: {:?}", e))?;

    let metadata = loader.metadata.clone();
    let plugin_id = Uuid::new_v4();

    // PluginHostActorにloaderを渡す
    let host_addr = PluginHostActor::new(loader).start();

    // PluginInfoを作成
    let plugin_info = PluginInfo {
        name: metadata.name.clone(),
        plugin_id,
        role: metadata.roles.clone(),
        api_version: metadata.api_version.clone(),
        host_addr: host_addr.clone(),
    };

    self.plugins.insert(plugin_id, plugin_info);

    Ok((plugin_id, host_addr))
}
```

#### plugin_host_actor.rs

`PluginLoader`を保持し、メッセージを転送。

```rust
pub struct PluginHostActor {
    plugin_loader: PluginLoader,
    core_addr: Option<Addr<CoreActor>>,
}

impl PluginHostActor {
    pub fn new(plugin_loader: PluginLoader) -> Self {
        Self {
            plugin_loader,
            core_addr: None,
        }
    }
}

impl Handler<SendMessageToPlugin> for PluginHostActor {
    type Result = ();

    fn handle(&mut self, msg: SendMessageToPlugin, _ctx: &mut Self::Context) {
        let message_json = serde_json::to_string(&msg.message).unwrap();

        if let Err(e) = self.plugin_loader.send_message(&message_json) {
            eprintln!("Failed to send message to plugin: {:?}", e);
        }
    }
}
```

---

### プラグイン側修正（plugin-dummy等）

既存のPlugin trait実装を維持しつつ、C ABI関数をエクスポート。

#### src/lib.rs

```rust
mod plugin_impl;
use plugin_impl::DummyPlugin;
use std::ffi::{CStr, CString, c_char, c_void};
use std::sync::Mutex;

// グローバルにプラグインインスタンスを保持
static PLUGIN_INSTANCE: Mutex<Option<DummyPlugin>> = Mutex::new(None);
static CALLBACK: Mutex<Option<extern "C" fn(*const c_char)>> = Mutex::new(None);

#[no_mangle]
pub extern "C" fn plugin_get_metadata() -> *const c_char {
    let metadata = r#"{
        "id": "plugin-dummy",
        "name": "Dummy Plugin",
        "version": "0.1.0",
        "api_version": "v2",
        "roles": ["dummy"]
    }"#;

    CString::new(metadata).unwrap().into_raw()
}

#[no_mangle]
pub extern "C" fn plugin_init(_host_context: *mut c_void) -> i32 {
    let mut instance = PLUGIN_INSTANCE.lock().unwrap();
    *instance = Some(DummyPlugin::new());
    0 // 成功
}

#[no_mangle]
pub extern "C" fn plugin_send_message(message_json: *const c_char) -> i32 {
    let message_json_cstr = unsafe { CStr::from_ptr(message_json) };
    let message_json_str = match message_json_cstr.to_str() {
        Ok(s) => s,
        Err(_) => return -1,
    };

    // JSONをパース
    let message: mcv_messages::Message = match serde_json::from_str(message_json_str) {
        Ok(m) => m,
        Err(_) => return -1,
    };

    // DummyPlugin::on_message()を呼び出し
    let mut instance = PLUGIN_INSTANCE.lock().unwrap();
    if let Some(ref mut plugin) = *instance {
        // PluginHost実装を渡す必要があるが、C ABIでは難しい
        // 代わりにコールバック関数を使用
        let host = CApiPluginHost;

        if let Err(e) = plugin.on_message(message, &host).await {
            eprintln!("Plugin on_message error: {:?}", e);
            return -1;
        }
    }

    0 // 成功
}

#[no_mangle]
pub extern "C" fn plugin_set_callback(callback: extern "C" fn(*const c_char)) -> i32 {
    let mut cb = CALLBACK.lock().unwrap();
    *cb = Some(callback);
    0 // 成功
}

#[no_mangle]
pub extern "C" fn plugin_shutdown() -> i32 {
    let mut instance = PLUGIN_INSTANCE.lock().unwrap();
    *instance = None;
    0 // 成功
}

// PluginHost実装（コールバック経由でmcvにメッセージ送信）
struct CApiPluginHost;

#[async_trait]
impl mcv_plugin_interface::PluginHost for CApiPluginHost {
    async fn send_message(&self, message: mcv_messages::Message) -> Result<(), mcv_plugin_interface::PluginError> {
        let message_json = serde_json::to_string(&message).unwrap();
        let message_cstr = CString::new(message_json).unwrap();

        let callback = CALLBACK.lock().unwrap();
        if let Some(cb) = *callback {
            cb(message_cstr.as_ptr());
        }

        Ok(())
    }
}
```

#### Cargo.toml修正

```toml
[lib]
crate-type = ["cdylib"]  # DLLとしてビルド
```

---

## セキュリティ

### チェックサム検証

すべてのダウンロードファイルをSHA-256で検証。

#### 実装

```rust
use sha2::{Sha256, Digest};
use std::fs::File;
use std::io::{Read, BufReader};

pub fn verify_checksum(file_path: &Path, expected_sha256: &str) -> Result<bool, std::io::Error> {
    let mut file = BufReader::new(File::open(file_path)?);
    let mut hasher = Sha256::new();
    let mut buffer = [0; 4096];

    loop {
        let bytes_read = file.read(&mut buffer)?;
        if bytes_read == 0 {
            break;
        }
        hasher.update(&buffer[..bytes_read]);
    }

    let result = hasher.finalize();
    let calculated_hash = format!("{:x}", result);

    Ok(calculated_hash == expected_sha256)
}
```

### HTTPS通信

配布APIはHTTPSのみ許可。reqwestのデフォルト設定でTLS証明書を検証。

```rust
use reqwest::Client;

let client = Client::builder()
    .https_only(true)  // HTTPS強制
    .build()
    .unwrap();
```

### プラグインDLL検証

DLLロード前にチェックサムを確認。

```rust
// プラグイン情報取得
let plugin_info = get_plugin_info_from_plugins_json(plugin_id);

// チェックサム検証
if !verify_checksum(&plugin_dll_path, &plugin_info.sha256)? {
    return Err("Plugin DLL checksum mismatch".to_string());
}

// DLLロード
let loader = PluginLoader::load(&plugin_dll_path)?;
```

---

## 実装計画

### フェーズ1: プラグイン動的ロード対応（2-3日）

#### タスク
1. **mcv-plugin-loaderクレート作成**
   - `crates/mcv-plugin-loader/Cargo.toml` 作成
   - `crates/mcv-plugin-loader/src/lib.rs` 実装
   - libloadingでDLLロード
   - C ABI関数呼び出し
   - メタデータ取得

2. **mcv-core修正**
   - `crates/mcv-core/src/plugin_manager.rs` 修正
   - `crates/mcv-core/src/plugin_host_actor.rs` 修正
   - DLLロード対応

3. **plugin-dummyをDLL化**
   - `crates/plugin-dummy/src/lib.rs` にC ABIエクスポート追加
   - `crates/plugin-dummy/Cargo.toml` に `crate-type = ["cdylib"]` 追加
   - ビルドテスト

4. **mcv本体でDLLロードテスト**
   - `apps/mcv/src-tauri/src/main.rs` 修正
   - `plugins/` ディレクトリからDLLロード
   - 既存機能の動作確認

#### 検証
- DummyPluginがDLLとしてロードされること
- 既存のコメント生成機能が動作すること
- プラグイン-mcv間のメッセージング動作確認

---

### フェーズ2: mcv-updaterクレート実装（1-2日）

#### タスク
1. **mcv-updaterクレート作成**
   - `crates/mcv-updater/Cargo.toml` 作成
   - `crates/mcv-updater/src/lib.rs` 実装
   - UpdateChecker実装
   - API通信（reqwest）
   - バージョン比較（semver）

2. **ダウンロード機能**
   - ファイルダウンロード
   - 進捗コールバック
   - SHA-256検証

3. **ユニットテスト**
   - モックAPIでテスト
   - チェックサム検証テスト

#### 検証
- 配布APIに接続してバージョン取得
- ファイルダウンロードと進捗表示
- SHA-256検証の正常動作

---

### フェーズ3: mcv本体に更新確認機能追加（1日）

#### タスク
1. **Tauriコマンド追加**
   - `check_for_updates` コマンド実装
   - `launch_installer_for_update` コマンド実装

2. **React UI実装**
   - 更新プロンプトダイアログ
   - 起動時の更新確認ロジック

#### 検証
- mcv起動時に更新確認が実行される
- 更新プロンプトが正しく表示される
- インストーラが起動される

---

### フェーズ4: インストーラアプリケーション実装（3-5日）

#### タスク
1. **Tauriプロジェクトセットアップ**
   - `apps/installer` ディレクトリ作成
   - Tauri CLI で初期化
   - React + TypeScript + Vite セットアップ
   - Tailwind CSS設定

2. **Tauriバックエンド実装**
   - `apps/installer/src-tauri/src/main.rs` 実装
   - Tauriコマンド実装:
     - `check_installer_update`
     - `check_mcv_update`
     - `list_plugins`
     - `download_file`
     - `verify_checksum`
     - `install_mcv`
     - `install_plugin`

3. **React UI実装**
   - 7つの画面コンポーネント作成
   - 画面遷移ロジック
   - プログレスバー
   - エラーハンドリング

4. **インストーラ自身の更新機能**
   - ブートストラップ方式実装
   - `--replace-self` フラグ処理

5. **統合テスト**
   - 新規インストールシナリオ
   - アップデートシナリオ
   - エラーハンドリング

#### 検証
- 全画面フローのテスト
- ダウンロード進捗の表示確認
- エラーケースの動作確認

---

### フェーズ5: ドキュメント更新とテスト（1日）

#### タスク
1. **docs/specifications.md 更新**
   - プラグイン動的ロード関連を追記

2. **CLAUDE.md 更新**
   - インストーラ関連の情報追記

3. **統合テスト**
   - プラグインDLLロード統合テスト
   - インストーラ新規インストールテスト
   - インストーラアップデートテスト
   - インストーラ自身の更新テスト
   - mcv内部からの更新テスト

---

## 付録

### 用語集

| 用語 | 説明 |
|------|------|
| mcv | MultiCommentViewer本体アプリケーション |
| インストーラ | mcvとプラグインのインストール/アップデータアプリケーション |
| プラグイン | 配信サイト対応の拡張機能（DLL形式） |
| 配布API | インストーラが通信するバージョン情報・ダウンロードURL提供API |
| C ABI | C言語の関数呼び出し規約、異なる言語間のFFIに使用 |
| SHA-256 | 暗号学的ハッシュ関数、ファイルの改ざん検出に使用 |
| ブートストラップ | プログラム自身が自身を更新する仕組み |

---

### 参考資料

- [Tauri公式ドキュメント](https://tauri.app/docs)
- [libloading crateドキュメント](https://docs.rs/libloading/)
- [reqwest crateドキュメント](https://docs.rs/reqwest/)
- [semver crateドキュメント](https://docs.rs/semver/)
- [SHA-256について](https://en.wikipedia.org/wiki/SHA-2)

---

**文書履歴**:
- v1.0.0 (2026-01-13): 初版作成
