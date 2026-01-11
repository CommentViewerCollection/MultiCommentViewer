# 名称
正式名称はMultiCommentViewer
略してmcv

# 特徴
複数の配信サイトのコメントを同時に取得できる
一人の配信者が複数の配信サイトで同時配信を行ったり、複数の配信者が参加する企画を視聴する際にコメントを1か所でまとめて見れる
他のコメントビューアが対応していない配信サイトに対応している場合もある
棒読みちゃんとの連携機能を使ってコメントの読み上げもしてもらえる

# 用語
- core: mcv 本体。UI管理、プラグイン管理、コメント統合表示を担当
- plugin: 特定の配信サイトに対応する機能単位
- plugin-host: plugin を隔離して実行する実行環境（actor）
- connection: 1つの配信URLに対する接続インスタンス
# 基本的な使い方
接続を追加ボタンを押す
URL入力欄に配信のURLを入力する
コメントを投稿する場合はログイン情報が入ったcookieが必要だから適切なブラウザを選択する
接続ボタンを押す
取得したコメントがコメント欄に表示される

# 構造
mcvはcoreとpluginで構成されている
1つのpluginに1つのplugin-hostが割り当てられる
coreのplugin-managerがpluginを読み込み、plugin-hostを割り当てる
coreとplugin-hostはactor modelでメッセージをやり取りする
pluginとplugin-hostはインタフェースを通してメッセージをやり取りする

pluginの設定 JSON schemaでUI構成をcoreに伝える

plugin role必要？

エラー情報は貯め込んで終了時に送信。送信できなかった場合はerror.txtとして保存、次回起動時に送信

# 技術スタック
## フロントエンド
- フレームワーク: Tauri
- UI: React
- スタイリング: Tailwind CSS

## バックエンド
- フレームワーク: Tauri
- core-plugin-host間通信: actix

# ライフサイクル



# connection
 1つの配信URLに対する接続インスタンス。コンテキストとして大きく分けて配信サイト、input、ブラウザをもつ。

## 配信サイト
接続先の配信サイトまたはそのサービス名。"YouTubeLive"、"ツイキャスプライベート配信"等。


## input
単純にコメントを取得するだけであれば配信ページのURLや配信IDだけで足りる場合もあるが、コメント投稿にはほとんどの場合ログインが必要だし、例えばツイキャスのプライベート配信には合言葉と呼ばれるパスワードのような文字列が必要。
mcvではこのような多様なサイトの仕様に対応できるような柔軟なUIや内部構造を提供したい。

## ブラウザ
配信サイトに渡すcookieの取得先。
起動時にcoreが対応済みブラウザの中でインストールされているブラウザを確認して選択肢として用意する。さらにユーザーがcookies.txt等のcookie情報ファイルを入力し、任意の名前を付けるとそれも選択肢として表示されるようにする。
ブラウザによってはプロファイルを作成して複数のユーザー情報を保持できる。プロファイルが複数ある場合はそれぞれを別のブラウザとして扱う。



# core-plugin間メッセージ
本ドキュメント中で `{}` で囲まれた文字列はプレースホルダを表し、
実際のメッセージでは具体的な値に置き換えられる。

## 命名規則
- メッセージ名はすべて kebab-case とする
- 要求系メッセージは動詞から始める（add / remove / connect / get 等）
- 応答・通知系メッセージは過去形または状態を表す名前とする
- 失敗を表すメッセージには `-failed` を付与する

## メッセージヘッダー

```
{
  "type": "add-connection",
  "src": "core | plugin:{plugin_id}",
  "dst": "core | plugin:{plugin_id}",
  "request_id": "{uuid}" | null,
  "timestamp": 1700000000,
  "payload": { ... }
}
```

## メッセージ種別
### connection


