using Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ryu_s.BrowserCookie;
using SitePlugin;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using YouTubeLiveSitePlugin.Test2;

namespace YouTubeLiveSitePlugin.Next
{
    class HashGenerator
    {
        public string GetSapiSid()
        {
            var cookies = Tools.ExtractCookies(_cc);
            var c = cookies.Find(cookie => cookie.Name == "SAPISID");
            return c?.Value;
        }
        protected virtual long GetCurrentUnixTime()
        {
            return Common.UnixTimeConverter.ToUnixTime(DateTime.Now);
        }
        private string ComputeSHA1(string s)
        {
            var bytes = Encoding.UTF8.GetBytes(s);
            byte[] hashValue;
            using (var crypto = new SHA1CryptoServiceProvider())
            {
                hashValue = crypto.ComputeHash(bytes);
            }
            var sb = new StringBuilder();
            foreach (var b in hashValue)
            {
                sb.AppendFormat("{0:X2}", b);
            }
            return sb.ToString();
        }
        public string CreateHash()
        {
            var unixTime = GetCurrentUnixTime();
            var sapiSid = GetSapiSid();
            var origin = "https://www.youtube.com";
            if (sapiSid == null)
            {
                throw new SpecChangedException("");
            }
            var s = $"{unixTime} {sapiSid} {origin}";
            var hash = ComputeSHA1(s).ToLower();
            return $"{unixTime}_{hash}";
        }
        public HashGenerator(CookieContainer cc)
        {
            _cc = cc;
        }
        private readonly CookieContainer _cc;
    }
    class YtInitialData
    {
        private readonly dynamic _d;
        public string Raw { get; }
        private ChatContinuation _chatContinuation;
        public ChatContinuation ChatContinuation
        {
            get
            {
                if (_chatContinuation != null)
                {
                    return _chatContinuation;
                }
                //if(_d.contents.liveChatRenderer == null)
                //{
                //チャット無効。配信が終わったかチャットが無効に設定されている。
                //}
                var chatContinuation = new ChatContinuation
                {
                    AllChatContinuation = (string)_d.contents.liveChatRenderer.header.liveChatHeaderRenderer.viewSelector.sortFilterSubMenuRenderer.subMenuItems[1].continuation.reloadContinuationData.continuation,
                    JouiChatContinuation = (string)_d.contents.liveChatRenderer.header.liveChatHeaderRenderer.viewSelector.sortFilterSubMenuRenderer.subMenuItems[0].continuation.reloadContinuationData.continuation,
                };
                _chatContinuation = chatContinuation;
                return chatContinuation;
            }
        }
        public bool IsLoggedIn
        {
            get
            {
                var match = Regex.Match(Raw, "{\"key\":\"logged_in\",\"value\":\"(\\d)\"}");
                if (!match.Success)
                {
                    throw new Exception("");
                }
                var n = int.Parse(match.Groups[1].Value);
                return n == 1;
            }
        }
        public List<IInternalMessage> GetActions()
        {
            var list = new List<IInternalMessage>();
            var actions = _d.contents?.liveChatRenderer?.actions;
            if (actions == null)
            {
                return list;
            }
            foreach (var action in actions)
            {
                var message = (IInternalMessage)Parser2.ParseAction(action);
                if (message == null) continue;
                list.Add(message);
            }
            return list;
        }
        public string GetDelegatedSessionId()
        {
            string s;
            try
            {
                s = (string)_d.responseContext.webResponseContextExtensionData.ytConfigData.delegatedSessionId;
            }
            catch (Microsoft.CSharp.RuntimeBinder.RuntimeBinderException ex)
            {
                throw new SpecChangedException(Raw, ex);
            }
            return s;
        }
        public string GetClientIdPrefix()
        {
            string @params;
            try
            {
                @params = (string)_d.contents.liveChatRenderer.actionPanel.liveChatMessageInputRenderer.sendButton.buttonRenderer.serviceEndpoint.sendLiveChatMessageEndpoint.clientIdPrefix;
            }
            catch (Microsoft.CSharp.RuntimeBinder.RuntimeBinderException ex)
            {
                throw new SpecChangedException(Raw, ex);
            }
            return @params;
        }
        public YtInitialData(string json)
        {
            Raw = json;
            _d = JsonConvert.DeserializeObject(json);
        }
    }
    class YtCfg
    {
        private readonly dynamic _d;
        public string Raw { get; }
        public string DelegatedSessionId => (string)_d.DELEGATED_SESSION_ID;
        public string InnerTubeApiKey => (string)_d.INNERTUBE_API_KEY;
        public string VisitorData => (string)_d.VISITOR_DATA;
        public string InnerTubeContext => (string)_d.INNERTUBE_CONTEXT.ToString(Formatting.None);
        public YtCfg(string json)
        {
            Raw = json;
            _d = JsonConvert.DeserializeObject(json);
        }

