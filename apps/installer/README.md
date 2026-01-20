# MultiCommentViewer Installer & Updater

MultiCommentViewerのインストーラー・アンインストーラー・更新管理ツール

## ビルド方法

### 必要な環境

- Rust (stable)
- Node.js
- Windows SDK (mt.exeが必要)

### 開発ビルド（UAC manifest付き）

```powershell
cd apps\installer
.\build-with-manifest.ps1
```

生成される実行ファイル: `target\debug\mcv-installer.exe`

### リリースビルド（UAC manifest付き）

```powershell
cd apps\installer
.\build-with-manifest.ps1 -Release
```

生成される実行ファイル: `target\release\mcv-installer.exe`

### 通常のビルド（UAC manifestなし）

```bash
cargo build -p mcv-installer
# または
cargo build -p mcv-installer --release
```

**注意:** 通常のビルドでは管理者権限が自動的に要求されないため、Windowsアプリ登録が失敗する可能性があります。

## 機能

### インストール

- mcv本体のダウンロード・インストール
- プラグインのダウンロード・インストール
- デスクトップショートカット作成
- スタートメニューエントリ作成
- Windowsアプリとして登録（「設定→アプリ→インストールされているアプリ」に表示）

### アンインストール

- mcv本体のアンインストール
- プラグインの削除
- ショートカット削除
- Windowsアプリ登録解除
- ユーザーデータ保持/全削除の選択

**アンインストール方法:**
1. Windowsの「設定→アプリ→インストールされているアプリ」から "MultiCommentViewer" をアンインストール
2. または、インストーラーを起動して「アンインストール」ボタンをクリック

### 更新管理

- mcv本体の更新チェック
- インストーラー自身の更新チェック
- プラグインの更新管理

## アーキテクチャ

- フロントエンド: React + TypeScript + Tailwind CSS
- バックエンド: Rust + Tauri 2.x
- UAC: Windows manifestファイルで管理者権限を要求

## ディレクトリ構造

```
apps/installer/
├── src/                        # React frontend
│   ├── screens/               # UI screens
│   │   ├── WelcomeScreen.tsx
│   │   ├── OptionsScreen.tsx
│   │   ├── InstallingScreen.tsx
│   │   ├── UninstallOptionsScreen.tsx
│   │   ├── UninstallingScreen.tsx
│   │   └── ...
│   └── types/                 # TypeScript types
├── src-tauri/                 # Rust backend
│   ├── src/
│   │   └── main.rs           # Tauri commands
│   ├── mcv-installer.exe.manifest  # UAC manifest
│   └── Cargo.toml
└── build-with-manifest.ps1   # Build script with UAC
```

## トラブルシューティング

### mt.exeが見つからない

Windows SDKをインストールしてください:
https://developer.microsoft.com/ja-jp/windows/downloads/windows-sdk/

### 管理者権限エラー

`build-with-manifest.ps1`を使用してビルドしてください。通常のビルドでは管理者権限が自動的に要求されません。
