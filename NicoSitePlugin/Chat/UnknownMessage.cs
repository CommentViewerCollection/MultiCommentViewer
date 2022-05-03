namespace NicoSitePlugin.Chat
{
    class UnknownMessage : IChatMessage
    {
        public UnknownMessage(string raw)
        {
            Raw = raw;
        }

        public string Raw { get; }
    }
}
