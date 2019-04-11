namespace PeriscopeSitePlugin
{
    internal class WebsocketMessage : IWebsocketMessage
    {
        public int Kind { get; set; }
        public string Payload { get; set; }
        public string Raw { get; set; }
    }
}
