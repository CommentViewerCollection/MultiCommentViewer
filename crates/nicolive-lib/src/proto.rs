//! ニコニコ生放送 protobuf スキーマ定義
//!
//! 実際のフィールドと一致しない箇所は prost が無視するため、デコードは失敗しない。
//! prost 0.13 の Message / Oneof は Debug を自動 derive するため明示不要。

// ── ChunkedEntry (viewUri レスポンス) ────────────────────────────────────────

/// viewUri のレスポンスに含まれる1エントリ。
///
/// viewUri は長ポーリングで叩くエンドポイントで、接続中はこのエントリが繰り返し届く。
/// 各エントリは「セグメントのURL」「過去セグメントのURL」「次回ポーリング可能時刻」
/// のいずれかを表す。
///
/// dwango.nicolive.chat.service.edge.ChunkedEntry
#[derive(Clone, PartialEq, ::prost::Message)]
pub struct ChunkedEntry {
    #[prost(oneof = "chunked_entry::Entry", tags = "1, 2, 3, 4")]
    pub entry: Option<chunked_entry::Entry>,
}

pub mod chunked_entry {
    #[derive(Clone, PartialEq, ::prost::Oneof)]
    pub enum Entry {
        /// ライブ中の現セグメント。
        /// この URI に GET するとリアルタイムのコメント群が取得できる。
        /// dwango.nicolive.chat.service.edge.MessageSegment
        #[prost(message, tag = "1")]
        Segment(super::MessageSegment),
        /// 過去データ（後方検索）セグメント。
        /// 接続直後に過去コメントを遡るために使う。フォーマットが通常と異なる。
        /// dwango.nicolive.chat.service.edge.BackwardSegment
        #[prost(message, tag = "2")]
        Backward(super::BackwardSegment),
        /// 直近の過去セグメント。
        /// Segment と同じフォーマットで取得可能な、わずかに前の時間帯のコメント群。
        /// dwango.nicolive.chat.service.edge.MessageSegment
        #[prost(message, tag = "3")]
        Previous(super::MessageSegment),
        /// 次回 viewUri にポーリング可能になる時刻。
        /// この時刻まで待ってから次の viewUri リクエストを送る。
        /// dwango.nicolive.chat.service.edge.ChunkedEntry.ReadyForNext
        #[prost(message, tag = "4")]
        Next(super::ReadyForNext),
    }
}

/// コメントセグメントの URI 情報。
///
/// dwango.nicolive.chat.service.edge.MessageSegment
#[derive(Clone, PartialEq, ::prost::Message)]
pub struct MessageSegment {
    /// このセグメントが対象とする時間帯の開始時刻。
    #[prost(message, optional, tag = "1")]
    pub from: Option<Timestamp>,
    /// このセグメントが対象とする時間帯の終了時刻。
    #[prost(message, optional, tag = "2")]
    pub until: Option<Timestamp>,
    /// セグメントデータを GET するための URL。
    #[prost(string, tag = "3")]
    pub uri: String,
}

/// 過去セグメントの参照情報。
///
/// 接続直後に過去コメントを遡るために使う。通常の MessageSegment とはフォーマットが異なる。
///
/// dwango.nicolive.chat.service.edge.BackwardSegment
#[derive(Clone, PartialEq, ::prost::Message)]
pub struct BackwardSegment {
    /// このセグメントが対象とする時間帯の終了時刻。
    #[prost(message, optional, tag = "1")]
    pub until: Option<Timestamp>,
    /// 次の過去セグメントへの参照（さらに古いコメントへ）。
    /// dwango.nicolive.chat.service.edge.PackedSegment.Next
    #[prost(message, optional, tag = "2")]
    pub segment: Option<PackedSegmentNext>,
    /// 状態スナップショットへの参照（放送状態の初期値を取得するために使う）。
    /// dwango.nicolive.chat.service.edge.PackedSegment.StateSnapshot
    #[prost(message, optional, tag = "3")]
    pub snapshot: Option<PackedSegmentStateSnapshot>,
}

/// 次の過去セグメントを指す URI。
///
/// dwango.nicolive.chat.service.edge.PackedSegment.Next
#[derive(Clone, PartialEq, ::prost::Message)]
pub struct PackedSegmentNext {
    /// 次の過去セグメントデータを GET するための URL。
    #[prost(string, tag = "1")]
    pub uri: String,
}

/// 放送状態スナップショットを指す URI。
///
/// dwango.nicolive.chat.service.edge.PackedSegment.StateSnapshot
#[derive(Clone, PartialEq, ::prost::Message)]
pub struct PackedSegmentStateSnapshot {
    /// 状態スナップショットデータを GET するための URL。
    #[prost(string, tag = "1")]
    pub uri: String,
}

/// 次回 viewUri ポーリング可能になる時刻。
///
/// この Unix タイムスタンプ（ミリ秒）まで待ってから viewUri を再度リクエストする。
///
/// dwango.nicolive.chat.service.edge.ChunkedEntry.ReadyForNext
#[derive(Clone, PartialEq, ::prost::Message)]
pub struct ReadyForNext {
    /// 次回ポーリング可能な Unix タイムスタンプ（ミリ秒単位）。
    #[prost(int64, tag = "1")]
    pub at: i64,
}

/// google.protobuf.Timestamp
#[derive(Clone, PartialEq, ::prost::Message)]
pub struct Timestamp {
    /// Unix エポックからの秒数。
    #[prost(int64, tag = "1")]
    pub seconds: i64,
    /// ナノ秒の端数（0〜999,999,999）。
    #[prost(int32, tag = "2")]
    pub nanos: i32,
}

/// google.protobuf.Duration（時間の長さ）
#[derive(Clone, PartialEq, ::prost::Message)]
pub struct Duration {
    /// 秒数。
    #[prost(int64, tag = "1")]
    pub seconds: i64,
    /// ナノ秒の端数（0〜999,999,999）。
    #[prost(int32, tag = "2")]
    pub nanos: i32,
}

// ── ChunkedMessage (セグメントレスポンス) ────────────────────────────────────

/// セグメント URI から取得したコメント等のメッセージ1件。
///
/// セグメントバイナリを length-delimited でデコードすると、この構造体の列が得られる。
/// payload は「通常メッセージ」「放送状態更新」「フラッシュ完了シグナル」のいずれか。
///
/// dwango.nicolive.chat.service.edge.ChunkedMessage
#[derive(Clone, PartialEq, ::prost::Message)]
pub struct ChunkedMessage {
    /// メッセージのメタデータ（ID・受信時刻）。
    #[prost(message, optional, tag = "1")]
    pub meta: Option<ChunkedMessageMeta>,
    /// メッセージ本体（コメント / 状態更新 / シグナルのいずれか）。
    #[prost(oneof = "chunked_message::Payload", tags = "2, 4, 5")]
    pub payload: Option<chunked_message::Payload>,
}

