//! YouTube Live

use std::{
    collections::{HashMap, VecDeque},
    sync::Arc,
    time::Duration,
};

use mcv_common::SiteId;
use mcv_messages::{
    AccountInfo, AddSiteAckPayload, AddSitePayload, ChannelId, CommentReceivedPayload,
    ConnectPayload, ConnectedPayload, ConnectionRemovedPayload, DisconnectPayload,
    DisconnectedPayload, FetchAccountInfoPayload, GetBrowserPluginAckPayload,
    GetBrowserPluginPayload, GetCookieAckPayload, GetCookiePayload, McvEnvelope,
    Message as McvMessage, MessageDestination, MessagePart as McvMessagePart, MessageSource,
    MessageType, MonetaryInfo, Money, PluginHelloAckPayload, PluginHelloPayload, PluginId,
    ProviderBadge, ProviderContent, ProviderMessage, ProviderMessageKind, ProviderSender,
    SendCommentPayload, ServiceId, SetConnectionSitePayload, StreamMetadataPayload, SystemKind,
    UpdateConnectionAccountPayload,
};
use once_cell::sync::Lazy;
use plugin_abi_helper::v3::prelude::*;
use regex::Regex;
use serde::Deserialize;
use tokio::{
    sync::{mpsc, watch},
    task::JoinHandle,
    time::sleep,
};
use uuid::Uuid;
use youtube_live_lib::{Action, LiveChatPaidMessage, LiveChatTextMessage, MessagePart, Vid};

use youtube_live_lib::domain_state_machine::{
    DomainCommand, DomainEvent, LiveChatServer, ReqwestServer, YoutubeLiveStateMachine,
};

#[derive(Default)]
struct YouTubeLiveStateMachinePlugin {
    logical_plugin_id: PluginId,
    is_initialized: bool,
    connections: HashMap<Uuid, Connection>,
}

struct Connection {
    id: Uuid,
    cancel_tx: Option<watch::Sender<bool>>,
    task: Option<JoinHandle<()>>,
    running: bool,
    browser_id: Option<mcv_messages::BrowserId>,
    event_tx: Option<mpsc::UnboundedSender<DomainEvent>>,
    metadata_cancel_tx: Option<watch::Sender<bool>>,
}

impl Connection {
    fn new(id: Uuid) -> Self {
        Self {
            id,
            cancel_tx: None,
            task: None,
            running: false,
            browser_id: None,
            event_tx: None,
            metadata_cancel_tx: None,
        }
    }

    fn post_comment(&self, text: &str) {
        if let Some(tx) = &self.event_tx {
            let _ = tx.send(DomainEvent::SendChat {
                text: text.to_string(),
            });
        }
    }

