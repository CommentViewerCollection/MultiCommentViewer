using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SitePlugin;
using Common;
namespace MultiCommentViewer.Test
{
    public class SitePluginLoaderTest : ISitePluginLoader
    {
        public IEnumerable<ISiteContext> LoadSitePlugins(IOptions options, ILogger logger)
        {
            return new List<ISiteContext>
            {
                new TwitchSitePlugin.TwitchSiteContext(options, logger),
                //new TestSiteContext(options, logger),
                new YouTubeLiveSitePlugin.Old.YouTubeLiveSiteContext(options),
                //new NicoSitePlugin.Test.NicoSiteContext(options),
            };
        }
    }
}
