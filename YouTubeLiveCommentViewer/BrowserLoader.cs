using System.Collections.Generic;
using ryu_s.BrowserCookie;

namespace YouTubeLiveCommentViewer
{
    public class BrowserLoader : IBrowserLoader
    {
        public IEnumerable<IBrowserProfile> LoadBrowsers()
        {
            var list = new List<IBrowserProfile>();
            list.AddRange(new ChromeManager().GetProfiles());
            return list;
        }
    }
}
