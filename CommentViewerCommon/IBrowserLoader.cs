using System.Collections.Generic;
using ryu_s.BrowserCookie;

namespace CommentViewerCommon
{
    public interface IBrowserLoader
    {
        IEnumerable<IBrowserProfile> LoadBrowsers();
    }
}
