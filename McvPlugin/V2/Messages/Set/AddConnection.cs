namespace Mcv.PluginV2.Messages;

public class RequestAddConnection : ISetMessageToCoreV2 { }

public record NotifyConnectionAdded(IConnectionStatus ConnSt) : INotifyMessageV2
{
    public string Raw
    {
        get
        {
            return "";
        }
    }
}