# xtask

ビルド・パッケージング・配布の全工程を管理する内部 CLI ツール。

## 前提

リポジトリルートで `cargo xtask <command>` として実行する。
プラグインのメタデータは `tools/plugins.json` で管理されている。

## コマンド一覧

### build — ワークスペースビルド

```
cargo xtask build [--release]
```

Tauri アプリを含むワークスペース全体をビルドする。
フロントエンド（npm）の変更も自動検知してビルドする。

| オプション  | 説明                                  |
|------------|---------------------------------------|
| `--release` | リリースビルド（省略時はデバッグビルド） |

---

### install — ローカルデバッグインストール

```
cargo xtask install --dir PATH [--plugin ID ...]
```

ワークスペース全体（Tauri アプリ含む）をデバッグビルドし、mcv 本体と指定プラグインを `--dir` に配置する。
プラグインは `{dir}/plugins/{id}/` 形式でデプロイされる（`get_dir_dll_plugin_path` 方式）。

| オプション     | 説明                                                          |
|--------------|---------------------------------------------------------------|
| `--dir PATH`  | インストール先ディレクトリ（例: `%LOCALAPPDATA%\MultiCommentViewer`） |
| `--plugin ID` | インストールするプラグインの ID（複数指定可、省略可）             |

**例:**

```
cargo xtask install --dir "C:/Users/user/AppData/Local/MultiCommentViewer" --plugin bouyomi --plugin twitch
```

---

### pack — プラグイン単体の ZIP 化

```
cargo xtask pack --plugin ID [--channel alpha|beta|stable]
```

指定プラグインをリリースビルドし、`output/` に ZIP ファイルを生成する。
ZIP 内には新形式の `plugin.json`（`id`, `name`, `description`, `version`, `channel`, `entry`, `api` フィールド）が含まれる。

| オプション      | 説明                                          |
|---------------|-----------------------------------------------|
| `--plugin ID`  | プラグイン ID（`tools/plugins.json` に定義）     |
| `--channel`    | 配布チャンネル（デフォルト: `alpha`）             |

**例:**

```
cargo xtask pack --plugin bouyomi --channel beta
```

**生成物:** `output/bouyomi-{version}-beta.zip`

---

### dist — 配布用バンドル生成

```
cargo xtask dist [--channel alpha|beta|stable] [--plugin ID ...]
```

apps/mcv をリリースビルドし、指定プラグインを ZIP 化して最終的な配布 ZIP を `output/` に生成する。

| オプション      | 説明                                                                |
|---------------|---------------------------------------------------------------------|
| `--channel`    | 配布チャンネル（デフォルト: `alpha`）                                  |
| `--plugin ID`  | 対象プラグイン ID（複数指定可。**省略時は `tools/plugins.json` の全プラグイン**） |

**例:**

```sh
# 全プラグインを含む alpha 配布 ZIP を生成
cargo xtask dist --channel alpha

# 特定プラグインのみ含む beta 配布 ZIP を生成
cargo xtask dist --channel beta --plugin bouyomi --plugin twitch
```

**生成物:** `output/MultiCommentViewer_v{version}_{channel}.zip`

```
MultiCommentViewer_v1.0.0_alpha.zip
├── MultiCommentViewer.exe
├── MultiCommentViewer.pdb
└── plugins/
    ├── bouyomi-{version}-alpha.zip
    ├── twitch-{version}-alpha.zip
    └── ...
```
