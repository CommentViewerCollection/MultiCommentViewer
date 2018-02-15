using System;
using SitePlugin;
using System.Collections.Generic;

namespace TwicasSitePlugin
{
    class CommentData : ICommentData
    {
        public long Id { get; set; }
        public IEnumerable<IMessagePart> Message { get; set; }
        public string Name { get; set; }
        public DateTime Date { get; set; }
        public string ThumbnailUrl { get; set; }

        public string UserId { get; set; }

        public int ThumbnailWidth { get; set; }

        public int ThumbnailHeight { get; set; }
    }
}