        internal string GetXsrfToken()
        {
            throw new NotImplementedException();
        }
    }
    class DataToPost
    {
        private readonly dynamic _d;
        public void SetContinuation(string continuation)
        {
            _d.continuation = continuation;
        }
        public DataToPost(YtCfg ytCfg)
        {
            dynamic d = JsonConvert.DeserializeObject("{\"context\":{}}", new JsonSerializerSettings { Formatting = Formatting.None });
            dynamic context = JsonConvert.DeserializeObject(ytCfg.InnerTubeContext, new JsonSerializerSettings { Formatting = Formatting.None });
            d.context = context;
            if (ytCfg.DelegatedSessionId != null)//未ログインの場合は設定されない
            {
                dynamic user = JsonConvert.DeserializeObject("{\"onBehalfOfUser\":" + "\"" + ytCfg.DelegatedSessionId + "\"" + "}", new JsonSerializerSettings { Formatting = Formatting.None });
                d.context.user = user;
            }
            else
            {
                dynamic user = JsonConvert.DeserializeObject("{}", new JsonSerializerSettings { Formatting = Formatting.None });
                d.context.user = user;
            }
            _d = d;
        }
        public override string ToString()
        {
            return _d.ToString(Formatting.None);
        }
    }
    class Parser2
    {
        private static (string url, int width, int height) GetThumbnail(dynamic authorPhoto)
        {
            var thumbnail = authorPhoto.thumbnails[0];
            var url = (string)thumbnail.url;
            var width = (int)thumbnail.width;
            var height = (int)thumbnail.height;
            return (url, width, height);
        }
        private static List<IMessagePart> ParseRunsOrSimpleText(dynamic ren)
        {
            var messageItems = new List<IMessagePart>();
            if (ren.ContainsKey("runs"))
            {
                foreach (var r in ren.runs)
                {
                    if (r.ContainsKey("text"))
                    {
                        var text = (string)r.text;
                        messageItems.Add(MessagePartFactory.CreateMessageText(text));
                    }
                    else if (r.ContainsKey("emoji"))
                    {
                        var emoji = r.emoji;
                        var thumbnail = emoji.image.thumbnails[0];
                        var emojiUrl = thumbnail.url;
                        var emojiWidth = (int)thumbnail.width;
                        var emojiHeight = (int)thumbnail.height;
                        var emojiAlt = emoji.image.accessibility.accessibilityData.label;
                        messageItems.Add(new MessageImage { Url = emojiUrl, Alt = emojiAlt, Height = emojiHeight, Width = emojiWidth });
                    }
                    else
                    {
                        throw new ParseException();
                    }
                }
            }
            else if (ren.ContainsKey("simpleText"))
            {
                var text = (string)ren.simpleText;
                messageItems.Add(MessagePartFactory.CreateMessageText(text));
            }
            else
            {
                throw new ParseException();
            }
            return messageItems;
        }
        private static List<IMessagePart> GetMessageParts(dynamic ren)
        {
            var messageItems = new List<IMessagePart>();
            if (!ren.ContainsKey("message"))//PaidMessageではコメント無しも可能
            {
                return messageItems;
            }
            if (ren.message.ContainsKey("runs"))
            {
                foreach (var r in ren.message.runs)
                {
                    if (r.ContainsKey("text"))
                    {
                        var text = (string)r.text;
                        messageItems.Add(MessagePartFactory.CreateMessageText(text));
                    }
                    else if (r.ContainsKey("emoji"))
                    {
                        var emoji = r.emoji;
                        var thumbnail = emoji.image.thumbnails[0];
                        var emojiUrl = thumbnail.url;
                        var emojiWidth = (int)thumbnail.width;
                        var emojiHeight = (int)thumbnail.height;
                        var emojiAlt = emoji.image.accessibility.accessibilityData.label;
                        messageItems.Add(new MessageImage { Url = emojiUrl, Alt = emojiAlt, Height = emojiHeight, Width = emojiWidth });
                    }
                    else
                    {
                        throw new ParseException();
                    }
                }
            }
            else if (ren.message.ContainsKey("simpleText"))
            {
                var text = (string)ren.message.simpleText;
                messageItems.Add(MessagePartFactory.CreateMessageText(text));
            }
            else
            {
                throw new ParseException();
            }
            return messageItems;
        }
        private static List<IMessagePart> GetNameParts(dynamic ren)
        {
            var nameItems = new List<IMessagePart>();
            if (ren.ContainsKey("authorName"))
            {
                nameItems.Add(MessagePartFactory.CreateMessageText((string)ren.authorName.simpleText));
            }
            if (ren.ContainsKey("authorBadges"))
            {
                foreach (var badge in ren.authorBadges)
                {
                    if (badge.liveChatAuthorBadgeRenderer.ContainsKey("customThumbnail"))
                    {
                        var url = (string)badge.liveChatAuthorBadgeRenderer.customThumbnail.thumbnails[0].url;
                        var alt = (string)badge.liveChatAuthorBadgeRenderer.tooltip;
                        nameItems.Add(new MessageImage { Url = url, Alt = alt, Width = 16, Height = 16 });
                    }
                    else if (badge.liveChatAuthorBadgeRenderer.ContainsKey("icon"))
                    {
                        var iconType = (string)badge.liveChatAuthorBadgeRenderer.icon.iconType;
                        var alt = (string)badge.liveChatAuthorBadgeRenderer.tooltip;
                    }
                    else
                    {
                        throw new ParseException();
                    }
                }
            }
            return nameItems;
        }
        public static InternalSuperChat ParseLiveChatPaidMessageRenderer(dynamic ren)
        {
            var ahh = new InternalSuperChat
            {
                UserId = (string)ren.authorExternalChannelId,
                TimestampUsec = long.Parse((string)ren.timestampUsec),
                Id = (string)ren.id
            };

            //authorPhoto
            (ahh.ThumbnailUrl, ahh.ThumbnailWidth, ahh.ThumbnailHeight) = (ValueTuple<string, int, int>)GetThumbnail(ren.authorPhoto);
            //message
            ahh.MessageItems = (List<IMessagePart>)GetMessageParts(ren);
            //name
            ahh.NameItems = (List<IMessagePart>)GetNameParts(ren);
            //purchaseAmount
            ahh.PurchaseAmount = (string)ren.purchaseAmountText.simpleText;

            return ahh;
        }
        public static InternalComment ParseLiveChatTextMessageRenderer(dynamic ren)
        {
            var ahh = new InternalComment
            {
                UserId = (string)ren.authorExternalChannelId,
                TimestampUsec = long.Parse((string)ren.timestampUsec),
                Id = (string)ren.id
            };

            //authorPhoto
            (ahh.ThumbnailUrl, ahh.ThumbnailWidth, ahh.ThumbnailHeight) = (ValueTuple<string, int, int>)GetThumbnail(ren.authorPhoto);
            //message
            ahh.MessageItems = (List<IMessagePart>)GetMessageParts(ren);
            //name
            ahh.NameItems = (List<IMessagePart>)GetNameParts(ren);
            return ahh;
        }
        public static IInternalMessage ParseAction(string raw)
        {
            var d = JsonConvert.DeserializeObject(raw);
            return ParseAction(d);
        }
        public static IInternalMessage ParseAction(dynamic action)
        {
            IInternalMessage ret;
            if (action.ContainsKey("addChatItemAction"))
            {
                var item = action.addChatItemAction.item;
                if (item.ContainsKey("liveChatTextMessageRenderer"))
                {
                    ret = Parser2.ParseLiveChatTextMessageRenderer(item.liveChatTextMessageRenderer);
                }
                else if (item.ContainsKey("liveChatPaidMessageRenderer"))
                {
                    var ren = item.liveChatPaidMessageRenderer;
                    var commentData = Parser2.ParseLiveChatPaidMessageRenderer(ren);
                    ret = commentData;
                }
                else if (item.ContainsKey("liveChatViewerEngagementMessageRenderer"))
                {
                    var ren = item.liveChatViewerEngagementMessageRenderer;
                    //ブラウザで見ると表示される"チャットへようこそ！ご自身のプライバシーを守るとともに～"というやつ
                    ret = null;
                }
                else if (item.ContainsKey("liveChatMembershipItemRenderer"))
                {
                    //メンバーに登録した時に流れる
                    //{{"liveChatMembershipItemRenderer":{"id":"ChwKGkNNYTBvZmI2anU0Q0ZZT0R3Z0VkVk4wSWNn","timestampUsec":"1610199056715518","authorExternalChannelId":"UC3NDq4U3m399k6Xvu3Xjmdw","headerSubtext":{"runs":[{"text":"★THEかなた★"},{"text":"へようこそ！"}]},"authorName":{"simpleText":"NightStrix"},"authorPhoto":{"thumbnails":[{"url":"https://yt4.ggpht.com/ytc/AAUvwnhRqfpVnCOX-xA6HyfwiAGXePe_Ahc3MjLaetfwYQ=s32-c-k-c0x00ffffff-no-rj","width":32,"height":32},{"url":"https://yt4.ggpht.com/ytc/AAUvwnhRqfpVnCOX-xA6HyfwiAGXePe_Ahc3MjLaetfwYQ=s64-c-k-c0x00ffffff-no-rj","width":64,"height":64}]},"authorBadges":[{"liveChatAuthorBadgeRenderer":{"customThumbnail":{"thumbnails":[{"url":"https://yt3.ggpht.com/kjXx5nboby_LOvHUnWn4phLsmJw-zyUjZccLSCV3vXx2pvouqWxALzm2KFtWcf7ylkTQVcodow=s16-c-k"},{"url":"https://yt3.ggpht.com/kjXx5nboby_LOvHUnWn4phLsmJw-zyUjZccLSCV3vXx2pvouqWxALzm2KFtWcf7ylkTQVcodow=s32-c-k"}]},"tooltip":"新規メンバー","accessibility":{"accessibilityData":{"label":"新規メンバー"}}}}],"contextMenuEndpoint":{"commandMetadata":{"webCommandMetadata":{"ignoreNavigation":true}},"liveChatItemContextMenuEndpoint":{"params":"Q2g0S0hBb2FRMDFoTUc5bVlqWnFkVFJEUmxsUFJIZG5SV1JXVGpCSlkyY1FBQm80Q2cwS0MxZ3plRE52YldONFJGSnJLaWNLR0ZWRFdteEVXSHBIYjI4M1pEUTBZbmRrVGs5aVJtRmpaeElMV0RONE0yOXRZM2hFVW1zZ0FpZ0JNaG9LR0ZWRE0wNUVjVFJWTTIwek9UbHJObGgyZFROWWFtMWtkdyUzRCUzRA=="}},"contextMenuAccessibility":{"accessibilityData":{"label":"コメントの操作"}}}}}
                    ret = Parser2.ParseLiveChatMembershipItemRenderer(item.liveChatMembershipItemRenderer);
                }
                else if (item.ContainsKey("liveChatPaidStickerRenderer"))
                {

                    //
                    //{{
                    //  "liveChatPaidStickerRenderer": {
                    //    "id": "ChwKGkNOYlRuNjJWai00Q0ZjTU01d29kRk9RTTNB",
                    //    "contextMenuEndpoint": {
                    //      "clickTrackingParams": "CAYQ77sEIhMIrNzTs5WP7gIVI6DCCh14zQFJ",
                    //      "commandMetadata": {
                    //        "webCommandMetadata": {
                    //          "ignoreNavigation": true
                    //        }
                    //      },
                    //      "liveChatItemContextMenuEndpoint": {
                    //        "params": "Q2g0S0hBb2FRMDVpVkc0Mk1sWnFMVFJEUm1OTlRUVjNiMlJHVDFGTk0wRVFBQm80Q2cwS0MyRm5iM28zYzJReGJrZHJLaWNLR0ZWRE1XOXdTRlZ5ZHpoeWRtNXpZV1JVTFdsSGNEZERaeElMWVdkdmVqZHpaREZ1UjJzZ0FpZ0JNaG9LR0ZWRFJUVnlRMGRxWlRGc1dqUTFObkZOZDBsZmNscGtadyUzRCUzRA=="
                    //      }
                    //    },
                    //    "contextMenuAccessibility": {
                    //      "accessibilityData": {
                    //        "label": "コメントの操作"
                    //      }
                    //    },
                    //    "timestampUsec": "1610206160591525",
                    //    "authorPhoto": {
                    //      "thumbnails": [
                    //        {
                    //          "url": "https://yt4.ggpht.com/ytc/AAUvwnjpwOBLdPMAdYAoEyoQRdVeu17VcJqAXkwNc0wA=s32-c-k-c0x00ffffff-no-rj",
                    //          "width": 32,
                    //          "height": 32
                    //        },
                    //        {
                    //          "url": "https://yt4.ggpht.com/ytc/AAUvwnjpwOBLdPMAdYAoEyoQRdVeu17VcJqAXkwNc0wA=s64-c-k-c0x00ffffff-no-rj",
                    //          "width": 64,
                    //          "height": 64
                    //        }
                    //      ]
                    //    },
                    //    "authorName": {
                    //      "simpleText": "qfeuille3"
                    //    },
                    //    "authorExternalChannelId": "UCE5rCGje1lZ456qMwI_rZdg",
                    //    "sticker": {
                    //      "thumbnails": [
                    //        {
                    //          "url": "//lh3.googleusercontent.com/1GF4XO0fhtEnQiPQwgLDQ49XhFOJxV7aJW3ku9eJEJptm1UwdE-vzQb4wTF5Utg5rcsSJuBY7sCkwyTLkeg=s104-rg",
                    //          "width": 104,
                    //          "height": 104
                    //        },
                    //        {
                    //          "url": "//lh3.googleusercontent.com/1GF4XO0fhtEnQiPQwgLDQ49XhFOJxV7aJW3ku9eJEJptm1UwdE-vzQb4wTF5Utg5rcsSJuBY7sCkwyTLkeg=s208-rg",
                    //          "width": 208,
                    //          "height": 208
                    //        }
                    //      ],
                    //      "accessibility": {
                    //        "accessibilityData": {
                    //          "label": "伝統的な衣装を身につけて扇子を振っている柴犬"
                    //        }
                    //      }
                    //    },
                    //    "moneyChipBackgroundColor": 4294953512,
                    //    "moneyChipTextColor": 3741319168,
                    //    "purchaseAmountText": {
                    //      "simpleText": "SGD 10.00"
                    //    },
                    //    "stickerDisplayWidth": 104,
                    //    "stickerDisplayHeight": 104,
                    //    "backgroundColor": 4294947584,
                    //    "authorNameTextColor": 2315255808,
                    //    "trackingParams": "CAYQ77sEIhMIrNzTs5WP7gIVI6DCCh14zQFJ"
                    //  }
                    //}}
                    ret = null;
                }
                else if (item.ContainsKey("liveChatPlaceholderItemRenderer"))
                {
                    //{{
                    //  "liveChatPlaceholderItemRenderer": {
                    //    "id": "CjkKGkNJYXhrdnVNa080Q0ZRaXJ3UW9kS2E4TWhBEhtDTjI4N00yTGtPNENGWUd0RFFvZHFDc0hIZzQ%3D",
                    //    "timestampUsec": "1610238258354345"
                    //  }
                    //}}
                    ret = null;
                }
                else
                {
                    ret = null;
                }
            }
            else if (action.ContainsKey("addLiveChatTickerItemAction"))
            {
                ret = null;
            }
            else if (action.ContainsKey("markChatItemAsDeletedAction"))
            {
                //{{
                //  "markChatItemAsDeletedAction": {
                //    "deletedStateMessage": {
                //     "runs": [
                //        {
                //          "text": "[メッセージが撤回されました]"
                //        }
                //      ]
                //   },
                //    "targetItemId": "CjoKGkNMQ0RvOVA2anU0Q0Zid1RyUVlkV05rT1NBEhxDTDd3d3RuNGp1NENGUU9jandvZERGOEZSUTEw"
                //  }
                //}}
                ret = null;
            }
            else if (action.ContainsKey("markChatItemsByAuthorAsDeletedAction"))
            {
                //{{
                //  "markChatItemsByAuthorAsDeletedAction": {
                //    "deletedStateMessage": {
                //      "runs": [
                //        {
                //          "text": "[メッセージが削除されました]"
                //        }
                //      ]
                //    },
                //    "externalChannelId": "UCo1q4cmG01Emu7LSuLzunzA"
                //  }
                //}}
                ret = null;
            }
            else
            {
                ret = null;
            }
            return ret;
        }

