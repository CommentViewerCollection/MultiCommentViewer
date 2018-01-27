using System;
using System.Collections.Generic;
using SitePlugin;
namespace MultiCommentViewer.Test
{
    public class SitePluginManager : ISitePluginManager
    {
        List<ISiteContext> _sites;
        public void LoadSitePlugins(IOptions options)
        {
            _sites = new List<ISiteContext>
            {
                new TestSiteContext(options),
            };
        }
    }
}
