using SitePlugin;
using System.Collections.Generic;

namespace MildomSitePlugin
{
    internal class MildomComment : MessageBase, IMildomComment
    {
        public override SiteType SiteType { get; } = SiteType.Mixer;
        public MildomMessageType MildomMessageType { get; } = MildomMessageType.Comment;
        //public string Comment { get; set; }
        public string Id { get; set; }
        //public string UserName { get; set; }
        public string UserId { get; set; }
        public string PostTime { get; set; }
        public IMessageImage UserIcon { get; set; }
        public MildomComment(OnChatMessage chat, string raw) : base(raw)
        {
            UserId = chat.UserId.ToString();
            Id = "";
            CommentItems = chat.MessageItems;
            NameItems = new List<IMessagePart> { Common.MessagePartFactory.CreateMessageText(chat.UserName) };
            UserIcon = null;
            PostTime = "";// SitePluginCommon.Utils.UnixtimeToDateTime(commentData.CreatedAt).ToString("HH:mm:ss");
        }
    }
}
