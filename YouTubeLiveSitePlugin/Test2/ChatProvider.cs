using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using System.Threading;
using Codeplex.Data;
using SitePlugin;
using Common;

namespace YouTubeLiveSitePlugin.Test2
{
    [Serializable]
    public class ReloadException : Exception
    {
        public ReloadException() { }
    }
    [Serializable]
    public class ContinuationContentsNullException : Exception
    {
        public ContinuationContentsNullException() { }
    }

    [Serializable]
    public class ParseException : Exception
    {
        public ParseException() { }
        public ParseException(string message) : base(message) { }
        public ParseException(string message, Exception inner) : base(message, inner) { }
    }

    [Serializable]
    public class NoContinuationException : Exception
    {
        public NoContinuationException() { }
    }
    class ChatProvider
    {
        public event EventHandler<List<CommentData>> InitialActionsReceived;
        public event EventHandler<List<CommentData>> ActionsReceived;
        CancellationTokenSource _cts;
        private readonly ILogger _logger;

        public void Disconnect()
        {
            if(_cts != null)
            {
                _cts.Cancel();
            }
        }
        public async Task ReceiveAsync(string vid, CookieContainer cc)
        {
            _cts = new CancellationTokenSource();
            var reconnectCount = 0;
reconnect:
            var wc = new MyWebClient(cc);
            wc.Headers["User-Agent"] = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/100.0.2924.87 Safari/537.36";
            var liveChatUrl = $"https://www.youtube.com/live_chat?v={vid}&is_popout=1";
            var bytes =await wc.DownloadDataTaskAsync(liveChatUrl);
            var liveChatHtml = Encoding.UTF8.GetString(bytes);
            var ytInitialDataJson = Tools.ExtractYtInitialData(liveChatHtml);
            var (initialContinuation, initialCommentData) = Tools.ParseYtInitialData(ytInitialDataJson);
            if (reconnectCount == 0)//InitialActionsを送るのは最初だけ
            {
                InitialActionsReceived?.Invoke(this, initialCommentData);
            }

            var continuation = initialContinuation;
            while (!_cts.IsCancellationRequested)
            {
                var getLiveChatUrl = $"https://www.youtube.com/live_chat/get_live_chat?continuation={continuation.Continuation}&pbj=1";
                wc.Headers["User-Agent"] = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/100.0.2924.87 Safari/537.36";
                string getLiveChatJson = null;
                try
                {
                    var getLiveChatBytes = await wc.DownloadDataTaskAsync(getLiveChatUrl);
                    getLiveChatJson = Encoding.UTF8.GetString(getLiveChatBytes);
                    var (c, a) = Tools.ParseGetLiveChat(getLiveChatJson);
                    continuation = c;
                    if (a.Count > 0)
                    {
                        if (c is ITimedContinuation timed)
                        {
                            var interval = c.TimeoutMs / a.Count;
                            foreach (var action in a)
                            {
                                ActionsReceived?.Invoke(this, new List<CommentData> { action });
                                await Task.Delay(interval, _cts.Token);
                            }
                        }
                        else
                        {
                            ActionsReceived?.Invoke(this, a);
                            await Task.Delay(1000, _cts.Token);
                        }
                    }
                    else
                    {
                        await Task.Delay(1000);
                    }
                    
                }
                catch(ParseException ex)
                {
                    _logger.LogException(ex, "get_live_chatのパースに失敗", getLiveChatJson);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
                catch (ReloadException)
                {
                    reconnectCount++;
                    if(!_cts.IsCancellationRequested)
                        goto reconnect;
                }
                catch (ContinuationContentsNullException)
                {
                    //放送終了
                    break;
                }
                catch (NoContinuationException)
                {
                    break;
                }
            }
        }
        public ChatProvider(ILogger logger)
        {
            _logger = logger;
        }
    }
    class CommentData
    {
        public List<IMessagePart> MessageItems { get; set; }
        public List<IMessagePart> NameItems { get; set; }
        public string UserId { get; set; }
        public string TimestampUsec { get; internal set; }
        public string Id { get; internal set; }
    }
    static class Tools
    {
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
            
            var actionList = new List<IAction>();
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
    interface IContinuation
    {
        string Continuation { get; }
        int TimeoutMs { get; }
    }
    interface ITimedContinuation: IContinuation
    {
    }
    interface IInvalidationContinuation : IContinuation
    {
        int ObjectSource { get; }
        string ProtoCreationTimestampMs { get; }
        string ObjectId { get; }
    }
    public class TimedContinuation : ITimedContinuation
    {
        public TimedContinuation() { }
        public TimedContinuation(Low.LiveChat.TimedContinuationData data)
        {
            Continuation = data.continuation;
            TimeoutMs = data.timeoutMs;
        }
        public TimedContinuation(Low.GetLiveChat.TimedContinuationData data)
        {
            Continuation = data.continuation;
            TimeoutMs = data.timeoutMs;
        }

