namespace NicoSitePlugin.Next
{
    public interface IPlayerStatus
    {
        string Raw { get; }
        string Title { get; }
        string Description { get; }
        string DefaultCommunity { get; }
        /// <summary>
        /// 配信者のUserID
        /// </summary>
        /// <remarks>名前はownerだけど、実際には配信者のID。コミュニティのオーナーアカウントのIDでは無い</remarks>
        string OwnerId { get; }

        string OwnerName { get; }
        ProviderType ProviderType { get; }
        int BaseTime { get; }
        int OpenTime { get; }
        int StartTime { get; }
        int? EndTime { get; }
        string UserId { get; }
        string Nickname { get; }
        string RoomLabel { get; }
        int RoomSeetNo { get; }
        bool IsJoin { get; }
        string IsPremium { get; }
        IMs Ms { get; }
        IMs[] MsList { get; }

    }
}
