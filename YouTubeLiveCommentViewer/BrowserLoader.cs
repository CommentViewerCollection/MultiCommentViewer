using System;
using System.Collections.Generic;
using ryu_s.BrowserCookie;

namespace YouTubeLiveCommentViewer
{
    public class BrowserLoader : IBrowserLoader
    {
        public IEnumerable<IBrowserProfile> LoadBrowsers()
        {
            var list = new List<IBrowserProfile>();
            var managers = new List<IBrowserManager>
            {
                new ChromeManager(),
                new FirefoxManager(),
                new OperaManager(),
            };
            foreach(var manager in managers)
            {
                try
                {
                    list.AddRange(manager.GetProfiles());
                }catch(Exception ex)
                {

                }
            }
            return list;
        }
    }
}
