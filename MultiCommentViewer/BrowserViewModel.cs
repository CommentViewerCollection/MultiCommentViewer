using GalaSoft.MvvmLight;
using ryu_s.BrowserCookie;

//TODO:過去コメントの取得


namespace MultiCommentViewer
{
    public class BrowserViewModel : ViewModelBase
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