    fn connect(
        &mut self,
        ctx: PluginContext,
        logical_plugin_id: PluginId,
        video_id: String,
        cookies: Vec<youtube_live_lib::Cookie>,
        browser_id: mcv_messages::BrowserId,
    ) {
        if self.running {
            return;
        }
        let (cancel_tx, mut cancel_rx) = watch::channel(false);
        let (event_tx, mut event_rx) = mpsc::unbounded_channel::<DomainEvent>();
        let connection_id = self.id;
        let mut machine = YoutubeLiveStateMachine::new();
        let vid = Vid::new(video_id.clone());
        let server = ReqwestServer::new(cookies.clone());

        // メタデータポーリングタスク用
        let (metadata_cancel_tx, metadata_cancel_rx) = watch::channel(false);
        let (ytcfg_tx, ytcfg_rx) = watch::channel::<Option<youtube_live_lib::Ytcfg>>(None);
        tokio::spawn(metadata_polling_loop(
            ctx.clone(),
            logical_plugin_id.clone(),
            connection_id,
            video_id,
            cookies,
            ytcfg_rx,
            metadata_cancel_rx,
        ));

        let task = tokio::spawn(async move {
            let mut queue = VecDeque::from([DomainEvent::Start]);
            let mut disconnected_sent = false;
            let mut ytcfg_notified = false;

            while !disconnected_sent {
                if queue.is_empty() {
                    tokio::select! {
                        _ = cancel_rx.changed() => {
                            queue.push_back(DomainEvent::Disconnect);
                        }
                        maybe_event = event_rx.recv() => {
                            match maybe_event {
                                Some(event) => queue.push_back(event),
                                None => queue.push_back(DomainEvent::Disconnect),
                            }
                        }
                    }
                }

                while let Some(event) = queue.pop_front() {
                    let commands = match machine.on_event(event) {
                        Ok(c) => c,
                        Err(e) => {
                            tracing::warn!(
                                target: "mcv::plugin-youtube-live",
                                connection_id = %connection_id,
                                error = %e,
                                "state transition failed"
                            );
                            continue;
                        }
                    };

                    for command in commands {
                        match command {
                            DomainCommand::FetchLiveChat => {
                                match server.get_live_chat(&vid).await {
                                    Ok(live_chat) => {
                                        queue.push_back(DomainEvent::LiveChat(live_chat))
                                    }
                                    Err(e) => {
                                        tracing::warn!(
                                            target: "mcv::plugin-youtube-live",
                                            connection_id = %connection_id,
                                            error = %e,
                                            "fetch live chat failed"
                                        );
                                        sleep(Duration::from_secs(5)).await;
                                        queue.push_back(DomainEvent::Start);
                                    }
                                }
                            }
                            DomainCommand::FetchMessages {
                                ytcfg,
                                continuation,
                                delay_ms,
                            } => {
                                if !ytcfg_notified {
                                    let _ = ytcfg_tx.send(Some(ytcfg.clone()));
                                    ytcfg_notified = true;
                                }
                                sleep(Duration::from_millis(delay_ms)).await;
                                match server
                                    .get_live_chat_messages(&vid, &ytcfg, &continuation)
                                    .await
                                {
                                    Ok((next, actions, raw)) => {
                                        queue.push_back(DomainEvent::Messages {
                                            continuation: next,
                                            actions,
                                            raw,
                                        });
                                    }
                                    Err(e) => {
                                        tracing::warn!(
                                            target: "mcv::plugin-youtube-live",
                                            connection_id = %connection_id,
                                            error = %e,
                                            "fetch messages failed"
                                        );
                                        sleep(Duration::from_secs(5)).await;
                                        queue.push_back(DomainEvent::Start);
                                    }
                                }
                            }
                            DomainCommand::EmitActions {
                                actions,
                                raw,
                                as_history,
                            } => {
                                let messages: Vec<ProviderMessage> = actions
                                    .iter()
                                    .filter_map(|a| {
                                        convert_action_to_provider_message(a, connection_id)
                                    })
                                    .map(|mut msg| {
                                        if as_history
                                            && matches!(msg.kind, ProviderMessageKind::Chat)
                                        {
                                            msg.kind = ProviderMessageKind::HistoryChat;
                                        }
                                        msg
                                    })
                                    .collect();
                                if messages.is_empty() {
                                    continue;
                                }
                                let payload = CommentReceivedPayload {
                                    connection_id,
                                    envelope: McvEnvelope {
                                        event_id: Uuid::new_v4(),
                                        connection_id,
                                        messages,
                                        received_at: chrono::Utc::now().timestamp(),
                                        raw_message: raw,
                                    },
                                };
                                let msg = McvMessage::new_notification(
                                    MessageType::CommentReceived,
                                    MessageSource::Plugin {
                                        plugin_id: logical_plugin_id.clone(),
                                    },
                                    MessageDestination::Core,
                                    serde_json::to_value(payload).unwrap(),
                                );
                                YouTubeLiveStateMachinePlugin::send_message(ctx.clone(), msg).await;
                            }
                            DomainCommand::EmitAccount {
                                display_name,
                                avatar_url,
                            } => {
                                let account_msg = McvMessage::new_notification(
                                    MessageType::UpdateConnectionAccount,
                                    MessageSource::Plugin {
                                        plugin_id: logical_plugin_id.clone(),
                                    },
                                    MessageDestination::Core,
                                    serde_json::to_value(UpdateConnectionAccountPayload {
                                        connection_id,
                                        account: Some(AccountInfo {
                                            user_id: display_name.clone(),
                                            display_name,
                                            avatar_url,
                                        }),
                                    })
                                    .unwrap(),
                                );
                                YouTubeLiveStateMachinePlugin::send_message(
                                    ctx.clone(),
                                    account_msg,
                                )
                                .await;
                            }
                            DomainCommand::SendChat {
                                ytcfg,
                                send_message_params,
                                text,
                            } => {
                                if let Err(e) =
                                    server.send_chat(&ytcfg, &send_message_params, &text).await
                                {
                                    tracing::warn!(
                                        target: "mcv::plugin-youtube-live",
                                        connection_id = %connection_id,
                                        error = %e,
                                        "send chat failed"
                                    );
                                }
                            }
                            DomainCommand::EmitDisconnected => {
                                let disconnected = McvMessage::new_notification(
                                    MessageType::Disconnected,
                                    MessageSource::Plugin {
                                        plugin_id: logical_plugin_id.clone(),
                                    },
                                    MessageDestination::Core,
                                    serde_json::to_value(DisconnectedPayload { connection_id })
                                        .unwrap(),
                                );
                                YouTubeLiveStateMachinePlugin::send_message(
                                    ctx.clone(),
                                    disconnected,
                                )
                                .await;
                                disconnected_sent = true;
                                break;
                            }
                        }
                    }

                    if disconnected_sent {
                        break;
                    }
                }
            }
        });

        self.cancel_tx = Some(cancel_tx);
        self.task = Some(task);
        self.running = true;
        self.browser_id = Some(browser_id);
        self.event_tx = Some(event_tx);
        self.metadata_cancel_tx = Some(metadata_cancel_tx);
    }

