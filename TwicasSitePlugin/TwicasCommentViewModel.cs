using System;
using System.Collections.Generic;
using SitePlugin;

namespace TwicasSitePlugin
{
    class TwicasCommentViewModel : CommentViewModelBase
    {
        public override string UserId { get; }
        public TwicasCommentViewModel(ConnectionName connectionName, IOptions options, ICommentData data, IUser user) :
            base(connectionName, options)
        {
            UserId = data.UserId;
            Id = data.Id.ToString();
            NameItems = new List<IMessagePart> { new MessageText(data.Name) };
            MessageItems = data.Message;
            User = user;
        }
    }
}