pub mod chunked_message {
    #[derive(Clone, PartialEq, ::prost::Oneof)]
    pub enum Payload {
        /// コメント・ギフト・通知などのメッセージ本体。
        #[prost(message, tag = "2")]
        Message(super::NicoliveMessage),
        /// 放送の状態更新（統計・アンケート・テロップ等）。コメントではない。
        #[prost(message, tag = "4")]
        State(super::NicoliveState),
        /// このセグメントの全データ送信完了を示すシグナル。
        #[prost(enumeration = "Signal", tag = "5")]
        Signal(i32),
    }

    #[derive(Clone, Copy, Debug, PartialEq, Eq, Hash, ::prost::Enumeration)]
    #[repr(i32)]
    pub enum Signal {
        /// セグメント内の全メッセージを送り終えたことを示す。
        Flushed = 0,
    }
}

/// ChunkedMessage のメタデータ。
///
/// dwango.nicolive.chat.service.edge.ChunkedMessage.Meta
#[derive(Clone, PartialEq, ::prost::Message)]
pub struct ChunkedMessageMeta {
    /// メッセージの一意 ID（文字列）。
    #[prost(string, tag = "1")]
    pub id: String,
    /// このメッセージが到達した時刻（サーバー側の受信時刻）。
    #[prost(message, optional, tag = "2")]
    pub at: Option<Timestamp>,
}

// ── Chat ──────────────────────────────────────────────────────────────────────

/// ユーザーが投稿した通常コメント。
///
/// ニコ生の最も基本的なメッセージ。コメント本文・投稿者情報・装飾情報を持つ。
///
/// dwango.nicolive.chat.data.Chat
#[derive(Clone, PartialEq, ::prost::Message)]
pub struct Chat {
    /// コメント本文（テキスト）。
    #[prost(string, tag = "1")]
    pub content: String,
    /// 投稿者の表示名。ログイン済みユーザーは設定したニックネーム、匿名は None の場合がある。
    #[prost(string, optional, tag = "2")]
    pub name: Option<String>,
    /// 動画再生位置（vpos）。放送開始からの経過時間をセンチ秒（1/100秒）で表す。
    /// タイムシフト視聴時に元の位置を特定するために使う。
    #[prost(int32, tag = "3")]
    pub vpos: i32,
    /// アカウント状態（一般会員 / プレミアム会員）。
    #[prost(enumeration = "chat::AccountStatus", tag = "4")]
    pub account_status: i32,
    /// ログイン済みユーザーの生のユーザー ID（数値）。匿名投稿の場合は None。
    #[prost(int64, optional, tag = "5")]
    pub raw_user_id: Option<i64>,
    /// 匿名ユーザーのハッシュ化ユーザー ID。同一放送内で同じユーザーを追跡できる。
    /// ログイン済みの場合は None。
    #[prost(string, optional, tag = "6")]
    pub hashed_user_id: Option<String>,
    /// コメントの装飾情報（表示位置・サイズ・色・フォント等）。
    #[prost(message, optional, tag = "7")]
    pub modifier: Option<chat::Modifier>,
    /// このコメントの通し番号（放送内で一意の連番）。
    #[prost(int32, tag = "8")]
    pub no: i32,
}

pub mod chat {
    /// アカウントの種別。
    #[derive(Clone, Copy, Debug, PartialEq, Eq, Hash, ::prost::Enumeration)]
    #[repr(i32)]
    pub enum AccountStatus {
        /// 一般会員（無料）。
        Standard = 0,
        /// プレミアム会員（有料）。コメント投稿の優先度が高い。
        Premium = 1,
    }

    /// コメントの表示スタイル情報。
    ///
    /// ニコ生ではコメントに位置・サイズ・色・フォント・透明度を指定できる。
    ///
    /// dwango.nicolive.chat.data.Chat.Modifier
    #[derive(Clone, PartialEq, ::prost::Message)]
    pub struct Modifier {
        /// コメントの表示位置（流れる位置）。
        #[prost(enumeration = "modifier::Pos", tag = "1")]
        pub position: i32,
        /// コメントのフォントサイズ。
        #[prost(enumeration = "modifier::Size", tag = "2")]
        pub size: i32,
        /// コメントの文字色。色名指定か RGB 値指定のいずれか。
        #[prost(oneof = "modifier::Color", tags = "3, 4")]
        pub color: Option<modifier::Color>,
        /// コメントのフォント種別。
        #[prost(enumeration = "modifier::Font", tag = "5")]
        pub font: i32,
        /// コメントの透明度。
        #[prost(enumeration = "modifier::Opacity", tag = "6")]
        pub opacity: i32,
    }

    pub mod modifier {
        /// RGB 値で指定するフルカラー。
        ///
        /// dwango.nicolive.chat.data.Chat.Modifier.FullColor
        #[derive(Clone, PartialEq, ::prost::Message)]
        pub struct FullColor {
            /// 赤成分（0〜255）。
            #[prost(int32, tag = "1")]
            pub r: i32,
            /// 緑成分（0〜255）。
            #[prost(int32, tag = "2")]
            pub g: i32,
            /// 青成分（0〜255）。
            #[prost(int32, tag = "3")]
            pub b: i32,
        }

        /// 色の指定方法（色名か RGB 値）。
        #[derive(Clone, PartialEq, ::prost::Oneof)]
        pub enum Color {
            /// 定義済みの色名で指定（白・赤・青など）。
            #[prost(enumeration = "ColorName", tag = "3")]
            NamedColor(i32),
            /// RGB 値でフルカラー指定（プレミアム会員向け拡張）。
            #[prost(message, tag = "4")]
            FullColor(FullColor),
        }

        /// コメントの表示位置。
        #[derive(Clone, Copy, Debug, PartialEq, Eq, Hash, ::prost::Enumeration)]
        #[repr(i32)]
        pub enum Pos {
            /// 画面を左から右に流れる（通常のコメント）。
            Naka = 0,
            /// 画面下部に固定表示（下コメ）。
            Shita = 1,
            /// 画面上部に固定表示（上コメ）。
            Ue = 2,
        }

        /// コメントの文字サイズ。
        #[derive(Clone, Copy, Debug, PartialEq, Eq, Hash, ::prost::Enumeration)]
        #[repr(i32)]
        pub enum Size {
            /// 通常サイズ。
            Medium = 0,
            /// 小さいサイズ（「小」コマンド）。
            Small = 1,
            /// 大きいサイズ（「大」コマンド）。
            Big = 2,
        }

        /// 定義済みの色名一覧。
        /// 一般会員は一部の色のみ使用可能。プレミアム会員は全色＋フルカラー使用可。
        #[derive(Clone, Copy, Debug, PartialEq, Eq, Hash, ::prost::Enumeration)]
        #[repr(i32)]
        pub enum ColorName {
            White = 0,
            Red = 1,
            Pink = 2,
            Orange = 3,
            Yellow = 4,
            Green = 5,
            Cyan = 6,
            Blue = 7,
            Purple = 8,
            Black = 9,
            /// プレミアム会員向け拡張色（White の別バリエーション）。
            White2 = 10,
            Red2 = 11,
            Pink2 = 12,
            Orange2 = 13,
            Yellow2 = 14,
            Green2 = 15,
            Cyan2 = 16,
            Blue2 = 17,
            Purple2 = 18,
            Black2 = 19,
        }

