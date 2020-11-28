using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using SitePlugin;
using Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Diagnostics;
using System.Text;
using Xceed.Wpf.Toolkit;

namespace YouTubeLiveSitePlugin.Test2
{
    class LiveChatContext
    {
        public string XsrfToken { get; set; }
        public bool IsLoggedIn { get; set; }
    }
    class ChatContinuation
    {
        public string AllChatContinuation { get; set; }
        public string JouiChatContinuation { get; set; }
    }
    static class Tools
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="timestampUsec"></param>
        /// <returns>UTC</returns>
        public static DateTime ParseTimestampUsec(long timestampUsec)
        {
            return new DateTime(timestampUsec * 10, DateTimeKind.Utc).AddTicks(new DateTime(1970, 1, 1).Ticks);
        }
        public static LiveChatContext GetLiveChatContext(string liveChatHtml)
        {
            var context = new LiveChatContext();

            var html = liveChatHtml;

            //XSRF_TOKEN
            {
                var match = Regex.Match(html, "\"XSRF_TOKEN\":\"([^\"]+)\"");
                if (match.Success)
                {
                    context.XsrfToken = match.Groups[1].Value;
                }
            }
            //LOGGED_IN
            {
                var match = Regex.Match(html, "\"LOGGED_IN\":([a-zA-Z]+)");
                if (match.Success)
                {
                    var s = match.Groups[1].Value;
                    context.IsLoggedIn = s == "true";
                }
            }
            return context;
        }
        public static PostCommentContext GetPostCommentContext(string ytInitialData)
        {
            if (!Tools.TryExtractSendButtonServiceEndpoint(ytInitialData, out string serviceEndpoint))
            {

            }
            throw new NotImplementedException();
        }
        public static bool TryExtractSendButtonServiceEndpoint(string ytInitialData, out string serviceEndPoint)
        {
            var arr = new[]
            {
                "contents",
                "liveChatRenderer",
                //"continuationContents",
                //"liveChatContinuation",
                "actionPanel",
                "liveChatMessageInputRenderer",
                "sendButton",
                "buttonRenderer",
                "serviceEndpoint",
            };
            var data = (JObject)JsonConvert.DeserializeObject(ytInitialData);
            var temp = data[arr[0]];
            for (int i = 1; i < arr.Length; i++)
            {
                var s = arr[i];
                temp = temp[s];
                if (temp == null)
                {
                    Debug.WriteLine("次の要素がない:" + s);
                    serviceEndPoint = null;
                    return false;
                }
            }
            serviceEndPoint = temp.ToString();
            return true;
        }
        public static bool TryGetVid(string input, out string vid)
        {
            if (Regex.IsMatch(input, "^[^/?=:]+$"))
            {
                vid = input;
                return true;
            }
            var match = Regex.Match(input, "youtube\\.com/watch\\?v=([^?=/]+)");
            if (match.Success)
            {
                vid = match.Groups[1].Value;
                return true;
            }
            vid = null;
            return false;
        }

