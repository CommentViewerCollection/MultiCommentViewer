use async_trait::async_trait;
use serde::{Deserialize, Serialize};

use crate::{
    Action, Continuation, Cookie, LiveChat, Vid, Ytcfg, extract_ytcfg, get_live_chat,
    get_live_chat_messages, get_yt_initial_data, send_chat_message,
};

/// ドメイン層で扱うエラー。
///
/// 設計意図:
/// - 状態遷移違反 (`InvalidTransition`) と、外部依存失敗 (`Parse/Server`) を分離する。
/// - 呼び出し側が「ロジックバグ」か「入力/通信失敗」かを判定しやすくする。
#[derive(Debug, Clone, PartialEq, Eq)]
pub enum DomainError {
    InvalidTransition {
        state: &'static str,
        event: &'static str,
    },
    ParseFailed(String),
    ServerFailed(String),
}

impl std::fmt::Display for DomainError {
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        match self {
            Self::InvalidTransition { state, event } => {
                write!(f, "invalid transition: state={state}, event={event}")
            }
            Self::ParseFailed(msg) => write!(f, "parse failed: {msg}"),
            Self::ServerFailed(msg) => write!(f, "server failed: {msg}"),
        }
    }
}

impl std::error::Error for DomainError {}

/// 1ステップ処理の中間出力（内部専用）。
///
/// 設計意図:
/// - `on_event` から直接 Command を組み立てる際の一時データとして使う。
/// - 複数値(actions/continuation/raw/viewer情報)をまとめて扱うため struct にしている。
#[derive(Clone)]
struct DomainOutput {
    pub actions: Vec<Action>,
    pub continuation: Option<Continuation>,
    pub raw: Option<String>,
    pub send_message_params: Option<String>,
    pub viewer_name: Option<String>,
    pub viewer_avatar_url: Option<String>,
}

/// 状態機械の内部状態。
///
/// 設計意図:
/// - `WaitingInitialLiveChat`: 初回HTML待ち。
/// - `Polling`: continuation を持って継続取得可能。
/// - `Stopped`: これ以上遷移しない終端。
#[derive(Clone)]
enum DomainState {
    WaitingInitialLiveChat,
    Polling {
        ytcfg: Ytcfg,
        continuation: Continuation,
        send_message_params: Option<String>,
    },
    Stopped,
}

/// 現状態から見た「次に期待するI/O」の要約。
///
/// 設計意図:
/// - 外部ランナーがポーリング方針を決めるためのヒントとして使用する。
#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub enum NextStep {
    FetchLiveChat,
    FetchMessages,
    Halted,
}

/// 外部観測用の状態スナップショット。
///
/// 設計意図:
/// - 内部実装型(`DomainState`)を公開せず、テスト/デバッグで状態を参照可能にする。
#[derive(Debug, Clone, PartialEq, Eq)]
pub enum StateSnapshot {
    WaitingInitialLiveChat,
    Polling {
        needs_reload: bool,
        timeout_ms: Option<u64>,
        can_send_chat: bool,
    },
    Stopped,
}

/// 状態機械の挙動設定。
///
/// 設計意図:
/// - ポーリング間隔やリトライ待機など、ランナーと共有したいポリシー値を注入可能にする。
#[derive(Debug, Clone)]
pub struct StateMachineConfig {
    pub default_poll_delay_ms: u64,
    pub min_poll_delay_ms: u64,
    pub max_poll_delay_ms: u64,
    pub reload_retry_delay_ms: u64,
    pub max_consecutive_errors: usize,
}

impl Default for StateMachineConfig {
    fn default() -> Self {
        Self {
            default_poll_delay_ms: 5000,
            min_poll_delay_ms: 500,
            max_poll_delay_ms: 8000,
            reload_retry_delay_ms: 5000,
            max_consecutive_errors: 3,
        }
    }
}

/// 状態機械への入力イベント。
///
/// 設計意図:
/// - ログ再生・実通信のどちらでも同じ経路を通せるよう、
///   「結果受領イベント」(LiveChat/Messages) を明示的に持つ。
pub enum DomainEvent {
    Start,
    LiveChat(LiveChat),
    Messages {
        continuation: Option<Continuation>,
        actions: Vec<Action>,
        raw: String,
    },
    SendChat {
        text: String,
    },
    Disconnect,
}

