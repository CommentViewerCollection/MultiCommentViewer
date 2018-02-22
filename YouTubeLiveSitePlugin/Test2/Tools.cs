using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Codeplex.Data;
using SitePlugin;
using Common;
namespace YouTubeLiveSitePlugin.Test2
{
    static class Tools
    {
        public static bool TryGetVid(string input, out string vid)
        {
            if (Regex.IsMatch(input, "^[^/?=:]+$"))
            {
                vid = input;
                return true;
            }
            var match = Regex.Match(input, "youtube\\.com/watch?v=([^?=/]+)");
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
                throw new ParseException("仕様変更？");
            }
            var ytCfg = match.Groups[1].Value;
            return ytCfg;
        }
        public static string ExtractYtInitialData(string liveChatHtml)
        {
            var match = Regex.Match(liveChatHtml, "window\\[\"ytInitialData\"\\] = ({.+});\\s*</script>", RegexOptions.Singleline);
            if (!match.Success)
            {
                throw new Exception("仕様変更？");
            }
            var ytInitialData = match.Groups[1].Value;
            return ytInitialData;
        }
        public static (IContinuation continuation, List<CommentData> actions) ParseYtInitialData(string s)
        {
            var json = DynamicJson.Parse(s);
            if (!json.contents.liveChatRenderer.IsDefined("continuations"))
            {
                throw new Exception("Continuations無し");
            }

            IContinuation continuation;
            var lowContinuations = json.contents.liveChatRenderer.continuations;
            if (lowContinuations[0].IsDefined("invalidationContinuationData"))
            {
                var data = lowContinuations[0].invalidationContinuationData;
                var inv = new InvalidationContinuation();
                inv.Continuation = data.continuation;
                inv.TimeoutMs = (int)data.timeoutMs;
                inv.ObjectId = data.invalidationId.objectId;
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
                    if (action.IsDefined("addChatItemAction"))
                    {
                        var item = action.addChatItemAction.item;
                        if (item.IsDefined("liveChatTextMessageRenderer"))
                        {
                            dataList.Add(GetCommandData(item.liveChatTextMessageRenderer));
                        }
                        else if (item.IsDefined("liveChatPaidMessageRenderer"))
                        {
                            var ren = item.liveChatPaidMessageRenderer;
                            var commentData = GetCommandData(ren);
                            var purchaseAmount = ren.purchaseAmountText.simpleText;
                            commentData.MessageItems.Insert(0, new MessageText(purchaseAmount));
                            dataList.Add(commentData);
                        }
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

        public static CommentData GetCommandData(dynamic ren)
        {
            var commentData = new CommentData();
            commentData.UserId = ren.authorExternalChannelId;
            commentData.TimestampUsec = ren.timestampUsec;
            commentData.Id = ren.id;
            //authorPhoto
            {
                var authorPhoto = new List<IMessagePart>();
                var thumbnail = ren.authorPhoto.thumbnails[0];
                var url = thumbnail.url;
                var width = (int)thumbnail.width;
                var height = (int)thumbnail.height;
                authorPhoto.Add(new MessageImage { Url = url, Width = width, Height = height });
            }
            //message
            {
                var messageItems = new List<IMessagePart>();
                commentData.MessageItems = messageItems;
                if (ren.message.IsDefined("runs"))
                {
                    foreach (var r in ren.message.runs)
                    {
                        if (r.IsDefined("text"))
                        {
                            var text = r.text;
                            messageItems.Add(new MessageText(text));
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
                else
                {
                    throw new ParseException();
                }
            }
            //name
            {
                var nameItems = new List<IMessagePart>();
                commentData.NameItems = nameItems;
                nameItems.Add(new MessageText(ren.authorName.simpleText));
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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="getLiveChatJson"></param>
        /// <returns></returns>
        /// <exception cref="ContinuationContentsNullException"></exception>
        /// <exception cref="NoContinuationException">放送終了</exception>
        public static (IContinuation, List<CommentData>) ParseGetLiveChat(string getLiveChatJson)
        {
            var json = DynamicJson.Parse(getLiveChatJson);
            if (!json.response.IsDefined("continuationContents"))
            {
                throw new ContinuationContentsNullException();
            }
            
            if (!json.response.continuationContents.liveChatContinuation.IsDefined("continuations"))
            {
                throw new NoContinuationException();
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
                            dataList.Add(GetCommandData(item.liveChatTextMessageRenderer));
                        }
                        else if (item.IsDefined("liveChatPaidMessageRenderer"))
                        {
                            var ren = item.liveChatPaidMessageRenderer;
                            var commentData = GetCommandData(ren);
                            var purchaseAmount = ren.purchaseAmountText.simpleText;
                            commentData.MessageItems.Insert(0, new MessageText(purchaseAmount));
                            dataList.Add(commentData);
                        }
                    }
                }
            }
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
            return (continuation, dataList);
        }
    }
}