    fn stop(&mut self) {
        if let Some(event_tx) = &self.event_tx {
            let _ = event_tx.send(DomainEvent::Disconnect);
        }
        if let Some(tx) = &self.cancel_tx {
            let _ = tx.send(true);
        }
        if let Some(tx) = &self.metadata_cancel_tx {
            let _ = tx.send(true);
        }
        self.event_tx = None;
        self.cancel_tx = None;
        self.metadata_cancel_tx = None;
        self.task = None;
        self.running = false;
    }
}

async fn metadata_polling_loop(
    ctx: PluginContext,
    logical_plugin_id: PluginId,
    connection_id: Uuid,
    video_id: String,
    cookies: Vec<youtube_live_lib::Cookie>,
    mut ytcfg_rx: watch::Receiver<Option<youtube_live_lib::Ytcfg>>,
    mut cancel_rx: watch::Receiver<bool>,
) {
    tracing::info!(
        target: "mcv::plugin-youtube-live",
        connection_id = %connection_id,
        video_id = %video_id,
        cookie_count = cookies.len(),
        "metadata_polling_loop: 起動、ytcfg 待機中"
    );

    // ytcfg が確定するまで待機
    let ytcfg = loop {
        tokio::select! {
            result = ytcfg_rx.changed() => {
                if result.is_err() {
                    tracing::warn!(
                        target: "mcv::plugin-youtube-live",
                        connection_id = %connection_id,
                        "metadata_polling_loop: ytcfg チャンネルがクローズ、終了"
                    );
                    return;
                }
                if let Some(cfg) = ytcfg_rx.borrow().clone() {
                    tracing::info!(
                        target: "mcv::plugin-youtube-live",
                        connection_id = %connection_id,
                        "metadata_polling_loop: ytcfg 受信、ポーリング開始"
                    );
                    break cfg;
                }
            }
            result = cancel_rx.changed() => {
                if result.is_err() || *cancel_rx.borrow() {
                    tracing::info!(
                        target: "mcv::plugin-youtube-live",
                        connection_id = %connection_id,
                        "metadata_polling_loop: ytcfg 待機中にキャンセル、終了"
                    );
                    return;
                }
            }
        }
    };

    let vid = youtube_live_lib::Vid::new(video_id);
    let mut poll_count: u64 = 0;

    loop {
        if *cancel_rx.borrow() {
            tracing::info!(
                target: "mcv::plugin-youtube-live",
                connection_id = %connection_id,
                poll_count,
                "metadata_polling_loop: キャンセル受信、終了"
            );
            break;
        }

        poll_count += 1;
        tracing::debug!(
            target: "mcv::plugin-youtube-live",
            connection_id = %connection_id,
            poll_count,
            "metadata_polling_loop: fetch_updated_metadata 呼び出し"
        );

        let interval_ms =
            match youtube_live_lib::fetch_updated_metadata(&vid, &ytcfg, &cookies).await {
                Ok(meta) => {
                    tracing::info!(
                        target: "mcv::plugin-youtube-live",
                        connection_id = %connection_id,
                        poll_count,
                        title = ?meta.title,
                        viewer_count = ?meta.viewer_count,
                        next_poll_ms = meta.timeout_ms,
                        "metadata_polling_loop: メタデータ取得成功、StreamMetadata 送信"
                    );
                    let payload = StreamMetadataPayload {
                        connection_id,
                        title: meta.title,
                        viewer_count: meta.viewer_count,
                        total_viewer_count: None,
                        start_time: None,
                        others: None,
                    };
                    let msg = McvMessage::new_notification(
                        MessageType::StreamMetadata,
                        MessageSource::Plugin {
                            plugin_id: logical_plugin_id.clone(),
                        },
                        MessageDestination::Core,
                        serde_json::to_value(payload).unwrap(),
                    );
                    YouTubeLiveStateMachinePlugin::send_message(ctx.clone(), msg).await;
                    meta.timeout_ms.clamp(5_000, 120_000)
                }
                Err(e) => {
                    tracing::warn!(
                        target: "mcv::plugin-youtube-live",
                        connection_id = %connection_id,
                        poll_count,
                        error = %e,
                        "metadata_polling_loop: fetch_updated_metadata 失敗、30秒後にリトライ"
                    );
                    30_000
                }
            };

        tracing::debug!(
            target: "mcv::plugin-youtube-live",
            connection_id = %connection_id,
            poll_count,
            interval_ms,
            "metadata_polling_loop: 次回ポーリングまで待機"
        );

        tokio::select! {
            _ = tokio::time::sleep(Duration::from_millis(interval_ms)) => {}
            result = cancel_rx.changed() => {
                if result.is_err() || *cancel_rx.borrow() {
                    tracing::info!(
                        target: "mcv::plugin-youtube-live",
                        connection_id = %connection_id,
                        poll_count,
                        "metadata_polling_loop: 待機中にキャンセル受信、終了"
                    );
                    break;
                }
            }
        }
    }
}

impl YouTubeLiveStateMachinePlugin {
    fn initialize(&mut self, logical_plugin_id: PluginId) {
        if self.is_initialized {
            return;
        }
        self.is_initialized = true;
        self.logical_plugin_id = logical_plugin_id;
    }

