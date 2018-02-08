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
                panel.SetViewModel(new NicoSiteOptionsViewModel(_siteOptions));
                return new NicoOptionsTabPage(DisplayName, panel);
            }
        }

        public ICommentProvider CreateCommentProvider(ConnectionName connectionName)
        {
            return new NicoCommentProvider(connectionName, _options, _siteOptions);
        }

        public void LoadOptions(string siteOptionsStr)
        {
            _siteOptions = new NicoSiteOptions();
        }

        public string SaveOptions()
        {
            throw new NotImplementedException();
        }
        private NicoSiteOptions _siteOptions;
        private readonly IOptions _options;
        public NicoSiteContext(IOptions options)
        {
            _options = options;
        }
    }
}
