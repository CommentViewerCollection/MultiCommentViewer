using Common;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Windows;
using System.Windows.Input;

namespace BigoSitePlugin
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
            try
            {
                if (CanPostComment)
                {
                    var commentTemp = Comment;
                    Comment = "";
                    var success = await _commentProvider.PostCommentAsync(commentTemp);
                    if (!success)
                    {
                        Comment = commentTemp;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
            }
        }
        private string _comment;
        private readonly CommentProvider _commentProvider;
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
        public CommentPostPanelViewModel(CommentProvider commentProvider, ILogger logger)
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
