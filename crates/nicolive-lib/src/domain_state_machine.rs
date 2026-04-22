//! ニコニコ生放送 WebSocket セッション用ドメイン状態機械
//!
//! 設計意図:
//! - WebSocket セッションの論理状態管理を副作用なしに実装し、テスト容易性を確保する。
//! - on_event(DomainEvent) → Vec<DomainCommand> の純粋変換として定義する。
//! - ランナー（connection.rs）は返された DomainCommand を実行するだけに責務を限定する。

use chrono::DateTime;
use serde_json::Value;

/// ドメイン層エラー
///
/// 設計意図:
/// - 状態遷移違反（InvalidTransition）を呼び出し側が検出しやすくする。
#[derive(Debug, Clone, PartialEq, Eq)]
pub enum DomainError {
    InvalidTransition {
        state: &'static str,
        event: &'static str,
    },
}

impl std::fmt::Display for DomainError {
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        match self {
            Self::InvalidTransition { state, event } => {
                write!(f, "invalid transition: state={state}, event={event}")
            }
        }
    }
}

impl std::error::Error for DomainError {}

/// 状態機械の内部状態
///
/// 設計意図:
/// - WaitingForSeat: startWatching 送信後、seat メッセージ待ち
/// - Active: seat 受信済み、keepSeat インターバルで定期送信しながらコメント受信中
/// - Stopped: 終端（再遷移なし）
enum DomainState {
    WaitingForSeat,
    Active {
        keep_interval_secs: u64,
        view_polling_active: bool,
    },
    Stopped,
}

/// 外部観測用の状態スナップショット
///
/// 設計意図:
/// - 内部実装型（DomainState）を公開せず、テスト・デバッグで状態を参照可能にする。
#[derive(Debug, Clone, PartialEq, Eq)]
pub enum StateSnapshot {
    WaitingForSeat,
    Active {
        keep_interval_secs: u64,
        view_polling_active: bool,
    },
    Stopped,
}

/// 状態機械への入力イベント
///
/// 設計意図:
/// - WsText はニコ生アプリプロトコルの JSON テキスト（WebSocket テキストフレーム）。
/// - WS プロトコルレベルの Ping/Pong はランナーが直接処理し、ここには含まない。
pub enum DomainEvent {
    /// WebSocket テキストメッセージ（ニコ生アプリプロトコル JSON）
    WsText(String),
    /// WebSocket が閉じられた（サーバー側クローズまたはネットワーク切断）
    WsClosed,
    /// keepSeat タイマーが発火した（ランナーが指定間隔で送信する）
    KeepSeatTick,
    /// ユーザーが切断を要求した
    Disconnect,
}

/// 状態機械が要求する副作用コマンド
///
/// 設計意図:
/// - 状態遷移（純粋ロジック）と副作用実行（通信・タスク管理）を完全分離する。
/// - ランナーはこのコマンドを実行するだけに責務を限定できる。
#[derive(Debug)]
pub enum DomainCommand {
    /// アプリレベルの pong を WebSocket で送信する
    SendPong,
    /// keepSeat メッセージを WebSocket で送信する
    SendKeepSeat,
    /// keepSeat タイマーを設定する（指定間隔ごとに KeepSeatTick イベントが発火するよう設定）
    SetKeepSeatInterval { interval_secs: u64 },
    /// viewUri ポーリングタスクをスポーンする
    SpawnViewPolling { view_uri: String },
    /// ServerTimeCache を更新する
    UpdateServerTime { server_secs: i64 },
    /// 視聴者数統計を StreamMetadata として送信する
    EmitStatistics { viewers: u64 },
    /// audience_token で再接続する（ランナーが URL を構築し wait_secs 後に接続）
    ReconnectTo {
        audience_token: String,
        wait_secs: u64,
    },
    /// セッションを終了し Disconnected を通知する
    EmitDisconnected,
}

/// ニコニコ生放送 WebSocket セッション状態機械
///
/// 設計意図:
/// - on_event() を主入口として、イベントからコマンドリストを純粋に計算する。
/// - 副作用は持たない。テスト時は実際の WebSocket・HTTP なしに遷移を検証できる。
pub struct NicoLiveStateMachine {
    state: DomainState,
}

impl NicoLiveStateMachine {
    /// startWatching 送信直後（seat 待ち）の状態で初期化する。
    pub fn new() -> Self {
        Self {
            state: DomainState::WaitingForSeat,
        }
    }

