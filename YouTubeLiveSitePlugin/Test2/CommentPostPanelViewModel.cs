using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System.Windows;
using System.Windows.Input;

namespace YouTubeLiveSitePlugin.Test2
{
    class CommentPostPanelViewModel:ViewModelBase
    {
        public ICommand PostCommentCommand { get; }
        private async void PostComment()
        {
            await _commentProvider.PostCommentAsync(Comment);
            Comment = "";
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
            PostCommentCommand = new RelayCommand(PostComment);
        }
        public CommentPostPanelViewModel()
        {
        }
    }
}
