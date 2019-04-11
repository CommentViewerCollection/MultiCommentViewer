namespace PeriscopeSitePlugin
{
    public interface IWebsocketMessage
    {
        int Kind { get; }
        string Payload { get; }
        string Raw { get; }
    }
}
