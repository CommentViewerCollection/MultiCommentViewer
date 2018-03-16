namespace NicoSitePlugin.Next
{
    interface IXmlWsRoomInfo :IRoomInfo
    {
        string XmlSocketAddr { get; }
        int XmlSocketPort { get; }
        string WebSocketUri { get; }
        string Name { get; }
        long Id { get; }
        string ThreadId { get; }
    }
}
