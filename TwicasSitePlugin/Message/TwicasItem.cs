using Mcv.PluginV2;
using System.Collections.Generic;

namespace TwicasSitePlugin
{
    internal class TwicasItem : MessageBase2, ITwicasItem
    {
        public override SiteType SiteType { get; } = SiteType.Twicas;
        public TwicasMessageType TwicasMessageType { get; } = TwicasMessageType.Item;
        public string UserName { get; set; }
        public IEnumerable<IMessagePart> CommentItems { get; set; }
        //public string Id { get; set; }
        public string UserId { get; set; }
        //public string PostTime { get; set; }
        public IMessageImage UserIcon { get; set; }
        public string ItemName { get; set; }
        public string ItemId { get; set; }

        public TwicasItem(string raw) : base(raw)
        {

        }
    }
}