/// 状態機械が要求する副作用コマンド。
///
/// 設計意図:
/// - 状態遷移(純粋ロジック)と副作用実行(通信/通知)を完全分離する。
/// - Runner はこの Command を実行するだけに責務を限定できる。
pub enum DomainCommand {
    FetchLiveChat,
    FetchMessages {
        ytcfg: Ytcfg,
        continuation: Continuation,
        delay_ms: u64,
    },
    EmitActions {
        actions: Vec<Action>,
        raw: Option<String>,
        as_history: bool,
    },
    EmitAccount {
        display_name: String,
        avatar_url: Option<String>,
    },
    SendChat {
        ytcfg: Ytcfg,
        send_message_params: String,
        text: String,
    },
    EmitDisconnected,
}

/// ログ再生や永続化向けのシリアライズ可能イベント。
///
/// 設計意図:
/// - `DomainEvent` は実行時型(`LiveChat`, `Action`)を含むため、そのままJSON化しにくい。
/// - 入出力をログに残す用途ではこの型を使い、実行時型へ変換する。
#[derive(Debug, Clone, Serialize, Deserialize)]
#[serde(tag = "type", rename_all = "snake_case")]
pub enum ReplayEvent {
    Start,
    LiveChatHtml {
        html: String,
    },
    MessagesRaw {
        continuation: Option<ReplayContinuation>,
        raw: String,
    },
    SendChat {
        text: String,
    },
    Disconnect,
}

#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct ReplayContinuation {
    pub value: String,
    pub timeout_ms: Option<u64>,
    pub needs_reload: bool,
}

impl ReplayEvent {
    pub fn into_domain_event(self, actions: Vec<Action>) -> DomainEvent {
        match self {
            ReplayEvent::Start => DomainEvent::Start,
            ReplayEvent::LiveChatHtml { html } => DomainEvent::LiveChat(LiveChat::new(html)),
            ReplayEvent::MessagesRaw { continuation, raw } => DomainEvent::Messages {
                continuation: continuation.map(|c| {
                    let mut cont = Continuation::new(c.value);
                    cont.timeout_ms = c.timeout_ms;
                    cont.needs_reload = c.needs_reload;
                    cont
                }),
                actions,
                raw,
            },
            ReplayEvent::SendChat { text } => DomainEvent::SendChat { text },
            ReplayEvent::Disconnect => DomainEvent::Disconnect,
        }
    }
}

/// ログ向けのシリアライズ可能コマンド。
#[derive(Debug, Clone, Serialize, Deserialize)]
#[serde(tag = "type", rename_all = "snake_case")]
pub enum ReplayCommand {
    FetchLiveChat,
    FetchMessages {
        continuation: ReplayContinuation,
        delay_ms: u64,
    },
    EmitActions {
        as_history: bool,
    },
    EmitAccount,
    SendChat,
    EmitDisconnected,
}

impl From<&DomainCommand> for ReplayCommand {
    fn from(value: &DomainCommand) -> Self {
        match value {
            DomainCommand::FetchLiveChat => ReplayCommand::FetchLiveChat,
            DomainCommand::FetchMessages {
                continuation,
                delay_ms,
                ..
            } => ReplayCommand::FetchMessages {
                continuation: ReplayContinuation {
                    value: continuation.value().to_string(),
                    timeout_ms: continuation.timeout_ms,
                    needs_reload: continuation.needs_reload,
                },
                delay_ms: *delay_ms,
            },
            DomainCommand::EmitActions { as_history, .. } => ReplayCommand::EmitActions {
                as_history: *as_history,
            },
            DomainCommand::EmitAccount { .. } => ReplayCommand::EmitAccount,
            DomainCommand::SendChat { .. } => ReplayCommand::SendChat,
            DomainCommand::EmitDisconnected => ReplayCommand::EmitDisconnected,
        }
    }
}

/// YouTube Live 用のドメイン状態機械本体。
///
/// 設計意図:
/// - 入力(Event)から出力(Command)を決定する中核。
/// - 副作用は持たず、テストしやすい純粋ロジックを保つ。
pub struct YoutubeLiveStateMachine {
    state: DomainState,
    config: StateMachineConfig,
}

