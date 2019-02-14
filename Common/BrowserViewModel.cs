using ryu_s.BrowserCookie;

namespace Common
{
    public class BrowserViewModel
    {
        public override int GetHashCode()
        {
            return Browser.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            if(obj is BrowserViewModel vm)
            {
                return this.Browser.Equals(vm.Browser);
            }
            return false;
        }
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
