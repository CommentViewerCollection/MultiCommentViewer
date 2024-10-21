using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using SitePlugin;

namespace BLiveSitePlugin
{
    public interface IBLiveCommentViewModel : ICommentViewModel
    {
        string PostDate { get; }
        string Elapsed { get; }
        bool IsStamp { get; }
        bool IsYell { get; }
    }
    class BLiveCommentViewModel : CommentViewModelBase, IBLiveCommentViewModel
    {
        public override MessageType MessageType { get; protected set; }
        private ICommentOptions _options;
        private readonly IBLiveSiteOptions _siteOptions;

        public string PostDate { get; }
        public string Elapsed { get; }
        public override string UserId { get; }
        public bool IsStamp { get; }
        public bool IsYell { get; }
        public BLiveCommentViewModel(IBLiveCommentData commentData, ICommentOptions options, IBLiveSiteOptions siteOptions, ICommentProvider commentProvider, bool isFirstComment, IUser user)
            : base(options, user, commentProvider, isFirstComment)
        {
            MessageType = MessageType.Comment;
            _options = options;
            _siteOptions = siteOptions;
            UserId = commentData.UserId;
            Id = commentData.Id;
            PostDate = commentData.PostTime.ToString("HH:mm:ss");
            var elapsed = commentData.Elapsed;
            Elapsed = Tools.ElapsedToString(elapsed);
            IsStamp = commentData.Stamp != null;
            IsYell = commentData.IsYell;
            if (!string.IsNullOrEmpty(commentData.UserIconUrl))
            {
                Thumbnail = new MessageImage { Url = commentData.UserIconUrl };
            }
            if (siteOptions.IsAutoSetNickname)
            {
                var nick = ExtractNickname(commentData.Message);
                if (!string.IsNullOrEmpty(nick))
                {
                    user.Nickname = nick;
                }
            }
            //Name
            {
                var nameItems = new List<IMessagePart>();
                nameItems.Add(MessagePartFactory.CreateMessageText(commentData.Name));
                nameItems.AddRange(commentData.NameIcons);
                NameItemsInternal = nameItems;
            }
            //Message
            {
                var messageItems = new List<IMessagePart>();
                if (commentData.IsYell)
                {
                    MessageType = MessageType.BroadcastInfo;
                    messageItems.Add(MessagePartFactory.CreateMessageText("エールポイント：" + commentData.YellPoints + Environment.NewLine));
                }
                messageItems.Add(MessagePartFactory.CreateMessageText(commentData.Message));
                if (commentData.Stamp != null)
                {
                    MessageType = MessageType.BroadcastInfo;
                    messageItems.Add(commentData.Stamp);
                }
                MessageItems = messageItems;
            }
            Init();
        }
        protected virtual void PlaySound(string filePath)
        {
            var player = new System.Media.SoundPlayer(filePath);
            player.Play();
        }
        public override async Task AfterCommentAdded()
        {
            await Task.Yield();
            try
            {
                if (IsStamp)
                {
                    if (_siteOptions.IsPlayStampMusic && !string.IsNullOrEmpty(_siteOptions.StampMusicFilePath))
                    {
                        PlaySound(_siteOptions.StampMusicFilePath);
                    }
                }
                if (IsYell)
                {
                    if (_siteOptions.IsPlayYellMusic && !string.IsNullOrEmpty(_siteOptions.YellMusicFilePath))
                    {
                        PlaySound(_siteOptions.YellMusicFilePath);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }
    }
}
