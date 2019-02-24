namespace NicoSitePlugin
{
    interface IRoomInfo
    {
        string Addr { get; }
        int Port { get; }
        string Thread { get; }
        string RoomLabel { get; }
    }
}
