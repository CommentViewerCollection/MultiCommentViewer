using SitePlugin;

namespace WhowatchSitePlugin
{
    internal class WhowatchItem : MessageBase2, IWhowatchItem
    {
        public override SiteType SiteType { get; } = SiteType.Whowatch;
        public WhowatchMessageType WhowatchMessageType { get; } = WhowatchMessageType.Item;
        public string ItemName { get; set; }
        public int ItemCount { get; set; }
        public string Comment { get; set; }
        public long Id { get; set; }
        public string UserName { get; set; }
        public string UserPath { get; set; }
        public long UserId { get; set; }
        public string AccountName { get; set; }
        public long PostedAt { get; set; }
        public string UserIconUrl { get; set; }
        public WhowatchItem(string raw) : base(raw)
        {

        }
    }
}
