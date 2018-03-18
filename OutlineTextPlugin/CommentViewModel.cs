using Plugin;

namespace OutlineTextPlugin
{
    class CommentViewModel
    {
        public string ThumbnailUrl { get; }
        public int ThumbnailWidth { get; }
        public int ThumbnailHeight { get; }
        public string Name { get; }
        public string Message { get; }
        public CommentViewModel(ICommentData comment)
        {
            ThumbnailUrl = comment.ThumbnailUrl;
            ThumbnailWidth = comment.ThumbnailWidth;
            ThumbnailHeight = comment.ThumbnailHeight;
            Name = comment.Nickname;
            Message = comment.Comment;
        }
    }
}
