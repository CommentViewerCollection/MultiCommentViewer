namespace NicoSitePlugin.Metadata
{
    class Ping : IMetaMessage
    {
        public string Raw => "{\"type\":\"ping\"}";
    }
}