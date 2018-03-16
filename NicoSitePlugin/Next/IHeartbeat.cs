namespace NicoSitePlugin.Next
{
    public interface IHeartbeat
    {
        string CommentCount { get; }
        string Isrestrict { get; }
        string Status { get; }
        string Ticket { get; }
        string Time { get; }
        string WaitTime { get; }
        string WatchCount { get; }
    }
}
