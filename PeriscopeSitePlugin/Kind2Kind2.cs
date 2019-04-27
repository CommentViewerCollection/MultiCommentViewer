namespace PeriscopeSitePlugin
{
    internal class Kind2Kind2 : IInternalMessage
    {
        public string RoomId { get; }
        public bool Following { get; }
        public bool Unlimited { get; }
        public InternalMessageType MessageType => InternalMessageType.Control_LEAVE;
        public string Raw { get; }
        public string UserId { get; }
        public string DisplayName { get; }
        public string Username { get; }
        public string ProfileImageUrl { get; }
        public Kind2Kind2(Low.kind2kind2.RootObject kind2kind2Low, Low.kind2payloadkind2.Sender sender, string raw)
        {
            Raw = raw;
            RoomId = kind2kind2Low.Room;
            Following = kind2kind2Low.Following;
            Unlimited = kind2kind2Low.Unlimited;
            UserId = sender.UserId;
            DisplayName = sender.DisplayName;
            Username = sender.Username;
            ProfileImageUrl = sender.ProfileImageUrl;
        }
    }
}