using SitePlugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YouTubeLiveSitePlugin.Next
{
    interface IInternalMessage { }

    /// <summary>
    /// メンバー登録があった時に流れるメッセージ
    /// </summary>
    class InternalMembership : IInternalMessage
    {
        public string UserId { get; internal set; }
        public long TimestampUsec { get; internal set; }
        public string Id { get; internal set; }
        public List<IMessagePart> MessageItems { get; internal set; }
        public List<IMessagePart> NameItems { get; internal set; }
        public string ThumbnailUrl { get; internal set; }
        public int ThumbnailWidth { get; internal set; }
        public int ThumbnailHeight { get; internal set; }
    }
    class UnknownAction : IInternalMessage
    {
        public UnknownAction(string raw)
        {
            Raw = raw;
        }

        public string Raw { get; }
    }
}
