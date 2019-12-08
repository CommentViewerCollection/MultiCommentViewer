using SitePlugin;
using System.Collections.Generic;

namespace MirrativSitePlugin
{
    internal class MirrativComment : MessageBase, IMirrativComment
    {
        public override SiteType SiteType { get; } = SiteType.Mirrativ;
        public MirrativMessageType MirrativMessageType { get; } = MirrativMessageType.Comment;
        //public string Comment { get; set; }
        public string Id { get; set; }
        //public string UserName { get; set; }
        public string UserId { get; set; }
        public string PostTime { get; set; }
        public IMessageImage UserIcon { get; set; }
        public MirrativComment(Message commentData, string raw) : base(raw)
        {
            UserId = commentData.UserId;
            Id = commentData.Id;
            CommentItems = new List<IMessagePart> { Common.MessagePartFactory.CreateMessageText(commentData.Comment) };
            NameItems = new List<IMessagePart> { Common.MessagePartFactory.CreateMessageText(commentData.Username) };
            UserIcon = null;
            PostTime = SitePluginCommon.Utils.UnixtimeToDateTime(commentData.CreatedAt).ToString("HH:mm:ss");
        }
    }
}
