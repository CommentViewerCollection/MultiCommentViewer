# MCV EXE Plugin Sample

MultiCommentViewer (MCV) のEXEプラグイン開発・デバッグ用のTauriベースGUIツールです。

## 概要

このツールは、EXEプラグインとMCVコア間のWebSocket通信をテストし、メッセージフローをリアルタイムで確認できるデバッグツールです。

## 機能

### 1. プラグインタブ
- 登録済みプラグインの一覧表示
- `plugin-hello` メッセージの送信
- `plugin-removed` メッセージの送信

### 2. コネクションタブ
- 登録済みコネクションの一覧表示
- `add-connection` メッセージの送信
- `connect` / `disconnect` / `remove-connection` メッセージの送信
- `rename-connection` メッセージの送信

### 3. コメントタブ
- `send-comment` メッセージの送信（コメント投稿依頼）
- `comment-received` メッセージの送信（テスト用）

### 4. 生メッセージタブ
- JSON形式でメッセージを直接入力して送信
- テンプレート機能
- JSONフォーマッター

### 5. ログタブ
- 受信したすべてのメッセージをリアルタイム表示
- フィルター機能（メッセージタイプ、送信元、宛先）
- 詳細なペイロード表示

## セットアップ

### 前提条件
- Rust 1.70+
- Node.js 18+
- Tauri CLI

### インストール

```bash
# 依存関係のインストール
cd apps/exe-plugin-sample
npm install

# 開発モード起動
npm run tauri dev

# ビルド
npm run tauri build
```

## 使い方

1. **MCVアプリを起動**
   - `plugin-exe-manager` が自動的にWebSocketサーバー（デフォルト: `ws://127.0.0.1:28901`）を起動します

2. **exe-plugin-sampleを起動**
   - WebSocket URL: `ws://127.0.0.1:28901`（デフォルト）
   - プラグイン名: 任意の名前
   - 「接続」ボタンをクリック

3. **メッセージの送受信**
   - 各タブから各種メッセージを送信
   - ログタブでリアルタイムに受信メッセージを確認

## アーキテクチャ

```
exe-plugin-sample (Tauri App)
  ├── src-tauri/ (Rust backend)
  │   ├── main.rs - WebSocketクライアント実装
  │   └── Cargo.toml
  └── src/ (React frontend)
      ├── App.tsx - メインコンポーネント
      ├── components/
      │   ├── PluginTab.tsx
      │   ├── ConnectionTab.tsx
      │   ├── CommentTab.tsx
      │   ├── RawMessageTab.tsx
      │   └── LogViewTab.tsx
      └── types.ts - 型定義
```

### 通信フロー

```
React Frontend
  ↕ Tauri Commands/Events
Rust Backend (ExePluginClient)
  ↕ WebSocket (JSON)
plugin-exe-manager (DLL)
  ↕ actix messages
MCV Core
```

## 開発

### Tauriコマンド
- `connect_to_mcv(url, plugin_name, roles)` - WebSocket接続
- `disconnect_from_mcv()` - WebSocket切断
- `send_message(message_json)` - メッセージ送信
- `get_connection_status()` - 接続状態取得
- `get_plugin_id()` - plugin_id取得

### Tauriイベント
- `message-received` - メッセージ受信時

## トラブルシューティング

### 接続できない
- MCVアプリが起動しているか確認
- `plugin-exe-manager` が正しくロードされているか確認
- WebSocketポート（28901）が使用可能か確認

### メッセージが受信できない
- ログタブでメッセージを確認
- ブラウザの開発者ツール（DevTools）でエラーを確認
- Rustバックエンドのログを確認

## ライセンス

このプロジェクトはMultiCommentViewerの一部です。
