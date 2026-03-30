use std::sync::Arc;

use tokio::io::AsyncWriteExt;
use tokio::sync::watch;
use tokio::time::{timeout, Duration};

use crate::{
    client::{IrcLineReader, IrcWriter},
    error::IrcError,
    message::{IrcCommand, IrcMessage},
    profile::{ProfileEvent, ServerProfile},
};

/// IRC セッション：認証・JOIN・PING/PONG・受信ループを管理する
pub struct IrcSession {
    reader: IrcLineReader,
    writer: IrcWriter,
    profile: Arc<dyn ServerProfile>,
}

impl IrcSession {
    pub fn new(reader: IrcLineReader, writer: IrcWriter, profile: Arc<dyn ServerProfile>) -> Self {
        Self {
            reader,
            writer,
            profile,
        }
    }

    /// セッション確立（CAP → 認証コマンド → 001 待機）
    pub async fn authenticate(&mut self, nick: &str, pass: Option<&str>) -> Result<(), IrcError> {
        let caps = self.profile.capabilities();
        if !caps.is_empty() {
            self.send_raw(&format!("CAP REQ :{}", caps.join(" ")))
                .await?;
        }
        for cmd in self.profile.auth_commands(nick, pass) {
            self.send_raw(&cmd).await?;
        }
        timeout(Duration::from_secs(30), self.wait_for_welcome())
            .await
            .map_err(|_| IrcError::AuthTimeout)?
    }

    async fn wait_for_welcome(&mut self) -> Result<(), IrcError> {
        loop {
            let line = self
                .reader
                .next_line()
                .await?
                .ok_or(IrcError::ConnectionClosed)?;
            tracing::trace!(line = %line, "IRC recv (auth)");
            let msg = IrcMessage::parse(&line)?;
            match &msg.command {
                IrcCommand::Numeric(1) => return Ok(()), // RPL_WELCOME
                IrcCommand::Cap => {}                    // CAP ACK/NAK は無視
                IrcCommand::Ping => {
                    let server = msg.trailing().unwrap_or("").to_string();
                    self.send_raw(&format!("PONG :{server}")).await?;
                }
                IrcCommand::Numeric(n) if *n >= 400 => {
                    let text = msg.trailing().unwrap_or("unknown error").to_string();
                    return Err(IrcError::ServerError {
                        code: *n,
                        message: text,
                    });
                }
                _ => {}
            }
        }
    }

    /// チャンネル参加（JOIN + post_join_commands + 366 待機）
    pub async fn join_channel(&mut self, channel: &str) -> Result<(), IrcError> {
        self.send_raw(&format!("JOIN {channel}")).await?;
        for cmd in self.profile.post_join_commands(channel) {
            self.send_raw(&cmd).await?;
        }
        timeout(Duration::from_secs(15), self.wait_for_join(channel))
            .await
            .map_err(|_| IrcError::JoinTimeout)?
    }

    async fn wait_for_join(&mut self, channel: &str) -> Result<(), IrcError> {
        loop {
            let line = self
                .reader
                .next_line()
                .await?
                .ok_or(IrcError::ConnectionClosed)?;
            tracing::trace!(line = %line, "IRC recv (join)");
            let msg = IrcMessage::parse(&line)?;
            match &msg.command {
                IrcCommand::Join => {
                    // :nick!user@host JOIN #channel または trailing に channel が入る場合
                    let joined_channel = msg
                        .params
                        .first()
                        .map(String::as_str)
                        .or_else(|| msg.trailing());
                    if joined_channel.is_some_and(|c| c.eq_ignore_ascii_case(channel)) {
                        return Ok(());
                    }
                }
                IrcCommand::Numeric(366) => return Ok(()), // RPL_ENDOFNAMES
                IrcCommand::Ping => {
                    let server = msg.trailing().unwrap_or("").to_string();
                    self.send_raw(&format!("PONG :{server}")).await?;
                }
                IrcCommand::Numeric(n) if *n >= 400 => {
                    let text = msg.trailing().unwrap_or("unknown error").to_string();
                    return Err(IrcError::ServerError {
                        code: *n,
                        message: text,
                    });
                }
                _ => {}
            }
        }
    }

    /// 受信ループを実行し ProfileEvent をコールバックに渡す。
    ///
    /// `cancel_rx` が `true` になるか、サーバーが切断したら終了する。
    pub async fn run_loop<F>(mut self, mut cancel_rx: watch::Receiver<bool>, mut on_event: F)
    where
        F: FnMut(ProfileEvent) + Send,
    {
        loop {
            tokio::select! {
                _ = cancel_rx.changed() => {
                    if *cancel_rx.borrow() {
                        tracing::debug!("IRC loop cancelled");
                        break;
                    }
                }
                result = self.reader.next_line() => {
                    match result {
                        Ok(Some(line)) => {
                            tracing::trace!(line = %line, "IRC recv");
                            match IrcMessage::parse(&line) {
                                Ok(msg) => self.handle_message(msg, &mut on_event).await,
                                Err(e) => {
                                    tracing::warn!(error = %e, raw = %line, "IRC parse error");
                                }
                            }
                        }
                        Ok(None) => {
                            tracing::info!("IRC connection closed by server");
                            on_event(ProfileEvent::SystemMessage(
                                "接続がサーバーにより切断されました".to_string(),
                            ));
                            break;
                        }
                        Err(e) => {
                            tracing::error!(error = %e, "IRC read error");
                            break;
                        }
                    }
                }
            }
        }
    }

    async fn handle_message<F>(&mut self, msg: IrcMessage, on_event: &mut F)
    where
        F: FnMut(ProfileEvent),
    {
        match &msg.command {
            IrcCommand::Ping => {
                let server = msg.trailing().unwrap_or("").to_string();
                if let Err(e) = self.send_raw(&format!("PONG :{server}")).await {
                    tracing::error!(error = %e, "PONG send failed");
                }
            }
            IrcCommand::Privmsg => {
                if let Some(chat) = self.profile.parse_chat(&msg) {
                    on_event(ProfileEvent::Chat(chat));
                }
            }
            _ => {
                for event in self.profile.handle_other(&msg) {
                    on_event(event);
                }
            }
        }
    }

    /// 生ラインを送信（CRLF 付与）
    pub async fn send_raw(&mut self, line: &str) -> Result<(), IrcError> {
        tracing::trace!(line = %line, "IRC send");
        let bytes = format!("{line}\r\n");
        self.writer.write_all(bytes.as_bytes()).await?;
        self.writer.flush().await?;
        Ok(())
    }
}