| メッセージ名                                                | 説明（用途など）         | 方向           |
| ----------------------------------------------------- | ---------------- | ------------ |
| [add-connection](#add-connection)                     | 新しい接続の追加要求       | plugin->core |
| [connection-added](#connection-added)                 | 接続が正常に追加されたことの通知 | core->plugin |
| [connection-add-failed](#connection-add-failed)       | 接続の追加に失敗したことの通知  | core->plugin |
| [remove-connection](#remove-connection)               | 既存接続の削除要求        | plugin->core |
| [connection-removed](#connection-removed)             | 接続が正常に削除されたことの通知 | core->plugin |
| [connection-remove-failed](#connection-remove-failed) | 接続の削除に失敗したことの通知  | core->plugin |
| [connect](#connect)                                   | 接続開始の要求          | plugin->core |
| [connected](#connected)                               | 接続が確立したことの通知     | core->plugin |
| [connect-failed](#connect-failed)                     | 接続に失敗したことの通知     | core->plugin |
| [disconnect](#disconnect)                             | 切断要求             | plugin->core |
| [disconnected](#disconnected)                         | 切断が完了したことの通知     | core->plugin |
| [disconnect-failed](#disconnect-failed)               | 切断に失敗したことの通知     | core->plugin |
|[get-connection-status](#get-connection-status)||plugin->core|
### コメント

| メッセージ名                                | 説明（用途など） | 方向  |
| ------------------------------------- | -------- | --- |
| [comment-received](#comment-received) |          |     |

### plugin

| メッセージ名                                                        | 説明（用途など）             | 方向           |
| ------------------------------------------------------------- | -------------------- | ------------ |
| [plugin-hello](#plugin-hello)                                 | プラグインの登録要求           | plugin->core |
| [plugin-added](#plugin-added)                                 | プラグインが追加されたことの通知     | core->plugin |
| [plugin-removed](#plugin-removed)                             | プラグインが削除・無効化されたことの通知 | core->plugin |
| [plugin-error](#plugin-error)                                 | プラグイン内でエラーが発生したことの通知 | plugin->core |
| [get-plugin-settings-dir-path](#get-plugin-settings-dir-path) | プラグイン設定ディレクトリのパスを要求  | plugin->core |
| [get-plugin-settings](#get-plugin-settings)                   | プラグイン設定の取得要求         | plugin->core |

### その他

| メッセージ名                  | 説明（用途など）                   | 方向           |
| ----------------------- | -------------------------- | ------------ |
| [direct-message](#direct-message)          | プラグインから特定のプラグインへのメッセージ送信要求 | plugin->core |
| [direct-message-received](#direct-message-received) | 他のプラグインからメッセージが送信されたことの通知  | core->plugin |
| [get-app-name](#get-app-name)       |   | plugin->core |
| [get-app-version](#get-app-version) |            | plugin->core |




## add-connection
```
{}
```
## connection-added
```
{
  "connection_id": "{uuid}"
}
```

## connection-add-failed
```
{
  "reason":"{reason}"
}
```

## remove-connection
```
{
  "connection_id": "{uuid}"
}
```

## connection-removed
```
{
  "connection_id": "{uuid}"
}
```
## connection-remove-failed
```
{
  "reason":"{reason}"
}
```

## connect
```
{
  "site": {
    "name": "{name}",
    "id": "{plugin_id}"
  },
  "input": {
    "input_type": "{input_type}",
    ...
  },
  "browser": {
    "name": "{name}",
    "id": "{uuid}"
  }
}
```
inputの形式はinput_type毎に異なる。
input_type: "normal"
```
{
  "url": "{url | live_id}"
}
```
input_type: "twicas_private"
```
{
  "url": "{url | live_id}",
  "password": "{合言葉}"
}
```
## connected
```
{
  "connection_id": "{uuid}"
}
```
## connect-failed
```
{
  "reason":"{reason}"
}
```
## disconnect
```
{
  "connection_id": "{uuid}"
}
```
## disconnected
```
{
  "connection_id": "{uuid}"
}
```
## disconnect-failed
```
{
  "connection_id": "{uuid}",
  "reason":"{reason}"
}
```
## get-connection-status
```
{
  "connection_id": "{uuid}"
}
```
## comment-received


## plugin-hello
```
{
  "name": "{plugin-name}",
  "plugin_id": "{uuid}",
  "role": ["{role}"...],
  "api_version": "v2"
}
```

role
そのプラグインの機能を表す。
youtubelive, twitcas-private, nicolive

api_version
メッセージ形式のバージョン。

## plugin-added
```
{
  "name": "{plugin-name}",
  "plugin_id": "{uuid}",
  "role": ["{role}"...],
  "api_version": "v2"
}
```
## plugin-removed

## plugin-error

## get-plugin-settings-dir-path

## get-plugin-settings

## direct-message
pluginからpluginへメッセージを送信する。
## direct-message-received

```
{
  
}
```
## get-app-name

## get-app-version


## メッセージフロー
接続を追加
UIの"接続を追加"ボタンが押される→coreがadd-connectionメッセージをpluginに送信→pluginはconnectionを作成する→pluginはconnecitonが作成できたらconnection-addedをcoreに送信する

接続を削除

接続
UIの"接続"ボタンが押される→coreがconnectメッセージをpluginに送信→pluginは配信サイトのコメント取得を開始する→pluginはconnectedメッセージをcoreに送信する

切断

プラグインの読み込み
plugin-managerがプラグインを読み込む→plugin-hostがpluginのonLoaded()を実行→pluginがplugin-helloをplugin-managerに送信→plugin-managerはplugin-helloを送信したplugin以外の全てのpluginにplugin-addedを送信

コメントを取得した

プラグイン内エラー

プラグイン間通信


# plugin-plugin-host間メッセージ
pluginインタフェース

plugin-hostインタフェース

# ディレクトリ構成
プロジェクトのルートディレクトリのファイルとディレクトリを示します。

| 名称         | 用途                              |
| ---------- | ------------------------------- |
| Cargo.toml | Workspaceの管理                    |
| .gitignore |                                 |
| .git       | git本体                           |
| crates     | 本プロジェクト用に作成したRust crate置き場      |
| apps       | mcv本体やインストーラ等の実行ファイルのプロジェクト置き場  |
| packages   | 本プロジェクト用に作成したReactパッケージモジュール置き場 |
| docs       | ドキュメント置き場                       |




# 対応したい機能
## コメントの遅延表示
配信サイトによってはコメントの配信にラグがあることがある。複数のサイトのコメントを同時に表示すると、配信に対する反応にズレがあって気になる場合にあえてコメントの表示に遅延を入れることでタイミングを合わせる

## プラグイン配布サイト

## インストーラ
使いたいプラグインにチェックを入れてもらう

## アップデータ

## core-plugin
coreの機能を切り出して保守管理しやすいようにしたい。

# 検討事項
connectionとコメント投稿欄はサイト毎に異なる。json schemaでできる？
