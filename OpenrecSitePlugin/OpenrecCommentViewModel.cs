using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using SitePlugin;

namespace OpenrecSitePlugin
{
    public interface IOpenrecCommentViewModel : ICommentViewModel
    {
        string PostDate { get; }
        string Elapsed { get; }
    }
    class OpenrecCommentViewModel : CommentViewModelBase, IOpenrecCommentViewModel
    {
        private ICommentOptions _options;

        public string PostDate { get; }
        public string Elapsed { get; }
        public override string UserId { get; }
        public override IUser User { get; }
        public OpenrecCommentViewModel(IOpenrecCommentData commentData, ICommentOptions options ,UserViewModel userVm,ICommentProvider commentProvider, bool isFirstComment,IUser user)
            : base(options)
        {
            _options = options;
            UserId = commentData.UserId;
            Id = commentData.Id;
            IsFirstComment = isFirstComment;
            PostDate = commentData.PostTime.ToString("HH:mm:ss");
            var elapsed = commentData.Elapsed;
            Elapsed = Tools.ElapsedToString(elapsed);
            IsVisible = !userVm.IsNgUser;
            User = user;
            //Name
            {
                var nameItems = new List<IMessagePart>();
                nameItems.Add(new MessageText(commentData.Name));
                nameItems.AddRange(commentData.NameIcons);
                NameItems = nameItems;
            }
            //Message
            {
                var messageItems = new List<IMessagePart>();
                if (commentData.IsYell)
                {
                    messageItems.Add(new MessageText("エールポイント：" + commentData.YellPoints + Environment.NewLine));
                }
                messageItems.Add(commentData.Message);
                if(commentData.Stamp != null)
                {
                    messageItems.Add(commentData.Stamp);
                }
                MessageItems = messageItems;
            }

            userVm.PropertyChanged += (s, e) =>
            {
                switch (e.PropertyName)
                {
                    case nameof(userVm.IsNgUser):
                        IsVisible = !userVm.IsNgUser;
                        break;
                }
            };
        }
    }
}
