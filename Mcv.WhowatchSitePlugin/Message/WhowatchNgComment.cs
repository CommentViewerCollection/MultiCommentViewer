namespace WhowatchSitePlugin
{
    internal class WhowatchNgComment : WhowatchComment, IWhowatchNgComment
    {
        public string OriginalMessage { get; set; }
        public WhowatchNgComment(string raw) : base(raw)
        {
        }
    }
}
