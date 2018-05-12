namespace YouTubeLiveCommentViewer
{
    public class CommentData : Plugin.ICommentData
    {
        public string ThumbnailUrl { get; set; }
        public int ThumbnailWidth { get; set; }
        public int ThumbnailHeight { get; set; }
        public string Id { get; set; }

        public string UserId { get; set; }

        public string Nickname { get; set; }

        public string Comment { get; set; }
        public bool IsNgUser { get; set; }

        public bool IsFirstComment { get; set; }
    }
}
