use mcv_messages::{McvEnvelope, Money, ProviderContent, ProviderMessageKind, SystemKind};

use crate::types::CommentRow;

/// "delete-all-by-user" イベントのペイロード
#[derive(Debug, Clone, serde::Serialize)]
pub(crate) struct DeleteAllByUserPayload {
    pub(crate) user_id: String,
    pub(crate) connection_id: String,
}

/// Money を表示用テキストに変換する（例: "¥8000"、"$10.00"）
pub(crate) fn format_money(money: &Money) -> String {
    match money.currency.as_str() {
        "JPY" => format!("¥{}", money.value_minor),
        "KRW" => format!("₩{}", money.value_minor),
        "TWD" => format!("NT${}", money.value_minor),
        "USD" => format!("${:.2}", money.value_minor as f64 / 100.0),
        "EUR" => format!("€{:.2}", money.value_minor as f64 / 100.0),
        "GBP" => format!("£{:.2}", money.value_minor as f64 / 100.0),
        "AUD" => format!("A${:.2}", money.value_minor as f64 / 100.0),
        "CAD" => format!("C${:.2}", money.value_minor as f64 / 100.0),
        "HKD" => format!("HK${:.2}", money.value_minor as f64 / 100.0),
        _ if !money.currency.is_empty() => {
            format!("{} {}", money.currency, money.value_minor)
        }
        _ => format!("{}", money.value_minor),
    }
}

/// McvEnvelope の ProviderMessage を CommentRow のリストに変換する
/// MessageDeleteAll は CommentRow に変換しない（別イベントで処理）
pub(crate) fn envelope_to_comment_rows(envelope: &McvEnvelope) -> Vec<CommentRow> {
    envelope
        .messages
        .iter()
        .filter(|msg| {
            !matches!(
                &msg.kind,
                ProviderMessageKind::System(SystemKind::MessageDeleteAll { .. })
            )
        })
        .map(|msg| {
            let extract_text = |content: &ProviderContent| match content {
                ProviderContent::Text { text } => text.clone(),
                ProviderContent::Empty => vec![],
            };

            let (is_visible, replaces_id, text) = match &msg.kind {
                ProviderMessageKind::System(SystemKind::Placeholder) => (false, None, vec![]),
                ProviderMessageKind::System(SystemKind::MessageUpdate { target_message_id }) => (
                    true,
                    Some(target_message_id.clone()),
                    extract_text(&msg.content),
                ),
                ProviderMessageKind::System(SystemKind::MessageDelete { target_message_id }) => {
                    (false, Some(target_message_id.clone()), vec![])
                }
                _ => (true, None, extract_text(&msg.content)),
            };

            let (kind, amount_text) = match &msg.kind {
                ProviderMessageKind::Chat => ("chat", None),
                ProviderMessageKind::HistoryChat => ("history_chat", None),
                ProviderMessageKind::Monetary(info) => {
                    ("monetary", Some(format_money(&info.amount)))
                }
                _ => ("system", None),
            };
            let kind = kind.to_string();

            CommentRow {
                id: msg.id.clone(),
                user_name: msg.sender.display_name.clone(),
                user_id: msg.sender.id.clone(),
                badges: msg.sender.badges.clone(),
                text,
                timestamp: msg.timestamp,
                connection_id: envelope.connection_id.to_string(),
                is_visible,
                replaces_id,
                kind,
                avatar_url: msg.sender.avatar_url.clone(),
                amount_text,
            }
        })
        .collect()
}

#[cfg(test)]
mod tests {
    use super::*;

    fn make_money(currency: &str, value: i64) -> Money {
        Money {
            currency: currency.to_string(),
            value_minor: value,
        }
    }

    #[test]
    fn format_money_jpy() {
        assert_eq!(format_money(&make_money("JPY", 500)), "¥500");
    }

    #[test]
    fn format_money_usd_cents() {
        assert_eq!(format_money(&make_money("USD", 1000)), "$10.00");
        assert_eq!(format_money(&make_money("USD", 50)), "$0.50");
    }

    #[test]
    fn format_money_eur() {
        assert_eq!(format_money(&make_money("EUR", 200)), "€2.00");
    }

    #[test]
    fn format_money_krw() {
        assert_eq!(format_money(&make_money("KRW", 10000)), "₩10000");
    }

    #[test]
    fn format_money_unknown_currency() {
        assert_eq!(format_money(&make_money("XYZ", 42)), "XYZ 42");
    }

