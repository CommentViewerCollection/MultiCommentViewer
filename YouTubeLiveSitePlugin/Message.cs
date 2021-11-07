﻿using Common;
using ryu_s.YouTubeLive.Message.Action;
using SitePlugin;
using System;
using System.Collections.Generic;
using System.Linq;

namespace YouTubeLiveSitePlugin
{
    internal class YouTubeLiveConnected : MessageBase2, IYouTubeLiveConnected
    {
        public override SiteType SiteType { get; } = SiteType.YouTubeLive;
        public YouTubeLiveMessageType YouTubeLiveMessageType { get; } = YouTubeLiveMessageType.Connected;
        public string Text { get; }
        public YouTubeLiveConnected(string raw) : base(raw)
        {
            Text = "接続しました";
        }
    }
    internal class YouTubeLiveDisconnected : MessageBase2, IYouTubeLiveDisconnected
    {
        public override SiteType SiteType { get; } = SiteType.YouTubeLive;
        public YouTubeLiveMessageType YouTubeLiveMessageType { get; } = YouTubeLiveMessageType.Disconnected;
        public string Text { get; }
        public YouTubeLiveDisconnected(string raw) : base(raw)
        {
            Text = "切断しました";
        }
    }
    internal class YouTubeLiveMembership : MessageBase2, IYouTubeLiveMembership
    {
        public override SiteType SiteType { get; } = SiteType.YouTubeLive;
        public YouTubeLiveMessageType YouTubeLiveMessageType { get; } = YouTubeLiveMessageType.Membership;
        //public string Comment { get; set; }
        public string Id { get; set; }
        public IEnumerable<IMessagePart> NameItems { get; set; }
        public IEnumerable<IMessagePart> CommentItems { get; set; }
        //public string UserName { get; set; }
        public string UserId { get; set; }
        public DateTime PostedAt { get; set; }
        public IMessageImage UserIcon { get; set; }
        public YouTubeLiveMembership(Next.InternalMembership comment) : base("")
        {
            UserId = comment.UserId;
            Id = comment.Id;
            CommentItems = comment.MessageItems;
            NameItems = comment.NameItems;
            UserIcon = new Common.MessageImage
            {
                Height = comment.ThumbnailHeight,
                Width = comment.ThumbnailWidth,
                Url = comment.ThumbnailUrl,
            };
            PostedAt = SitePluginCommon.Utils.UnixtimeToDateTime(comment.TimestampUsec / (1000 * 1000));
        }
    }
    internal class YouTubeLiveSuperchat : MessageBase2, IYouTubeLiveSuperchat
    {
        public override SiteType SiteType { get; } = SiteType.YouTubeLive;
        public YouTubeLiveMessageType YouTubeLiveMessageType { get; } = YouTubeLiveMessageType.Superchat;
        //public string Comment { get; set; }
        public string Id { get; set; }
        public IEnumerable<IMessagePart> NameItems { get; set; }
        public IEnumerable<IMessagePart> CommentItems { get; set; }
        public string UserId { get; set; }
        public DateTime PostedAt { get; set; }
        public IMessageImage UserIcon { get; set; }
        public string PurchaseAmount { get; }
        public YouTubeLiveSuperchat(Test2.CommentData commentData) : base(commentData.Raw)
        {
            UserId = commentData.UserId;
            Id = commentData.Id;

            var list = new List<IMessagePart>();
            var s = commentData.PurchaseAmount;
            if (commentData.MessageItems.Count > 0)
                s += Environment.NewLine;
            list.Add(MessagePartFactory.CreateMessageText(s));
            list.AddRange(commentData.MessageItems);
            CommentItems = list;

            NameItems = commentData.NameItems;
            PurchaseAmount = commentData.PurchaseAmount;
            UserIcon = commentData.Thumbnail;
            PostedAt = SitePluginCommon.Utils.UnixtimeToDateTime(commentData.TimestampUsec / (1000 * 1000));
        }
        public YouTubeLiveSuperchat(Next.InternalSuperChat comment) : base("")
        {
            UserId = comment.UserId;
            Id = comment.Id;
            CommentItems = comment.MessageItems;
            NameItems = comment.NameItems;
            UserIcon = new Common.MessageImage
            {
                Height = comment.ThumbnailHeight,
                Width = comment.ThumbnailWidth,
                Url = comment.ThumbnailUrl,
            };
            PostedAt = SitePluginCommon.Utils.UnixtimeToDateTime(comment.TimestampUsec / (1000 * 1000));
            PurchaseAmount = comment.PurchaseAmount;
        }
        public YouTubeLiveSuperchat(SuperChat text) : base("")
        {
            UserId = text.AuthorExternalChannelId;//.UserId;
            Id = text.Id;
            CommentItems = text.MessageItems.Select(a => MessageConverter.Parse(a)).ToList();
            var nameItems = new List<IMessagePart>();
            if (text.AuthorName != null)
            {
                nameItems.Add(Common.MessagePartFactory.CreateMessageText(text.AuthorName));
            }
            foreach (var badge in text.AuthorBadges)
            {

            }
            NameItems = nameItems;
            UserIcon = new Common.MessageImage
            {
                Height = text.AuthorPhoto.Height,
                Width = text.AuthorPhoto.Width,
                Url = text.AuthorPhoto.Url,
            };
            PostedAt = SitePluginCommon.Utils.UnixtimeToDateTime(text.TimestampUsec / (1000 * 1000));
            PurchaseAmount = text.PurchaseAmount;
        }
    }
    internal class YouTubeLiveComment : MessageBase2, IYouTubeLiveComment
    {
        public override SiteType SiteType { get; } = SiteType.YouTubeLive;
        public YouTubeLiveMessageType YouTubeLiveMessageType { get; } = YouTubeLiveMessageType.Comment;
        //public string Comment { get; set; }
        public string Id { get; set; }
        public IEnumerable<IMessagePart> NameItems { get; set; }
        public IEnumerable<IMessagePart> CommentItems { get; set; }
        //public string UserName { get; set; }
        public string UserId { get; set; }
        public DateTime PostedAt { get; set; }
        public IMessageImage UserIcon { get; set; }
        public YouTubeLiveComment(Test2.CommentData commentData) : base(commentData.Raw)
        {
            UserId = commentData.UserId;
            Id = commentData.Id;
            CommentItems = commentData.MessageItems;
            NameItems = commentData.NameItems;
            UserIcon = commentData.Thumbnail;
            PostedAt = SitePluginCommon.Utils.UnixtimeToDateTime(commentData.TimestampUsec / (1000 * 1000));
        }
        public YouTubeLiveComment(Next.InternalComment comment) : base("")
        {
            UserId = comment.UserId;
            Id = comment.Id;
            CommentItems = comment.MessageItems;
            NameItems = comment.NameItems;
            UserIcon = new Common.MessageImage
            {
                Height = comment.ThumbnailHeight,
                Width = comment.ThumbnailWidth,
                Url = comment.ThumbnailUrl,
            };
            PostedAt = SitePluginCommon.Utils.UnixtimeToDateTime(comment.TimestampUsec / (1000 * 1000));
        }

