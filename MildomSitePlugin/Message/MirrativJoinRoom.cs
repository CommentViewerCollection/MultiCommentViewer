using MildomSitePlugin;
using SitePlugin;
using System.Collections.Generic;

internal class MildomJoinRoom : MessageBase, IMildomJoinRoom
{
    public override SiteType SiteType { get; } = SiteType.Mildom;
    public MildomMessageType MildomMessageType { get; } = MildomMessageType.JoinRoom;
    //public string Comment { get; set; }
    public string Id { get; set; }
    //public string UserName { get; set; }
    public string UserId { get; set; }
    public string PostTime { get; set; }
    public IMessageImage UserIcon { get; set; }
    public MildomJoinRoom(OnAddMessage add) : base(add.Raw)
    {
        UserId = add.UserId.ToString();
        Id = "";
        CommentItems = new List<IMessagePart> { Common.MessagePartFactory.CreateMessageText(add.Message) };
        NameItems = new List<IMessagePart> { Common.MessagePartFactory.CreateMessageText(add.UserName) };
        UserIcon = new Common.MessageImage
        {
            Url = add.UserImg,
            X = null,
            Y = null,
            Height = null,
            Width = null,
        };
        PostTime = add.PostedAt.ToString("HH:mm:ss");
    }
}