# apps/mcv と全プラグインをデバッグビルドしてローカルにインストールする
#
# cargo xtask install がフルビルド（Tauri 含む）とデプロイを一括で行う。

$ErrorActionPreference = "Stop"

cargo xtask install `
  --dir "$env:LOCALAPPDATA\MultiCommentViewer" `
  --plugin youtubelive `
  --plugin chrome-cookie `
  --plugin bouyomi `
  --plugin twitch `
  --plugin cookies-txt `
  --plugin twicas `
  --plugin exe-manager-v3 `
  --plugin nicolive