impl YoutubeLiveStateMachine {
    /// 新規インスタンスは初回 LiveChat 待ちで開始する。
    pub fn new() -> Self {
        Self {
            state: DomainState::WaitingInitialLiveChat,
            config: StateMachineConfig::default(),
        }
    }

    pub fn with_config(config: StateMachineConfig) -> Self {
        Self {
            state: DomainState::WaitingInitialLiveChat,
            config,
        }
    }

    pub fn config(&self) -> &StateMachineConfig {
        &self.config
    }

    pub fn current_state(&self) -> StateSnapshot {
        match &self.state {
            DomainState::WaitingInitialLiveChat => StateSnapshot::WaitingInitialLiveChat,
            DomainState::Polling {
                continuation,
                send_message_params,
                ..
            } => StateSnapshot::Polling {
                needs_reload: continuation.needs_reload,
                timeout_ms: continuation.timeout_ms,
                can_send_chat: send_message_params.is_some(),
            },
            DomainState::Stopped => StateSnapshot::Stopped,
        }
    }

    /// 現状態から見た次ステップの要約。
    pub fn next_step(&self) -> NextStep {
        match &self.state {
            DomainState::WaitingInitialLiveChat => NextStep::FetchLiveChat,
            DomainState::Polling { continuation, .. } => {
                if continuation.needs_reload {
                    NextStep::FetchLiveChat
                } else {
                    NextStep::FetchMessages
                }
            }
            DomainState::Stopped => NextStep::Halted,
        }
    }

    /// Event を受けて、状態更新と Command 生成を行う。
    ///
    /// 設計意図:
    /// - 外部からはこの関数を主入口として使う。
    /// - `Start -> Fetch*`, `LiveChat/Messages -> Emit* + 次Fetch`, `SendChat -> SendChat`
    ///   のように、遷移と副作用要求を一貫して決定する。
    pub fn on_event(&mut self, event: DomainEvent) -> Result<Vec<DomainCommand>, DomainError> {
        match event {
            DomainEvent::Start => match &self.state {
                DomainState::WaitingInitialLiveChat => Ok(vec![DomainCommand::FetchLiveChat]),
                DomainState::Polling {
                    ytcfg,
                    continuation,
                    ..
                } => Ok(vec![
                    self.next_fetch_command(ytcfg.clone(), continuation.clone()),
                ]),
                DomainState::Stopped => Err(DomainError::InvalidTransition {
                    state: self.state_name(),
                    event: "Start",
                }),
            },
            DomainEvent::LiveChat(live_chat) => {
                let was_waiting = matches!(self.state, DomainState::WaitingInitialLiveChat);
                let output = self.on_live_chat(live_chat)?;
                let mut commands = vec![];

                if was_waiting {
                    if let Some(name) = output.viewer_name {
                        commands.push(DomainCommand::EmitAccount {
                            display_name: name,
                            avatar_url: output.viewer_avatar_url,
                        });
                    }
                    if !output.actions.is_empty() {
                        commands.push(DomainCommand::EmitActions {
                            actions: output.actions,
                            raw: output.raw,
                            as_history: true,
                        });
                    }
                }

                if let Some(next) = output.continuation {
                    commands.push(self.next_fetch_command(self.poll_context()?.0.clone(), next));
                } else {
                    commands.push(DomainCommand::EmitDisconnected);
                }
                Ok(commands)
            }
            DomainEvent::Messages {
                continuation,
                actions,
                raw,
            } => {
                let output = self.on_messages(continuation, actions, raw)?;
                let mut commands = vec![];
                if !output.actions.is_empty() {
                    commands.push(DomainCommand::EmitActions {
                        actions: output.actions,
                        raw: output.raw,
                        as_history: false,
                    });
                }
                if let Some(next) = output.continuation {
                    commands.push(self.next_fetch_command(self.poll_context()?.0.clone(), next));
                } else {
                    commands.push(DomainCommand::EmitDisconnected);
                }
                Ok(commands)
            }
            DomainEvent::SendChat { text } => {
                if let DomainState::Polling {
                    ytcfg,
                    send_message_params: Some(params),
                    ..
                } = &self.state
                {
                    Ok(vec![DomainCommand::SendChat {
                        ytcfg: ytcfg.clone(),
                        send_message_params: params.clone(),
                        text,
                    }])
                } else {
                    Err(DomainError::InvalidTransition {
                        state: self.state_name(),
                        event: "SendChat",
                    })
                }
            }
            DomainEvent::Disconnect => match self.state {
                DomainState::Stopped => Ok(vec![]),
                _ => {
                    self.state = DomainState::Stopped;
                    Ok(vec![DomainCommand::EmitDisconnected])
                }
            },
        }
    }