        public static string ExtractYtcfg(string liveChatHtml)
        {
            var match = Regex.Match(liveChatHtml, "ytcfg\\.set\\(({.+?})\\);\r?\n", RegexOptions.Singleline);
            if (!match.Success)
            {
                throw new ParseException(liveChatHtml);
            }
            var ytCfg = match.Groups[1].Value;
            return ytCfg;
        }
        public static string ExtractYtInitialDataFromLiveChatHtml(string liveChatHtml)
        {
            var match = Regex.Match(liveChatHtml, "window\\[\"ytInitialData\"\\] = ({.+});\\s*</script>", RegexOptions.Singleline);
            if (!match.Success)
            {
                throw new ParseException(liveChatHtml);
            }
            var ytInitialData = match.Groups[1].Value;
            return ytInitialData;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="channelHtml"></param>
        /// <returns>成功したらYtInitialData,失敗したらnull</returns>
        private static string ExtractYtInitialDataFromChannelHtmlInternal1(string channelHtml)
        {
            //window["ytInitialData"] = JSON.parse("{\"responseContext\":{\"se
            var match = Regex.Match(channelHtml, "window\\[\"ytInitialData\"\\]\\s*=\\s*(.+?})(?:\"\\))?;", RegexOptions.Singleline);
            if (!match.Success)
            {
                return null;
            }
            var preYtInitialData = match.Groups[1].Value;
            string ytInitialData;
            if (preYtInitialData.StartsWith("JSON"))
            {
                var preJson = preYtInitialData.Substring(12);//先頭の"JSON.parse(\""を消す
                //preJsonは\や"がエスケープされた状態になっているため外す
                var sb = new StringBuilder(preJson);
                sb.Replace("\\\\", "\\");
                sb.Replace("\\\"", "\"");
                ytInitialData = sb.ToString();
            }
            else
            {
                ytInitialData = preYtInitialData;
            }
            return ytInitialData;
        }
        private static string ExtractYtInitialDataFromChannelHtmlInternal2(string channelHtml)
        {
            //2020/11/09 ttps://www.youtube.com/channel/CHANNEL_ID?view_as=subscriber のHTMLの仕様が通常のものと違った
            //<script nonce=\"orPyHr12z1j4Y/4tOnK12A\">var ytInitialData = {\"responseContext\":
            var match = Regex.Match(channelHtml, "<script nonce=\"[^\"]+\">var ytInitialData\\s*=\\s*(.+?});</script>", RegexOptions.Singleline);
            if (!match.Success)
            {
                return null;
            }
            var ytInitialData = match.Groups[1].Value;
            return ytInitialData;
        }
        public static string ExtractYtInitialDataFromChannelHtml(string channelHtml)
        {
            var ytInitialData = ExtractYtInitialDataFromChannelHtmlInternal1(channelHtml);
            if (!string.IsNullOrEmpty(ytInitialData)) return ytInitialData;

            ytInitialData = ExtractYtInitialDataFromChannelHtmlInternal2(channelHtml);
            if (!string.IsNullOrEmpty(ytInitialData)) return ytInitialData;

            throw new ParseException(channelHtml);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="s"></param>
        /// <exception cref="ChatUnavailableException"></exception>
        /// 
        /// <returns></returns>
        public static (IContinuation continuation, ChatContinuation, List<CommentData> actions) ParseYtInitialData(string s)
        {
            dynamic json = JsonConvert.DeserializeObject(s);
            if (!json.ContainsKey("contents"))
            {
                //"{\"responseContext\":{\"errors\":{\"error\":[{\"domain\":\"gdata.CoreErrorDomain\",\"code\":\"INVALID_VALUE\",\"debugInfo\":\"Error decrypting and parsing the live chat ID.\",\"externalErrorMessage\":\"不明なエラーです。\"}]},\"serviceTrackingParams\":[{\"service\":\"CSI\",\"params\":[{\"key\":\"GetLiveChat_rid\",\"value\":\"0x3365759ba77f978f\"},{\"key\":\"c\",\"value\":\"WEB\"},{\"key\":\"cver\",\"value\":\"2.20190529\"},{\"key\":\"yt_li\",\"value\":\"1\"}]},{\"service\":\"GFEEDBACK\",\"params\":[{\"key\":\"e\",\"value\":\"23720702,23736685,23744176,23750984,23751767,23752869,23755886,23755898,23759224,23766102,23767634,23771992,23785333,23788845,23793834,23794471,23799777,23804281,23804294,23805410,23806435,23808949,23809331,23810273,23811378,23811593,23812530,23812566,23813310,23813548,23813622,23813949,23814199,23814507,23815144,23815164,23815172,23815485,23815949,23817343,23817794,23817825,23818213,9407610,9441381,9449243,9471235\"},{\"key\":\"logged_in\",\"value\":\"1\"}]},{\"service\":\"GUIDED_HELP\",\"params\":[{\"key\":\"creator_channel_id\",\"value\":\"UCK6F1ecql0T_9hHGTw7heBA\"},{\"key\":\"logged_in\",\"value\":\"1\"}]},{\"service\":\"ECATCHER\",\"params\":[{\"key\":\"client.name\",\"value\":\"WEB\"},{\"key\":\"client.version\",\"value\":\"2.20190529\"},{\"key\":\"innertube.build.changelist\",\"value\":\"250485423\"},{\"key\":\"innertube.build.experiments.source_version\",\"value\":\"250547910\"},{\"key\":\"innertube.build.label\",\"value\":\"youtube.ytfe.innertube_20190528_7_RC1\"},{\"key\":\"innertube.build.timestamp\",\"value\":\"1559140061\"},{\"key\":\"innertube.build.variants.checksum\",\"value\":\"7e46d96e46a45788f840d135c2cf4890\"},{\"key\":\"innertube.run.job\",\"value\":\"ytfe-innertube-replica-only.ytfe\"}]}],\"webResponseContextExtensionData\":{\"ytConfigData\":{\"csn\":\"4wLwXOyiG5OPgAOH4LYI\",\"visitorData\":\"CgtpTXJTMXZJR3ZLayjjhcDnBQ%3D%3D\",\"sessionIndex\":1}}},\"trackingParams\":\"CAAQ0b4BIhMIrKDMwNTD4gIVkwdgCh0HsA0B\"}";
                throw new YouTubeLiveServerErrorException();
            }
            if (!json.contents.ContainsKey("liveChatRenderer"))
            {
                throw new ChatUnavailableException();
            }
            if (!json.contents.liveChatRenderer.ContainsKey("continuations"))
            {
                throw new ContinuationNotExistsException();
            }
            var chatContinuation = new ChatContinuation
            {
                AllChatContinuation = (string)json.contents.liveChatRenderer.header.liveChatHeaderRenderer.viewSelector.sortFilterSubMenuRenderer.subMenuItems[1].continuation.reloadContinuationData.continuation,
                JouiChatContinuation = (string)json.contents.liveChatRenderer.header.liveChatHeaderRenderer.viewSelector.sortFilterSubMenuRenderer.subMenuItems[0].continuation.reloadContinuationData.continuation,
            };

            IContinuation continuation;
            var lowContinuations = json.contents.liveChatRenderer.continuations;
            if (lowContinuations[0].ContainsKey("invalidationContinuationData"))
            {
                var data = lowContinuations[0].invalidationContinuationData;
                var inv = new InvalidationContinuation
                {
                    Continuation = data.continuation,
                    TimeoutMs = (int)data.timeoutMs,
                    ObjectId = data.invalidationId.objectId
                };
                continuation = inv;
            }
            else
            {
                var data = lowContinuations[0].timedContinuationData;
                var timed = new TimedContinuation()
                {
                    Continuation = data.continuation,
                    TimeoutMs = (int)data.timeoutMs,
                };
                continuation = timed;
            }

            var dataList = new List<CommentData>();
            if (json.contents.liveChatRenderer.ContainsKey("actions"))
            {
                foreach (var action in json.contents.liveChatRenderer.actions)
                {
                    try
                    {
                        if (action.ContainsKey("addChatItemAction"))
                        {
                            var item = action.addChatItemAction.item;
                            if (item.ContainsKey("liveChatTextMessageRenderer"))
                            {
                                var commentData = (CommentData)Parser.ParseLiveChatTextMessageRenderer(item.liveChatTextMessageRenderer);
                                commentData.Raw = action.ToString();
                                dataList.Add(commentData);
                            }
                            else if (item.ContainsKey("liveChatPaidMessageRenderer"))
                            {
                                var ren = item.liveChatPaidMessageRenderer;
                                var commentData = Parser.ParseLiveChatPaidMessageRenderer(ren);
                                commentData.Raw = action.ToString();
                                dataList.Add(commentData);
                            }
                        }
                    }
                    catch (ParseException ex)
                    {
                        throw new ParseException(s, ex);
                    }
                }
            }



            //var actions = lowLiveChat.contents.liveChatRenderer.actions;
            //var actionList = new List<IAction>();
            //foreach(var action in actions)
            //{
            //    if(action.addChatItemAction != null)
            //    {
            //        if(action.addChatItemAction.item.liveChatTextMessageRenderer != null)
            //        {
            //            actionList.Add(new TextMessage(action.addChatItemAction.item.liveChatTextMessageRenderer));
            //        }
            //        else if(action.addChatItemAction.item.liveChatPaidMessageRenderer != null)
            //        {
            //            actionList.Add(new PaidMessage(action.addChatItemAction.item.liveChatPaidMessageRenderer));
            //        }
            //    }
            //}
            return (continuation, chatContinuation, dataList);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ytPlayerConfig">ExtractYtPlayerConfig(string)の戻り値</param>
        /// <returns>{"isLiveNow":true,"startTimestamp":"2020-11-12T12:16:53+00:00"}</returns>
        public static string ExtractLiveBroadcastDetails(string ytPlayerConfig)
        {
            if (string.IsNullOrEmpty(ytPlayerConfig)) return null;
            var match = Regex.Match(ytPlayerConfig, "\\\\\"liveBroadcastDetails\\\\\":({.+?})");
            if (!match.Success) return null;
            return match.Groups[1].Value.Replace("\\\"", "\"");
        }

        public static string ExtractYtPlayerConfig(string html)
        {
            var match = Regex.Match(html, "ytplayer\\.config\\s*=\\s*({.+?\"}});");
            if (!match.Success) return null;
            return match.Groups[1].Value;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="html"></param>
        /// <returns>{"isLiveNow":true,"startTimestamp":"2020-11-12T14:02:34+00:00"}</returns>
        public static string ExtractLiveBroadcastDetailsFromLivePage(string html)
        {
            //"liveBroadcastDetails":{"isLiveNow":true,"startTimestamp":"2020-11-12T14:02:34+00:00"}
            //liveBroadcastDetailsはページによってytPlayerConfigに入っている場合とytInitialPlayerResponseの場合を確認。
            //あんまり検証していないから詳細は不明。
            //ytplayerconfigの場合、liveBroadcastDetailsを要素に持つJSON自体が文字列としてJSONの値に格納されている関係で\"liveBroadcastDetails\"のようにエスケープされている。            

            //ちなみにHTMLのmetaタグでも配信開始日時が格納されていた。
            ////<meta itemprop="startDate" content="2020-11-12T14:02:34+00:00">

            var match = Regex.Match(html, "(?:\\\\)?\"liveBroadcastDetails(?:\\\\)?\":({.+?})");
            if (!match.Success) return null;
            return match.Groups[1].Value.Replace("\\\"", "\"");
        }
        public static string ToElapsedString(TimeSpan timeSpan)
        {
            var days = timeSpan.Days;
            var hours = timeSpan.Hours;
            var mins = timeSpan.Minutes;
            var secs = timeSpan.Seconds;
            string ret;
            if (days <= 0)
            {
                ret = $"{hours:00}:{mins:00}:{secs:00}";
            }
            else
            {
                ret = $"{days}日{hours:00}:{mins:00}:{secs:00}";
            }
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="getLiveChatJson"></param>
        /// <returns></returns>
        /// <exception cref="ContinuationContentsNullException"></exception>
        /// <exception cref="NoContinuationException">放送終了</exception>
        public static (IContinuation, List<CommentData>, string sessionToken) ParseGetLiveChat(string getLiveChatJson)
        {
            try
            {
                dynamic json = JsonConvert.DeserializeObject(getLiveChatJson);
                if (!json.response.ContainsKey("continuationContents"))
                {
                    throw new ContinuationContentsNullException();
                }

                if (!json.response.continuationContents.liveChatContinuation.ContainsKey("continuations"))
                {
                    throw new ContinuationNotExistsException();
                }

                IContinuation continuation;
                var continuations = json.response.continuationContents.liveChatContinuation.continuations;
                if (continuations[0].ContainsKey("invalidationContinuationData"))
                {
                    var invalidation = continuations[0].invalidationContinuationData;
                    var inv = new InvalidationContinuation
                    {
                        Continuation = invalidation.continuation,
                        TimeoutMs = (int)invalidation.timeoutMs,
                        ObjectId = invalidation.invalidationId.objectId,
                        ObjectSource = (int)invalidation.invalidationId.objectSource,
                        ProtoCreationTimestampMs = invalidation.invalidationId.protoCreationTimestampMs
                    };
                    continuation = inv;
                }
                else if (continuations[0].ContainsKey("timedContinuationData"))
                {
                    var timed = continuations[0].timedContinuationData;
                    var inv = new TimedContinuation
                    {
                        Continuation = timed.continuation,
                        TimeoutMs = (int)timed.timeoutMs,
                    };
                    continuation = inv;
                }
                else if (continuations[0].ContainsKey("reloadContinuationData"))
                {
                    var reload = continuations[0].reloadContinuationData;
                    var inv = new ReloadContinuation
                    {
                        Continuation = reload.continuation,
                    };
                    continuation = inv;
                }
                else
                {
                    throw new ParseException(getLiveChatJson);
                }

                var dataList = new List<CommentData>();
                if (json.response.continuationContents.liveChatContinuation.ContainsKey("actions"))
                {
                    var actions = json.response.continuationContents.liveChatContinuation.actions;
                    foreach (var action in actions)
                    {
                        if (action.ContainsKey("addChatItemAction"))
                        {
                            var item = action.addChatItemAction.item;
                            if (item.ContainsKey("liveChatTextMessageRenderer"))
                            {
                                dataList.Add(Parser.ParseLiveChatTextMessageRenderer(item.liveChatTextMessageRenderer));
                            }
                            else if (item.ContainsKey("liveChatPaidMessageRenderer"))
                            {
                                var ren = item.liveChatPaidMessageRenderer;
                                var commentData = Parser.ParseLiveChatPaidMessageRenderer(ren);
                                dataList.Add(commentData);
                            }
                        }
                    }
                }
                var sessionToken = json.xsrf_token;
                //var actions = lowLiveChat.response.continuationContents.liveChatContinuation.actions;
                //var actionList = new List<IAction>();
                //if (actions != null)
                //{
                //    foreach (var action in actions)
                //    {
                //        if (action.addChatItemAction != null)
                //        {
                //            if (action.addChatItemAction.item.liveChatTextMessageRenderer != null)
                //            {
                //                actionList.Add(new TextMessage(action.addChatItemAction.item.liveChatTextMessageRenderer));
                //            }
                //            else if (action.addChatItemAction.item.liveChatPaidMessageRenderer != null)
                //            {
                //                actionList.Add(new PaidMessage(action.addChatItemAction.item.liveChatPaidMessageRenderer));
                //            }
                //        }
                //    }
                //}
                //return (continuation, actionList);
                return (continuation, dataList, sessionToken);
            }
            catch (Microsoft.CSharp.RuntimeBinder.RuntimeBinderException ex)
            {
                throw new ParseException(getLiveChatJson, ex);
            }
        }

    }
    static class Parser
    {
        public static CommentData ParseLiveChatPaidMessageRenderer(string renderer)
        {
            dynamic d = JsonConvert.DeserializeObject(renderer);
            return ParseLiveChatPaidMessageRenderer(d);
        }
        public static CommentData ParseLiveChatPaidMessageRenderer(dynamic liveChatPaidMessageRenderer)
        {
            var commentData = Parser.ParseLiveChatTextMessageRenderer(liveChatPaidMessageRenderer);
            var purchaseAmount = (string)liveChatPaidMessageRenderer.purchaseAmountText.simpleText;
            commentData.PurchaseAmount = purchaseAmount;
            return commentData;
        }
        public static CommentData ParseLiveChatTextMessageRenderer(string renderer)
        {
            var d = JsonConvert.DeserializeObject(renderer);
            return ParseLiveChatTextMessageRenderer(d);
        }
        public static CommentData ParseLiveChatTextMessageRenderer(dynamic ren)
        {
            var commentData = new CommentData
            {
                UserId = ren.authorExternalChannelId,
                TimestampUsec = long.Parse((string)ren.timestampUsec),
                Id = ren.id
            };
            //authorPhoto
            {
                var thumbnail = ren.authorPhoto.thumbnails[0];
                var url = thumbnail.url;
                var width = (int)thumbnail.width;
                var height = (int)thumbnail.height;
                var authorPhoto = new MessageImage { Url = url, Width = width, Height = height };
                commentData.Thumbnail = authorPhoto;
            }
            //message
            {
                var messageItems = new List<IMessagePart>();
                commentData.MessageItems = messageItems;
                if (ren.ContainsKey("message"))//PaidMessageではコメント無しも可能
                {
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
                }
            }
            //name
            {
                var nameItems = new List<IMessagePart>();
                commentData.NameItems = nameItems;
                nameItems.Add(MessagePartFactory.CreateMessageText((string)ren.authorName.simpleText));
                if (ren.ContainsKey("authorBadges"))
                {
                    foreach (var badge in ren.authorBadges)
                    {
                        if (badge.liveChatAuthorBadgeRenderer.ContainsKey("customThumbnail"))
                        {
                            var url = badge.liveChatAuthorBadgeRenderer.customThumbnail.thumbnails[0].url;
                            var alt = badge.liveChatAuthorBadgeRenderer.tooltip;
                            nameItems.Add(new MessageImage { Url = url, Alt = alt, Width = 16, Height = 16 });
                        }
                        else if (badge.liveChatAuthorBadgeRenderer.ContainsKey("icon"))
                        {
                            var iconType = badge.liveChatAuthorBadgeRenderer.icon.iconType;
                            var alt = badge.liveChatAuthorBadgeRenderer.tooltip;
                        }
                        else
                        {
                            throw new ParseException();
                        }
                    }
                }
            }
            return commentData;
        }
    }
}
