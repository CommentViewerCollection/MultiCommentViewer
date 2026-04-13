# apps/mcv と全プラグインをリリースビルドしてローカルにインストールする
#
# cargo xtask install がフルビルド（Tauri 含む）とデプロイを一括で行う。

$ErrorActionPreference = "Stop"

cargo xtask install `
  --channel stable `
  --release `
  --dir "$env:LOCALAPPDATA\MultiCommentViewer" `
  --plugin youtubelive `
  --plugin chrome-cookie `
  --plugin firefox-cookie `
  --plugin bouyomi `
  --plugin twitch `
  --plugin cookies-txt `
  --plugin twicas `
  --plugin exe-manager-v3 `
  --plugin nicolive `
  --plugin exe-plugin-sample `
  --plugin kick `
  --plugin irc `
  --plugin openrec
  