    /// continuation から次の取得コマンドを構築する。
    ///
    /// 設計意図:
    /// - reload 要求と通常ポーリングをここで一本化する。
    /// - timeoutMs の clamp もここに集約する。
    fn next_fetch_command(&self, ytcfg: Ytcfg, continuation: Continuation) -> DomainCommand {
        if continuation.needs_reload {
            DomainCommand::FetchLiveChat
        } else {
            let delay_ms = continuation
                .timeout_ms
                .unwrap_or(self.config.default_poll_delay_ms)
                .clamp(self.config.min_poll_delay_ms, self.config.max_poll_delay_ms);
            DomainCommand::FetchMessages {
                ytcfg,
                continuation,
                delay_ms,
            }
        }
    }

    /// LiveChat HTML を解釈して初期状態を構築する内部処理。
    ///
    /// 設計意図:
    /// - 初回または reload 時のみ受け付ける。
    /// - `ytcfg`, `continuation`, `send_message_params`, `viewer情報` を抽出し
    ///   Polling 状態へ遷移する。
    fn on_live_chat(&mut self, live_chat: LiveChat) -> Result<DomainOutput, DomainError> {
        match &self.state {
            DomainState::WaitingInitialLiveChat => {}
            DomainState::Polling { continuation, .. } if continuation.needs_reload => {}
            DomainState::Polling { .. } => {
                return Err(DomainError::InvalidTransition {
                    state: "Polling",
                    event: "LiveChat",
                });
            }
            DomainState::Stopped => {
                return Err(DomainError::InvalidTransition {
                    state: "Stopped",
                    event: "LiveChat",
                });
            }
        }

        let ytcfg =
            extract_ytcfg(&live_chat).map_err(|e| DomainError::ParseFailed(e.to_string()))?;
        let initial =
            get_yt_initial_data(&live_chat).map_err(|e| DomainError::ParseFailed(e.to_string()))?;

        let continuation = initial.continuation().clone();
        // reload要求後は既存パラメータを再利用しない。
        let send_message_params = initial.send_message_params();
        let output = DomainOutput {
            actions: initial.actions().clone(),
            continuation: Some(continuation.clone()),
            raw: Some(initial.raw().to_string()),
            send_message_params: send_message_params.clone(),
            viewer_name: initial.viewer_name().map(|s| s.to_string()),
            viewer_avatar_url: initial.viewer_avatar_url().map(|s| s.to_string()),
        };

        self.state = DomainState::Polling {
            ytcfg,
            continuation,
            send_message_params,
        };

        Ok(output)
    }

    /// get_live_chat_messages の結果を反映する内部処理。
    ///
    /// 設計意図:
    /// - continuation があれば Polling 継続、なければ Stopped へ遷移する。
    fn on_messages(
        &mut self,
        maybe_continuation: Option<Continuation>,
        actions: Vec<Action>,
        raw: String,
    ) -> Result<DomainOutput, DomainError> {
        let DomainState::Polling {
            ytcfg,
            send_message_params,
            ..
        } = &self.state
        else {
            return Err(DomainError::InvalidTransition {
                state: self.state_name(),
                event: "Messages",
            });
        };

        let ytcfg = ytcfg.clone();
        let send_message_params = send_message_params.clone();

        match maybe_continuation.clone() {
            Some(continuation) => {
                self.state = DomainState::Polling {
                    ytcfg,
                    continuation: continuation.clone(),
                    send_message_params: send_message_params.clone(),
                };
                Ok(DomainOutput {
                    actions,
                    continuation: Some(continuation),
                    raw: Some(raw),
                    send_message_params,
                    viewer_name: None,
                    viewer_avatar_url: None,
                })
            }
            None => {
                self.state = DomainState::Stopped;
                Ok(DomainOutput {
                    actions,
                    continuation: None,
                    raw: Some(raw),
                    send_message_params,
                    viewer_name: None,
                    viewer_avatar_url: None,
                })
            }
        }
    }

