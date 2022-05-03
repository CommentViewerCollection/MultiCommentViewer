using Mcv.PluginV2;

namespace Mcv.MainViewPlugin
{
    class ConnectionStatusDiff : IConnectionStatusDiff
    {
        public ConnectionId Id { get; }
        public string Name { get; set; }
        public string Input { get; set; }
        public SiteId SelectedSite { get; set; }
        public BrowserProfileId SelectedBrowser { get; set; }
        public bool? IsConnected { get; set; }
        public bool? CanConnect { get; set; }
        public bool? CanDisconnect { get; set; }

        public ConnectionStatusDiff(ConnectionId id)
        {
            Id = id;
        }
    }
}
