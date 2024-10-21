using System;
using System.Collections.Generic;
using SitePlugin;

namespace BLiveSitePlugin
{
    class BLiveCommentData : IBLiveCommentData
    {
        public bool IsYell => !string.IsNullOrEmpty(YellPoints);
        public string YellPoints { get; set; }
        public string Message { get; set; }
        public string Id { get; set; }
        public string UserId { get; set; }
        public DateTime PostTime { get; set; }
        public string UserKey { get; set; }
        public string UserType { get; set; }
        public IMessageImage Stamp { get; set; }
        public string Name { get; set; }
        public List<IMessagePart> NameIcons { get; set; }
        public TimeSpan Elapsed { get; set; }
        public string UserIconUrl { get; set; }
    }
}