    /// Polling に必要な `ytcfg/continuation` を取得する。
    ///
    /// 設計意図:
    /// - Runner 側が `FetchMessages` を実行する際の文脈取得に使う。
    pub fn poll_context(&self) -> Result<(&Ytcfg, &Continuation), DomainError> {
        if let DomainState::Polling {
            ytcfg,
            continuation,
            ..
        } = &self.state
        {
            Ok((ytcfg, continuation))
        } else {
            Err(DomainError::InvalidTransition {
                state: self.state_name(),
                event: "PollContext",
            })
        }
    }

    /// エラー表示用の状態名。
    fn state_name(&self) -> &'static str {
        match self.state {
            DomainState::WaitingInitialLiveChat => "WaitingInitialLiveChat",
            DomainState::Polling { .. } => "Polling",
            DomainState::Stopped => "Stopped",
        }
    }
}

impl Default for YoutubeLiveStateMachine {
    fn default() -> Self {
        Self::new()
    }
}

/// ドメイン層が要求するサーバーI/O抽象。
///
/// 設計意図:
/// - 実通信・ログ再生・テストモックを同一インターフェースで差し替える。
#[async_trait]
pub trait LiveChatServer {
    async fn get_live_chat(&self, vid: &Vid) -> Result<LiveChat, DomainError>;
    async fn get_live_chat_messages(
        &self,
        vid: &Vid,
        ytcfg: &Ytcfg,
        continuation: &Continuation,
    ) -> Result<(Option<Continuation>, Vec<Action>, String), DomainError>;
    async fn send_chat(
        &self,
        ytcfg: &Ytcfg,
        send_message_params: &str,
        text: &str,
    ) -> Result<(), DomainError>;
}

/// `LiveChatServer` と state machine を束ねる補助ランナー。
///
/// 設計意図:
/// - 旧API互換や段階移行用の薄いラッパとして保持。
/// - 新規コードは `on_event + DomainCommand` 直接利用を推奨。
pub struct YoutubeLiveDomain<S> {
    vid: Vid,
    server: S,
    machine: YoutubeLiveStateMachine,
}

impl<S> YoutubeLiveDomain<S>
where
    S: LiveChatServer + Send + Sync,
{
    /// ランナー生成。
    pub fn new(vid: Vid, server: S) -> Self {
        Self {
            vid,
            server,
            machine: YoutubeLiveStateMachine::new(),
        }
    }

    /// 内部状態機械への参照。
    pub fn machine(&self) -> &YoutubeLiveStateMachine {
        &self.machine
    }

    /// 1ステップ実行（非推奨）。
    ///
    /// `on_event` + `DomainCommand` 方式へ移行済みのため、
    /// 新規コードでは `YoutubeLiveStateMachine::on_event` を直接利用する。
    #[deprecated(note = "Use YoutubeLiveStateMachine::on_event + DomainCommand instead")]
    pub async fn run_step(&mut self) -> Result<Vec<ReplayCommand>, DomainError> {
        match self.machine.next_step() {
            NextStep::FetchLiveChat => {
                let live_chat = self.server.get_live_chat(&self.vid).await?;
                let cmds = self.machine.on_event(DomainEvent::LiveChat(live_chat))?;
                Ok(cmds.iter().map(ReplayCommand::from).collect())
            }
            NextStep::FetchMessages => {
                let (ytcfg, continuation) = self.machine.poll_context()?;
                let (next, actions, raw) = self
                    .server
                    .get_live_chat_messages(&self.vid, ytcfg, continuation)
                    .await?;
                let cmds = self.machine.on_event(DomainEvent::Messages {
                    continuation: next,
                    actions,
                    raw,
                })?;
                Ok(cmds.iter().map(ReplayCommand::from).collect())
            }
            NextStep::Halted => Ok(vec![]),
        }
    }
}

/// 実通信版 `LiveChatServer` 実装。
///
/// 設計意図:
/// - `youtube-live-lib` のHTTP関数群を trait に接続するだけの薄いアダプタ。
pub struct ReqwestServer {
    cookies: Vec<Cookie>,
}

impl ReqwestServer {
    /// Cookie を注入して生成。
    pub fn new(cookies: Vec<Cookie>) -> Self {
        Self { cookies }
    }
}