    async fn send_message(ctx: PluginContext, message: McvMessage) {
        if let Err(e) = ctx.send_notification(message).await {
            tracing::error!(
                target: "mcv::plugin-youtube-live",
                error = %e,
                "send_notification failed"
            );
        }
    }

    async fn send_plugin_hello(
        &self,
        ctx: PluginContext,
        payload: PluginHelloPayload,
        plugin_id: PluginId,
    ) -> Result<PluginHelloAckPayload, RequestError> {
        let message = McvMessage::new_request(
            MessageType::PluginHello,
            MessageSource::Plugin { plugin_id },
            MessageDestination::Core,
            serde_json::to_value(&payload).unwrap(),
        );
        let response = ctx.send_request(message, Duration::from_secs(10)).await?;
        if response.message_type != MessageType::PluginHelloAck {
            return Err(RequestError::Internal(format!(
                "unexpected response for PluginHello: {:?}",
                response.message_type
            )));
        }
        serde_json::from_value(response.payload)
            .map_err(|e| RequestError::Internal(format!("invalid PluginHelloAck payload: {}", e)))
    }

    async fn send_add_site(
        &self,
        ctx: PluginContext,
        payload: AddSitePayload,
        plugin_id: PluginId,
    ) -> Result<AddSiteAckPayload, RequestError> {
        let message = McvMessage::new_request(
            MessageType::AddSite,
            MessageSource::Plugin { plugin_id },
            MessageDestination::Core,
            serde_json::to_value(&payload).unwrap(),
        );
        let response = ctx.send_request(message, Duration::from_secs(10)).await?;
        if response.message_type != MessageType::AddSiteAck {
            return Err(RequestError::Internal(format!(
                "unexpected response for AddSite: {:?}",
                response.message_type
            )));
        }
        serde_json::from_value(response.payload)
            .map_err(|e| RequestError::Internal(format!("invalid AddSiteAck payload: {}", e)))
    }

    async fn clear_connection_account(&self, ctx: PluginContext, connection_id: Uuid) {
        let clear_account = McvMessage::new_notification(
            MessageType::UpdateConnectionAccount,
            MessageSource::Plugin {
                plugin_id: self.logical_plugin_id.clone(),
            },
            MessageDestination::Core,
            serde_json::to_value(UpdateConnectionAccountPayload {
                connection_id,
                account: None,
            })
            .unwrap(),
        );
        Self::send_message(ctx, clear_account).await;
    }
}

#[async_trait::async_trait]
impl PluginImplV3Async for YouTubeLiveStateMachinePlugin {
    async fn on_loaded(&mut self, ctx: PluginContext) {
        let uuid = Uuid::new_v4();
        let logical_plugin_id = PluginId::new(format!("YouTubeLive_logical_{}", uuid));
        let adapter = Arc::new(PluginContextAdapter::new(ctx.clone()));
        #[cfg(feature = "alpha")]
        let log_level = "trace";
        #[cfg(all(feature = "beta", not(feature = "alpha")))]
        let log_level = "info";
        #[cfg(all(not(feature = "alpha"), not(feature = "beta"), feature = "stable"))]
        let log_level = "error";
        #[cfg(all(not(feature = "alpha"), not(feature = "beta"), not(feature = "stable")))]
        let log_level = "trace";
        let _ =
            mcv_plugin_telemetry::init_tracing(uuid, adapter, env!("CARGO_PKG_VERSION"), log_level);
        self.initialize(logical_plugin_id);

        let hello_payload = PluginHelloPayload {
            name: "YouTubeLive".to_string(),
            plugin_id: self.logical_plugin_id.clone(),
            role: vec!["youtubelive".to_string()],
            api_version: "v3".to_string(),
            send_comment_schema: None,
        };
        if let Err(e) = self
            .send_plugin_hello(ctx.clone(), hello_payload, self.logical_plugin_id.clone())
            .await
        {
            tracing::error!(
                target: "mcv::plugin-youtube-live",
                error = %e,
                "PluginHello request failed"
            );
            return;
        }

        let add_site = AddSitePayload {
            site_id: SiteId::new("YouTubeLive", "9f4f0528-9cf4-43d1-b6d4-9e6e19464481"),
            display_name: "YouTube Live".to_owned(),
            options_schema: serde_json::from_str("{}").unwrap(),
        };
        if let Err(e) = self
            .send_add_site(ctx, add_site, self.logical_plugin_id.clone())
            .await
        {
            tracing::error!(
                target: "mcv::plugin-youtube-live",
                error = %e,
                "AddSite request failed"
            );
        }
    }