        private static IInternalMessage ParseLiveChatMembershipItemRenderer(dynamic ren)
        {
            var ahh = new InternalMembership
            {
                UserId = (string)ren.authorExternalChannelId,
                TimestampUsec = long.Parse((string)ren.timestampUsec),
                Id = (string)ren.id
            };

            //authorPhoto
            (ahh.ThumbnailUrl, ahh.ThumbnailWidth, ahh.ThumbnailHeight) = (ValueTuple<string, int, int>)GetThumbnail(ren.authorPhoto);
            //message
            ahh.MessageItems = (List<IMessagePart>)ParseRunsOrSimpleText(ren.headerSubtext);
            //name
            ahh.NameItems = (List<IMessagePart>)GetNameParts(ren);
            return ahh;
        }
    }
    class GetLiveChat
    {
        private IContinuation _continuation;
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ChatUnavailableException"></exception>
        /// <exception cref="ContinuationNotExistsException"></exception>
        public IContinuation GetContinuation()
        {
            if (_continuation != null)
            {
                return _continuation;
            }
            if (!_d.ContainsKey("continuationContents"))
            {
                throw new ChatUnavailableException();
            }
            if (!_d.continuationContents.liveChatContinuation.ContainsKey("continuations"))
            {
                //2021/01/11 仕様変更後、こっちに来ることは無いかも。全部ChatUnavailableExceptionになる気がする。要検証。
                throw new ContinuationNotExistsException();
            }
            if (_d.continuationContents.liveChatContinuation.continuations[0].ContainsKey("timedContinuationData"))
            {
                var c = new TimedContinuation
                {
                    Continuation = (string)_d.continuationContents.liveChatContinuation.continuations[0].timedContinuationData.continuation,
                    TimeoutMs = (int)_d.continuationContents.liveChatContinuation.continuations[0].timedContinuationData.timeoutMs,
                };
                _continuation = c;
                return c;
            }
            else if (_d.continuationContents.liveChatContinuation.continuations[0].ContainsKey("invalidationContinuationData"))
            {
                //  {
                //    "invalidationContinuationData": {
                //      "invalidationId": {
                //        "objectSource": 1056,
                //        "objectId": "Y2hhdH5YM3gzb21jeERSa341MzY3MzM1",
                //        "topic": "chat~X3x3omcxDRk~5367335",
                //        "subscribeToGcmTopics": true,
                //        "protoCreationTimestampMs": "1610200735376"
                //      },
                //      "timeoutMs": 10000,
                //      "continuation": "0ofMyAOHAhqyAUNqZ0tEUW9MV0RONE0yOXRZM2hFVW1zcUp3b1lWVU5hYkVSWWVrZHZiemRrTkRSaWQyUk9UMkpHWVdObkVndFlNM2d6YjIxamVFUlNheHBEcXJuQnZRRTlDanRvZEhSd2N6b3ZMM2QzZHk1NWIzVjBkV0psTG1OdmJTOXNhWFpsWDJOb1lYUV9hWE5mY0c5d2IzVjBQVEVtZGoxWU0zZ3piMjFqZUVSU2F5QUNLQUUlM0Qoqq2rlIGP7gIwADgAQAJKGwgAEAAYACAAOgBAAEoAUPrSiML5ju4CWAN4AFDak9OUgY_uAlj5jbbq4Y3uAmgBggECCAGIAQCgAarK5ZaBj-4C"
                //    }
                //  }
                var i = new InvalidationContinuation
                {
                    Continuation = (string)_d.continuationContents.liveChatContinuation.continuations[0].invalidationContinuationData.continuation,
                    TimeoutMs = (int)_d.continuationContents.liveChatContinuation.continuations[0].invalidationContinuationData.timeoutMs,
                };
                return i;
            }
            else if (_d.continuationContents.liveChatContinuation.continuations[0].ContainsKey("reloadContinuationData"))
            {
                var r = new ReloadContinuation
                {
                    Continuation = (string)_d.continuationContents.liveChatContinuation.continuations[0].reloadContinuationData.continuation,
                };
                _continuation = r;
                return r;
            }
            throw new SpecChangedException(Raw);
        }

