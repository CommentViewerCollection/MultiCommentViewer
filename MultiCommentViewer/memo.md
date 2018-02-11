## 初期リリースまでにすべきこと
自動アップデート
エラーリポート


## 未実装
#### 各CommentProviderのConnectAsyncでInputの形式が正しいか評価し、不正であればInfoMessageで伝える
#### アップデートチェック
#### IOptionのファイルへの読み込み、書き出し
#### SQLiteUserStoreを実装する
#### DataGridのDataVirtualization
#### 過去コメントの取得
#### Mixer,Discord,Twicas,LINELIVE,ふわっちを実装
#### 各サイトプラグインのSiteOptions作成
#### ネットにアクセスする時にUserAgentを設定する。形式は"MultiCommentViewer_v0.0.1"みたいな感じでいいだろう
#### コメント投稿
各サイトプラグインにコメント投稿パネルを持たせる  

#### URLの自動認識
・InputにURLを入力すると自動でサイトを選択する。

#### コメントの保存、読み込み
コメントの読み込みは全てのConnectionが非接続時に可能とする。  
読み込まれたコメントのConnectionのInputや接続、切断ボタンはIsEnabled=false  
ブラウザComboBoxは"ファイル"とでも表示  
#### 初コメのフォント変更
各CommentViewModel内で。
#### Donateボタン
是非欲しい！！  
押すとryu-s.github.ioの専用ページに飛ぶ。  
Bitcoinとかで受取り。あんまり期待してないけど、貰えたらモチベーション上がるだろうな・・・。  
"寄付してもらえると作者の開発モチベーションがあがります"
#### 使い方ページ
出来れば動画を作りたい。  
あとはAPNGとかGIFでビジュアルに説明したい。  

#### YouTubeLive
URLにgamingとかchannelとか沢山種類があるからenum UrlTypeがあると良いかも。
URLの形式一覧
https://gaming.youtube.com/channel/UCEwvS8JGjGFHuopjeiukYtg/live
https://gaming.youtube.com/watch?v=Rn9VTh-oCHY​
https://www.youtube.com/user/liryu1973/live

InvalidationContinuationに対応

#### Twitch
名前の後ろにbadgeを表示する

#### ColorPicker

#### FontPicker

#### プラグイン
プラグインメニューから項目をクリックしてもプラグインの設定画面が表示されないことがある


## 不安とか悩みとか
IUserStoreはDIでsitrcontextに渡したい。でもサイト固有の実装にすべきかも。。。どちらにしろ、現状の実装は変更すべき。
