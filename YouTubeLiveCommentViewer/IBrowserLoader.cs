using System.Collections.Generic;
using ryu_s.BrowserCookie;

namespace YouTubeLiveCommentViewer
{
    public interface IBrowserLoader
    {
        IEnumerable<IBrowserProfile> LoadBrowsers();
    }
}
