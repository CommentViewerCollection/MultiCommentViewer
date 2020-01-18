using SitePlugin;
using System;
using System.Collections.Generic;

namespace TestSitePlugin
{
    class TestComment : ITestComment
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public DateTime PostedAt { get; set; }
        public IMessageImage UserIcon { get; set; }
        public string Raw { get; set; }
        public SiteType SiteType { get; } = SiteType.Unknown;
        public TestMessageType TestMessageType { get; } = TestMessageType.Comment;
        public string UserName { get; set; }
        public string Text { get; set; }

        public event EventHandler<ValueChangedEventArgs> ValueChanged;
    }
}
