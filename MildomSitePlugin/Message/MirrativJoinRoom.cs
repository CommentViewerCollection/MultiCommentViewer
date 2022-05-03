using Mcv.PluginV2;
using MildomSitePlugin;
using System;
using System.Collections.Generic;

internal class MildomJoinRoom : MessageBase2, IMildomJoinRoom
{
    public override SiteType SiteType { get; } = SiteType.Mildom;
    public MildomMessageType MildomMessageType { get; } = MildomMessageType.JoinRoom;
    //public string Comment { get; set; }
    public string Id { get; set; }
    public IEnumerable<IMessagePart> NameItems { get; set; }
    public IEnumerable<IMessagePart> CommentItems { get; set; }
    //public string UserName { get; set; }
    public string UserId { get; set; }
    public DateTime PostedAt { get; set; }
    public IMessageImage UserIcon { get; set; }
    public MildomJoinRoom(OnAddMessage add) : base(add.Raw)
    {
        UserId = add.UserId.ToString();
        Id = "";
        CommentItems = new List<IMessagePart> { MessagePartFactory.CreateMessageText(add.Message) };
        NameItems = new List<IMessagePart> { MessagePartFactory.CreateMessageText(add.UserName) };
        UserIcon = new MessageImage
        {
            Url = add.UserImg,
            X = null,
            Y = null,
            Height = null,
            Width = null,
        };
        PostedAt = add.PostedAt;
    }
}