        /// フォント種別。
        #[derive(Clone, Copy, Debug, PartialEq, Eq, Hash, ::prost::Enumeration)]
        #[repr(i32)]
        pub enum Font {
            /// デフォルトフォント（ゴシック系）。
            Defont = 0,
            /// 明朝体。
            Mincho = 1,
            /// ゴシック体（明示的指定）。
            Gothic = 2,
        }

        /// コメントの透明度。
        #[derive(Clone, Copy, Debug, PartialEq, Eq, Hash, ::prost::Enumeration)]
        #[repr(i32)]
        pub enum Opacity {
            /// 不透明（通常）。
            Normal = 0,
            /// 半透明（コメントが薄く表示される）。
            Translucent = 1,
        }
    }
}

// ── OperatorComment ───────────────────────────────────────────────────────────

/// 運営・放送主からの公式コメント（運コメ）。
///
/// 通常のユーザーコメントとは区別される。テロップ（Marquee）の中に含まれることが多い。
///
/// dwango.nicolive.chat.data.OperatorComment
#[derive(Clone, PartialEq, ::prost::Message)]
pub struct OperatorComment {
    /// コメント本文。
    #[prost(string, tag = "1")]
    pub content: String,
    /// 表示名（放送主名など）。
    #[prost(string, optional, tag = "2")]
    pub name: Option<String>,
    /// 装飾情報（表示位置・色等）。
    #[prost(message, optional, tag = "3")]
    pub modifier: Option<chat::Modifier>,
    /// コメントにリンクが付いている場合の URL。
    #[prost(string, optional, tag = "4")]
    pub link: Option<String>,
}

// ── SimpleNotification ────────────────────────────────────────────────────────

/// ニコ生上で発生したイベントを通知するシステムメッセージ（旧バージョン）。
///
/// ユーザーによるコメントではなく、サービス側が生成する通知。
/// 「〇〇さんが来場しました」「ランキング入り」「放送延長」などを表す。
/// 各バリアントは String 型で通知メッセージを保持する。
///
/// dwango.nicolive.chat.data.SimpleNotification
#[derive(Clone, PartialEq, ::prost::Message)]
pub struct SimpleNotification {
    #[prost(oneof = "simple_notification::Message", tags = "1, 2, 3, 4, 5, 6, 7, 8, 9, 10")]
    pub message: Option<simple_notification::Message>,
}

pub mod simple_notification {
    /// SimpleNotification のイベント種別。
    #[derive(Clone, PartialEq, ::prost::Oneof)]
    pub enum Message {
        /// ニコニコ市場（関連商品）が追加・更新された。
        #[prost(string, tag = "1")]
        Ichiba(String),
        /// 別の放送を引用（紹介）した。
        #[prost(string, tag = "2")]
        Quote(String),
        /// 感情スタンプが押された（「w」「感動」などのエモーション）。
        #[prost(string, tag = "3")]
        Emotion(String),
        /// クルーズユーザー（別番組から訪問中のユーザー）が来場した。
        #[prost(string, tag = "4")]
        Cruise(String),
        /// 放送時間が延長された。
        #[prost(string, tag = "5")]
        ProgramExtended(String),
        /// 番組がランキングに入った。
        #[prost(string, tag = "6")]
        RankingIn(String),
        /// 誰かが訪問してきた（来場通知）。
        #[prost(string, tag = "7")]
        Visited(String),
        /// ランキング順位が更新された。
        #[prost(string, tag = "8")]
        RankingUpdated(String),
        /// サポーターが登録された（放送主を支援するユーザー）。
        #[prost(string, tag = "9")]
        SupporterRegistered(String),
        /// ユーザーレベルが上昇した。
        #[prost(string, tag = "10")]
        UserLevelUp(String),
    }
}

// ── SimpleNotificationV2 (atoms) ─────────────────────────────────────────────

/// ニコ生上で発生したイベントを通知するシステムメッセージ（新バージョン）。
///
/// `show_in_list` が true のものだけコメント一覧に表示することが想定されている。
/// `show_in_telop` が true のものはテロップ（画面下部の流れるテキスト）にも表示される。
///
/// dwango.nicolive.chat.data.atoms.SimpleNotificationV2
#[derive(Clone, PartialEq, ::prost::Message)]
pub struct SimpleNotificationV2 {
    /// イベントの種別。
    #[prost(enumeration = "simple_notification_v2::NotificationType", tag = "1")]
    pub r#type: i32,
    /// 通知のテキストメッセージ（「〇〇さんが来場しました」等）。
    #[prost(string, tag = "2")]
    pub message: String,
    /// true の場合、テロップ（画面下のスクロールテキスト）に表示する。
    #[prost(bool, tag = "3")]
    pub show_in_telop: bool,
    /// true の場合、コメント一覧に表示する。false の場合は非表示。
    #[prost(bool, tag = "4")]
    pub show_in_list: bool,
}

pub mod simple_notification_v2 {
    /// SimpleNotificationV2 のイベント種別。
    #[derive(Clone, Copy, Debug, PartialEq, Eq, Hash, ::prost::Enumeration)]
    #[repr(i32)]
    pub enum NotificationType {
        /// 不明・未定義。
        Unknown = 0,
        /// ニコニコ市場イベント。
        Ichiba = 1,
        /// 感情スタンプ。
        Emotion = 2,
        /// クルーズ来場。
        Cruise = 3,
        /// 放送延長。
        ProgramExtended = 4,
        /// ランキング入り。
        RankingIn = 5,
        /// 来場通知。
        Visited = 6,
        /// サポーター登録。
        SupporterRegistered = 7,
        /// ユーザーレベルアップ。
        UserLevelUp = 8,
        /// ユーザーフォロー。
        UserFollow = 9,
    }
}

// ── Gift ──────────────────────────────────────────────────────────────────────

/// ニコ生ギフト（投げ銭的な仮想アイテムの贈り物）。
///
/// 視聴者が放送主にギフトアイテムを贈った際に送信される。
/// ポイント数・贈り主・アイテム名・メッセージを含む。
///
/// dwango.nicolive.chat.data.Gift
#[derive(Clone, PartialEq, ::prost::Message)]
pub struct Gift {
    /// ギフトアイテムの ID（アイテム種別を一意に識別する文字列）。
    #[prost(string, tag = "1")]
    pub item_id: String,
    /// 贈り主（広告主）のユーザー ID。匿名ギフトの場合は None。
    #[prost(int64, optional, tag = "2")]
    pub advertiser_user_id: Option<i64>,
    /// 贈り主の表示名。
    #[prost(string, tag = "3")]
    pub advertiser_name: String,
    /// このギフトのポイント数（ニコ生ポイント換算）。
    #[prost(int64, tag = "4")]
    pub point: i64,
    /// ギフトに添えたメッセージ（空文字の場合あり）。
    #[prost(string, tag = "5")]
    pub message: String,
    /// ギフトアイテムの名称（例: 「ハート」「星」等）。
    #[prost(string, tag = "6")]
    pub item_name: String,
    /// 贈り主のギフト貢献ランキング順位（1位〜。None の場合はランク圏外）。
    #[prost(int32, optional, tag = "7")]
    pub contribution_rank: Option<i32>,
    /// ギフトバー（次のレベル報酬までの進捗）の更新情報。None の場合は更新なし。
    #[prost(message, optional, tag = "8")]
    pub gift_bar_update: Option<gift::GiftBarUpdate>,
}