        public string Continuation { get; set; }
        public int TimeoutMs { get; set; }
    }
    public class InvalidationContinuation : IInvalidationContinuation
    {
        public InvalidationContinuation() { }
        public InvalidationContinuation(Low.LiveChat.InvalidationContinuationData data)
        {
            Continuation = data.continuation;
            TimeoutMs = data.timeoutMs;
            ObjectSource = data.invalidationId.objectSource;
            ProtoCreationTimestampMs = data.invalidationId.protoCreationTimestampMs;
            ObjectId = data.invalidationId.objectId;
        }
        public InvalidationContinuation(Low.GetLiveChat.InvalidationContinuationData data)
        {
            Continuation = data.continuation;
            TimeoutMs = data.timeoutMs;
            ObjectSource = data.invalidationId.objectSource;
            ProtoCreationTimestampMs = data.invalidationId.protoCreationTimestampMs;
            ObjectId = data.invalidationId.objectId;
        }

        public string Continuation { get; set; }
        public int TimeoutMs { get; set; }
        public int ObjectSource { get; set; }
        public string ProtoCreationTimestampMs { get; set; }
        public string ObjectId { get; set; }
    }
    class Photo
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public string Url { get; set; }
    }
    public class Badge
    {
        public string Url { get; set; }
        public string Alt { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }
    interface IAction { }
    interface ITextMessage:IAction
    {
        string Id { get; }
        Photo Thumbnail { get; }
        string Name { get; }
        long TimestampUsec { get; }
        string Message { get; }
        string UserId { get; }
        List<Badge> Badges { get; }
    }
    interface IPaidMessage: IAction
    {
        string Id { get; }
        Photo Thumbnail { get; }
        string Name { get; }
        long TimestampUsec { get; }
        string Message { get; }
        string UserId { get; }
        List<Badge> Badges { get; }
        string PurchaseAmount { get; }

        //背景色とかもあるけど後回し
    }

    class TextMessage : ITextMessage
    {
        public string Id { get; set; }

        public Photo Thumbnail { get; set; }

        public string Name { get; set; }

        public long TimestampUsec { get; set; }

        public string Message { get; set; }

        public string UserId { get; set; }

        public List<Badge> Badges { get; set; } = new List<Badge>();
        public TextMessage(Low.GetLiveChat.LiveChatTextMessageRenderer textMessage)
        {
            if (textMessage == null)
            {
                throw new ArgumentNullException(nameof(textMessage));
            }
            Id = textMessage.id;
            if (textMessage.authorPhoto.thumbnails.Count > 0)
            {
                var thumbnail = textMessage.authorPhoto.thumbnails[0];
                Thumbnail = new Photo { Url = thumbnail.url, Height = thumbnail.height, Width = thumbnail.width };
            }
            foreach (var r in textMessage.message.runs)
            {
                Message += r.text;
            }
            UserId = textMessage.authorExternalChannelId;
            Name = textMessage.authorName.simpleText;
            TimestampUsec = long.Parse(textMessage.timestampUsec);
            if (textMessage.authorBadges != null && textMessage.authorBadges.Count > 0)
            {
                foreach (var badge in textMessage.authorBadges)
                {
                    if (badge.liveChatAuthorBadgeRenderer.customThumbnail != null)
                    {
                        var url = badge.liveChatAuthorBadgeRenderer.customThumbnail.thumbnails[0].url;
                        var alt = badge.liveChatAuthorBadgeRenderer.tooltip;
                        Badges.Add(new Badge { Url = url, Alt = alt, Width = 16, Height = 16 });
                    }
                    else
                    {
                        //var iconType = badge.liveChatAuthorBadgeRenderer.icon.iconType;
                        var alt = badge.liveChatAuthorBadgeRenderer.tooltip;
                        //TODO:どうやって画像を取る？
                    }
                }
            }
        }