#[async_trait]
impl LiveChatServer for ReqwestServer {
    async fn get_live_chat(&self, vid: &Vid) -> Result<LiveChat, DomainError> {
        get_live_chat(vid, &self.cookies)
            .await
            .map_err(|e| DomainError::ServerFailed(e.to_string()))
    }

    async fn get_live_chat_messages(
        &self,
        vid: &Vid,
        ytcfg: &Ytcfg,
        continuation: &Continuation,
    ) -> Result<(Option<Continuation>, Vec<Action>, String), DomainError> {
        get_live_chat_messages(vid, ytcfg, continuation)
            .await
            .map_err(|e| DomainError::ServerFailed(e.to_string()))
    }

    async fn send_chat(
        &self,
        ytcfg: &Ytcfg,
        send_message_params: &str,
        text: &str,
    ) -> Result<(), DomainError> {
        send_chat_message(&self.cookies, ytcfg, send_message_params, text)
            .await
            .map_err(|e| DomainError::ServerFailed(e.to_string()))
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    const LIVE_CHAT_HTML: &str = r#"<html><script>ytcfg.set({"INNERTUBE_CONTEXT":{"client":{"clientName":"WEB","clientVersion":"2.20250101.00.00"}},"INNERTUBE_API_KEY":"test_api_key","VISITOR_DATA":"visitor"});window["ytInitialData"] = {"contents":{"liveChatRenderer":{"continuations":[{"timedContinuationData":{"continuation":"CONT_1","timeoutMs":1000}}],"actions":[],"actionPanel":{"liveChatMessageInputRenderer":{"sendButton":{"buttonRenderer":{"serviceEndpoint":{"sendLiveChatMessageEndpoint":{"params":"SEND_PARAMS"}}}}}}}}};</script></html>"#;

    #[tokio::test]
    async fn start_event_requests_live_chat() {
        let mut machine = YoutubeLiveStateMachine::new();
        let cmds = machine.on_event(DomainEvent::Start).expect("start");
        assert!(matches!(cmds.first(), Some(DomainCommand::FetchLiveChat)));
    }

    #[tokio::test]
    async fn second_live_chat_in_polling_is_invalid() {
        let mut machine = YoutubeLiveStateMachine::new();
        machine
            .on_event(DomainEvent::LiveChat(LiveChat::new(
                LIVE_CHAT_HTML.to_string(),
            )))
            .expect("first live chat");
        let err = match machine.on_event(DomainEvent::LiveChat(LiveChat::new(
            LIVE_CHAT_HTML.to_string(),
        ))) {
            Ok(_) => panic!("second live chat must fail"),
            Err(e) => e,
        };
        assert_eq!(
            err,
            DomainError::InvalidTransition {
                state: "Polling",
                event: "LiveChat"
            }
        );
    }

    #[tokio::test]
    async fn send_chat_event_generates_send_command_when_params_exist() {
        let mut machine = YoutubeLiveStateMachine::new();
        machine
            .on_event(DomainEvent::LiveChat(LiveChat::new(
                LIVE_CHAT_HTML.to_string(),
            )))
            .expect("live chat");
        let cmds = machine
            .on_event(DomainEvent::SendChat {
                text: "hello".to_string(),
            })
            .expect("send chat");
        assert!(matches!(cmds.first(), Some(DomainCommand::SendChat { .. })));
    }

    #[tokio::test]
    async fn config_controls_fetch_messages_delay() {
        let config = StateMachineConfig {
            default_poll_delay_ms: 100,
            min_poll_delay_ms: 50,
            max_poll_delay_ms: 200,
            reload_retry_delay_ms: 1000,
            max_consecutive_errors: 5,
        };
        let mut machine = YoutubeLiveStateMachine::with_config(config);
        machine
            .on_event(DomainEvent::LiveChat(LiveChat::new(
                LIVE_CHAT_HTML.to_string(),
            )))
            .expect("live chat");

        let mut c = Continuation::new("CONT_NEXT".to_string());
        c.timeout_ms = Some(1000);
        let cmds = machine
            .on_event(DomainEvent::Messages {
                continuation: Some(c),
                actions: vec![],
                raw: "{}".to_string(),
            })
            .expect("messages");
        assert!(
            cmds.iter()
                .any(|cmd| matches!(cmd, DomainCommand::FetchMessages { delay_ms: 200, .. }))
        );
    }
}
