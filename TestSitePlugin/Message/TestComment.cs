using SitePlugin;
using System;
using System.Collections.Generic;

namespace TestSitePlugin
{
    class TestComment : ITestComment
    {
        public string Id { get; }
        public string UserId { get; }
        public string PostTime { get; }
        public IMessageImage UserIcon { get; set; }
        public string Raw { get; }
        public SiteType SiteType { get; }
        public IEnumerable<IMessagePart> NameItems { get; }
        public IEnumerable<IMessagePart> CommentItems { get; }
        public TestMessageType TestMessageType { get; } = TestMessageType.Comment;

        public event EventHandler<ValueChangedEventArgs> ValueChanged;
        public TestComment(string userId, string comment)
        {
            UserId = userId;
            CommentItems = new List<IMessagePart> { Common.MessagePartFactory.CreateMessageText(comment) };
        }
        public TestComment(string userId,string name, string comment)
        {
            UserId = userId;
            NameItems = new List<IMessagePart> { Common.MessagePartFactory.CreateMessageText(name) };
            CommentItems = new List<IMessagePart> { Common.MessagePartFactory.CreateMessageText(comment) };
        }
    }
}
