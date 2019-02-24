using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Codeplex.Data;
using Newtonsoft.Json;
using SitePlugin;

namespace WhowatchSitePlugin
{
    class ItemNameResolver
    {
        private readonly Dictionary<long, PlayItem> _itemNameDict;

        public string Resolve(int play_item_pattern_id)
        {
            if(_itemNameDict.TryGetValue(play_item_pattern_id, out PlayItem item))
            {
                return item.Name;
            }
            throw new Exception("起こるはずの無いエラー");
        }
        public ItemNameResolver(Dictionary<long, PlayItem> itemNameDict)
        {
            _itemNameDict = itemNameDict;
        }
    }
    /// <summary>
    /// InternalCommentProviderの内部でのみ使用する
    /// </summary>
    internal class WhowatchInternalMessage
    {
        public string Raw { get; set; }
        public int? JoinRef { get; set; }
        public int? Ref { get; set; }
        public string Topic { get; set; }
        private string _event;
        public string Event
        {
            get => _event;
            set
            {
                if (_event == value)
                    return;
                _event = value;
                InternalMessageType = ParseInternalMessageTypeString(value);
            }
        }
        public string Payload { get; set; }
        public WhowatchInternalMessageType InternalMessageType { get; private set; }
        public WhowatchInternalMessageType ParseInternalMessageTypeString(string internalMessageTypeString)
        {
            WhowatchInternalMessageType type;
            switch (internalMessageTypeString)
            {
                case "shout":
                    type = WhowatchInternalMessageType.Shout;
                    break;
                default:
                    type = WhowatchInternalMessageType.Unknown; 
                    break;
            }
            return type;
        }
    }
    internal static class MessageParser
    {
        public static ItemNameResolver Resolver { get; set; }
        public static WhowatchInternalMessage ParseRawString2InternalMessage(string raw)
        {
            var match = Regex.Match(raw, "^\\[(?:\"(?<join_ref>\\d+)\"|(?<join_ref>null)),(?:\"(?<ref>\\d+)\"|(?<ref>null)),\"(?<topic>[a-z0-9_:]+)\",\"(?<event>[a-z0-9_]+)\",(?<payload>.+)\\]$");
            if (!match.Success)
            {
                throw new ParseException(raw);
            }
            var join_ref = match.Groups["join_ref"].Value;
            var @ref = match.Groups["ref"].Value;
            var topic = match.Groups[3].Value;
            var @event = match.Groups[4].Value;
            var payload = match.Groups[5].Value;
            return new WhowatchInternalMessage
            {
                Raw = raw,
                JoinRef = join_ref == "null" ? (int?)null : int.Parse(join_ref),
                Ref = @ref == "null" ? (int?)null : int.Parse(@ref),
                Topic = topic,
                Event = @event,
                Payload = payload,
            };
        }
        public static IWhowatchMessage ParseShoutMessage(WhowatchInternalMessage shoutMessage)
        {
            IWhowatchMessage message = null;

            var raw = shoutMessage.Raw;
            dynamic d = DynamicJson.Parse(shoutMessage.Payload);
            string commentType = d.comment.comment_type;
            var comment = d.comment;
            switch (commentType)
            {
                case "BY_PLAYITEM":
                    message = new WhowatchItem(raw)
                    {
                        AccountName = comment.user.account_name,
                        NameItems = new List<IMessagePart> { Common.MessagePartFactory.CreateMessageText(comment.user.name) },
                        CommentItems = new List<IMessagePart> { Common.MessagePartFactory.CreateMessageText(comment.message) },
                        Id = (long)comment.id,
                        PostedAt = (long)comment.posted_at,
                        UserId = (long)comment.user.id,
                        UserPath = comment.user.user_path,
                        ItemCount = (int)comment.item_count,
                        ItemName = Resolver.Resolve((int)comment.play_item_pattern_id),
                        UserIconUrl = comment.user.icon_url,
                    };
                    break;
                case "BY_PUBLIC":
                case "BY_FOLLOWER":
                case "BY_SYSTEM":
                    //運営コメント
                    message = new WhowatchComment(raw)
                    {
                        AccountName = comment.user.account_name,
                        NameItems = new List<IMessagePart> { Common.MessagePartFactory.CreateMessageText(comment.user.name) },
                        CommentItems = new List<IMessagePart> { Common.MessagePartFactory.CreateMessageText(comment.message) },
                        Id = comment.id.ToString(),
                        PostTime = SitePluginCommon.Utils.UnixtimeToDateTime((long)comment.posted_at / 1000).ToString("HH:mm:ss"),
                        UserId = comment.user.id.ToString(),
                        UserPath = comment.user.user_path,
                        UserIcon = new MessageImage
                        {
                            Url = (string)comment.user.icon_url,
                            Alt = null,
                            Height = 40,
                            Width = 40,
                        }
                    };
                    break;
                default:
                    break;
            }
            return message;
        }
        //public static IWhowatchMessage Parse(WhowatchInternalMessage internalMessage)
        //{
        //    var raw = internalMessage.Raw;
        //    IWhowatchMessage message = null;
        //    switch (internalMessage.InternalMessageType)
        //    {
        //        case WhowatchInternalMessageType.Shout:
        //            //
        //            dynamic d = DynamicJson.Parse(internalMessage.Payload);
        //            string commentType = d.comment.comment_type;
        //            var comment = d.comment;
        //            switch (commentType)
        //            {
        //                case "BY_PLAYITEM":
        //                    message = new WhowatchItem
        //                    {
        //                        AccountName = comment.user.account_name,
        //                        UserName = comment.user.name,
        //                        Id = (long)comment.id,
        //                        PostedAt = (long)comment.posted_at,
        //                        UserId = (long)comment.user.id,
        //                        UserPath = comment.user.user_path,
        //                        Raw = raw,
        //                        Comment = comment.message,
        //                        ItemCount = (int)comment.item_count,
        //                        ItemName = Resolver.Resolve((int)comment.play_item_pattern_id),
        //                    };
        //                    break;
        //                case "BY_PUBLIC":
        //                case "BY_FOLLOWER":
        //                    message = new WhowatchComment
        //                    {
        //                        AccountName = comment.user.account_name,
        //                        UserName = comment.user.name,
        //                        Id = (long)comment.id,
        //                        PostedAt = (long)comment.posted_at,
        //                        UserId = (long)comment.user.id,
        //                         UserPath=comment.user.user_path,
        //                        UserIconUrl = comment.user.icon_url,
        //                        Raw = raw,
        //                        Comment = comment.message,
        //                    };
        //                    break;
        //                case "BY_SYSTEM":
        //                    //運営コメント
        //                    break;
        //                default:
        //                    break;
        //            }
        //            //var low = Newtonsoft.Json.JsonConvert.DeserializeObject<MessageParsing.Shout.Payload>(payload);
        //            //message = new Shout(payload);
        //            break;
        //        case WhowatchInternalMessageType.PhxReply:
        //            break;
        //        default:
        //            throw new ParseException(raw);
        //    }
        //    if(message == null)
        //    {
        //        throw new ParseException(raw);
        //    }
        //    return message;
        //}
    }

    internal abstract class Message
    {
        string Raw { get; }
    }
    internal class Shout : ShoutBase
    {
        public Shout(string raw)
        {
            Raw = raw;
            var low = JsonConvert.DeserializeObject<MessageParsing.Shout.Payload>(raw);
            var comment = low.Comment;
            Message = comment.Message;
            UserPath = comment.User.UserPath;
        }
    }
    /// <summary>
    /// ShoutとItemの共通項目
    /// </summary>
    internal abstract class ShoutBase : Message
    {
        public string Message { get; protected set; }
        public string UserPath { get; protected set; }
        public string Raw { get; protected set; }
    }
    internal class Item : ShoutBase
    {
        public string ItemName { get; }
        public int ItemCount { get; }

        public Item(string raw)
        {
            Raw = raw;
            var low = JsonConvert.DeserializeObject<MessageParsing.Shout.Payload>(raw);
            var comment = low.Comment;
            Message = comment.Message;
            UserPath = comment.User.UserPath;
        }
    }
}