pub mod gift {
    /// ギフトバーのレベル進捗情報。
    ///
    /// ギフトバーは視聴者のギフト累計に応じてレベルが上がり、報酬が解放される仕組み。
    ///
    /// dwango.nicolive.chat.data.Gift.GiftBarUpdate
    #[derive(Clone, PartialEq, ::prost::Message)]
    pub struct GiftBarUpdate {
        /// ギフトバーの現在レベル。
        #[prost(int32, tag = "1")]
        pub current_level: i32,
        /// 次のレベル報酬に必要なギフト数（あと何個で解放されるか）。
        #[prost(int32, tag = "2")]
        pub next_level_reward_count: i32,
        /// 次のレベルに到達するまでの残りポイント数。
        #[prost(int32, tag = "3")]
        pub remaining_points_for_next_level: i32,
        /// 次のレベルに到達するために必要な総ポイント数。
        #[prost(int32, tag = "4")]
        pub required_points_for_next_level: i32,
    }
}

// ── Nicoad ────────────────────────────────────────────────────────────────────

/// ニコニコ広告（ニコ広）。
///
/// 視聴者がニコニコポイントを使って放送を「広告」したときに送信される。
/// 広告することで放送の露出が上がる仕組み。V0（旧）と V1（新）の2フォーマットがある。
///
/// dwango.nicolive.chat.data.Nicoad
#[derive(Clone, PartialEq, ::prost::Message)]
pub struct Nicoad {
    #[prost(oneof = "nicoad::Versions", tags = "1, 2")]
    pub versions: Option<nicoad::Versions>,
}

pub mod nicoad {
    #[derive(Clone, PartialEq, ::prost::Oneof)]
    pub enum Versions {
        /// 旧フォーマット。最新の広告情報とランキングを含む。
        #[prost(message, tag = "1")]
        V0(super::NicoadV0),
        /// 新フォーマット。累計広告ポイントとメッセージのみ。
        #[prost(message, tag = "2")]
        V1(super::NicoadV1),
    }
}

/// ニコ広 旧フォーマット（V0）。
///
/// dwango.nicolive.chat.data.Nicoad.V0
#[derive(Clone, PartialEq, ::prost::Message)]
pub struct NicoadV0 {
    /// 直近の広告情報（最後に広告したユーザーの情報）。
    #[prost(message, optional, tag = "1")]
    pub latest: Option<nicoad_v0::Latest>,
    /// 広告貢献ランキング上位ユーザーのリスト。
    #[prost(message, repeated, tag = "2")]
    pub ranking: Vec<nicoad_v0::Ranking>,
    /// この放送への累計広告ポイント数。
    #[prost(int32, tag = "3")]
    pub total_point: i32,
}

pub mod nicoad_v0 {
    /// 最新の広告情報。
    #[derive(Clone, PartialEq, ::prost::Message)]
    pub struct Latest {
        /// 広告を行ったユーザーの名前。
        #[prost(string, tag = "1")]
        pub advertiser: String,
        /// この広告で消費したポイント数。
        #[prost(int32, tag = "2")]
        pub point: i32,
        /// 広告時に添えたメッセージ（任意）。
        #[prost(string, optional, tag = "3")]
        pub message: Option<String>,
    }

    /// 広告貢献ランキングの1エントリ。
    #[derive(Clone, PartialEq, ::prost::Message)]
    pub struct Ranking {
        /// ユーザー名。
        #[prost(string, tag = "1")]
        pub advertiser: String,
        /// 全体ランキング順位（1位〜）。
        #[prost(int32, tag = "2")]
        pub rank: i32,
        /// そのユーザーが送ったメッセージ（任意）。
        #[prost(string, optional, tag = "3")]
        pub message: Option<String>,
        /// 自分のランキング順位（閲覧者に対して表示する場合用）。
        #[prost(int32, optional, tag = "4")]
        pub user_rank: Option<i32>,
    }
}

/// ニコ広 新フォーマット（V1）。
///
/// dwango.nicolive.chat.data.Nicoad.V1
#[derive(Clone, PartialEq, ::prost::Message)]
pub struct NicoadV1 {
    /// この放送への累計広告ポイント数。
    #[prost(int32, tag = "1")]
    pub total_ad_point: i32,
    /// 広告に表示されるメッセージ（「〇〇が×pt 広告しました」等）。
    #[prost(string, tag = "2")]
    pub message: String,
}

// ── Statistics ────────────────────────────────────────────────────────────────

/// 放送の統計情報（視聴者数・コメント数等）。
///
/// NicoliveState に含まれ、定期的に更新される。
///
/// dwango.nicolive.chat.data.Statistics
#[derive(Clone, PartialEq, ::prost::Message)]
pub struct Statistics {
    /// 現在の視聴者数（ライブ視聴者のみ）。
    #[prost(int64, optional, tag = "1")]
    pub viewers: Option<i64>,
    /// 累計コメント数。
    #[prost(int64, optional, tag = "2")]
    pub comments: Option<i64>,
    /// 累計広告ポイント数（ニコ広）。
    #[prost(int64, optional, tag = "3")]
    pub ad_points: Option<i64>,
    /// 累計ギフトポイント数。
    #[prost(int64, optional, tag = "4")]
    pub gift_points: Option<i64>,
    /// タイムシフト予約数。
    #[prost(int64, optional, tag = "6")]
    pub timeshift_reservations: Option<i64>,
}

// ── TagUpdated ────────────────────────────────────────────────────────────────

/// 放送タグが更新されたことを示す通知。
///
/// 放送主またはモデレーターがタグを追加・削除・変更した際に届く。
/// タグはニコニコ動画における検索・分類に使われる。
///
/// dwango.nicolive.chat.data.TagUpdated
#[derive(Clone, PartialEq, ::prost::Message)]
pub struct TagUpdated {
    /// 更新後のタグ一覧（全件）。
    #[prost(message, repeated, tag = "1")]
    pub tags: Vec<tag_updated::Tag>,
    /// タグが放送主によってロックされているか。ロック時は視聴者によるタグ変更不可。
    #[prost(bool, tag = "2")]
    pub owner_locked: bool,
}

pub mod tag_updated {
    /// タグの1件分の情報。
    #[derive(Clone, PartialEq, ::prost::Message)]
    pub struct Tag {
        /// タグのテキスト。
        #[prost(string, tag = "1")]
        pub text: String,
        /// true の場合、このタグはロックされており変更不可。
        #[prost(bool, tag = "2")]
        pub locked: bool,
        /// true の場合、このタグは予約タグ（ニコニコ側が管理する特別なタグ）。
        #[prost(bool, tag = "3")]
        pub reserved: bool,
        /// このタグのニコニコ大百科ページの URL（存在する場合）。
        #[prost(string, optional, tag = "4")]
        pub nicopedia_uri: Option<String>,
    }
}

// ── Enquete ───────────────────────────────────────────────────────────────────

