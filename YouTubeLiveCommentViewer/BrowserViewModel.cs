using ryu_s.BrowserCookie;

namespace YouTubeLiveCommentViewer
{
    public class BrowserViewModel
    {
        public string DisplayName { get { return $"{_browser.Type}({_browser.ProfileName})"; } }
        public IBrowserProfile Browser { get { return _browser; } }
        private readonly IBrowserProfile _browser;
        public BrowserViewModel(IBrowserProfile browser)
        {
            _browser = browser;
        }
    }
}
