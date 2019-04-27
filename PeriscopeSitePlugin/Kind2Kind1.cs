namespace PeriscopeSitePlugin
{
    internal class Kind2Kind1 : IInternalMessage
    {
        public string RoomId { get; }
        public bool Following { get; }
        public bool Unlimited { get; }
        public InternalMessageType MessageType => InternalMessageType.Control_JOIN;
        public string Raw { get; }
        public string UserId { get; }
        public string DisplayName { get; }
        public string Username { get; }
        public string ProfileImageUrl { get; }
        public Kind2Kind1(Low.kind2kind1.RootObject kind2kind1Low, Low.kind2payloadkind1.Sender sender, string raw)
        {
            Raw = raw;
            RoomId = kind2kind1Low.Room;
            Following = kind2kind1Low.Following;
            Unlimited = kind2kind1Low.Unlimited;
            UserId = sender.UserId;
            DisplayName = sender.DisplayName;
            Username = sender.Username;
            ProfileImageUrl = sender.ProfileImageUrl;
        }
    }
}