    /// 現在の状態スナップショットを返す。
    pub fn current_state(&self) -> StateSnapshot {
        match &self.state {
            DomainState::WaitingForSeat => StateSnapshot::WaitingForSeat,
            DomainState::Active {
                keep_interval_secs,
                view_polling_active,
            } => StateSnapshot::Active {
                keep_interval_secs: *keep_interval_secs,
                view_polling_active: *view_polling_active,
            },
            DomainState::Stopped => StateSnapshot::Stopped,
        }
    }

    /// イベントを受けて状態を更新し、ランナーが実行すべきコマンドリストを返す。
    pub fn on_event(&mut self, event: DomainEvent) -> Result<Vec<DomainCommand>, DomainError> {
        match event {
            DomainEvent::WsText(text) => self.handle_ws_text(text),
            DomainEvent::WsClosed => {
                self.state = DomainState::Stopped;
                Ok(vec![DomainCommand::EmitDisconnected])
            }
            DomainEvent::KeepSeatTick => match self.state {
                DomainState::Active { .. } => Ok(vec![DomainCommand::SendKeepSeat]),
                _ => Err(DomainError::InvalidTransition {
                    state: self.state_name(),
                    event: "KeepSeatTick",
                }),
            },
            DomainEvent::Disconnect => match self.state {
                DomainState::Stopped => Ok(vec![]),
                _ => {
                    self.state = DomainState::Stopped;
                    Ok(vec![DomainCommand::EmitDisconnected])
                }
            },
        }
    }

    /// WsText イベントを解析して状態遷移とコマンドを決定する内部処理。
    ///
    /// 設計意図:
    /// - JSON パース失敗・未知メッセージ型はいずれも Ok(vec![]) を返す（無視）。
    ///   ランナー側でトレースログを出力することで可視性を確保する。
    fn handle_ws_text(&mut self, text: String) -> Result<Vec<DomainCommand>, DomainError> {
        let json: Value = match serde_json::from_str(&text) {
            Ok(v) => v,
            Err(_) => return Ok(vec![]),
        };

        let msg_type = match json["type"].as_str() {
            Some(t) => t,
            None => return Ok(vec![]),
        };

        match msg_type {
            "ping" => Ok(vec![DomainCommand::SendPong]),

            "seat" => {
                let keep_interval_secs = json["data"]["keepIntervalSec"].as_u64().unwrap_or(30);
                self.state = DomainState::Active {
                    keep_interval_secs,
                    view_polling_active: false,
                };
                Ok(vec![DomainCommand::SetKeepSeatInterval {
                    interval_secs: keep_interval_secs,
                }])
            }

            "messageServer" => {
                let view_uri = json["data"]["viewUri"]
                    .as_str()
                    .unwrap_or_default()
                    .to_string();
                if view_uri.is_empty() {
                    return Ok(vec![]);
                }
                if let DomainState::Active {
                    view_polling_active,
                    ..
                } = &mut self.state
                {
                    *view_polling_active = true;
                }
                Ok(vec![DomainCommand::SpawnViewPolling { view_uri }])
            }

            "reconnect" => {
                let audience_token = json["data"]["audienceToken"]
                    .as_str()
                    .unwrap_or_default()
                    .to_string();
                let wait_secs = json["data"]["waitTimeSec"].as_u64().unwrap_or(0);
                self.state = DomainState::Stopped;
                Ok(vec![DomainCommand::ReconnectTo {
                    audience_token,
                    wait_secs,
                }])
            }

            "disconnect" => {
                self.state = DomainState::Stopped;
                Ok(vec![DomainCommand::EmitDisconnected])
            }

            "serverTime" => {
                let server_secs = json["data"]["currentMs"]
                    .as_str()
                    .and_then(|s| DateTime::parse_from_rfc3339(s).ok())
                    .map(|dt| dt.timestamp());
                match server_secs {
                    Some(server_secs) => Ok(vec![DomainCommand::UpdateServerTime { server_secs }]),
                    None => Ok(vec![]),
                }
            }

            "statistics" => {
                let viewers = json["data"]["viewers"].as_u64().unwrap_or(0);
                Ok(vec![DomainCommand::EmitStatistics { viewers }])
            }

            _ => Ok(vec![]),
        }
    }

