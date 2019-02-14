using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Codeplex.Data;
using SitePlugin;
using Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Diagnostics;

namespace YouTubeLiveSitePlugin.Test2
{
    class LiveChatContext
    {
        public string XsrfToken { get; set; }
        public bool IsLoggedIn { get; set; }
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
            for(int i = 1;i<arr.Length;i++)
            {
                var s = arr[i];
                temp = temp[s];
                if(temp == null)
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
            //正規表現だけだとうまくいかないから、まずメインのytcfgのケツ以降を切り捨てる
            var n = liveChatHtml.IndexOf("});\n");
            var sub = liveChatHtml.Substring(0, n+5);
            var match = Regex.Match(sub, "ytcfg\\.set\\(({.+})\\);", RegexOptions.Singleline);
            if (!match.Success)
            {
                throw new ParseException(liveChatHtml);
            }
            var ytCfg = match.Groups[1].Value;
            return ytCfg;
        }
        public static string ExtractYtInitialData(string liveChatHtml)
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
        /// <param name="s"></param>
        /// <exception cref="ChatUnavailableException"></exception>
        /// 
        /// <returns></returns>
        public static (IContinuation continuation, List<CommentData> actions) ParseYtInitialData(string s)
        {
            var json = DynamicJson.Parse(s);
            if (!json.contents.IsDefined("liveChatRenderer"))
            {
                throw new ChatUnavailableException();
            }
            if (!json.contents.liveChatRenderer.IsDefined("continuations"))
            {
                throw new ContinuationNotExistsException();
            }

            IContinuation continuation;
            var lowContinuations = json.contents.liveChatRenderer.continuations;
            if (lowContinuations[0].IsDefined("invalidationContinuationData"))
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
            if (json.contents.liveChatRenderer.IsDefined("actions"))
            {
                foreach(var action in json.contents.liveChatRenderer.actions)
                {
                    try
                    {
                        if (action.IsDefined("addChatItemAction"))
                        {
                            var item = action.addChatItemAction.item;
                            if (item.IsDefined("liveChatTextMessageRenderer"))
                            {
                                var commentData = (CommentData)Parser.ParseLiveChatTextMessageRenderer(item.liveChatTextMessageRenderer);
                                commentData.Raw = action.ToString();
                                dataList.Add(commentData);
                            }
                            else if (item.IsDefined("liveChatPaidMessageRenderer"))
                            {
                                var ren = item.liveChatPaidMessageRenderer;
                                var commentData = Parser.ParseLiveChatPaidMessageRenderer(ren);
                                commentData.Raw = action.ToString();
                                dataList.Add(commentData);
                            }
                        }
                    }catch(ParseException ex)
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
            return (continuation, dataList);
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
            var json = DynamicJson.Parse(getLiveChatJson);
            if (!json.response.IsDefined("continuationContents"))
            {
                throw new ContinuationContentsNullException();
            }
            
            if (!json.response.continuationContents.liveChatContinuation.IsDefined("continuations"))
            {
                throw new ContinuationNotExistsException();
            }

            IContinuation continuation;
            var continuations = json.response.continuationContents.liveChatContinuation.continuations;
            if (continuations[0].IsDefined("invalidationContinuationData"))
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
            else
            {
                var timed = continuations[0].timedContinuationData;
                var inv = new TimedContinuation
                {
                    Continuation = timed.continuation,
                    TimeoutMs = (int)timed.timeoutMs,
                };
                continuation = inv;
            }

            var dataList = new List<CommentData>();
            if (json.response.continuationContents.liveChatContinuation.IsDefined("actions"))
            {
                var actions = json.response.continuationContents.liveChatContinuation.actions;
                foreach (var action in actions)
                {
                    if (action.IsDefined("addChatItemAction"))
                    {
                        var item = action.addChatItemAction.item;
                        if (item.IsDefined("liveChatTextMessageRenderer"))
                        {
                            dataList.Add(Parser.ParseLiveChatTextMessageRenderer(item.liveChatTextMessageRenderer));
                        }
                        else if (item.IsDefined("liveChatPaidMessageRenderer"))
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

    }
    static class Parser
    {
        public static CommentData ParseLiveChatPaidMessageRenderer(string renderer)
        {
            var d = DynamicJson.Parse(renderer);
            return ParseLiveChatPaidMessageRenderer(d);
        }
        public static CommentData ParseLiveChatPaidMessageRenderer(dynamic liveChatPaidMessageRenderer)
        {
            var commentData = Parser.ParseLiveChatTextMessageRenderer(liveChatPaidMessageRenderer);
            var purchaseAmount = liveChatPaidMessageRenderer.purchaseAmountText.simpleText;
            commentData.PurchaseAmount = purchaseAmount;
            return commentData;
        }
        public static CommentData ParseLiveChatTextMessageRenderer(string renderer)
        {
            var d = DynamicJson.Parse(renderer);
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
                if (ren.IsDefined("message"))//PaidMessageではコメント無しも可能
                {
                    if (ren.message.IsDefined("runs"))
                    {
                        foreach (var r in ren.message.runs)
                        {
                            if (r.IsDefined("text"))
                            {
                                var text = r.text;
                                messageItems.Add(MessagePartFactory.CreateMessageText(text));
                            }
                            else if (r.IsDefined("emoji"))
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
                    else if (ren.message.IsDefined("simpleText"))
                    {
                        var text = ren.message.simpleText;
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
                nameItems.Add(MessagePartFactory.CreateMessageText(ren.authorName.simpleText));
                if (ren.IsDefined("authorBadges"))
                {
                    foreach (var badge in ren.authorBadges)
                    {
                        if (badge.liveChatAuthorBadgeRenderer.IsDefined("customThumbnail"))
                        {
                            var url = badge.liveChatAuthorBadgeRenderer.customThumbnail.thumbnails[0].url;
                            var alt = badge.liveChatAuthorBadgeRenderer.tooltip;
                            nameItems.Add(new MessageImage { Url = url, Alt = alt, Width = 16, Height = 16 });
                        }
                        else if (badge.liveChatAuthorBadgeRenderer.IsDefined("icon"))
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
