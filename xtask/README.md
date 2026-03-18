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
cargo xtask install --dir PATH [--channel alpha|beta|stable] [--plugin ID ...]
```

`apps/mcv` 本体と `--plugin` で指定したプラグインのみをデバッグビルドし、`--dir` に配置する。
プラグインは `{dir}/plugins/{id}/` 形式でデプロイされる（`get_dir_dll_plugin_path` 方式）。

| オプション     | 説明                                                          |
|--------------|---------------------------------------------------------------|
| `--dir PATH`  | インストール先ディレクトリ（例: `%LOCALAPPDATA%\MultiCommentViewer`） |
| `--channel`   | 配布チャンネル（デフォルト: `alpha`）                           |
| `--plugin ID` | インストールするプラグインの ID（複数指定可、省略可）             |

**例:**

```
cargo xtask install --dir "C:/Users/user/AppData/Local/MultiCommentViewer" --channel beta --plugin bouyomi --plugin twitch
```

---

### pack — プラグイン単体の ZIP 化

```
cargo xtask pack --plugin ID [--channel alpha|beta|stable]
```

指定プラグインをリリースビルドし、`output/` に ZIP ファイルを生成する。
ZIP 内には新形式の `plugin.json`（`id`, `name`, `description`, `version`, `channel`, `entry` フィールド）が含まれる。

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

---

## フロントエンドへの feature フラグ連動

`dist` / `install` でチャンネルを指定すると、xtask は `apps/mcv/src-tauri/Cargo.toml` の feature implies 関係を解析し、対応する `CARGO_FEATURE_*` 環境変数をフロントエンド（Vite）ビルドへ自動的に渡す。

### 仕組み

```
Cargo.toml の feature 定義
  alpha = ["comment-search", ...]
          ↓ cargo metadata で解析（channel_implied_features）
xtask が CARGO_FEATURE_COMMENT_SEARCH=1 を npm run build に渡す
          ↓
vite.config.ts が CARGO_FEATURE_COMMENT_SEARCH を読み取り
  __IS_SEARCH_ENABLED__ = true としてビルド時定数を注入
          ↓
App.tsx で __IS_SEARCH_ENABLED__ が true の時のみ検索タブを表示
```

### 対象となる feature

Cargo.toml の feature リスト内で、**純粋な feature 名**（`crate/feature` や `dep:crate` 形式でないもの）が自動的に `CARGO_FEATURE_<NAME>=1` に変換される。

| Cargo.toml のエントリ | 変換結果 | 備考 |
| --- | --- | --- |
| `"comment-search"` | `CARGO_FEATURE_COMMENT_SEARCH=1` | 対象（ハイフン → アンダースコア、大文字化） |
| `"mcv-log-core/alpha"` | （スキップ） | `crate/feature` 形式は除外 |
| `"dep:some-crate"` | （スキップ） | `dep:` 形式は除外 |

### 新しい alpha-only 機能を追加する場合

`Cargo.toml` に feature を追加するだけでよい。xtask や vite.config.ts の変更は不要。

```toml
# Cargo.toml
[features]
new-feature = []
alpha = ["comment-search", "new-feature", ...]  # ← ここに追加するだけ
```

フロントエンドでは `CARGO_FEATURE_NEW_FEATURE` 環境変数が自動的に渡されるため、`vite.config.ts` で参照できる。

```
MultiCommentViewer_v1.0.0_alpha.zip
├── MultiCommentViewer.exe
├── MultiCommentViewer.pdb
└── plugins/
    ├── bouyomi-{version}-alpha.zip
    ├── twitch-{version}-alpha.zip
    └── ...
```