    async fn on_message(&mut self, ctx: PluginContext, msg: &[u8]) {
        let message: McvMessage = match serde_json::from_slice(msg) {
            Ok(m) => m,
            Err(e) => {
                tracing::warn!(
                    target: "mcv::plugin-youtube-live",
                    error = %e,
                    "invalid incoming message"
                );
                return;
            }
        };

        match message.message_type {
            MessageType::SetConnectionSite => {
                if let Ok(payload) =
                    serde_json::from_value::<SetConnectionSitePayload>(message.payload)
                {
                    if let Some(conn) = self.connections.get_mut(&payload.connection_id) {
                        conn.stop();
                        conn.browser_id = None;
                    } else {
                        self.connections
                            .entry(payload.connection_id)
                            .or_insert_with(|| Connection::new(payload.connection_id));
                    }
                    self.clear_connection_account(ctx.clone(), payload.connection_id)
                        .await;
                }
            }
            MessageType::Connect => {
                let payload = match serde_json::from_value::<ConnectPayload>(message.payload) {
                    Ok(v) => v,
                    Err(e) => {
                        tracing::warn!(
                            target: "mcv::plugin-youtube-live",
                            error = %e,
                            "invalid Connect payload"
                        );
                        return;
                    }
                };
                let extra = match serde_json::from_value::<Input>(payload.input.extra.clone()) {
                    Ok(v) => v,
                    Err(e) => {
                        tracing::warn!(
                            target: "mcv::plugin-youtube-live",
                            error = %e,
                            "invalid Connect input.extra"
                        );
                        return;
                    }
                };
                let Some(vid) = extract_video_id(&extra.url) else {
                    tracing::warn!(
                        target: "mcv::plugin-youtube-live",
                        url = extra.url,
                        "failed to extract youtube video id"
                    );
                    return;
                };
                let browser_id = payload.browser.id.clone();
                let yt_cookies =
                    fetch_cookies_for_connect(&ctx, self.logical_plugin_id.clone(), &browser_id)
                        .await
                        .iter()
                        .map(|c| youtube_live_lib::Cookie {
                            name: c.name.clone(),
                            value: c.value.clone(),
                        })
                        .collect::<Vec<_>>();

                let browser_changed = {
                    let conn = self
                        .connections
                        .entry(payload.connection_id)
                        .or_insert_with(|| Connection::new(payload.connection_id));
                    let changed = conn
                        .browser_id
                        .as_ref()
                        .map(|b| b != &browser_id)
                        .unwrap_or(false);
                    if changed {
                        conn.stop();
                    }
                    changed
                };
                if browser_changed {
                    self.clear_connection_account(ctx.clone(), payload.connection_id)
                        .await;
                }

                let conn = self
                    .connections
                    .entry(payload.connection_id)
                    .or_insert_with(|| Connection::new(payload.connection_id));
                conn.connect(
                    ctx.clone(),
                    self.logical_plugin_id.clone(),
                    vid,
                    yt_cookies,
                    browser_id,
                );

                let connected = McvMessage::new_notification(
                    MessageType::Connected,
                    MessageSource::Plugin {
                        plugin_id: self.logical_plugin_id.clone(),
                    },
                    MessageDestination::Core,
                    serde_json::to_value(ConnectedPayload {
                        connection_id: payload.connection_id,
                    })
                    .unwrap(),
                );
                Self::send_message(ctx, connected).await;
            }
            MessageType::SendComment => {
                if let Ok(payload) = serde_json::from_value::<SendCommentPayload>(message.payload) {
                    if let Some(conn) = self.connections.get(&payload.connection_id) {
                        conn.post_comment(&payload.text);
                    }
                }
            }
            MessageType::FetchAccountInfo => {
                if let Ok(payload) =
                    serde_json::from_value::<FetchAccountInfoPayload>(message.payload)
                {
                    let cookies = fetch_cookies_for_connect(
                        &ctx,
                        self.logical_plugin_id.clone(),
                        &payload.browser.id,
                    )
                    .await;
                    let yt_cookies = cookies
                        .iter()
                        .map(|c| youtube_live_lib::Cookie {
                            name: c.name.clone(),
                            value: c.value.clone(),
                        })
                        .collect::<Vec<_>>();

                    let account =
                        match youtube_live_lib::fetch_account_info_from_home(&yt_cookies).await {
                            Ok(Some(info)) => Some(AccountInfo {
                                user_id: info.user_id,
                                display_name: info.display_name,
                                avatar_url: info.avatar_url,
                            }),
                            _ => None,
                        };
                    let account_msg = McvMessage::new_notification(
                        MessageType::UpdateConnectionAccount,
                        MessageSource::Plugin {
                            plugin_id: self.logical_plugin_id.clone(),
                        },
                        MessageDestination::Core,
                        serde_json::to_value(UpdateConnectionAccountPayload {
                            connection_id: payload.connection_id,
                            account,
                        })
                        .unwrap(),
                    );
                    Self::send_message(ctx, account_msg).await;
                }
            }
            MessageType::Disconnect => {
                if let Ok(payload) = serde_json::from_value::<DisconnectPayload>(message.payload) {
                    if let Some(conn) = self.connections.get_mut(&payload.connection_id) {
                        conn.stop();
                    }
                }
            }
            MessageType::ConnectionRemoved => {
                if let Ok(payload) =
                    serde_json::from_value::<ConnectionRemovedPayload>(message.payload)
                {
                    if let Some(mut conn) = self.connections.remove(&payload.connection_id) {
                        conn.stop();
                    }
                    self.clear_connection_account(ctx.clone(), payload.connection_id)
                        .await;
                }
            }
            _ => {}
        }
    }

