namespace NicoSitePlugin
{
    public interface IThread
    {
        int? LastRes { get; }
        string Raw { get; }
        int? Resultcode { get; }
        int? Revision { get; }
        long? ServerTime { get; }
        string ThreadId { get; }
        string Ticket { get; }
    }
}