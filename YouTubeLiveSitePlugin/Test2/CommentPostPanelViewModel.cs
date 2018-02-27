using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System.Windows;
using System.Windows.Input;

namespace YouTubeLiveSitePlugin.Test2
{
    class CommentPostPanelViewModel:ViewModelBase
    {

        private bool _CanPostComment;
        public bool CanPostComment
        {
            get { return _CanPostComment; }
            set
            {
                if (_CanPostComment == value) return;
                _CanPostComment = value;
                RaisePropertyChanged();
            }
        }
        public ICommand PostCommentCommand { get; }
        private async void PostComment()
        {
            if (CanPostComment)
            {
                CanPostComment = false;
                await _commentProvider.PostCommentAsync(Comment);
                Comment = "";
                CanPostComment = true;
            }
        }
        private string _Comment;
        private readonly CommentProvider _commentProvider;

        public string Comment
        {
            get { return _Comment; }
            set
            {
                if (_Comment == value) return;
                _Comment = value;
                RaisePropertyChanged();
            }
        }
        public CommentPostPanelViewModel(CommentProvider commentProvider)
        {
            _commentProvider = commentProvider;
            _commentProvider.LoggedInStateChanged += (s, e) =>
            {
                CanPostComment = _commentProvider.IsLoggedIn;
                if (!CanPostComment)
                {
                    Comment = "未ログインのためコメントできません";
                }
                else
                {
                    Comment = "";
                }
            };
            PostCommentCommand = new RelayCommand(PostComment);
        }
        public CommentPostPanelViewModel()
        {
        }
    }
}
