using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SitePlugin;
using Common;
using System.Windows.Threading;
using System.Windows.Controls;
using SitePluginCommon;

namespace MultiCommentViewer.Test
{
    public class SitePluginLoaderTest : ISitePluginLoader
    {
        Dictionary<Guid, ISiteContext> _dict = new Dictionary<Guid, ISiteContext>();
        public IEnumerable<(string displayName, Guid guid)> LoadSitePlugins(ICommentOptions options, ILogger logger, IUserStoreManager userStoreManager, string userAgent)
        {
            var list = new List<ISiteContext>
            {
                new YouTubeLiveSitePlugin.Test2.YouTubeLiveSiteContext(options, new YouTubeLiveSitePlugin.Test2.YouTubeLiveServer(), logger, userStoreManager),
                new OpenrecSitePlugin.OpenrecSiteContext(options, logger, userStoreManager),
                new BLiveSitePlugin.BLiveSiteContext(options, logger, userStoreManager),
                new MixchSitePlugin.MixchSiteContext(options, logger, userStoreManager),
                new TwitchSitePlugin.TwitchSiteContext(options,new TwitchSitePlugin.TwitchServer(), logger, userStoreManager),
                new NicoSitePlugin.NicoSiteContext(options,new NicoSitePlugin.DataSource(userAgent), logger, userStoreManager),
                new TwicasSitePlugin.TwicasSiteContext(options,logger, userStoreManager),
                new LineLiveSitePlugin.LineLiveSiteContext(options,new LineLiveSitePlugin.LineLiveServer(), logger, userStoreManager),
                new WhowatchSitePlugin.WhowatchSiteContext(options, logger, userStoreManager),
                new MirrativSitePlugin.MirrativSiteContext(options,new MirrativSitePlugin.MirrativServer(), logger, userStoreManager),
                new PeriscopeSitePlugin.PeriscopeSiteContext(options,new PeriscopeSitePlugin.PeriscopeServer(), logger,userStoreManager),
                new ShowRoomSitePlugin.ShowRoomSiteContext(options,new ShowRoomSitePlugin.ShowRoomServer(), logger,userStoreManager),
                new MildomSitePlugin.MildomSiteContext(options, new MildomSitePlugin.MildomServer(),logger, userStoreManager),
                new BigoSitePlugin.BigoSiteContext(options, new BigoSitePlugin.BigoServer(), logger, userStoreManager),
#if DEBUG
                new TestSitePlugin.TestSiteContext(options),
#endif
            };
            foreach (var site in list)
            {
                site.Init();
                _dict.Add(site.Guid, site);
            }
            return _dict.Select(kv => (kv.Value.DisplayName, kv.Key));
        }
        public void Save()
        {
            foreach (var siteContext in GetSiteContexts())
            {
                siteContext.Save();
            }
        }
        private IEnumerable<ISiteContext> GetSiteContexts()
        {
            return _dict.Select(s => s.Value);
        }
        public ISiteContext GetSiteContext(Guid guid)
        {
            return _dict[guid];
        }
        public ICommentProvider CreateCommentProvider(Guid guid)
        {
            var siteContext = GetSiteContext(guid);
            return siteContext.CreateCommentProvider();
        }
        public Guid GetValidSiteGuid(string input)
        {
            foreach (var siteContext in GetSiteContexts())
            {
                var b = siteContext.IsValidInput(input);
                if (b)
                {
                    return siteContext.Guid;
                }
            }
            return Guid.Empty;
        }

        public UserControl GetCommentPostPanel(Guid guid, ICommentProvider commentProvider)
        {
            var site = _dict[guid];
            return site.GetCommentPostPanel(commentProvider);
        }
    }
}
