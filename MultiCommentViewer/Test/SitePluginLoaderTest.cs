﻿using System;
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
        public IEnumerable<ISiteContext> LoadSitePlugins(IOptions options, ILogger logger, IUserStore userStore, Dispatcher dispatcher)
        {
            return new List<ISiteContext>
            {
                new YouTubeLiveSitePlugin.Test2.YouTubeLiveSiteContext(options, logger),
                new TwitchSitePlugin.TwitchSiteContext(options, logger, userStore, dispatcher),
                new NicoSitePlugin.Old.NicoSiteContext(options, logger, userStore),
                new TwicasSitePlugin.TwicasSiteContext(options,logger, userStore, dispatcher),
#if DEBUG
                new TestSiteContext(options, logger),
#endif
            };
        }
    }
}
