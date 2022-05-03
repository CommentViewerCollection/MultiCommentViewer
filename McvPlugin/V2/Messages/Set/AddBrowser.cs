namespace Mcv.PluginV2.Messages;

public record NotifyBrowserAdded(BrowserProfileId BrowserProfileId, string BrowserDisplayName, string ProfileDisplayName) : INotifyMessageV2
{
    public string Raw
    {
        get
        {
            return "";
        }
    }
}