        public YouTubeLiveComment(TextMessage text) : base("")
        {
            UserId = text.AuthorExternalChannelId;//.UserId;
            Id = text.Id;
            CommentItems = text.MessageItems.Select(a => MessageConverter.Parse(a)).ToList();
            var nameItems = new List<IMessagePart>();
            if (text.AuthorName != null)
            {
                nameItems.Add(Common.MessagePartFactory.CreateMessageText(text.AuthorName));
            }
            var badges = new List<IMessagePart>();
            foreach (var badge in text.AuthorBadges)
            {
                var parsed = MessageConverter.Parse(badge);
                if (parsed == null) continue;
                badges.Add(parsed);
            }
            nameItems.AddRange(badges);
            NameItems = nameItems;
            UserIcon = new Common.MessageImage
            {
                Height = text.AuthorPhoto.Height,
                Width = text.AuthorPhoto.Width,
                Url = text.AuthorPhoto.Url,
            };
            PostedAt = SitePluginCommon.Utils.UnixtimeToDateTime(text.TimestampUsec / (1000 * 1000));
        }

    }
    static class MessageConverter
    {
        public static IMessagePart Parse(ryu_s.YouTubeLive.Message.IMessagePart a)
        {
            switch (a)
            {
                case ryu_s.YouTubeLive.Message.ITextPart text:
                    return Common.MessagePartFactory.CreateMessageText(text.Raw);
                case ryu_s.YouTubeLive.Message.EmojiPart emoji:
                    return new Common.MessageSvgImage
                    {
                        Url = emoji.Url,
                        Alt = "",
                        Height = 24,
                        Width = 24,
                    };
                case ryu_s.YouTubeLive.Message.CustomEmojiPart custom:
                    return new Common.MessageImage
                    {
                        Url = custom.Url,
                        Height = custom.Height,
                        Width = custom.Width,
                        Alt = custom.Tooltip,
                    };
                default:
                    throw new NotImplementedException();
            }
        }
        public static IMessagePart Parse(ryu_s.YouTubeLive.Message.IBadge a)
        {
            throw new NotImplementedException();
        }
        public static IMessagePart? Parse(ryu_s.YouTubeLive.Message.Action.IAuthorBadge a)
        {
            switch (a)
            {
                case AuthorBadgeCustomThumb customThumb:
                    return new Common.MessageImage
                    {
                        Url = customThumb.Thumbnails[1].Url,
                        Alt = customThumb.Tooltip,
                        Height = 16,
                        Width = 16,
                    };
                case AuthorBadgeIcon icon:
                    var data = icon.IconType switch
                    {
                        "MODERATOR" => @"<svg viewBox=""0 0 16 16"" preserveAspectRatio=""xMidYMid meet"" focusable=""false"" class=""style-scope yt-icon"" style=""pointer-events: none; display: block; width: 100%; height: 100%;"" xmlns=""http://www.w3.org/2000/svg"" version=""1.1""><g class=""style-scope yt-icon""><path d=""M9.64589146,7.05569719 C9.83346524,6.562372 9.93617022,6.02722257 9.93617022,5.46808511 C9.93617022,3.00042984 7.93574038,1 5.46808511,1 C4.90894765,1 4.37379823,1.10270499 3.88047304,1.29027875 L6.95744681,4.36725249 L4.36725255,6.95744681 L1.29027875,3.88047305 C1.10270498,4.37379824 1,4.90894766 1,5.46808511 C1,7.93574038 3.00042984,9.93617022 5.46808511,9.93617022 C6.02722256,9.93617022 6.56237198,9.83346524 7.05569716,9.64589147 L12.4098057,15 L15,12.4098057 L9.64589146,7.05569719 Z"" class=""style-scope yt-icon""></path></g></svg>",
                        "OWNER" => null,
                        "VERIFIED" => null,
                        _ => null,
                    };
                    if (data == null) return null;
                    return new Common.MessageSvgData
                    {
                        Data = data,
                        Alt = icon.Tooltip,
                        Height = 16,
                        Width = 16,
                    };
                default:
                    return null;
            }
        }
    }
}
