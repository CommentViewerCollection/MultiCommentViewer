using System;
using SitePlugin;
using System.Diagnostics;
using Common;
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
        private string GetOptionsPath(string dir)
        {
            return System.IO.Path.Combine(dir, DisplayName + ".txt");
        }
        public void LoadOptions(string path, IIo io)
        {
            _siteOptions = new NicoSiteOptions();
            try
            {
                var s = io.ReadFile(path);
                
                _siteOptions.Deserialize(s);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        public void SaveOptions(string path, IIo io)
        {
            var s = _siteOptions.Serialize();
            io.WriteFile(path, s);
        }
        private NicoSiteOptions _siteOptions;
        private readonly IOptions _options;
        public NicoSiteContext(IOptions options)
        {
            _options = options;
        }
    }
}