        public List<IInternalMessage> GetActions()
        {
            var list = new List<IInternalMessage>();
            if (!_d.ContainsKey("continuationContents"))
            {
                return list;
            }
            if (!_d.continuationContents.liveChatContinuation.ContainsKey("actions"))
            {
                return list;
            }
            var actions = _d.continuationContents.liveChatContinuation.actions;
            foreach (var action in actions)
            {
                var message = (IInternalMessage)Parser2.ParseAction(action);
                if (message == null) continue;
                list.Add(message);
            }
            return list;
        }
        public string Raw { get; }
        public GetLiveChat(string json)
        {
            Raw = json;
            _d = JsonConvert.DeserializeObject(json);
        }
        private readonly dynamic _d;
    }
    static class Tools
    {
        public static YtInitialData ExtractYtInitialData(string liveChatHtml)
        {
            var match = Regex.Match(liveChatHtml, "window\\[\"ytInitialData\"\\] = ({.+?});");
            if (!match.Success)
            {
                throw new Exception("");
            }
            var raw = match.Groups[1].Value;
            return new YtInitialData(raw);
        }
        public static string ExtractYtCfg(string liveChatHtml)
        {
            var match = Regex.Match(liveChatHtml, "ytcfg\\.set\\(({.+?})\\);\r?\n", RegexOptions.Singleline);
            if (!match.Success)
            {
                throw new ParseException(liveChatHtml);
            }
            var ytCfg = match.Groups[1].Value;
            dynamic d = JsonConvert.DeserializeObject(ytCfg);
            var matches = Regex.Matches(liveChatHtml, "ytcfg\\.set\\(\"([^\"]+)\",\\s*(.+?)\\);?\\r?\n", RegexOptions.Singleline);
            foreach (Match m in matches)
            {
                var key = m.Groups[1].Value;
                var value = m.Groups[2].Value;
                var s = "{\"" + key + "\":" + value + "}";
                var obb = JsonConvert.DeserializeObject(s);
                d.Merge(obb);
            }
            return d.ToString(Formatting.None);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="innerTubeApiKey"></param>
        /// <param name="cc"></param>
        /// <param name="loginInfo"></param>
        /// <returns></returns>
        /// <exception cref="HttpRequestException">多分500番台のエラーだけ</exception>
        public static async Task<GetLiveChat> GetGetLiveChat(DataToPost data, string innerTubeApiKey, CookieContainer cc, ILoginState loginInfo)
        {
            //dataの構造
            //context
            //+client
            //+request
            //++sessionId
            //+user
            //++onBehalfOfUser
            //continuation

            var url = $"https://www.youtube.com/youtubei/v1/live_chat/get_live_chat?key={innerTubeApiKey}";
            var handler = new HttpClientHandler
            {
                UseCookies = true,
                CookieContainer = cc,
            };
            var client = new HttpClient(handler);
            var c = new StringContent(data.ToString(), Encoding.UTF8, "application/json");
            if (loginInfo is LoggedIn loggedIn)
            {
                var hash = new HashGenerator(cc).CreateHash();
                client.DefaultRequestHeaders.Add("Authorization", $"SAPISIDHASH {hash}");
            }

            client.DefaultRequestHeaders.Add("Origin", "https://www.youtube.com");
            var k = await client.PostAsync(url, c);
            var s = await k.Content.ReadAsStringAsync();
            var getLiveChat = new GetLiveChat(s);
            return getLiveChat;
        }
        /// <summary>
        /// CookieContainerから全てのCookieを取り出す
        /// </summary>
        /// <param name="container"></param>
        /// <returns></returns>
        public static List<Cookie> ExtractCookies(CookieContainer container)
        {
            var cookies = new List<Cookie>();

            var table = (Hashtable)container.GetType().InvokeMember("m_domainTable",
                                                                    BindingFlags.NonPublic |
                                                                    BindingFlags.GetField |
                                                                    BindingFlags.Instance,
                                                                    null,
                                                                    container,
                                                                    Array.Empty<object>());

            foreach (var key in table.Keys)
            {
                if (!(key is string domain))
                    continue;

                if (domain.StartsWith("."))
                    domain = domain.Substring(1);

                var address = string.Format("http://{0}/", domain);

                if (Uri.TryCreate(address, UriKind.RelativeOrAbsolute, out Uri uri) == false)
                    continue;

                foreach (Cookie cookie in container.GetCookies(uri))
                {
                    if (cookie == null) continue;
                    cookies.Add(cookie);
                }
            }

            return cookies;
        }

        internal static ILoginState CreateLoginInfo(bool isLoggedin)
        {
            if (isLoggedin)
            {
                return new LoggedIn();
            }
            else
            {
                return new NotLoggedin();
            }
        }
    }
}