    #[test]
    fn format_money_empty_currency() {
        assert_eq!(format_money(&make_money("", 99)), "99");
    }

    #[test]
    fn envelope_to_comment_rows_filters_delete_all() {
        use mcv_messages::{
            ChannelId, McvEnvelope, ProviderMessage, ProviderSender, ServiceId, SystemKind,
        };
        use uuid::Uuid;

        let conn_id = Uuid::new_v4();
        let event_id = Uuid::new_v4();

        let delete_all_msg = ProviderMessage {
            id: "d1".to_string(),
            platform_message_id: None,
            service: ServiceId("test".to_string()),
            channel: ChannelId("ch".to_string()),
            sender: ProviderSender {
                id: "user1".to_string(),
                display_name: vec![],
                badges: vec![],
                role: None,
                avatar_url: None,
            },
            timestamp: 0,
            kind: ProviderMessageKind::System(SystemKind::MessageDeleteAll {
                user_id: "user1".to_string(),
            }),
            content: ProviderContent::Empty,
            reply_to: None,
            metadata: serde_json::json!({}),
        };

        let envelope = McvEnvelope {
            event_id,
            connection_id: conn_id,
            messages: vec![delete_all_msg],
            received_at: 0,
            raw_message: None,
        };

        let rows = envelope_to_comment_rows(&envelope);
        // MessageDeleteAll は CommentRow に変換されない
        assert!(rows.is_empty());
    }

    #[test]
    fn envelope_to_comment_rows_chat_kind() {
        use mcv_messages::{
            ChannelId, McvEnvelope, MessagePart, ProviderMessage, ProviderSender, ServiceId,
        };
        use uuid::Uuid;

        let conn_id = Uuid::new_v4();
        let event_id = Uuid::new_v4();

        let chat_msg = ProviderMessage {
            id: "c1".to_string(),
            platform_message_id: None,
            service: ServiceId("test".to_string()),
            channel: ChannelId("ch".to_string()),
            sender: ProviderSender {
                id: "user2".to_string(),
                display_name: vec![MessagePart::Text {
                    text: "User2".to_string(),
                }],
                badges: vec![],
                role: None,
                avatar_url: None,
            },
            timestamp: 100,
            kind: ProviderMessageKind::Chat,
            content: ProviderContent::Text {
                text: vec![MessagePart::Text {
                    text: "hello".to_string(),
                }],
            },
            reply_to: None,
            metadata: serde_json::json!({}),
        };

        let envelope = McvEnvelope {
            event_id,
            connection_id: conn_id,
            messages: vec![chat_msg],
            received_at: 0,
            raw_message: None,
        };

        let rows = envelope_to_comment_rows(&envelope);
        assert_eq!(rows.len(), 1);
        let row = &rows[0];
        assert_eq!(row.id, "c1");
        assert_eq!(row.user_id, "user2");
        assert_eq!(row.kind, "chat");
        assert_eq!(row.timestamp, 100);
        assert!(row.is_visible);
        assert!(row.replaces_id.is_none());
        assert!(row.amount_text.is_none());
        assert_eq!(row.connection_id, conn_id.to_string());
    }

    #[test]
    fn envelope_to_comment_rows_placeholder_is_invisible() {
        use mcv_messages::{
            ChannelId, McvEnvelope, ProviderMessage, ProviderSender, ServiceId, SystemKind,
        };
        use uuid::Uuid;

        let conn_id = Uuid::new_v4();
        let msg = ProviderMessage {
            id: "p1".to_string(),
            platform_message_id: None,
            service: ServiceId("test".to_string()),
            channel: ChannelId("ch".to_string()),
            sender: ProviderSender {
                id: "u".to_string(),
                display_name: vec![],
                badges: vec![],
                role: None,
                avatar_url: None,
            },
            timestamp: 0,
            kind: ProviderMessageKind::System(SystemKind::Placeholder),
            content: ProviderContent::Empty,
            reply_to: None,
            metadata: serde_json::json!({}),
        };

        let envelope = McvEnvelope {
            event_id: Uuid::new_v4(),
            connection_id: conn_id,
            messages: vec![msg],
            received_at: 0,
            raw_message: None,
        };

        let rows = envelope_to_comment_rows(&envelope);
        assert_eq!(rows.len(), 1);
        assert!(!rows[0].is_visible);
    }
}
