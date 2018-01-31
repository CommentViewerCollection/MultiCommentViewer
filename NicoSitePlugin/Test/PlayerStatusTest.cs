namespace NicoSitePlugin.Test
{
    public class PlayerStatusTest : IPlayerStatus
    {
        public string Raw { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public string OwnerId { get; set; }

        public string OwnerName { get; set; }

        public ProviderType ProviderType { get; set; }

        public long BaseTime { get; set; }

        public long OpenTime { get; set; }

        public long StartTime { get; set; }

        public long? EndTime { get; set; }

        public string UserId { get; set; }

        public string Nickname { get; set; }

        public string RoomLabel { get; set; }

        public int RoomSeetNo { get; set; }

        public bool IsJoin { get; set; }

        public IMs Ms { get; set; }

        public IMs[] MsList { get; set; }
    }
}