/// 放送中のアンケート（投票）。
///
/// 放送主がアンケートを開始・終了・結果表示した際に NicoliveState に含まれる。
///
/// dwango.nicolive.chat.data.Enquete
#[derive(Clone, PartialEq, ::prost::Message)]
pub struct Enquete {
    /// アンケートの質問文。
    #[prost(string, tag = "1")]
    pub question: String,
    /// 選択肢一覧。
    #[prost(message, repeated, tag = "2")]
    pub choices: Vec<enquete::Choice>,
    /// アンケートの現在の状態（受付中 / 終了 / 結果表示）。
    #[prost(enumeration = "enquete::Status", tag = "3")]
    pub status: i32,
}

pub mod enquete {
    /// アンケートの選択肢1件。
    #[derive(Clone, PartialEq, ::prost::Message)]
    pub struct Choice {
        /// 選択肢のテキスト。
        #[prost(string, tag = "1")]
        pub description: String,
        /// 投票結果の割合（千分率、0〜1000）。結果表示中のみ存在する。
        #[prost(int32, optional, tag = "3")]
        pub per_mille: Option<i32>,
    }

    /// アンケートの状態。
    #[derive(Clone, Copy, Debug, PartialEq, Eq, Hash, ::prost::Enumeration)]
    #[repr(i32)]
    pub enum Status {
        /// アンケート終了（または未実施）。
        Closed = 0,
        /// アンケート受付中（投票可能）。
        Poll = 1,
        /// 結果表示中（投票締め切り後）。
        Result = 2,
    }
}

// ── CommentLock ───────────────────────────────────────────────────────────────

/// コメント制限の状態。
///
/// 放送主がコメントを制限している場合に NicoliveState に含まれる。
/// フォロー限定コメント制限などを表す。
///
/// dwango.nicolive.chat.data.CommentLock
#[derive(Clone, PartialEq, ::prost::Message)]
pub struct CommentLock {
    /// コメント制限の種別（制限なし / 完全禁止 / 条件付き制限）。
    #[prost(enumeration = "comment_lock::Status", tag = "1")]
    pub status: i32,
    /// フォロー期間による制限の詳細（Restricted の場合のみ存在）。
    #[prost(message, optional, tag = "2")]
    pub follow_restriction: Option<comment_lock::FollowRestriction>,
}

pub mod comment_lock {
    /// コメント制限の種別。
    #[derive(Clone, Copy, Debug, PartialEq, Eq, Hash, ::prost::Enumeration)]
    #[repr(i32)]
    pub enum Status {
        /// 制限なし（誰でもコメント可）。
        Unrestricted = 0,
        /// コメント完全禁止。
        Locked = 1,
        /// 条件付き制限（フォロー期間など）。
        Restricted = 2,
    }

    /// フォロー期間による制限の詳細。
    #[derive(Clone, PartialEq, ::prost::Message)]
    pub struct FollowRestriction {
        /// コメントするために必要な最低フォロー期間。
        #[prost(message, optional, tag = "1")]
        pub minimum_follow_duration: Option<super::Duration>,
    }
}

// ── CommentMode ───────────────────────────────────────────────────────────────

/// コメントの表示レイアウトモード。
///
/// 放送主が選択できるコメントの画面上の表示形式。
///
/// dwango.nicolive.chat.data.CommentMode
#[derive(Clone, PartialEq, ::prost::Message)]
pub struct CommentMode {
    /// 表示レイアウト（通常 / 上下分割 / 背景）。
    #[prost(enumeration = "comment_mode::Layout", tag = "1")]
    pub layout: i32,
}

pub mod comment_mode {
    /// コメントのレイアウト種別。
    #[derive(Clone, Copy, Debug, PartialEq, Eq, Hash, ::prost::Enumeration)]
    #[repr(i32)]
    pub enum Layout {
        /// 通常の流れるコメント表示。
        Normal = 0,
        /// 画面を上下に分割して表示。
        SplitTop = 1,
        /// 背景に表示（ゲーム実況等で画面全体を使う際に使う）。
        Background = 2,
    }
}

// ── FingerPrint ───────────────────────────────────────────────────────────────

/// アンチスパム用フィンガープリント表示の設定。
///
/// スパム対策として、ユーザー識別用のロゴや記号を画面の特定位置に表示する機能。
/// 動画が無断転載された場合でも転載元を特定できる。
///
/// dwango.nicolive.chat.data.FingerPrint
#[derive(Clone, PartialEq, ::prost::Message)]
pub struct FingerPrint {
    /// 画面上のどの位置に表示するか。
    #[prost(enumeration = "finger_print::Position", tag = "1")]
    pub position: i32,
    /// 表示サイズ。
    #[prost(enumeration = "finger_print::Size", tag = "2")]
    pub size: i32,
    /// 表示継続時間。
    #[prost(message, optional, tag = "4")]
    pub duration: Option<Duration>,
}

pub mod finger_print {
    /// 画面上の表示位置。
    #[derive(Clone, Copy, Debug, PartialEq, Eq, Hash, ::prost::Enumeration)]
    #[repr(i32)]
    pub enum Position {
        /// 非表示。
        Off = 0,
        /// 左下。
        Hidarishita = 1,
        /// 下中央。
        Shita = 2,
        /// 右下。
        Migishita = 3,
        /// 左中央。
        Hidari = 4,
        /// 中央。
        Naka = 5,
        /// 右中央。
        Migi = 6,
        /// 左上。
        Hidariue = 7,
        /// 上中央。
        Ue = 8,
        /// 右上。
        Migiue = 9,
    }

    /// 表示サイズ。
    #[derive(Clone, Copy, Debug, PartialEq, Eq, Hash, ::prost::Enumeration)]
    #[repr(i32)]
    pub enum Size {
        Small = 0,
        Middle = 1,
        Big = 2,
    }
}

// ── TrialPanel ────────────────────────────────────────────────────────────────

/// トライアルパネル（試用・検証用 UI 要素）の表示設定。
///
/// 実験的な機能やユーザー認証が必要なパネルの表示/非表示を制御する。
///
/// dwango.nicolive.chat.data.TrialPanel
#[derive(Clone, PartialEq, ::prost::Message)]
pub struct TrialPanel {
    /// パネルを表示するか非表示にするか。
    #[prost(enumeration = "trial_panel::Panel", tag = "1")]
    pub panel: i32,
    /// 未認証ユーザーに対する動作（許可 / 制限 / 禁止）。
    #[prost(enumeration = "trial_panel::Mode", tag = "2")]
    pub unqualified_user: i32,
}

pub mod trial_panel {
    /// パネルの表示状態。
    #[derive(Clone, Copy, Debug, PartialEq, Eq, Hash, ::prost::Enumeration)]
    #[repr(i32)]
    pub enum Panel {
        Hidden = 0,
        Display = 1,
    }

    /// 未認証ユーザーへの対応モード。
    #[derive(Clone, Copy, Debug, PartialEq, Eq, Hash, ::prost::Enumeration)]
    #[repr(i32)]
    pub enum Mode {
        /// 許可（表示・操作可）。
        Allowed = 0,
        /// 制限（表示はするが操作不可等）。
        Restricted = 1,
        /// 禁止（表示・操作とも不可）。
        Forbidden = 2,
    }
}

