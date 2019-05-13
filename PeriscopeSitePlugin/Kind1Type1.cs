namespace PeriscopeSitePlugin
{
    internal class Kind1Type1 : IInternalMessage
    {
        public string Body { get; }
        public string DisplayName { get; }
        public long ParticipantIndex { get; }
        public string ProfileImageUrl { get; }
        public string RemoteId { get; }
        public long Timestamp { get; }
        public long Type { get; }
        public string Username { get; }
        public string UserId { get; }
        public string Uuid { get; }
        public long V { get; }
        public InternalMessageType MessageType => InternalMessageType.Chat_CHAT;
        public string Raw { get; }
        public Kind1Type1(Low.kind1type1.RootObject obj, Low.kind1payloadtype1.Sender sender, string raw)
        {
            Body = obj.Body;
            DisplayName = obj.DisplayName;
            ParticipantIndex = obj.ParticipantIndex;
            ProfileImageUrl = sender.ProfileImageUrl;
            RemoteId = obj.RemoteId;
            Timestamp = obj.Timestamp;
            Type = obj.Type;
            Username = obj.Username;
            UserId = sender.UserId;
            Uuid = obj.Uuid;
            V = obj.V;
            Raw = raw;
        }
        public Kind1Type1(Low.kind1payloadtype1.RootObject payload, Low.kind1type1_newtype.RootObject obj, Low.kind1payloadtype1.Sender sender, string raw)
        {
            Body = obj.Body;
            DisplayName = sender.DisplayName;
            ParticipantIndex = obj.ParticipantIndex;
            ProfileImageUrl = sender.ProfileImageUrl;
            RemoteId = sender.UserId;
            Timestamp = payload.Timestamp;
            Type = obj.Type;
            Username = sender.Username;
            UserId = sender.UserId;
            Uuid = payload.Uuid;
            V = obj.V;
            Raw = raw;
        }
    }
}
