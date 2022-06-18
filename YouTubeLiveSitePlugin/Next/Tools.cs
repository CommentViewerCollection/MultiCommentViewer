using Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ryu_s.BrowserCookie;
using ryu_s.YouTubeLive.Message;
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
    static class SapiSidHashGenerator
    {
        private static string ComputeSHA1(string s)
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
        public static string CreateHash(CookieContainer cc, DateTime currentDate)
        {
            var unixTime = Common.UnixTimeConverter.ToUnixTime(currentDate);
            var sapiSid = Tools.GetSapiSid(cc);
            var origin = "https://www.youtube.com";
            if (sapiSid == null)
            {
                var cookies = Tools.ExtractCookies(cc);
                var keys = string.Join(",", cookies.Select(c => c.Name));
                throw new SpecChangedException($"cookies.Count={cookies.Count},keys={keys}");
            }
            var s = $"{unixTime} {sapiSid} {origin}";
            var hash = ComputeSHA1(s).ToLower();
            return $"{unixTime}_{hash}";
        }
    }
    [Obsolete]
    class YtInitialDataOld
    {
        private readonly dynamic _d;
        public string Raw { get; }
        public string Cver
        {
            get
            {
                var @params = _d.responseContext.serviceTrackingParams[0].@params;
                foreach (var p in @params)
                {
                    if ((string)p.key == "cver")
                    {
                        return (string)p.value;
                    }
                }
                return null;
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
        /// <summary>
        /// チャットが利用可能か
        /// </summary>
        public bool CanChat
        {
            //チャットが無効に設定されている場合、"このライブストリームではチャットは無効です。"と表示されるからこれを利用する
            get
            {
                if (!_d.contents.ContainsKey("messageRenderer"))
                {
                    return true;
                }
                var message = (string)_d.contents.messageRenderer.text.runs[0].text;
                if (message.Contains("チャットは無効"))
                {
                    return false;
                }
                return true;
            }
        }
        public YtInitialDataOld(string json)
        {
            Raw = json;
            _d = JsonConvert.DeserializeObject(json);
        }
    }
    [Obsolete]
    class YtCfgOld
    {
        private readonly dynamic _d;
        public string Raw { get; }
        public string DelegatedSessionId
        {
            get
            {
                if (_d.ContainsKey("DELEGATED_SESSION_ID"))
                {
                    return (string)_d.DELEGATED_SESSION_ID;
                }
                return null;
            }
        }
        public string InnerTubeApiKey => (string)_d.INNERTUBE_API_KEY;
        public string VisitorData => (string)_d.VISITOR_DATA;
        public string InnerTubeContext => (string)_d.INNERTUBE_CONTEXT.ToString(Formatting.None);
        public string XsrfToken => (string)_d.XSRF_TOKEN;
        public YtCfgOld(string json)
        {
            Raw = json;
            _d = JsonConvert.DeserializeObject(json);
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
            dynamic context = JsonConvert.DeserializeObject(ytCfg.InnertubeContext, new JsonSerializerSettings { Formatting = Formatting.None });
            d.context = context;
            //2021/01/16
            //userが最初から設定されている場合があった
            //ただ、その要素は
            //user
            //+gaiaId
            //+userId
            //+lockedSafetyMode
            //onBehalfOfUserは含まれていなかった。上記の要素だけで十分なのかは不明。
            if (!d.ContainsKey("user"))
            {
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
            }
            _d = d;
        }
        public override string ToString()
        {
            return _d.ToString(Formatting.None);
        }
    }
    //class Parser2
    //{
    //    private static (string url, int width, int height) GetThumbnail(dynamic authorPhoto)
    //    {
    //        var thumbnail = authorPhoto.thumbnails[0];
    //        var url = (string)thumbnail.url;
    //        var width = (int)thumbnail.width;
    //        var height = (int)thumbnail.height;
    //        return (url, width, height);
    //    }
    //    private static List<IMessagePart> ParseRunsOrSimpleText(dynamic ren)
    //    {
    //        var messageItems = new List<IMessagePart>();
    //        if (ren.ContainsKey("runs"))
    //        {
    //            try
    //            {
    //                foreach (var r in ren.runs)
    //                {
    //                    if (r.ContainsKey("text"))
    //                    {
    //                        var text = (string)r.text;
    //                        messageItems.Add(MessagePartFactory.CreateMessageText(text));
    //                    }
    //                    else if (r.ContainsKey("emoji"))
    //                    {
    //                        var emoji = r.emoji;
    //                        var thumbnail = emoji.image.thumbnails[0];
    //                        var emojiUrl = (string)thumbnail.url;
    //                        if (emojiUrl.EndsWith(".svg"))
    //                        {
    //                            throw new ParseException();
    //                        }
    //                        else
    //                        {
    //                            var emojiWidth = thumbnail.ContainsKey("width") ? (int)thumbnail.width : 24;
    //                            var emojiHeight = thumbnail.ContainsKey("height") ? (int)thumbnail.height : 24;
    //                            var emojiAlt = emoji.image.accessibility.accessibilityData.label;
    //                            messageItems.Add(new MessageImage { Url = emojiUrl, Alt = emojiAlt, Height = emojiHeight, Width = emojiWidth });
    //                        }
    //                    }
    //                    else
    //                    {
    //                        throw new ParseException();
    //                    }
    //                }
    //            }catch(Exception ex)
    //            {
    //                var raw = ((string)ren.runs.ToString()).Replace(Environment.NewLine, "");
    //                throw new ParseException(raw, ex);
    //            }
    //        }
    //        else if (ren.ContainsKey("simpleText"))
    //        {
    //            var text = (string)ren.simpleText;
    //            messageItems.Add(MessagePartFactory.CreateMessageText(text));
    //        }
    //        else
    //        {
    //            throw new ParseException();
    //        }
    //        return messageItems;
    //    }
    //    private static List<IMessagePart> GetMessageParts(dynamic ren)
    //    {
    //        var messageItems = new List<IMessagePart>();
    //        if (!ren.ContainsKey("message"))//PaidMessageではコメント無しも可能
    //        {
    //            return messageItems;
    //        }
    //        if (ren.message.ContainsKey("runs"))
    //        {
    //            try
    //            {
    //                foreach (var r in ren.message.runs)
    //                {
    //                    if (r.ContainsKey("text"))
    //                    {
    //                        var text = (string)r.text;
    //                        messageItems.Add(MessagePartFactory.CreateMessageText(text));
    //                    }
    //                    else if (r.ContainsKey("emoji"))
    //                    {
    //                        //SVGは現状表示させられないから、urlの末尾が.svgの場合はTextとして扱い、emojiIdを表示する
    //                        var emoji = r.emoji;
    //                        var thumbnail = emoji.image.thumbnails[0];
    //                        var emojiUrl = (string)thumbnail.url;
    //                        if (emojiUrl.EndsWith(".svg"))
    //                        {
    //                            var text = (string)emoji.emojiId;
    //                            messageItems.Add(MessagePartFactory.CreateMessageText(text));
    //                        }
    //                        else
    //                        {
    //                            var emojiWidth = thumbnail.ContainsKey("width") ? (int)thumbnail.width : 24;
    //                            var emojiHeight = thumbnail.ContainsKey("height") ? (int)thumbnail.height : 24;
    //                            var emojiAlt = emoji.image.accessibility.accessibilityData.label;
    //                            messageItems.Add(new MessageImage { Url = emojiUrl, Alt = emojiAlt, Height = emojiHeight, Width = emojiWidth });
    //                        }
    //                    }
    //                    else
    //                    {
    //                        throw new ParseException();
    //                    }
    //                }
    //            }
    //            catch(Exception ex)
    //            {
    //                var raw = ((string)ren.message.runs.ToString()).Replace(Environment.NewLine, "");
    //                throw new ParseException(raw, ex);
    //            }
    //        }
    //        else if (ren.message.ContainsKey("simpleText"))
    //        {
    //            var text = (string)ren.message.simpleText;
    //            messageItems.Add(MessagePartFactory.CreateMessageText(text));
    //        }
    //        else
    //        {
    //            throw new ParseException();
    //        }
    //        return messageItems;
    //    }
    //    private static List<IMessagePart> GetNameParts(dynamic ren)
    //    {
    //        var nameItems = new List<IMessagePart>();
    //        if (ren.ContainsKey("authorName"))
    //        {
    //            nameItems.Add(MessagePartFactory.CreateMessageText((string)ren.authorName.simpleText));
    //        }
    //        if (ren.ContainsKey("authorBadges"))
    //        {
    //            foreach (var badge in ren.authorBadges)
    //            {
    //                if (badge.liveChatAuthorBadgeRenderer.ContainsKey("customThumbnail"))
    //                {
    //                    var url = (string)badge.liveChatAuthorBadgeRenderer.customThumbnail.thumbnails[0].url;
    //                    var alt = (string)badge.liveChatAuthorBadgeRenderer.tooltip;
    //                    nameItems.Add(new MessageImage { Url = url, Alt = alt, Width = 16, Height = 16 });
    //                }
    //                else if (badge.liveChatAuthorBadgeRenderer.ContainsKey("icon"))
    //                {
    //                    var iconType = (string)badge.liveChatAuthorBadgeRenderer.icon.iconType;
    //                    var alt = (string)badge.liveChatAuthorBadgeRenderer.tooltip;
    //                }
    //                else
    //                {
    //                    throw new ParseException();
    //                }
    //            }
    //        }
    //        return nameItems;
    //    }
    //    public static InternalSuperChat ParseLiveChatPaidMessageRenderer(dynamic ren)
    //    {
    //        var ahh = new InternalSuperChat
    //        {
    //            UserId = (string)ren.authorExternalChannelId,
    //            TimestampUsec = long.Parse((string)ren.timestampUsec),
    //            Id = (string)ren.id
    //        };

    //        //authorPhoto
    //        (ahh.ThumbnailUrl, ahh.ThumbnailWidth, ahh.ThumbnailHeight) = (ValueTuple<string, int, int>)GetThumbnail(ren.authorPhoto);
    //        //message
    //        ahh.MessageItems = (List<IMessagePart>)GetMessageParts(ren);
    //        //name
    //        ahh.NameItems = (List<IMessagePart>)GetNameParts(ren);
    //        //purchaseAmount
    //        ahh.PurchaseAmount = (string)ren.purchaseAmountText.simpleText;

    //        return ahh;
    //    }
    //    public static InternalComment ParseLiveChatTextMessageRenderer(dynamic ren)
    //    {
    //        var ahh = new InternalComment
    //        {
    //            UserId = (string)ren.authorExternalChannelId,
    //            TimestampUsec = long.Parse((string)ren.timestampUsec),
    //            Id = (string)ren.id
    //        };

    //        //authorPhoto
    //        (ahh.ThumbnailUrl, ahh.ThumbnailWidth, ahh.ThumbnailHeight) = (ValueTuple<string, int, int>)GetThumbnail(ren.authorPhoto);
    //        //message
    //        ahh.MessageItems = (List<IMessagePart>)GetMessageParts(ren);
    //        //name
    //        ahh.NameItems = (List<IMessagePart>)GetNameParts(ren);
    //        return ahh;
    //    }
    //    public static IInternalMessage ParseAction(string raw)
    //    {
    //        var d = JsonConvert.DeserializeObject(raw);
    //        return ParseAction(d);
    //    }
    //    public static IInternalMessage ParseAction(dynamic action)
    //    {
    //        IInternalMessage ret;
    //        if (action.ContainsKey("addChatItemAction"))
    //        {
    //            var item = action.addChatItemAction.item;
    //            if (item.ContainsKey("liveChatTextMessageRenderer"))
    //            {
    //                ret = Parser2.ParseLiveChatTextMessageRenderer(item.liveChatTextMessageRenderer);
    //            }
    //            else if (item.ContainsKey("liveChatPaidMessageRenderer"))
    //            {
    //                var ren = item.liveChatPaidMessageRenderer;
    //                var commentData = Parser2.ParseLiveChatPaidMessageRenderer(ren);
    //                ret = commentData;
    //            }
    //            else if (item.ContainsKey("liveChatViewerEngagementMessageRenderer"))
    //            {
    //                var ren = item.liveChatViewerEngagementMessageRenderer;
    //                //ブラウザで見ると表示される"チャットへようこそ！ご自身のプライバシーを守るとともに～"というやつ
    //                ret = null;
    //            }
    //            else if (item.ContainsKey("liveChatMembershipItemRenderer"))
    //            {
    //                //メンバーに登録した時に流れる
    //                //{{"liveChatMembershipItemRenderer":{"id":"ChwKGkNNYTBvZmI2anU0Q0ZZT0R3Z0VkVk4wSWNn","timestampUsec":"1610199056715518","authorExternalChannelId":"UC3NDq4U3m399k6Xvu3Xjmdw","headerSubtext":{"runs":[{"text":"★THEかなた★"},{"text":"へようこそ！"}]},"authorName":{"simpleText":"NightStrix"},"authorPhoto":{"thumbnails":[{"url":"https://yt4.ggpht.com/ytc/AAUvwnhRqfpVnCOX-xA6HyfwiAGXePe_Ahc3MjLaetfwYQ=s32-c-k-c0x00ffffff-no-rj","width":32,"height":32},{"url":"https://yt4.ggpht.com/ytc/AAUvwnhRqfpVnCOX-xA6HyfwiAGXePe_Ahc3MjLaetfwYQ=s64-c-k-c0x00ffffff-no-rj","width":64,"height":64}]},"authorBadges":[{"liveChatAuthorBadgeRenderer":{"customThumbnail":{"thumbnails":[{"url":"https://yt3.ggpht.com/kjXx5nboby_LOvHUnWn4phLsmJw-zyUjZccLSCV3vXx2pvouqWxALzm2KFtWcf7ylkTQVcodow=s16-c-k"},{"url":"https://yt3.ggpht.com/kjXx5nboby_LOvHUnWn4phLsmJw-zyUjZccLSCV3vXx2pvouqWxALzm2KFtWcf7ylkTQVcodow=s32-c-k"}]},"tooltip":"新規メンバー","accessibility":{"accessibilityData":{"label":"新規メンバー"}}}}],"contextMenuEndpoint":{"commandMetadata":{"webCommandMetadata":{"ignoreNavigation":true}},"liveChatItemContextMenuEndpoint":{"params":"Q2g0S0hBb2FRMDFoTUc5bVlqWnFkVFJEUmxsUFJIZG5SV1JXVGpCSlkyY1FBQm80Q2cwS0MxZ3plRE52YldONFJGSnJLaWNLR0ZWRFdteEVXSHBIYjI4M1pEUTBZbmRrVGs5aVJtRmpaeElMV0RONE0yOXRZM2hFVW1zZ0FpZ0JNaG9LR0ZWRE0wNUVjVFJWTTIwek9UbHJObGgyZFROWWFtMWtkdyUzRCUzRA=="}},"contextMenuAccessibility":{"accessibilityData":{"label":"コメントの操作"}}}}}
    //                ret = Parser2.ParseLiveChatMembershipItemRenderer(item.liveChatMembershipItemRenderer);
    //            }
    //            else if (item.ContainsKey("liveChatPaidStickerRenderer"))
    //            {

    //                //
    //                //{{
    //                //  "liveChatPaidStickerRenderer": {
    //                //    "id": "ChwKGkNOYlRuNjJWai00Q0ZjTU01d29kRk9RTTNB",
    //                //    "contextMenuEndpoint": {
    //                //      "clickTrackingParams": "CAYQ77sEIhMIrNzTs5WP7gIVI6DCCh14zQFJ",
    //                //      "commandMetadata": {
    //                //        "webCommandMetadata": {
    //                //          "ignoreNavigation": true
    //                //        }
    //                //      },
    //                //      "liveChatItemContextMenuEndpoint": {
    //                //        "params": "Q2g0S0hBb2FRMDVpVkc0Mk1sWnFMVFJEUm1OTlRUVjNiMlJHVDFGTk0wRVFBQm80Q2cwS0MyRm5iM28zYzJReGJrZHJLaWNLR0ZWRE1XOXdTRlZ5ZHpoeWRtNXpZV1JVTFdsSGNEZERaeElMWVdkdmVqZHpaREZ1UjJzZ0FpZ0JNaG9LR0ZWRFJUVnlRMGRxWlRGc1dqUTFObkZOZDBsZmNscGtadyUzRCUzRA=="
    //                //      }
    //                //    },
    //                //    "contextMenuAccessibility": {
    //                //      "accessibilityData": {
    //                //        "label": "コメントの操作"
    //                //      }
    //                //    },
    //                //    "timestampUsec": "1610206160591525",
    //                //    "authorPhoto": {
    //                //      "thumbnails": [
    //                //        {
    //                //          "url": "https://yt4.ggpht.com/ytc/AAUvwnjpwOBLdPMAdYAoEyoQRdVeu17VcJqAXkwNc0wA=s32-c-k-c0x00ffffff-no-rj",
    //                //          "width": 32,
    //                //          "height": 32
    //                //        },
    //                //        {
    //                //          "url": "https://yt4.ggpht.com/ytc/AAUvwnjpwOBLdPMAdYAoEyoQRdVeu17VcJqAXkwNc0wA=s64-c-k-c0x00ffffff-no-rj",
    //                //          "width": 64,
    //                //          "height": 64
    //                //        }
    //                //      ]
    //                //    },
    //                //    "authorName": {
    //                //      "simpleText": "qfeuille3"
    //                //    },
    //                //    "authorExternalChannelId": "UCE5rCGje1lZ456qMwI_rZdg",
    //                //    "sticker": {
    //                //      "thumbnails": [
    //                //        {
    //                //          "url": "//lh3.googleusercontent.com/1GF4XO0fhtEnQiPQwgLDQ49XhFOJxV7aJW3ku9eJEJptm1UwdE-vzQb4wTF5Utg5rcsSJuBY7sCkwyTLkeg=s104-rg",
    //                //          "width": 104,
    //                //          "height": 104
    //                //        },
    //                //        {
    //                //          "url": "//lh3.googleusercontent.com/1GF4XO0fhtEnQiPQwgLDQ49XhFOJxV7aJW3ku9eJEJptm1UwdE-vzQb4wTF5Utg5rcsSJuBY7sCkwyTLkeg=s208-rg",
    //                //          "width": 208,
    //                //          "height": 208
    //                //        }
    //                //      ],
    //                //      "accessibility": {
    //                //        "accessibilityData": {
    //                //          "label": "伝統的な衣装を身につけて扇子を振っている柴犬"
    //                //        }
    //                //      }
    //                //    },
    //                //    "moneyChipBackgroundColor": 4294953512,
    //                //    "moneyChipTextColor": 3741319168,
    //                //    "purchaseAmountText": {
    //                //      "simpleText": "SGD 10.00"
    //                //    },
    //                //    "stickerDisplayWidth": 104,
    //                //    "stickerDisplayHeight": 104,
    //                //    "backgroundColor": 4294947584,
    //                //    "authorNameTextColor": 2315255808,
    //                //    "trackingParams": "CAYQ77sEIhMIrNzTs5WP7gIVI6DCCh14zQFJ"
    //                //  }
    //                //}}
    //                ret = null;
    //            }
    //            else if (item.ContainsKey("liveChatPlaceholderItemRenderer"))
    //            {
    //                //{{
    //                //  "liveChatPlaceholderItemRenderer": {
    //                //    "id": "CjkKGkNJYXhrdnVNa080Q0ZRaXJ3UW9kS2E4TWhBEhtDTjI4N00yTGtPNENGWUd0RFFvZHFDc0hIZzQ%3D",
    //                //    "timestampUsec": "1610238258354345"
    //                //  }
    //                //}}
    //                ret = null;
    //            }
    //            else
    //            {
    //                ret = null;
    //            }
    //        }
    //        else if (action.ContainsKey("addLiveChatTickerItemAction"))
    //        {
    //            ret = null;
    //        }
    //        else if (action.ContainsKey("markChatItemAsDeletedAction"))
    //        {
    //            //{{
    //            //  "markChatItemAsDeletedAction": {
    //            //    "deletedStateMessage": {
    //            //     "runs": [
    //            //        {
    //            //          "text": "[メッセージが撤回されました]"
    //            //        }
    //            //      ]
    //            //   },
    //            //    "targetItemId": "CjoKGkNMQ0RvOVA2anU0Q0Zid1RyUVlkV05rT1NBEhxDTDd3d3RuNGp1NENGUU9jandvZERGOEZSUTEw"
    //            //  }
    //            //}}
    //            ret = null;
    //        }
    //        else if (action.ContainsKey("markChatItemsByAuthorAsDeletedAction"))
    //        {
    //            //{{
    //            //  "markChatItemsByAuthorAsDeletedAction": {
    //            //    "deletedStateMessage": {
    //            //      "runs": [
    //            //        {
    //            //          "text": "[メッセージが削除されました]"
    //            //        }
    //            //      ]
    //            //    },
    //            //    "externalChannelId": "UCo1q4cmG01Emu7LSuLzunzA"
    //            //  }
    //            //}}
    //            ret = null;
    //        }
    //        else
    //        {
    //            ret = null;
    //        }
    //        return ret;
    //    }

    //    private static IInternalMessage ParseLiveChatMembershipItemRenderer(dynamic ren)
    //    {
    //        var ahh = new InternalMembership
    //        {
    //            UserId = (string)ren.authorExternalChannelId,
    //            TimestampUsec = long.Parse((string)ren.timestampUsec),
    //            Id = (string)ren.id
    //        };

    //        //authorPhoto
    //        (ahh.ThumbnailUrl, ahh.ThumbnailWidth, ahh.ThumbnailHeight) = (ValueTuple<string, int, int>)GetThumbnail(ren.authorPhoto);
    //        //message
    //        if (ren.ContainsKey("headerSubtext"))
    //        {
    //            ahh.MessageItems = (List<IMessagePart>)ParseRunsOrSimpleText(ren.headerSubtext);
    //        }
    //        else if (ren.ContainsKey("headerPrimaryText") && ren.ContainsKey("message"))
    //        {
    //            var primaryText = (List<IMessagePart>)ParseRunsOrSimpleText(ren.headerPrimaryText);
    //            var message= (List<IMessagePart>)ParseRunsOrSimpleText(ren.message);
    //            var messageItems = new List<IMessagePart>();
    //            messageItems.AddRange(primaryText);
    //            messageItems.Add(Common.MessagePartFactory.CreateMessageText(Environment.NewLine));
    //            messageItems.AddRange(message);
    //            ahh.MessageItems = messageItems;
    //        }
         
    //        //name
    //        ahh.NameItems = (List<IMessagePart>)GetNameParts(ren);
    //        return ahh;
    //    }
    //}
    //class GetLiveChat
    //{
    //    private IContinuation _continuation;
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    /// <returns></returns>
    //    /// <exception cref="ChatUnavailableException"></exception>
    //    /// <exception cref="ContinuationNotExistsException"></exception>
    //    public IContinuation GetContinuation()
    //    {
    //        if (_continuation != null)
    //        {
    //            return _continuation;
    //        }
    //        if (!_d.ContainsKey("continuationContents"))
    //        {
    //            throw new ChatUnavailableException(Raw);
    //        }
    //        if (!_d.continuationContents.liveChatContinuation.ContainsKey("continuations"))
    //        {
    //            //2021/01/11 仕様変更後、こっちに来ることは無いかも。全部ChatUnavailableExceptionになる気がする。要検証。
    //            throw new ContinuationNotExistsException();
    //        }
    //        if (_d.continuationContents.liveChatContinuation.continuations[0].ContainsKey("timedContinuationData"))
    //        {
    //            var c = new TimedContinuation
    //            {
    //                Continuation = (string)_d.continuationContents.liveChatContinuation.continuations[0].timedContinuationData.continuation,
    //                TimeoutMs = (int)_d.continuationContents.liveChatContinuation.continuations[0].timedContinuationData.timeoutMs,
    //            };
    //            _continuation = c;
    //            return c;
    //        }
    //        else if (_d.continuationContents.liveChatContinuation.continuations[0].ContainsKey("invalidationContinuationData"))
    //        {
    //            //  {
    //            //    "invalidationContinuationData": {
    //            //      "invalidationId": {
    //            //        "objectSource": 1056,
    //            //        "objectId": "Y2hhdH5YM3gzb21jeERSa341MzY3MzM1",
    //            //        "topic": "chat~X3x3omcxDRk~5367335",
    //            //        "subscribeToGcmTopics": true,
    //            //        "protoCreationTimestampMs": "1610200735376"
    //            //      },
    //            //      "timeoutMs": 10000,
    //            //      "continuation": "0ofMyAOHAhqyAUNqZ0tEUW9MV0RONE0yOXRZM2hFVW1zcUp3b1lWVU5hYkVSWWVrZHZiemRrTkRSaWQyUk9UMkpHWVdObkVndFlNM2d6YjIxamVFUlNheHBEcXJuQnZRRTlDanRvZEhSd2N6b3ZMM2QzZHk1NWIzVjBkV0psTG1OdmJTOXNhWFpsWDJOb1lYUV9hWE5mY0c5d2IzVjBQVEVtZGoxWU0zZ3piMjFqZUVSU2F5QUNLQUUlM0Qoqq2rlIGP7gIwADgAQAJKGwgAEAAYACAAOgBAAEoAUPrSiML5ju4CWAN4AFDak9OUgY_uAlj5jbbq4Y3uAmgBggECCAGIAQCgAarK5ZaBj-4C"
    //            //    }
    //            //  }
    //            var i = new InvalidationContinuation
    //            {
    //                Continuation = (string)_d.continuationContents.liveChatContinuation.continuations[0].invalidationContinuationData.continuation,
    //                TimeoutMs = (int)_d.continuationContents.liveChatContinuation.continuations[0].invalidationContinuationData.timeoutMs,
    //            };
    //            return i;
    //        }
    //        else if (_d.continuationContents.liveChatContinuation.continuations[0].ContainsKey("reloadContinuationData"))
    //        {
    //            var r = new ReloadContinuation
    //            {
    //                Continuation = (string)_d.continuationContents.liveChatContinuation.continuations[0].reloadContinuationData.continuation,
    //            };
    //            _continuation = r;
    //            return r;
    //        }
    //        throw new SpecChangedException(Raw);
    //    }

    //    public List<IInternalMessage> GetActions()
    //    {
    //        var list = new List<IInternalMessage>();
    //        if (!_d.ContainsKey("continuationContents"))
    //        {
    //            return list;
    //        }
    //        if (!_d.continuationContents.liveChatContinuation.ContainsKey("actions"))
    //        {
    //            return list;
    //        }
    //        var actions = _d.continuationContents.liveChatContinuation.actions;
    //        foreach (var action in actions)
    //        {
    //            var message = (IInternalMessage)Parser2.ParseAction(action);
    //            if (message == null) continue;
    //            list.Add(message);
    //        }
    //        return list;
    //    }
    //    public string Raw { get; }
    //    public GetLiveChat(string json)
    //    {
    //        Raw = json;
    //        _d = JsonConvert.DeserializeObject(json);
    //        if (_d.ContainsKey("error"))
    //        {
    //            var message = (string)_d.error.message;
    //            throw new GetLiveChatException(message, json);
    //        }
    //    }
    //    private readonly dynamic _d;
    //}
    static class Tools
    {
        public static Input.IInput ParseInput(string input)
        {
            if (string.IsNullOrEmpty(input)) throw new ArgumentNullException(nameof(input));
            if (VidResolver.IsChannel(input))
            {
                return new Input.ChannelUrl(input);
            }
            else if (VidResolver.IsCustomChannel(input))
            {
                return new Input.CustomChannelUrl(input);
            }
            else if (VidResolver.IsStudio(input))
            {
                return new Input.StudioUrl(input);
            }
            else if (VidResolver.IsUser(input))
            {
                return new Input.UserUrl(input);
            }
            else if (VidResolver.IsVid(input))
            {
                return new Input.Vid(input);
            }
            else if (VidResolver.IsWatch(input))
            {
                return new Input.WatchUrl(input);
            }
            return new Input.InvalidInput(input);
        }
        public static string ToElapsedString(TimeSpan timeSpan)
        {
            var prefix = timeSpan.Ticks < 0 ? "-" : "";
            var days = Math.Abs(timeSpan.Days);
            var hours = Math.Abs(timeSpan.Hours);
            var mins = Math.Abs(timeSpan.Minutes);
            var secs = Math.Abs(timeSpan.Seconds);
            string ret;
            if (days <= 0)
            {
                ret = $"{hours:00}:{mins:00}:{secs:00}";
            }
            else
            {
                ret = $"{days}日{hours:00}:{mins:00}:{secs:00}";
            }
            return prefix + ret;
        }
        public static string GetSapiSid(CookieContainer cc)
        {
            var cookies = Tools.ExtractCookies(cc);
            var keys = new[] { "SAPISID", "APISID", "__Secure-3PAPISID", "SID" };
            foreach (var key in keys)
            {
                var cookie = cookies.Find(c => c.Name == key);
                if (cookie != null)
                {
                    return cookie.Value;
                }
            }
            return null;
        }
        public static YtInitialDataOld ExtractYtInitialData(string liveChatHtml)
        {
            if (string.IsNullOrEmpty(liveChatHtml))
            {
                throw new ArgumentNullException(nameof(liveChatHtml));
            }
            var match = Regex.Match(liveChatHtml, "window\\[\"ytInitialData\"\\] = ({.+?});");
            if (!match.Success)
            {
                throw new Exception("");
            }
            var raw = match.Groups[1].Value;
            if (string.IsNullOrEmpty(raw))
            {
                throw new SpecChangedException(liveChatHtml);
            }
            return new YtInitialDataOld(raw);
        }
        public static string ExtractYtCfg(string liveChatHtml)
        {
            var match = Regex.Match(liveChatHtml, "ytcfg\\.set\\(({.+?})\\);", RegexOptions.Singleline);
            if (!match.Success)
            {
                throw new Test2.ParseException(liveChatHtml);
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
        class GetGetLiveChatException : Exception
        {
            public GetGetLiveChatException(Exception innerException)
                : base("", innerException)
            {
            }

            public string Url { get; internal set; }
            public string DataToPost { get; internal set; }
            public bool IsLoggedIn { get; internal set; }
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
        public static async Task<GetLiveChat> GetGetLiveChat(DataToPost data, string innerTubeApiKey, CookieContainer cc, ILoginState loginInfo, ILogger logger)
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
                var hash = SapiSidHashGenerator.CreateHash(cc, DateTime.Now);
                client.DefaultRequestHeaders.Add("Authorization", $"SAPISIDHASH {hash}");
            }

            client.DefaultRequestHeaders.Add("Origin", "https://www.youtube.com");
            HttpResponseMessage k;
            try
            {
                k = await client.PostAsync(url, c);
            }
            catch (Exception ex)
            {
                throw new GetGetLiveChatException(ex)
                {
                    Url = url,
                    DataToPost = data.ToString(),
                    IsLoggedIn = loginInfo is LoggedIn,
                };
            }
            var s = await k.Content.ReadAsStringAsync();
            var getLiveChat = GetLiveChat.Parse(s);
            //var getLiveChat = new GetLiveChat(s);
            return getLiveChat;
        }
        /// <summary>
        /// CookieContainerから全てのCookieを取り出す
        /// https://stackoverflow.com/questions/13675154/how-to-get-cookies-info-inside-of-a-cookiecontainer-all-of-them-not-for-a-spe/36665793
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

                var httpAddress = string.Format("http://{0}/", domain);
                var httpsAddress = string.Format("https://{0}/", domain);

                if (Uri.TryCreate(httpAddress, UriKind.RelativeOrAbsolute, out var httpUri))
                {
                    foreach (Cookie cookie in container.GetCookies(httpUri))
                    {
                        cookies.Add(cookie);
                    }
                }
                if (Uri.TryCreate(httpsAddress, UriKind.RelativeOrAbsolute, out var httpsUri))
                {
                    foreach (Cookie cookie in container.GetCookies(httpsUri))
                    {
                        cookies.Add(cookie);
                    }
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
