using System.Collections.Generic;
using ryu_s.BrowserCookie;

namespace YouTubeLiveCommentViewer
{
    public class BrowserLoader : IBrowserLoader
    {
        public IEnumerable<IBrowserProfile> LoadBrowsers()
        {
            var list = new List<IBrowserProfile>();
            var chromeManger = new ChromeManager();
            list.AddRange(chromeManger.GetProfiles());

            var firefoxManager = new FirefoxManager();
            list.AddRange(firefoxManager.GetProfiles());

            var operaManager = new OperaManager();
            list.AddRange(operaManager.GetProfiles());
            return list;
        }
    }
}
