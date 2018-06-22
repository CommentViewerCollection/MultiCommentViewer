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
        public IEnumerable<ISiteContext> LoadSitePlugins(ICommentOptions options, ILogger logger, IUserStore userStore,Dictionary<ISiteContext, IUserStore> userStoreDict)
        {

            var list = new List<ISiteContext>
            {   
                //new LineLiveSitePlugin.LineLiveSiteContext(options,logger,userStore),
                new YouTubeLiveSitePlugin.Test2.YouTubeLiveSiteContext(options, new YouTubeLiveSitePlugin.Test2.YouTubeLiveServer(), logger, userStore),
                new OpenrecSitePlugin.OpenrecSiteContext(options, logger, userStore),
                new TwitchSitePlugin.TwitchSiteContext(options,new TwitchSitePlugin.TwitchServer(),()=>new TwitchSitePlugin.MessageProvider(), logger, userStore),
                new NicoSitePlugin.NicoSiteContext(options,new NicoSitePlugin.DataSource(), (addr,port,size,buffer)=> new NicoSitePlugin.StreamSocket(addr,port,size,buffer), logger, userStore),
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
