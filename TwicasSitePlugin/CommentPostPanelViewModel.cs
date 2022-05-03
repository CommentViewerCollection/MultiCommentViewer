using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mcv.PluginV2;
using System;
using System.Windows.Input;

namespace TwicasSitePlugin
{
    class CommentPostPanelViewModel : ObservableObject
    {

        private bool _canPostComment;
        public bool CanPostComment
        {
            get { return _canPostComment; }
            set
            {
                if (_canPostComment == value) return;
                _canPostComment = value;
                OnPropertyChanged();
            }
        }
        public ICommand PostCommentCommand { get; }
        private async void PostComment()
        {
            if (CanPostComment)
            {
                var commentTemp = Comment;
                Comment = "";
                try
                {
                    await _commentProvider.PostCommentAsync(commentTemp);
                }
                catch (Exception ex)
                {
                    _logger.LogException(ex, "", $"Comment={commentTemp}");
                    Comment = commentTemp;
                }
            }
        }
        private string _comment;
        private readonly TwicasCommentProvider2 _commentProvider;
        private readonly ILogger _logger;

        public string Comment
        {
            get { return _comment; }
            set
            {
                if (_comment == value) return;
                _comment = value;
                OnPropertyChanged();
            }
        }
        public CommentPostPanelViewModel(TwicasCommentProvider2 commentProvider, ILogger logger)
        {
            _commentProvider = commentProvider;
            _logger = logger;
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
