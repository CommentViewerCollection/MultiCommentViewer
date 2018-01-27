#if DEBUG
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SitePlugin;
namespace MultiCommentViewer.Test
{
    public class SitePluginLoaderTest : ISitePluginLoader
    {
        public IEnumerable<ISiteContext> LoadSitePlugins(IOptions options)
        {
            return new List<ISiteContext>
            {
                //new TestSiteContext(options),
                new YouTubeLiveSitePlugin.Old.YouTubeLiveSiteContext(options),
            };
        }
    }
}
#endif //DEBUG
