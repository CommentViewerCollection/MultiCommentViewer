using PeriscopeSitePlugin;
using SitePlugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PeriscopeSitePlugin
{
    internal class PeriscopeComment : MessageBase2, IPeriscopeComment
    {
        public override SiteType SiteType { get; } = SiteType.Periscope;
        public PeriscopeMessageType PeriscopeMessageType { get; } = PeriscopeMessageType.Comment;
        public string Id { get; set; }
        public string UserId { get; set; }
        public DateTime? PostedAt { get; set; }
        public string Text { get; }
        public string DisplayName { get; }
        public PeriscopeComment(Kind1Type1 kind1Type1) : base(kind1Type1.Raw)
        {
            Id = kind1Type1.Uuid;
            UserId = kind1Type1.UserId;
            Text = kind1Type1.Body;
            DisplayName = kind1Type1.DisplayName;
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
            else if (timestampStr.Length == 10)
            {
                utcDate = DateTimeFromUnixTimestampSec(kind1Type1.Timestamp);
            }
            if (utcDate.HasValue)
            {
                PostedAt = utcDate.Value.ToLocalTime();
            }
        }
        static readonly DateTime BaseTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        public static DateTime DateTimeFromUnixTimestampSec(long sec)
        {
            return BaseTime.AddSeconds(sec);
        }
        public static DateTime DateTimeFromUnixTimestampMillis(long millis)
        {
            return BaseTime.AddMilliseconds(millis);
        }
        public static DateTime DateTimeFromUnixTimestampNanos(long nano)
        {
            var millis = nano / 1000 / 1000;
            return BaseTime.AddMilliseconds(millis);
        }
    }
}
