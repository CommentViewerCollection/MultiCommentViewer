using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System.Windows;
using System.Windows.Input;

namespace YouTubeLiveSitePlugin.Test2
{
    class CommentPostPanelViewModel:ViewModelBase
    {

        private bool _canPostComment;
        public bool CanPostComment
        {
            get { return _canPostComment; }
            set
            {
                if (_canPostComment == value) return;
                _canPostComment = value;
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
        private string _comment;
        private readonly CommentProvider _commentProvider;

        public string Comment
        {
            get { return _comment; }
            set
            {
                if (_comment == value) return;
                _comment = value;
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
