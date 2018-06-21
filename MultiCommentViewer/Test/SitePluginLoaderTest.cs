using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SitePlugin;
using Common;
using System.Windows.Threading;
namespace MultiCommentViewer.Test
{
    public class SitePluginLoaderTest : ISitePluginLoader
    {
        public IEnumerable<ISiteContext> LoadSitePlugins(ICommentOptions options, ILogger logger, IUserStore userStore,Dictionary<ISiteContext, IUserStore> userStoreDict, Dispatcher dispatcher)
        {

            var list = new List<ISiteContext>
            {   
                //new LineLiveSitePlugin.LineLiveSiteContext(options,logger,userStore),
                new YouTubeLiveSitePlugin.Test2.YouTubeLiveSiteContext(options, logger, userStore),
                new OpenrecSitePlugin.OpenrecSiteContext(options, logger, userStore),
                new TwitchSitePlugin.TwitchSiteContext(options, logger, userStore, dispatcher),
                new NicoSitePlugin.NicoSiteContext(options, logger, userStore),
                new TwicasSitePlugin.TwicasSiteContext(options,logger, userStore),
#if DEBUG
                new TestSiteContext(options, logger),
#endif
            };
            foreach(var site in list)
            {
                userStoreDict.Add(site, userStore);
            }
            return list;
        }
    }
}
