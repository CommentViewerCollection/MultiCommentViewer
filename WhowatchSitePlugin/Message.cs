using SitePlugin;
using System.Collections.Generic;

namespace WhowatchSitePlugin
{
    internal class WhowatchConnected : MessageBase, IWhowatchConnected
    {
        public override SiteType SiteType { get; } = SiteType.Whowatch;
        public WhowatchMessageType WhowatchMessageType { get; } = WhowatchMessageType.Connected;

        public WhowatchConnected(string raw) : base(raw)
        {
            CommentItems = new List<IMessagePart>
            {
                Common.MessagePartFactory.CreateMessageText("接続しました"),
            };
        }
    }
    internal class WhowatchDisconnected : MessageBase, IWhowatchDisconnected
    {
        public override SiteType SiteType { get; } = SiteType.Whowatch;
        public WhowatchMessageType WhowatchMessageType { get; } = WhowatchMessageType.Disconnected;
        public WhowatchDisconnected(string raw) : base(raw)
        {
            CommentItems = new List<IMessagePart>
            {
                Common.MessagePartFactory.CreateMessageText("切断しました"),
            };
        }
    }
    internal class WhowatchItem : MessageBase, IWhowatchItem
    {
        public override SiteType SiteType { get; } = SiteType.Whowatch;
        public WhowatchMessageType WhowatchMessageType { get; } = WhowatchMessageType.Item;
        public string ItemName { get; set; }
        public int ItemCount { get; set; }
        //public string Comment { get; set; }
        public long Id { get; set; }
        //public string UserName { get; set; }
        public string UserPath { get; set; }
        public long UserId { get; set; }
        public string AccountName { get; set; }
        public long PostedAt { get; set; }
        public string UserIconUrl { get; set; }
        public WhowatchItem(string raw) : base(raw)
        {

        }
    }
    internal class WhowatchComment : MessageBase, IWhowatchComment
    {
        public override SiteType SiteType { get; } = SiteType.Whowatch;
        public WhowatchMessageType WhowatchMessageType { get; } = WhowatchMessageType.Comment;
        //public string Comment { get; set; }
        public string Id { get; set; }
        //public string UserName { get; set; }
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
