using PeriscopeSitePlugin;
using SitePlugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PeriscopeSitePlugin
{
    internal class PeriscopeComment : MessageBase, IPeriscopeComment
    {
        public override SiteType SiteType { get; } = SiteType.Periscope;
        public PeriscopeMessageType PeriscopeMessageType { get; } = PeriscopeMessageType.Comment;
        public string Id { get; set; }
        public string UserId { get; set; }
        public string PostTime { get; set; }
        public IMessageImage UserIcon { get; set; }
        public PeriscopeComment(Kind1Type1 kind1Type1) : base(kind1Type1.Raw)
        {
            Id = kind1Type1.Uuid;
            UserId = kind1Type1.UserId;
            CommentItems = new List<IMessagePart> { Common.MessagePartFactory.CreateMessageText(kind1Type1.Body) };
            NameItems = new List<IMessagePart> { Common.MessagePartFactory.CreateMessageText(kind1Type1.DisplayName ) };
            var timestampStr = kind1Type1.Timestamp.ToString();
            DateTime? utcDate = null;
            if (timestampStr.Length == 13)
            {
                utcDate = DateTimeFromUnixTimestampMillis(kind1Type1.Timestamp);
            }
            else if (timestampStr.Length == 19)
            {
                utcDate = DateTimeFromUnixTimestampNanos(kind1Type1.Timestamp);
            }
            else if(timestampStr.Length == 10)
            {
                utcDate = DateTimeFromUnixTimestampSec(kind1Type1.Timestamp);
            }
            if(utcDate.HasValue)
            {
                PostTime = utcDate.Value.ToLocalTime().ToString("HH:mm:ss");
            }
        }
        static readonly DateTime _baseTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        public static DateTime DateTimeFromUnixTimestampSec(long sec)
        {
            return _baseTime.AddSeconds(sec);
        }
        public static DateTime DateTimeFromUnixTimestampMillis(long millis)
        {
            return _baseTime.AddMilliseconds(millis);
        }
        public static DateTime DateTimeFromUnixTimestampNanos(long nano)
        {
            var millis = nano / 1000 / 1000;
            return _baseTime.AddMilliseconds(millis);
        }
    }
}
