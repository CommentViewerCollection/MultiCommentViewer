using SitePlugin;
using System.Collections.Generic;

namespace MirrativSitePlugin
{
    internal class MirrativItem : MessageBase, IMirrativItem
    {
        public override SiteType SiteType { get; } = SiteType.Mirrativ;
        public MirrativMessageType MirrativMessageType { get; } = MirrativMessageType.Item;
        public string UserId { get; }
        public string PostTime { get; }
        public string Id { get; }
        public MirrativItem(Message commentData, string raw) : base(raw)
        {
            UserId = commentData.UserId;
            Id = commentData.Id;
            CommentItems = new List<IMessagePart> { Common.MessagePartFactory.CreateMessageText(commentData.Comment) };
            NameItems = new List<IMessagePart> { Common.MessagePartFactory.CreateMessageText(commentData.Username) };
            //UserIcon = null;
            PostTime = SitePluginCommon.Utils.UnixtimeToDateTime(commentData.CreatedAt).ToString("HH:mm:ss");
        }
    }
}