// ── ProgramStatus ─────────────────────────────────────────────────────────────

/// 放送の状態。
///
/// 放送が終了したことを示す。NicoliveState に含まれる。
///
/// dwango.nicolive.chat.data.ProgramStatus
#[derive(Clone, PartialEq, ::prost::Message)]
pub struct ProgramStatus {
    /// 放送の現在の状態。
    #[prost(enumeration = "program_status::State", tag = "1")]
    pub state: i32,
}

pub mod program_status {
    #[derive(Clone, Copy, Debug, PartialEq, Eq, Hash, ::prost::Enumeration)]
    #[repr(i32)]
    pub enum State {
        /// 不明（放送中の場合もこの値）。
        Unknown = 0,
        /// 放送終了。
        Ended = 1,
    }
}

// ── MoveOrder / Jump / Redirect ───────────────────────────────────────────────

/// 別の放送・コンテンツへの誘導命令。
///
/// 放送主が「この放送は終了しました。こちらへどうぞ」と別の配信や URL に誘導する際に使う。
///
/// dwango.nicolive.chat.data.MoveOrder
#[derive(Clone, PartialEq, ::prost::Message)]
pub struct MoveOrder {
    #[prost(oneof = "move_order::To", tags = "1, 2")]
    pub to: Option<move_order::To>,
}

pub mod move_order {
    #[derive(Clone, PartialEq, ::prost::Oneof)]
    pub enum To {
        /// ニコニコ内の別コンテンツへのジャンプ。
        #[prost(message, tag = "1")]
        Jump(super::Jump),
        /// 外部 URL へのリダイレクト。
        #[prost(message, tag = "2")]
        Redirect(super::Redirect),
    }
}

/// ニコニコ内の別コンテンツへのジャンプ命令。
///
/// dwango.nicolive.chat.data.Jump
#[derive(Clone, PartialEq, ::prost::Message)]
pub struct Jump {
    /// ジャンプ先のコンテンツ ID（例: 放送 ID「lv〇〇〇」）。
    #[prost(string, tag = "1")]
    pub content: String,
    /// 視聴者に表示するメッセージ（「こちらで継続放送中です」等）。
    #[prost(string, tag = "2")]
    pub message: String,
    /// ジャンプ実行までの待機時間（カウントダウン用）。
    #[prost(message, optional, tag = "3")]
    pub wait: Option<Duration>,
}

/// 外部 URL へのリダイレクト命令。
///
/// dwango.nicolive.chat.data.Redirect
#[derive(Clone, PartialEq, ::prost::Message)]
pub struct Redirect {
    /// リダイレクト先の URL。
    #[prost(string, tag = "1")]
    pub uri: String,
    /// 視聴者に表示するメッセージ。
    #[prost(string, tag = "2")]
    pub message: String,
    /// リダイレクト実行までの待機時間。
    #[prost(message, optional, tag = "3")]
    pub wait: Option<Duration>,
}

// ── Marquee ───────────────────────────────────────────────────────────────────

/// テロップ（Marquee）の表示設定。
///
/// 放送主がコメント一覧の上部や画面に「運営コメント」を流す機能。
/// 放送の告知・注意事項などを表示するために使う。
///
/// dwango.nicolive.chat.data.Marquee
#[derive(Clone, PartialEq, ::prost::Message)]
pub struct Marquee {
    /// テロップの表示内容と設定。None の場合はテロップを非表示にする。
    #[prost(message, optional, tag = "1")]
    pub display: Option<marquee::Display>,
}

pub mod marquee {
    /// テロップの表示内容。
    #[derive(Clone, PartialEq, ::prost::Message)]
    pub struct Display {
        /// 運営コメントとして表示するコメント内容。
        #[prost(message, optional, tag = "1")]
        pub operator_comment: Option<super::OperatorComment>,
        /// 表示継続時間。
        #[prost(message, optional, tag = "3")]
        pub duration: Option<super::Duration>,
    }
}

// ── atoms: PreCensored ────────────────────────────────────────────────────────

/// 事前検閲済みコメント（BAN・スパム判定されたコメント）。
///
/// スパムフィルターに引っかかったコメントや、BAN されたユーザーのコメントは
/// 通常の Chat としては届かず、代わりにこの形式で届く。
/// プレースホルダーとして表示して後から削除することもある。
/// 個人情報（ユーザー ID 等）を含む可能性があるため UI には表示しないことが多い。
///
/// dwango.nicolive.chat.data.atoms.PreCensored
#[derive(Clone, PartialEq, ::prost::Message)]
pub struct PreCensored {
    /// 検閲されたコメントの元の内容。
    #[prost(message, optional, tag = "1")]
    pub chat: Option<Chat>,
    /// 「一人相撲」判定（自作自演・スパム送信の検知）の詳細。
    #[prost(message, optional, tag = "2")]
    pub hitorizumo: Option<pre_censored::Hitorizumo>,
    /// Douglas スパムフィルターによる判定結果（内部システム）。
    #[prost(message, optional, tag = "3")]
    pub douglas: Option<pre_censored::Douglas>,
    /// Boops スパムフィルターによる判定結果（内部システム）。
    #[prost(message, optional, tag = "4")]
    pub boops: Option<pre_censored::Boops>,
    /// パートナーシステム向けのコード（外部連携用）。
    #[prost(string, tag = "5")]
    pub code_for_partner_system: String,
    /// Douglas スパムスコア（高いほどスパムの疑いが強い）。
    #[prost(double, optional, tag = "6")]
    pub douglas_score: Option<f64>,
}

pub mod pre_censored {
    /// 一人相撲（自作自演・連投スパム）の判定理由。
    #[derive(Clone, PartialEq, ::prost::Message)]
    pub struct Hitorizumo {
        /// 制限の理由（コメントBAN / IP BAN）。
        #[prost(enumeration = "Reason", tag = "1")]
        pub reason: i32,
    }

    /// Douglas スパムフィルターの判定結果（詳細フィールド不明）。
    #[derive(Clone, PartialEq, ::prost::Message)]
    pub struct Douglas {}

    /// Boops スパムフィルターの判定結果（詳細フィールド不明）。
    #[derive(Clone, PartialEq, ::prost::Message)]
    pub struct Boops {}

    /// コメント制限の理由。
    #[derive(Clone, Copy, Debug, PartialEq, Eq, Hash, ::prost::Enumeration)]
    #[repr(i32)]
    pub enum Reason {
        /// コメントに対する BAN（該当コメントのみ非表示）。
        CommentBan = 0,
        /// IP アドレスに対する BAN（そのユーザーの全コメントが非表示）。
        IpBan = 1,
    }
}

// ── atoms: ForwardedChat ──────────────────────────────────────────────────────

