using System;
using SitePlugin;
namespace NicoSitePlugin.Test
{
    public class NicoSiteContext : ISiteContext
    {
        public Guid Guid => new Guid("5A477452-FF28-4977-9064-3A4BC7C63252");
        public string DisplayName => "ニコ生";

        public IOptionsTabPage TabPanel
        {
            get
            {
                var panel = new NicoOptionsPanel();
                return new NicoOptionsTabPage(DisplayName, panel);
            }
        }

        public ICommentProvider CreateCommentProvider(ConnectionName connectionName)
        {
            return new NicoCommentProvider(connectionName, _options, _siteOptions);
        }

        public void LoadOptions(string path)
        {
            _siteOptions = new NicoSiteOptions();
        }

        public void SaveOptions(string path)
        {
        }
        private NicoSiteOptions _siteOptions;
        private readonly IOptions _options;
        public NicoSiteContext(IOptions options)
        {
            _options = options;
        }
    }
}
