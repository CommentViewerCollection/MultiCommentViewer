using Mcv.PluginV2;

namespace McvCore;

class ConnectionStatusDiff : IConnectionStatusDiff
{
    public ConnectionId Id { get; }
    public string? Name { get; set; }
    public PluginId? SelectedSite { get; set; }
    public BrowserProfileId? SelectedBrowser { get; set; }
    public bool? IsConnected { get; set; }
    public bool? CanConnect { get; set; }
    public bool? CanDisconnect { get; set; }

    public ConnectionStatusDiff(ConnectionId id)
    {
        Id = id;
    }
}