/// 別の放送から転送されてきたコメント。
///
/// ニコ生の「クルーズ」機能などで、別の放送中の視聴者のコメントが
/// 自分の放送に流れてくることがある。また、コラボ配信での共有コメントも含まれる。
///
/// dwango.nicolive.chat.data.atoms.ForwardedChat
#[derive(Clone, PartialEq, ::prost::Message)]
pub struct ForwardedChat {
    /// 転送されてきたコメントの内容（通常の Chat と同じ構造）。
    #[prost(message, optional, tag = "1")]
    pub chat: Option<Chat>,
    /// コメントの一意 ID。
    #[prost(string, tag = "2")]
    pub message_id: String,
    /// コメントの発生元の放送 ID（lv〇〇〇の数値部分）。
    #[prost(int64, tag = "3")]
    pub source_live_id: i64,
    /// 転送の方式。
    #[prost(enumeration = "forwarded_chat::ForwardingMode", tag = "4")]
    pub mode: i32,
}

pub mod forwarded_chat {
    /// コメントの転送方式。
    #[derive(Clone, Copy, Debug, PartialEq, Eq, Hash, ::prost::Enumeration)]
    #[repr(i32)]
    pub enum ForwardingMode {
        Unknown = 0,
        /// クルーズ機能（別番組の視聴者が訪問中に送ったコメントが流れてくる）。
        FromCruise = 1,
        /// コラボ配信のコメント共有（複数の放送主が同じコメントを共有する）。
        CollabSharing = 2,
    }
}

// ── atoms: ModeratorUpdated ───────────────────────────────────────────────────

/// モデレーター（運営補佐）の変更通知。
///
/// 放送主がモデレーターを追加・削除した際に届く。
/// モデレーターはコメントの削除やユーザーの NG 設定などができる。
///
/// dwango.nicolive.chat.data.atoms.ModeratorUpdated
#[derive(Clone, PartialEq, ::prost::Message)]
pub struct ModeratorUpdated {
    /// 操作の種別（追加 / 削除）。
    #[prost(enumeration = "moderator_updated::ModeratorOperation", tag = "1")]
    pub operation: i32,
    /// 対象となったユーザーの情報。
    #[prost(message, optional, tag = "2")]
    pub operator: Option<moderator_updated::ModeratorUserInfo>,
    /// 変更が行われた時刻。
    #[prost(message, optional, tag = "3")]
    pub updated_at: Option<Timestamp>,
}

pub mod moderator_updated {
    /// モデレーターのユーザー情報。
    #[derive(Clone, PartialEq, ::prost::Message)]
    pub struct ModeratorUserInfo {
        /// ユーザー ID（数値）。
        #[prost(int64, tag = "1")]
        pub user_id: i64,
        /// ニックネーム（表示名）。
        #[prost(string, optional, tag = "2")]
        pub nickname: Option<String>,
        /// アイコン画像の URL。
        #[prost(string, optional, tag = "3")]
        pub icon_url: Option<String>,
    }

    /// モデレーターの操作種別。
    #[derive(Clone, Copy, Debug, PartialEq, Eq, Hash, ::prost::Enumeration)]
    #[repr(i32)]
    pub enum ModeratorOperation {
        /// モデレーターに追加した。
        Add = 0,
        /// モデレーターから削除した。
        Delete = 1,
    }
}

// ── atoms: SSNGUpdated ────────────────────────────────────────────────────────

/// SSNG（スーパー NG）の更新通知。
///
/// SSNG はニコ生の放送固有 NG 機能。放送主・モデレーターが特定のユーザーや
/// NG ワード・コマンドを設定すると、該当コメントが自動的に非表示になる。
/// この通知は SSNG リストが変更された際に届く（閲覧者には通常不要な情報）。
///
/// dwango.nicolive.chat.data.atoms.SSNGUpdated
#[derive(Clone, PartialEq, ::prost::Message)]
pub struct SsngUpdated {
    /// 操作の種別（追加 / 削除）。
    #[prost(enumeration = "ssng_updated::SsngOperation", tag = "1")]
    pub operation: i32,
    /// SSNG エントリの ID（追加時に採番される一意 ID）。
    #[prost(int64, tag = "2")]
    pub ssng_id: i64,
    /// この SSNG を設定したユーザー（放送主またはモデレーター）の情報。
    #[prost(message, optional, tag = "3")]
    pub operator: Option<ssng_updated::ModeratorUserInfo>,
    /// SSNG の種別（ユーザー NG / ワード NG / コマンド NG）。
    #[prost(enumeration = "ssng_updated::SsngType", optional, tag = "4")]
    pub r#type: Option<i32>,
    /// NG 対象の内容（ユーザー ID 文字列、NG ワード、NG コマンド等）。
    #[prost(string, optional, tag = "5")]
    pub source: Option<String>,
    /// 更新が行われた時刻。
    #[prost(message, optional, tag = "6")]
    pub updated_at: Option<Timestamp>,
    /// 設定者の役割（モデレーター / 放送主）。
    #[prost(enumeration = "ssng_updated::SsngOperatorType", tag = "7")]
    pub operator_type: i32,
}

pub mod ssng_updated {
    /// 設定したユーザーの情報。
    #[derive(Clone, PartialEq, ::prost::Message)]
    pub struct ModeratorUserInfo {
        #[prost(int64, tag = "1")]
        pub user_id: i64,
        #[prost(string, optional, tag = "2")]
        pub nickname: Option<String>,
        #[prost(string, optional, tag = "3")]
        pub icon_url: Option<String>,
    }

    /// SSNG の操作種別。
    #[derive(Clone, Copy, Debug, PartialEq, Eq, Hash, ::prost::Enumeration)]
    #[repr(i32)]
    pub enum SsngOperation {
        /// SSNG エントリを追加した。
        Add = 0,
        /// SSNG エントリを削除した（NG 解除）。
        Delete = 1,
    }

    /// SSNG の種別（何を NG にしたか）。
    #[derive(Clone, Copy, Debug, PartialEq, Eq, Hash, ::prost::Enumeration)]
    #[repr(i32)]
    pub enum SsngType {
        /// 特定ユーザーの NG（そのユーザーのコメント全体を非表示）。
        User = 0,
        /// 特定ワードの NG（そのワードを含むコメントを非表示）。
        Word = 1,
        /// 特定コマンドの NG（「/コマンド」による装飾を無効化）。
        Command = 2,
    }

    /// 設定者の役割。
    #[derive(Clone, Copy, Debug, PartialEq, Eq, Hash, ::prost::Enumeration)]
    #[repr(i32)]
    pub enum SsngOperatorType {
        /// モデレーターによる設定。
        Moderator = 0,
        /// 放送主による設定。
        Broadcaster = 1,
    }
}

// ── プレースホルダー型 ────────────────────────────────────────────────────────

/// ゲーム関連の更新通知（Akashic エンジン等）。詳細フィールドは不明。
///
/// dwango.nicolive.chat.data.GameUpdate
#[derive(Clone, PartialEq, ::prost::Message)]
pub struct GameUpdate {}

/// 機能フラグ等の更新通知。詳細フィールドは不明。
///
/// dwango.nicolive.chat.data.FeaturesUpdated
#[derive(Clone, PartialEq, ::prost::Message)]
pub struct FeaturesUpdated {}