        public TextMessage(Low.LiveChat.LiveChatTextMessageRenderer textMessage)
        {
            if (textMessage == null)
            {
                throw new ArgumentNullException(nameof(textMessage));
            }
            Id = textMessage.id;
            if(textMessage.authorPhoto.thumbnails.Count > 0)
            {
                var thumbnail = textMessage.authorPhoto.thumbnails[0];
                Thumbnail = new Photo { Url = thumbnail.url, Height = thumbnail.height, Width = thumbnail.width };
            }
            foreach(var r in textMessage.message.runs)
            {
                Message += r.text;
            }
            UserId = textMessage.authorExternalChannelId;
            Name = textMessage.authorName.simpleText;
            TimestampUsec = long.Parse(textMessage.timestampUsec);
            if (textMessage.authorBadges != null && textMessage.authorBadges.Count > 0)
            {
                foreach (var badge in textMessage.authorBadges)
                {
                    if (badge.liveChatAuthorBadgeRenderer.customThumbnail != null)
                    {
                        var url = badge.liveChatAuthorBadgeRenderer.customThumbnail.thumbnails[0].url;
                        var alt = badge.liveChatAuthorBadgeRenderer.tooltip;
                        Badges.Add(new Badge { Url = url, Alt = alt, Width = 16, Height = 16 });
                    }
                    else
                    {
                        var iconType = badge.liveChatAuthorBadgeRenderer.icon.iconType;
                        var alt = badge.liveChatAuthorBadgeRenderer.tooltip;
                        //TODO:どうやって画像を取る？
                    }
                }
            }
        }
    }
    class PaidMessage : IPaidMessage
    {
        public string Id { get; set; }
        public Photo Thumbnail { get; set; }
        public string Name { get; set; }
        public long TimestampUsec { get; set; }
        public string Message { get; set; }
        public string UserId { get; set; }
        public List<Badge> Badges { get; set; } = new List<Badge>();
        public string PurchaseAmount { get; set; }
        public PaidMessage(Low.GetLiveChat.LiveChatPaidMessageRenderer paidMessage)
        {
            if (paidMessage == null)
            {
                throw new ArgumentNullException(nameof(paidMessage));
            }
            Id = paidMessage.id;
            if (paidMessage.authorPhoto.thumbnails.Count > 0)
            {
                var thumbnail = paidMessage.authorPhoto.thumbnails[0];
                Thumbnail = new Photo { Url = thumbnail.url, Height = thumbnail.height, Width = thumbnail.width };
            }
            foreach (var r in paidMessage.message.runs)
            {
                Message += r.text;
            }
            UserId = paidMessage.authorExternalChannelId;
            //Name = paidMessage.authorName.simpleText;
            TimestampUsec = long.Parse(paidMessage.timestampUsec);
            if (paidMessage.authorBadges != null && paidMessage.authorBadges.Count > 0)
            {
                foreach (var badge in paidMessage.authorBadges)
                {
                    if (badge.liveChatAuthorBadgeRenderer.customThumbnail != null)
                    {
                        var url = badge.liveChatAuthorBadgeRenderer.customThumbnail.thumbnails[0].url;
                        var alt = badge.liveChatAuthorBadgeRenderer.tooltip;
                        Badges.Add(new Badge { Url = url, Alt = alt, Width = 16, Height = 16 });
                    }
                    else
                    {
                        //var iconType = badge.liveChatAuthorBadgeRenderer.icon.iconType;
                        var alt = badge.liveChatAuthorBadgeRenderer.tooltip;
                        //TODO:どうやって画像を取る？
                    }
                }
            }
            foreach (var r in paidMessage.purchaseAmountText.runs)
            {
                PurchaseAmount += r.text;
            }
        }
        public PaidMessage(Low.LiveChat.LiveChatPaidMessageRenderer paidMessage)
        {
            if (paidMessage == null)
            {
                throw new ArgumentNullException(nameof(paidMessage));
            }
            Id = paidMessage.id;
            if (paidMessage.authorPhoto.thumbnails.Count > 0)
            {
                var thumbnail = paidMessage.authorPhoto.thumbnails[0];
                Thumbnail = new Photo { Url = thumbnail.url, Height = thumbnail.height, Width = thumbnail.width };
            }
            foreach (var r in paidMessage.message.runs)
            {
                Message += r.text;
            }
            UserId = paidMessage.authorExternalChannelId;
            //Name = paidMessage.authorName.simpleText;
            TimestampUsec = long.Parse(paidMessage.timestampUsec);
            if (paidMessage.authorBadges != null && paidMessage.authorBadges.Count > 0)
            {
                foreach (var badge in paidMessage.authorBadges)
                {
                    if (badge.liveChatAuthorBadgeRenderer.customThumbnail != null)
                    {
                        var url = badge.liveChatAuthorBadgeRenderer.customThumbnail.thumbnails[0].url;
                        var alt = badge.liveChatAuthorBadgeRenderer.tooltip;
                        Badges.Add(new Badge { Url = url, Alt = alt, Width = 16, Height = 16 });
                    }
                    else
                    {
                        var iconType = badge.liveChatAuthorBadgeRenderer.icon.iconType;
                        var alt = badge.liveChatAuthorBadgeRenderer.tooltip;
                        //TODO:どうやって画像を取る？
                    }
                }
            }
            foreach (var r in paidMessage.purchaseAmountText.runs)
            {
                PurchaseAmount += r.text;
            }
        }
    }
}
