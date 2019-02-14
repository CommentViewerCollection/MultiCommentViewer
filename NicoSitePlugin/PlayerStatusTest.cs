namespace NicoSitePlugin
{
    public class PlayerStatusTest : IPlayerStatus
    {
        public string Raw { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public string OwnerId { get; set; }

        public string OwnerName { get; set; }

        public ProviderType ProviderType { get; set; }

        public int BaseTime { get; set; }

        public int OpenTime { get; set; }

        public int StartTime { get; set; }

        public int? EndTime { get; set; }

        public string UserId { get; set; }

        public string Nickname { get; set; }

        public string RoomLabel { get; set; }

        public int RoomSeetNo { get; set; }

        public bool IsJoin { get; set; }

        public string IsPremium { get; set; }

        public IMs Ms { get; set; }

        public IMs[] MsList { get; set; }

        public string DefaultCommunity { get; set; }
    }
}