/// Akashic ゲームエンジン（ニコ生インタラクティブコンテンツ）からのメッセージ。
/// 詳細フィールドは不明。
///
/// dwango.nicolive.chat.data.atoms.AkashicMessageEvent
#[derive(Clone, PartialEq, ::prost::Message)]
pub struct AkashicMessageEvent {}

/// ニコニコ市場（関連商品）の表示アイテムセット。詳細フィールドは不明。
///
/// dwango.nicolive.chat.data.IchibaLauncherItemSet
#[derive(Clone, PartialEq, ::prost::Message)]
pub struct IchibaLauncherItemSet {}

/// モデレーション関連のアナウンス。詳細フィールドは不明。
///
/// dwango.nicolive.chat.data.ModerationAnnouncement
#[derive(Clone, PartialEq, ::prost::Message)]
pub struct ModerationAnnouncement {}

/// 配信状態の変化通知。詳細フィールドは不明。
///
/// dwango.nicolive.chat.data.StreamStateChange
#[derive(Clone, PartialEq, ::prost::Message)]
pub struct StreamStateChange {}

/// Akashic エンジンの状態ルーティング。詳細フィールドは不明。
///
/// dwango.nicolive.chat.data.AkashicStateRouting
#[derive(Clone, PartialEq, ::prost::Message)]
pub struct AkashicStateRouting {}

// ── NicoliveState ─────────────────────────────────────────────────────────────

/// 放送の状態スナップショット。
///
/// コメントではなく「放送の状態」を表す。ChunkedMessage の Payload::State として届く。
/// 統計・アンケート・テロップ・コメント制限・放送終了などの状態変化を通知する。
///
/// dwango.nicolive.chat.data.NicoliveState
#[derive(Clone, PartialEq, ::prost::Message)]
pub struct NicoliveState {
    /// 視聴者数・コメント数・広告ポイント等の統計情報。
    #[prost(message, optional, tag = "1")]
    pub statistics: Option<Statistics>,
    /// アンケート（投票）の状態。
    #[prost(message, optional, tag = "2")]
    pub enquete: Option<Enquete>,
    /// 別の放送・URL への誘導命令（放送終了時のリダイレクト等）。
    #[prost(message, optional, tag = "3")]
    pub move_order: Option<MoveOrder>,
    /// テロップ（運営コメント）の表示設定。
    #[prost(message, optional, tag = "4")]
    pub marquee: Option<Marquee>,
    /// コメント制限の状態（フォロー限定コメント等）。
    #[prost(message, optional, tag = "5")]
    pub comment_lock: Option<CommentLock>,
    /// コメントの表示レイアウトモード。
    #[prost(message, optional, tag = "6")]
    pub comment_mode: Option<CommentMode>,
    /// トライアルパネルの表示設定（実験的機能）。
    #[prost(message, optional, tag = "7")]
    pub trial_panel: Option<TrialPanel>,
    /// アンチスパム用フィンガープリントの表示設定。
    #[prost(message, optional, tag = "8")]
    pub finger_print: Option<FingerPrint>,
    /// 放送の終了状態。
    #[prost(message, optional, tag = "9")]
    pub program_status: Option<ProgramStatus>,
    /// モデレーションアナウンス。
    #[prost(message, optional, tag = "10")]
    pub moderation_announcement: Option<ModerationAnnouncement>,
    /// ニコニコ市場のアイテムセット。
    #[prost(message, optional, tag = "11")]
    pub ichiba_launcher: Option<IchibaLauncherItemSet>,
    /// 配信状態の変化。
    #[prost(message, optional, tag = "12")]
    pub stream_state_change: Option<StreamStateChange>,
    /// Akashic エンジンの状態。
    #[prost(message, optional, tag = "13")]
    pub akashic_state: Option<AkashicStateRouting>,
}

// ── NicoliveMessage ───────────────────────────────────────────────────────────

/// ニコ生のメッセージ本体（コメント・ギフト・通知等すべてを含む）。
///
/// ChunkedMessage の Payload::Message として届く。
/// Data の oneof バリアントで、何種類のメッセージかを判別する。
///
/// dwango.nicolive.chat.data.NicoliveMessage
#[derive(Clone, PartialEq, ::prost::Message)]
pub struct NicoliveMessage {
    #[prost(
        oneof = "nicolive_message::Data",
        tags = "1, 7, 8, 9, 13, 17, 18, 19, 20, 21, 22, 23, 24, 25"
    )]
    pub data: Option<nicolive_message::Data>,
}

pub mod nicolive_message {
    /// メッセージの種別。
    #[derive(Clone, PartialEq, ::prost::Oneof)]
    pub enum Data {
        /// 通常のユーザーコメント。最も頻繁に届く。
        #[prost(message, tag = "1")]
        Chat(super::Chat),
        /// システム通知（来場・ランキング入り・放送延長等）。旧フォーマット。
        #[prost(message, tag = "7")]
        SimpleNotification(super::SimpleNotification),
        /// ニコ生ギフト（視聴者からの投げ銭的な仮想アイテム）。
        #[prost(message, tag = "8")]
        Gift(super::Gift),
        /// ニコニコ広告（視聴者がポイントを使って放送を宣伝した）。
        #[prost(message, tag = "9")]
        Nicoad(super::Nicoad),
        /// ゲーム関連の更新。詳細不明のプレースホルダー。
        #[prost(message, tag = "13")]
        GameUpdate(super::GameUpdate),
        /// 放送タグが更新された（タグの追加・削除・変更）。
        #[prost(message, tag = "17")]
        TagUpdated(super::TagUpdated),
        /// モデレーター（運営補佐）が追加または削除された。
        #[prost(message, tag = "18")]
        ModeratorUpdated(super::ModeratorUpdated),
        /// SSNG（放送固有の NG 設定）が更新された。モデレーター専用情報。
        #[prost(message, tag = "19")]
        SsngUpdated(super::SsngUpdated),
        /// コメントが溢れた際のコメント。Chat と同じ構造で処理できる。
        /// コメント投稿数が上限を超えた場合に古いものが OverflowedChat として届く。
        #[prost(message, tag = "20")]
        OverflowedChat(super::Chat),
        /// スパム・BAN 判定されたコメント（通常は UI に表示しない）。
        /// 個人情報（ユーザー ID・IP 関連）を含む可能性があるため慎重に扱う。
        #[prost(message, tag = "21")]
        PreCensored(super::PreCensored),
        /// 別の放送から転送されてきたコメント（クルーズ・コラボ等）。
        #[prost(message, tag = "22")]
        ForwardedChat(super::ForwardedChat),
        /// システム通知（来場・ランキング入り等）。新フォーマット。
        /// `show_in_list=true` のものだけコメント一覧に表示する。
        #[prost(message, tag = "23")]
        SimpleNotificationV2(super::SimpleNotificationV2),
        /// Akashic ゲームエンジンからのメッセージ。詳細不明のプレースホルダー。
        #[prost(message, tag = "24")]
        AkashicMessageEvent(super::AkashicMessageEvent),
        /// 機能フラグ等の更新。詳細不明のプレースホルダー。
        #[prost(message, tag = "25")]
        FeaturesUpdated(super::FeaturesUpdated),
    }
}
