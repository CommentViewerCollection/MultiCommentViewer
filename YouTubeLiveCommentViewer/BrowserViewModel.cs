using ryu_s.BrowserCookie;

namespace YouTubeLiveCommentViewer
{
    public class BrowserViewModel
    {
        public string DisplayName
        {
            get
            {
                if (string.IsNullOrEmpty(_browser.ProfileName))
                {
                    return $"{_browser.Type}";
                }
                else
                {
                    return $"{_browser.Type}({_browser.ProfileName})";
                }
            }
        }
        public IBrowserProfile Browser { get { return _browser; } }
        private readonly IBrowserProfile _browser;
        public BrowserViewModel(IBrowserProfile browser)
        {
            _browser = browser;
        }
    }
}
