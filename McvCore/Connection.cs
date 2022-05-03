using Mcv.PluginV2;

namespace McvCore;

class Connection : IConnectionStatus
{
    public string Name { get; set; } = "";
    public PluginId SelectedSite { get; set; }
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
        if (req.SelectedSite != null)
        {
            result.SelectedSite = SelectedSite = req.SelectedSite;
        }
        return result;
    }
    public Connection(ConnectionId connId, PluginId selectedSite)
    {
        Id = connId;
        SelectedSite = selectedSite;
    }
}