    /// エラー表示用の状態名。
    fn state_name(&self) -> &'static str {
        match self.state {
            DomainState::WaitingForSeat => "WaitingForSeat",
            DomainState::Active { .. } => "Active",
            DomainState::Stopped => "Stopped",
        }
    }
}

impl Default for NicoLiveStateMachine {
    fn default() -> Self {
        Self::new()
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn ping_generates_pong() {
        let mut m = NicoLiveStateMachine::new();
        let cmds = m
            .on_event(DomainEvent::WsText(r#"{"type":"ping"}"#.to_string()))
            .unwrap();
        assert!(matches!(cmds.first(), Some(DomainCommand::SendPong)));
    }

    #[test]
    fn seat_transitions_to_active_and_sets_interval() {
        let mut m = NicoLiveStateMachine::new();
        let cmds = m
            .on_event(DomainEvent::WsText(
                r#"{"type":"seat","data":{"keepIntervalSec":60}}"#.to_string(),
            ))
            .unwrap();
        assert!(matches!(
            cmds.first(),
            Some(DomainCommand::SetKeepSeatInterval { interval_secs: 60 })
        ));
        assert_eq!(
            m.current_state(),
            StateSnapshot::Active {
                keep_interval_secs: 60,
                view_polling_active: false,
            }
        );
    }

    #[test]
    fn seat_uses_default_30s_when_field_absent() {
        let mut m = NicoLiveStateMachine::new();
        let cmds = m
            .on_event(DomainEvent::WsText(
                r#"{"type":"seat","data":{}}"#.to_string(),
            ))
            .unwrap();
        assert!(matches!(
            cmds.first(),
            Some(DomainCommand::SetKeepSeatInterval { interval_secs: 30 })
        ));
    }

    #[test]
    fn message_server_sets_view_polling_active() {
        let mut m = NicoLiveStateMachine::new();
        m.on_event(DomainEvent::WsText(
            r#"{"type":"seat","data":{"keepIntervalSec":30}}"#.to_string(),
        ))
        .unwrap();
        let cmds = m
            .on_event(DomainEvent::WsText(
                r#"{"type":"messageServer","data":{"viewUri":"https://example.com/view"}}"#
                    .to_string(),
            ))
            .unwrap();
        assert!(matches!(
            cmds.first(),
            Some(DomainCommand::SpawnViewPolling { .. })
        ));
        if let Some(DomainCommand::SpawnViewPolling { view_uri }) = cmds.first() {
            assert_eq!(view_uri, "https://example.com/view");
        }
        assert_eq!(
            m.current_state(),
            StateSnapshot::Active {
                keep_interval_secs: 30,
                view_polling_active: true,
            }
        );
    }

    #[test]
    fn message_server_with_empty_view_uri_is_ignored() {
        let mut m = NicoLiveStateMachine::new();
        m.on_event(DomainEvent::WsText(
            r#"{"type":"seat","data":{"keepIntervalSec":30}}"#.to_string(),
        ))
        .unwrap();
        let cmds = m
            .on_event(DomainEvent::WsText(
                r#"{"type":"messageServer","data":{"viewUri":""}}"#.to_string(),
            ))
            .unwrap();
        assert!(cmds.is_empty());
        assert_eq!(
            m.current_state(),
            StateSnapshot::Active {
                keep_interval_secs: 30,
                view_polling_active: false,
            }
        );
    }

    #[test]
    fn reconnect_generates_reconnect_to_command() {
        let mut m = NicoLiveStateMachine::new();
        let cmds = m
            .on_event(DomainEvent::WsText(
                r#"{"type":"reconnect","data":{"audienceToken":"TOKEN_XYZ","waitTimeSec":5}}"#
                    .to_string(),
            ))
            .unwrap();
        match cmds.first() {
            Some(DomainCommand::ReconnectTo {
                audience_token,
                wait_secs,
            }) => {
                assert_eq!(audience_token, "TOKEN_XYZ");
                assert_eq!(*wait_secs, 5);
            }
            _ => panic!("ReconnectTo を期待したが別のコマンドが返った"),
        }
        assert_eq!(m.current_state(), StateSnapshot::Stopped);
    }

    #[test]
    fn server_disconnect_generates_emit_disconnected() {
        let mut m = NicoLiveStateMachine::new();
        let cmds = m
            .on_event(DomainEvent::WsText(r#"{"type":"disconnect"}"#.to_string()))
            .unwrap();
        assert!(matches!(
            cmds.first(),
            Some(DomainCommand::EmitDisconnected)
        ));
        assert_eq!(m.current_state(), StateSnapshot::Stopped);
    }

    #[test]
    fn ws_closed_generates_emit_disconnected() {
        let mut m = NicoLiveStateMachine::new();
        let cmds = m.on_event(DomainEvent::WsClosed).unwrap();
        assert!(matches!(
            cmds.first(),
            Some(DomainCommand::EmitDisconnected)
        ));
        assert_eq!(m.current_state(), StateSnapshot::Stopped);
    }

    #[test]
    fn keep_seat_tick_in_active_generates_send_keep_seat() {
        let mut m = NicoLiveStateMachine::new();
        m.on_event(DomainEvent::WsText(
            r#"{"type":"seat","data":{"keepIntervalSec":30}}"#.to_string(),
        ))
        .unwrap();
        let cmds = m.on_event(DomainEvent::KeepSeatTick).unwrap();
        assert!(matches!(cmds.first(), Some(DomainCommand::SendKeepSeat)));
    }

    #[test]
    fn keep_seat_tick_in_waiting_is_invalid_transition() {
        let mut m = NicoLiveStateMachine::new();
        let result = m.on_event(DomainEvent::KeepSeatTick);
        assert_eq!(
            result.unwrap_err(),
            DomainError::InvalidTransition {
                state: "WaitingForSeat",
                event: "KeepSeatTick",
            }
        );
    }

    #[test]
    fn disconnect_event_from_any_state_generates_emit_disconnected() {
        // WaitingForSeat から
        let mut m = NicoLiveStateMachine::new();
        let cmds = m.on_event(DomainEvent::Disconnect).unwrap();
        assert!(matches!(
            cmds.first(),
            Some(DomainCommand::EmitDisconnected)
        ));

        // Active から
        let mut m = NicoLiveStateMachine::new();
        m.on_event(DomainEvent::WsText(
            r#"{"type":"seat","data":{"keepIntervalSec":30}}"#.to_string(),
        ))
        .unwrap();
        let cmds = m.on_event(DomainEvent::Disconnect).unwrap();
        assert!(matches!(
            cmds.first(),
            Some(DomainCommand::EmitDisconnected)
        ));
    }

    #[test]
    fn disconnect_event_from_stopped_is_noop() {
        let mut m = NicoLiveStateMachine::new();
        m.on_event(DomainEvent::WsText(r#"{"type":"disconnect"}"#.to_string()))
            .unwrap();
        let cmds = m.on_event(DomainEvent::Disconnect).unwrap();
        assert!(cmds.is_empty());
    }

    #[test]
    fn statistics_generates_emit_statistics() {
        let mut m = NicoLiveStateMachine::new();
        let cmds = m
            .on_event(DomainEvent::WsText(
                r#"{"type":"statistics","data":{"viewers":1234}}"#.to_string(),
            ))
            .unwrap();
        assert!(matches!(
            cmds.first(),
            Some(DomainCommand::EmitStatistics { viewers: 1234 })
        ));
    }

    #[test]
    fn server_time_generates_update_server_time() {
        let mut m = NicoLiveStateMachine::new();
        let cmds = m
            .on_event(DomainEvent::WsText(
                r#"{"type":"serverTime","data":{"currentMs":"2025-01-01T00:00:00Z"}}"#.to_string(),
            ))
            .unwrap();
        assert!(matches!(
            cmds.first(),
            Some(DomainCommand::UpdateServerTime {
                server_secs: 1_735_689_600
            })
        ));
    }

    #[test]
    fn unknown_message_type_is_ignored() {
        let mut m = NicoLiveStateMachine::new();
        let cmds = m
            .on_event(DomainEvent::WsText(
                r#"{"type":"unknownFutureMessage","data":{}}"#.to_string(),
            ))
            .unwrap();
        assert!(cmds.is_empty());
    }

    #[test]
    fn invalid_json_is_ignored() {
        let mut m = NicoLiveStateMachine::new();
        let cmds = m
            .on_event(DomainEvent::WsText("not json at all".to_string()))
            .unwrap();
        assert!(cmds.is_empty());
    }
}