    async fn on_shutdown(&mut self, _ctx: PluginContext) {
        for conn in self.connections.values_mut() {
            conn.stop();
        }
        self.connections.clear();
    }
}

fn convert_action_to_provider_message(
    action: &Action,
    connection_id: Uuid,
) -> Option<ProviderMessage> {
    match action {
        Action::TextMessage(msg) => Some(convert_to_provider_message(msg)),

        Action::GiftAnnouncement(msg) => {
            let mut provider_msg = convert_to_provider_message(msg);
            provider_msg.kind = ProviderMessageKind::System(SystemKind::GiftAnnouncement);
            Some(provider_msg)
        }

        Action::PaidMessage(msg) => Some(convert_paid_message_to_provider_message(msg)),

        Action::PlaceholderItem(placeholder) => {
            let timestamp = placeholder.timestamp_usec.parse::<i64>().unwrap_or(0) / 1_000_000;
            Some(ProviderMessage {
                id: placeholder.id.clone(),
                platform_message_id: Some(placeholder.id.clone()),
                service: ServiceId("youtube".to_string()),
                channel: ChannelId("".to_string()),
                sender: ProviderSender {
                    id: String::new(),
                    display_name: vec![],
                    badges: vec![],
                    role: None,
                    avatar_url: None,
                },
                timestamp,
                kind: ProviderMessageKind::System(SystemKind::Placeholder),
                content: ProviderContent::Empty,
                reply_to: None,
                metadata: serde_json::Value::Null,
            })
        }

        Action::ReplaceChatItem(replace_action) => {
            let mut msg = convert_to_provider_message(&replace_action.message);
            msg.kind = ProviderMessageKind::System(SystemKind::MessageUpdate {
                target_message_id: replace_action.target_item_id.clone(),
            });
            Some(msg)
        }

        Action::RemoveChatItem(remove_action) => Some(ProviderMessage {
            id: Uuid::new_v4().to_string(),
            platform_message_id: None,
            service: ServiceId("youtube".to_string()),
            channel: ChannelId("".to_string()),
            sender: ProviderSender {
                id: String::new(),
                display_name: vec![],
                badges: vec![],
                role: None,
                avatar_url: None,
            },
            timestamp: 0,
            kind: ProviderMessageKind::System(SystemKind::MessageDelete {
                target_message_id: remove_action.target_item_id.clone(),
            }),
            content: ProviderContent::Empty,
            reply_to: None,
            metadata: serde_json::Value::Null,
        }),

        Action::RemoveChatItemByAuthor(remove_action) => Some(ProviderMessage {
            id: Uuid::new_v4().to_string(),
            platform_message_id: None,
            service: ServiceId("youtube".to_string()),
            channel: ChannelId("".to_string()),
            sender: ProviderSender {
                id: String::new(),
                display_name: vec![],
                badges: vec![],
                role: None,
                avatar_url: None,
            },
            timestamp: 0,
            kind: ProviderMessageKind::System(SystemKind::MessageDeleteAll {
                user_id: remove_action.external_channel_id.clone(),
            }),
            content: ProviderContent::Empty,
            reply_to: None,
            metadata: serde_json::Value::Null,
        }),

        Action::ParseError(raw) => {
            tracing::error!(
                target: "mcv::plugin-youtube-live",
                connection_id = %connection_id,
                raw = raw,
                "failed to parse action"
            );
            None
        }
        _ => None,
    }
}

fn convert_to_provider_message(msg: &LiveChatTextMessage) -> ProviderMessage {
    let text = msg
        .message_parts
        .iter()
        .filter_map(|part| match part {
            MessagePart::Text(s) => Some(McvMessagePart::Text { text: s.clone() }),
            MessagePart::Emoji(emoji) => {
                emoji
                    .thumbnails
                    .first()
                    .map(|thumbnail| McvMessagePart::Image {
                        url: thumbnail.url.clone(),
                        width: Some(thumbnail.width as u32),
                        height: Some(thumbnail.height as u32),
                        alt: Some(emoji.label.clone()),
                    })
            }
        })
        .collect::<Vec<_>>();

    let badges = msg
        .author_badges
        .iter()
        .map(|badge| ProviderBadge {
            id: badge.tooltip.clone(),
            name: badge.tooltip.clone(),
            image_url: badge.thumbnails.first().map(|t| t.url.clone()),
        })
        .collect::<Vec<_>>();

    let timestamp = msg.timestamp_usec.parse::<i64>().unwrap_or(0) / 1_000_000;
    let id = msg.timestamp_usec.clone();

    ProviderMessage {
        id: id.clone(),
        platform_message_id: Some(id),
        service: ServiceId("youtube".to_string()),
        channel: ChannelId("".to_string()),
        sender: ProviderSender {
            id: msg.author_external_channel_id.clone(),
            display_name: vec![McvMessagePart::Text {
                text: msg.author_name.clone(),
            }],
            badges,
            role: None,
            avatar_url: msg.author_photo_url.clone(),
        },
        timestamp,
        kind: ProviderMessageKind::Chat,
        content: ProviderContent::Text { text },
        reply_to: None,
        metadata: serde_json::Value::Null,
    }
}

