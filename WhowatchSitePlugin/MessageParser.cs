using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Codeplex.Data;
using Common;
using Newtonsoft.Json;
using SitePlugin;

namespace WhowatchSitePlugin
{
    class ItemNameResolver
    {
        private readonly Dictionary<long, PlayItem> _itemNameDict;

        public string Resolve(int play_item_pattern_id)
        {
            if (_itemNameDict.TryGetValue(play_item_pattern_id, out PlayItem item))
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
        public static IWhowatchMessage ParseMessage(Comment rawMessage, string raw)
        {
            var comment = rawMessage;
            IWhowatchMessage message;
            switch (rawMessage.CommentType)
            {
                case "BY_PLAYITEM":
                    message = new WhowatchItem(raw)
                    {
                        AccountName = rawMessage.User.AccountName,
                        UserName = rawMessage.User.Name,
                        Comment = rawMessage.Message,
                        Id = (long)rawMessage.Id,
                        PostedAt = (long)rawMessage.PostedAt,
                        UserId = (long)rawMessage.User.Id,
                        UserPath = rawMessage.User.UserPath,
                        ItemCount = (int)rawMessage.ItemCount,
                        ItemName = Resolver.Resolve((int)rawMessage.PlayItemPatternId),
                        UserIconUrl = rawMessage.User.IconUrl,
                    };
                    break;
                case "BY_PUBLIC":
                case "BY_FOLLOWER":
                case "BY_SYSTEM":
                case "BY_OWNER":
                    //運営コメント

                    //NGワードを含むコメント。ng_word_includedがtrueで、original_messageがある。
                    //[null,null,"room:10030668","shout",{"topic":"room_pub:10030668","event":"shout","comment":{"user":{"user_profile":{},"user_path":"w:ryu_s","name":"Ryu","is_admin":false,"id":1614280,"icon_url":"","account_name":"ふ:ryu_s"},"tts":{},"reply_to_user_id":0,"posted_at":1553429971000,"original_message":"あいうえお","not_escaped":false,"ng_word_included":true,"message":"この投稿は視聴者には表示されません。","live_id":10030668,"is_silent_comment":true,"is_reply_to_me":false,"id":633344989,"escaped_original_message":"<span class=\"ngword\">あいうえお</span>","escaped_message":"この投稿は視聴者には表示されません。","enabled":true,"comment_type":"BY_FOLLOWER","anonymized":false}}]
                    //匿名コメント。anonymizedがtrue
                    //[null,null,"room:10030668","shout",{"topic":"room_pub:10030668","event":"shout","comment":{"user":{"user_profile":{},"user_path":"w:秘密のチキンボーイ","name":"秘密のチキンボーイ","is_admin":false,"id":1024,"icon_url":"https://img.whowatch.tv/user_files/1024/profile_icon/1505207215448.jpeg","account_name":"ふ:秘密のチキンボーイ"},"reply_to_user_id":0,"posted_at":1553429977000,"play_item_pattern_id":116,"pickup_time":2000,"not_escaped":false,"ng_word_included":false,"message":"ひよこをプレゼントしました。","live_id":10030668,"item_count":1,"is_silent_comment":false,"is_reply_to_me":false,"id":633345152,"escaped_message":"ひよこをプレゼントしました。","enabled":true,"comment_type":"BY_PLAYITEM","anonymized":true}}]
                    //if (comment.n.IsDefined("ng_word_included") && comment.ng_word_included == true)
                    if (comment.NgWordIncluded)
                    {
                        //NGコメント
                        message = new WhowatchNgComment(raw)
                        {
                            AccountName = comment.User.AccountName,
                            UserName = comment.User.Name,
                            Comment = comment.Message,
                            Id = comment.Id.ToString(),
                            PostTime = SitePluginCommon.Utils.UnixtimeToDateTime((long)comment.PostedAt / 1000).ToString("HH:mm:ss"),
                            UserId = comment.User.Id.ToString(),
                            UserPath = comment.User.UserPath,
                            //OriginalMessage = comment.OriginalMessage,
                            UserIcon = new MessageImage
                            {
                                Url = (string)comment.User.IconUrl,
                                Alt = null,
                                Height = 40,
                                Width = 40,
                            }
                        };
                    }
                    else
                    {
                        message = new WhowatchComment(raw)
                        {
                            AccountName = comment.User.AccountName,
                            UserName = comment.User.Name,
                            Comment = comment.Message,
                            Id = comment.Id.ToString(),
                            PostTime = SitePluginCommon.Utils.UnixtimeToDateTime((long)comment.PostedAt / 1000).ToString("HH:mm:ss"),
                            UserId = comment.User.Id.ToString(),
                            UserPath = comment.User.UserPath,
                            UserIcon = new MessageImage
                            {
                                Url = (string)comment.User.IconUrl,
                                Alt = null,
                                Height = 40,
                                Width = 40,
                            }
                        };
                    }
                    break;
                default:
                    throw new ParseException(raw);
            }
            return message;
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
                        UserName = comment.user.name,
                        Comment = comment.message,
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
                case "BY_OWNER":
                    //運営コメント

                    //NGワードを含むコメント。ng_word_includedがtrueで、original_messageがある。
                    //[null,null,"room:10030668","shout",{"topic":"room_pub:10030668","event":"shout","comment":{"user":{"user_profile":{},"user_path":"w:ryu_s","name":"Ryu","is_admin":false,"id":1614280,"icon_url":"","account_name":"ふ:ryu_s"},"tts":{},"reply_to_user_id":0,"posted_at":1553429971000,"original_message":"あいうえお","not_escaped":false,"ng_word_included":true,"message":"この投稿は視聴者には表示されません。","live_id":10030668,"is_silent_comment":true,"is_reply_to_me":false,"id":633344989,"escaped_original_message":"<span class=\"ngword\">あいうえお</span>","escaped_message":"この投稿は視聴者には表示されません。","enabled":true,"comment_type":"BY_FOLLOWER","anonymized":false}}]
                    //匿名コメント。anonymizedがtrue
                    //[null,null,"room:10030668","shout",{"topic":"room_pub:10030668","event":"shout","comment":{"user":{"user_profile":{},"user_path":"w:秘密のチキンボーイ","name":"秘密のチキンボーイ","is_admin":false,"id":1024,"icon_url":"https://img.whowatch.tv/user_files/1024/profile_icon/1505207215448.jpeg","account_name":"ふ:秘密のチキンボーイ"},"reply_to_user_id":0,"posted_at":1553429977000,"play_item_pattern_id":116,"pickup_time":2000,"not_escaped":false,"ng_word_included":false,"message":"ひよこをプレゼントしました。","live_id":10030668,"item_count":1,"is_silent_comment":false,"is_reply_to_me":false,"id":633345152,"escaped_message":"ひよこをプレゼントしました。","enabled":true,"comment_type":"BY_PLAYITEM","anonymized":true}}]
                    if (comment.IsDefined("ng_word_included") && comment.ng_word_included == true)
                    {
                        //NGコメント
                        message = new WhowatchNgComment(raw)
                        {
                            AccountName = comment.user.account_name,
                            UserName = comment.user.name,
                            Comment = comment.message,
                            Id = comment.id.ToString(),
                            PostTime = SitePluginCommon.Utils.UnixtimeToDateTime((long)comment.posted_at / 1000).ToString("HH:mm:ss"),
                            UserId = comment.user.id.ToString(),
                            UserPath = comment.user.user_path,
                            OriginalMessage = comment.original_message,
                            UserIcon = new MessageImage
                            {
                                Url = (string)comment.user.icon_url,
                                Alt = null,
                                Height = 40,
                                Width = 40,
                            }
                        };
                    }
                    else
                    {
                        message = new WhowatchComment(raw)
                        {
                            AccountName = comment.user.account_name,
                            UserName = comment.user.name,
                            Comment = comment.message,
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
                    }
                    break;
                default:
                    throw new ParseException(raw);
            }
            return message;
        }
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
