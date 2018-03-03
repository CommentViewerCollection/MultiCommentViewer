using System;
using System.Collections.Generic;
using SitePlugin;
using Common;
namespace TwicasSitePlugin
{
    class TwicasCommentViewModel : CommentViewModelBase
    {
        public override string UserId { get; }
        public TwicasCommentViewModel(ICommentOptions options, ICommentData data, IUser user) :
            base(options)
        {
            UserId = data.UserId;
            Id = data.Id.ToString();
            NameItems = new List<IMessagePart> { new MessageText(data.Name) };
            MessageItems = data.Message;
            //User = user;
        }
    }
}
