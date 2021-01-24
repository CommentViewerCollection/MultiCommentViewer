namespace NicoSitePlugin.Metadata
{
    class Disconnect : IMetaMessage
    {
        public string Reason { get; }
        public Disconnect(string reason)
        {
            Reason = reason;
        }
        public string Raw => $"{{\"type\":\"disconnect\",\"data\":{{\"reason\":\"{Reason}\"}}}}";
    }
}