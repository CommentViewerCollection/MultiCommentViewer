namespace NicoSitePlugin.Metadata
{
    class UnknownMessage : IMetaMessage
    {
        public string Raw { get; }
        public UnknownMessage(string raw)
        {
            Raw = raw;
        }
    }
}