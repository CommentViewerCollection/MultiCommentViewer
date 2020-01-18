using SitePlugin;

namespace WhowatchSitePlugin
{
    internal class WhowatchComment : MessageBase2, IWhowatchComment
    {
        public override SiteType SiteType { get; } = SiteType.Whowatch;
        public WhowatchMessageType WhowatchMessageType { get; } = WhowatchMessageType.Comment;
        public string Comment { get; set; }
        public string Id { get; set; }
        public string UserName { get; set; }
        public string UserPath { get; set; }
        public string UserId { get; set; }
        public string AccountName { get; set; }
        public string PostTime { get; set; }
        public IMessageImage UserIcon { get; set; }
        public WhowatchComment(string raw) : base(raw)
        {

        }
    }
}
