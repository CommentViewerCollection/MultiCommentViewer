namespace Mcv.PluginV2.Messages;

public class SetMetadata : ISetMessageToCoreV2
{
    public SetMetadata(ConnectionId connId, PluginId siteId, IMetadata e)
    {
        ConnId = connId;
        SiteId = siteId;
        Metadata = e;
    }

    public ConnectionId ConnId { get; }
    public PluginId SiteId { get; }
    public IMetadata Metadata { get; }
    public string Raw
    {
        get
        {
            return "";
        }
    }
}
public class NotifyMetadataUpdated : INotifyMessageV2
{
    public NotifyMetadataUpdated(ConnectionId connId, PluginId siteId, IMetadata e)
    {
        ConnId = connId;
        SiteId = siteId;
        Metadata = e;
    }

    public ConnectionId ConnId { get; }
    public PluginId SiteId { get; }
    public IMetadata Metadata { get; }
    public string Raw
    {
        get
        {
            return "";
        }
    }
}
