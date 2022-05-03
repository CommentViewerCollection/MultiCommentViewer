namespace Mcv.PluginV2
{
    public interface IConnectionStatus
    {
        ConnectionId Id { get; }
        string Name { get; }
        //string Input { get; }
        PluginId SelectedSite { get; }
        //BrowserProfileId SelectedBrowser { get; }
        bool IsConnected { get; }
        bool CanConnect { get; }
        bool CanDisconnect { get; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        IConnectionStatusDiff SetDiff(IConnectionStatusDiff req);
    }
}