fn convert_paid_message_to_provider_message(msg: &LiveChatPaidMessage) -> ProviderMessage {
    let text = msg
        .message_parts
        .iter()
        .filter_map(|part| match part {
            MessagePart::Text(s) => Some(McvMessagePart::Text { text: s.clone() }),
            MessagePart::Emoji(emoji) => {
                emoji
                    .thumbnails
                    .first()
                    .map(|thumbnail| McvMessagePart::Image {
                        url: thumbnail.url.clone(),
                        width: Some(thumbnail.width as u32),
                        height: Some(thumbnail.height as u32),
                        alt: Some(emoji.label.clone()),
                    })
            }
        })
        .collect::<Vec<_>>();

    let badges = msg
        .author_badges
        .iter()
        .map(|badge| ProviderBadge {
            id: badge.tooltip.clone(),
            name: badge.tooltip.clone(),
            image_url: badge.thumbnails.first().map(|t| t.url.clone()),
        })
        .collect::<Vec<_>>();

    let timestamp = msg.timestamp_usec.parse::<i64>().unwrap_or(0) / 1_000_000;
    let id = msg.timestamp_usec.clone();
    let monetary_info = MonetaryInfo {
        amount: parse_money(&msg.purchase_amount_text),
        tier: None,
        recurring: false,
    };

    ProviderMessage {
        id: id.clone(),
        platform_message_id: Some(id),
        service: ServiceId("youtube".to_string()),
        channel: ChannelId("".to_string()),
        sender: ProviderSender {
            id: msg.author_external_channel_id.clone(),
            display_name: vec![McvMessagePart::Text {
                text: msg.author_name.clone(),
            }],
            badges,
            role: None,
            avatar_url: msg.author_photo_url.clone(),
        },
        timestamp,
        kind: ProviderMessageKind::Monetary(monetary_info),
        content: ProviderContent::Text { text },
        reply_to: None,
        metadata: serde_json::Value::Null,
    }
}

fn parse_money(text: &str) -> Money {
    let text = text.trim();
    let (currency, rest): (&str, &str) = if text.starts_with('¥') || text.starts_with('￥') {
        ("JPY", text.trim_start_matches(['¥', '￥']))
    } else if text.starts_with("HK$") {
        ("HKD", &text[3..])
    } else if text.starts_with("NT$") {
        ("TWD", &text[3..])
    } else if text.starts_with("A$") {
        ("AUD", &text[2..])
    } else if text.starts_with("C$") {
        ("CAD", &text[2..])
    } else if text.starts_with('$') {
        ("USD", text.trim_start_matches('$'))
    } else if text.starts_with('€') {
        ("EUR", text.trim_start_matches('€'))
    } else if text.starts_with('£') {
        ("GBP", text.trim_start_matches('£'))
    } else if text.ends_with('₩') {
        let numeric: String = text
            .trim_end_matches('₩')
            .chars()
            .filter(|c| c.is_ascii_digit())
            .collect();
        let value_minor = numeric.parse::<i64>().unwrap_or(0);
        return Money {
            currency: "KRW".to_string(),
            value_minor,
        };
    } else {
        ("", text)
    };
    let cleaned: String = rest
        .chars()
        .filter(|c| c.is_ascii_digit() || *c == '.')
        .collect();
    let value_minor = match currency {
        "JPY" | "TWD" => cleaned.parse::<i64>().unwrap_or(0),
        _ => cleaned
            .parse::<f64>()
            .map(|v| (v * 100.0).round() as i64)
            .unwrap_or(0),
    };
    Money {
        currency: currency.to_string(),
        value_minor,
    }
}

fn extract_video_id(input: &str) -> Option<String> {
    static ID_ONLY: Lazy<Regex> = Lazy::new(|| Regex::new(r"^[A-Za-z0-9_-]{11}$").unwrap());
    if ID_ONLY.is_match(input) {
        return Some(input.to_string());
    }
    static URL_RE: Lazy<Regex> =
        Lazy::new(|| Regex::new(r"(?:v=|youtu\.be/)([A-Za-z0-9_-]{11})").unwrap());
    URL_RE
        .captures(input)
        .and_then(|caps| caps.get(1))
        .map(|m| m.as_str().to_string())
}

#[derive(Deserialize)]
struct Input {
    url: String,
}

export_plugin_v3_async!(YouTubeLiveStateMachinePlugin);

