namespace NicoSitePlugin.Next
{
    public interface IPlayerStatusResponse
    {
        bool Success { get; }
        IPlayerStatus PlayerStatus { get; }
        IPlayerStatusError Error { get; }
    }
}
