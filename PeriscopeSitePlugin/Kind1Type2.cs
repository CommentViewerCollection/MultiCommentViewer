namespace PeriscopeSitePlugin
{
    internal class Kind1Type2:IInternalMessage
    {
        public string DisplayName { get; }
        public long Timestamp { get; }

        public InternalMessageType MessageType => InternalMessageType.Chat_HEART;

        public string Raw { get; }
        public Kind1Type2(Low.kind1type2.RootObject obj, string raw)
        {
            DisplayName = obj.DisplayName;
            Timestamp = obj.Timestamp;
            Raw = raw;
        }
    }
}