async fn fetch_cookies_for_connect(
    ctx: &PluginContext,
    logical_plugin_id: PluginId,
    browser_id: &mcv_messages::BrowserId,
) -> Vec<mcv_messages::Cookie> {
    let get_browser_plugin_message = McvMessage::new_request(
        MessageType::GetBrowserPlugin,
        MessageSource::Plugin {
            plugin_id: logical_plugin_id.clone(),
        },
        MessageDestination::Core,
        serde_json::to_value(GetBrowserPluginPayload {
            browser_id: browser_id.clone(),
        })
        .unwrap(),
    );
    let browser_plugin_id = match ctx
        .send_request(get_browser_plugin_message, Duration::from_secs(10))
        .await
    {
        Ok(response) if response.message_type == MessageType::GetBrowserPluginAck => {
            match serde_json::from_value::<GetBrowserPluginAckPayload>(response.payload) {
                Ok(payload) => Some(payload.plugin_id),
                Err(_) => None,
            }
        }
        _ => None,
    };

    let Some(browser_plugin_id) = browser_plugin_id else {
        return vec![];
    };

    let get_cookie_message = McvMessage::new_request(
        MessageType::GetCookie,
        MessageSource::Plugin {
            plugin_id: logical_plugin_id,
        },
        MessageDestination::Plugin {
            plugin_id: browser_plugin_id,
        },
        serde_json::to_value(GetCookiePayload {
            browser_id: browser_id.clone(),
            domain: "www.youtube.com".to_string(),
        })
        .unwrap(),
    );
    match ctx
        .send_request(get_cookie_message, Duration::from_secs(10))
        .await
    {
        Ok(response) if response.message_type == MessageType::GetCookieAck => {
            match serde_json::from_value::<GetCookieAckPayload>(response.payload) {
                Ok(payload) => payload.cookies,
                Err(_) => vec![],
            }
        }
        _ => vec![],
    }
}

#[cfg(test)]
mod tests {
    use super::*;
    use std::collections::VecDeque;
    use tokio::sync::Mutex;
    use youtube_live_lib::domain_state_machine::{
        DomainCommand, DomainError, DomainEvent, LiveChatServer,
    };
    use youtube_live_lib::{Continuation, LiveChat, Ytcfg};

    const LIVE_CHAT_HTML: &str = r#"<html><script>ytcfg.set({"INNERTUBE_CONTEXT":{"client":{"clientName":"WEB","clientVersion":"2.20250101.00.00"}},"INNERTUBE_API_KEY":"test_api_key","VISITOR_DATA":"visitor"});window["ytInitialData"] = {"contents":{"liveChatRenderer":{"continuations":[{"timedContinuationData":{"continuation":"CONT_1","timeoutMs":1000}}],"actions":[]}}};</script></html>"#;

    struct FakeServer {
        live_chat: LiveChat,
        poll_responses: Arc<Mutex<VecDeque<(Option<Continuation>, Vec<Action>, String)>>>,
    }

    impl FakeServer {
        fn new(poll_responses: Vec<(Option<Continuation>, Vec<Action>, String)>) -> Self {
            Self {
                live_chat: LiveChat::new(LIVE_CHAT_HTML.to_string()),
                poll_responses: Arc::new(Mutex::new(VecDeque::from(poll_responses))),
            }
        }
    }

    #[async_trait::async_trait]
    impl LiveChatServer for FakeServer {
        async fn get_live_chat(&self, _vid: &Vid) -> Result<LiveChat, DomainError> {
            Ok(LiveChat::new(self.live_chat.value().to_string()))
        }

        async fn get_live_chat_messages(
            &self,
            _vid: &Vid,
            _ytcfg: &Ytcfg,
            _continuation: &Continuation,
        ) -> Result<(Option<Continuation>, Vec<Action>, String), DomainError> {
            let mut guard = self.poll_responses.lock().await;
            guard
                .pop_front()
                .ok_or_else(|| DomainError::ServerFailed("no fake response".to_string()))
        }

        async fn send_chat(
            &self,
            _ytcfg: &Ytcfg,
            _send_message_params: &str,
            _text: &str,
        ) -> Result<(), DomainError> {
            Ok(())
        }
    }

    #[tokio::test]
    async fn initial_live_chat_moves_to_polling() {
        let mut machine = YoutubeLiveStateMachine::new();
        let cmds = machine
            .on_event(DomainEvent::Start)
            .expect("start must succeed");
        assert!(matches!(cmds.first(), Some(DomainCommand::FetchLiveChat)));

        let cmds = machine
            .on_event(DomainEvent::LiveChat(LiveChat::new(
                LIVE_CHAT_HTML.to_string(),
            )))
            .expect("live chat must succeed");
        assert!(cmds
            .iter()
            .any(|c| matches!(c, DomainCommand::FetchMessages { .. })));
    }

    #[tokio::test]
    async fn live_chat_in_polling_state_returns_error() {
        let mut machine = YoutubeLiveStateMachine::new();
        let _ = machine
            .on_event(DomainEvent::LiveChat(LiveChat::new(
                LIVE_CHAT_HTML.to_string(),
            )))
            .expect("initial live_chat must be accepted");
        let err = match machine.on_event(DomainEvent::LiveChat(LiveChat::new(
            LIVE_CHAT_HTML.to_string(),
        ))) {
            Ok(_) => panic!("second live_chat in polling state must fail"),
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
}
