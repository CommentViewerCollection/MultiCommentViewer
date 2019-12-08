namespace MildomSitePlugin
{
    interface ILiveInfo
    {
        string Broadcastkey { get; }
        string BcsvrKey { get; }
        int BroadcastPort { get; }
        long CreatedAt { get; }
        long HeartbeatedAt { get; }
        bool IsLive { get; }
        bool IsPrivate { get; }
        string LiveId { get; }
        string Title { get; }
        long OnlineUserNum { get; }
        long TotalViewerNum { get; }
        long StartedAt { get; }

    }
}
