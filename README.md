# MultiCommentViewer (mcv)

複数の配信サイトのコメントを同時に取得・表示できるコメントビューアアプリケーション。

## 現在の実装状況

このMVP（最小実行可能製品）版では、以下の機能を実装しています:

- **Core機能**: プラグイン管理、メッセージング、コメント統合表示
- **Plugin-Host**: actixベースのプラグイン隔離実行環境
- **ダミープラグイン**: ランダムにコメントを生成する動作確認用プラグイン
- **Tauri + React UI**: 接続コントロールとコメント表示

## 必要な環境

- Rust 1.70以上
- Node.js 18以上
- npm または yarn

## セットアップ

### 1. プロジェクトのクローン

```bash
cd C:\Users\ryu\Downloads\mcv
```

### 2. 依存関係のインストール

#### Rust依存関係
プロジェクトルートで:
```bash
cargo build
```

#### フロントエンド依存関係
```bash
cd apps/mcv
npm install
```

## 開発モードで実行

```bash
cd apps/mcv
npm run tauri dev
```

これにより、以下が実行されます:
1. Reactアプリケーションが起動（ホットリロード有効）
2. Tauriアプリケーションが起動
3. ダミープラグインが自動的に登録される

## 使い方

1. アプリケーションが起動したら、「接続」ボタンをクリック
2. 1-5秒間隔でランダムにコメントが生成されて表示される
3. 「切断」ボタンでコメント生成を停止

## プロジェクト構成

```
mcv/
├── Cargo.toml              # Rust Workspace設定
├── .gitignore
├── docs/
│   └── specifications.md   # 仕様書
├── crates/                 # Rustクレート群
│   ├── mcv-messages/      # メッセージ型定義
│   ├── mcv-common/        # 共通ユーティリティ
│   ├── mcv-plugin-interface/  # プラグインインターフェース定義
│   └── mcv-core/          # Core機能
├── apps/
│   └── mcv/               # Tauriアプリケーション
│       ├── src-tauri/     # Rustバックエンド
│       └── src/           # Reactフロントエンド
└── packages/              # プラグイン置き場
    └── plugin-dummy/      # ダミーコメント生成プラグイン
```

## 技術スタック

- **フロントエンド**: Tauri 2.x + React + TypeScript + Vite + Tailwind CSS
- **バックエンド**: Rust + actix（Actor model） + Tauri
- **メッセージング**: actixメッセージパッシング、JSON（serde_json）
- **非同期ランタイム**: tokio

## トラブルシューティング

### ビルドエラー

Rustのバージョンが古い場合、以下でアップデートしてください:
```bash
rustup update
```

### フロントエンドエラー

node_modulesを削除して再インストール:
```bash
cd apps/mcv
rm -rf node_modules
npm install
```

## 今後の実装予定

- 実配信サイトプラグイン（YouTube Live、ツイキャス、ニコ生等）
- ブラウザ管理・Cookie取得機能
- 複雑なinput UI（URL入力、パスワード入力フォーム）
- プラグイン動的ロード
- コメント投稿機能
- 棒読みちゃん連携
- コメント遅延表示
- プラグイン配布サイト
- インストーラ/アップデータ

## ライセンス

未定
