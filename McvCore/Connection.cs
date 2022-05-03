using Mcv.PluginV2;

namespace McvCore
{
    class Connection : IConnectionStatus
    {
        private readonly SitePluginManager _sitePluginManager;
        private readonly BrowserManager _browserManager;

        public string Name { get; set; } = "";
        public string Input { get; set; } = "";
        public SiteId SelectedSite { get; set; }
        public BrowserProfileId SelectedBrowser { get; set; }
        public bool IsConnected => !CanConnect;
        public bool CanConnect { get; set; } = true;
        public bool CanDisconnect { get; set; } = false;
        public ConnectionId Id { get; }

        public IConnectionStatusDiff SetDiff(IConnectionStatusDiff req)
        {
            var result = new ConnectionStatusDiff(req.Id);
            if (req.Name != null)
            {
                result.Name = Name = req.Name;
            }
            if (req.Input != null)
            {
                result.Input = Input = req.Input;
            }
            if (req.SelectedSite != null)
            {

                result.SelectedSite = SelectedSite = req.SelectedSite;
            }
            if (req.SelectedBrowser != null)
            {
                result.SelectedBrowser = SelectedBrowser = req.SelectedBrowser;
            }
            if (req.CanConnect != null && req.CanConnect == false)
            {
                _sitePluginManager.CreateCommentProvider(Id, SelectedSite);
                _sitePluginManager.Connect(Id, Input, _browserManager.GetBrowserProfile(SelectedBrowser));
                result.CanConnect = CanConnect = req.CanConnect.Value;
                result.CanDisconnect = CanDisconnect = !req.CanConnect.Value;
            }
            if (req.CanDisconnect != null && req.CanDisconnect == true)
            {
                _sitePluginManager.Disconnect(Id);
            }
            return result;
        }
        public Connection(ConnectionId connId, SiteId selectedSite, BrowserProfileId selectedBrowserId, SitePluginManager sitePluginManager, BrowserManager browserManager)
        {
            Id = connId;
            SelectedSite = selectedSite;
            SelectedBrowser = selectedBrowserId;
            _sitePluginManager = sitePluginManager;
            _browserManager = browserManager;
        }
    }

}
