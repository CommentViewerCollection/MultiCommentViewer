namespace Mcv.PluginV2.Messages
{
    public class RequestAddConnection : ISetMessageToCoreV2 { }

    public class NotifyConnectionAdded : INotifyMessageV2
    {
        public NotifyConnectionAdded(IConnectionStatus connSt)
        {
            ConnSt = connSt;
        }

        public IConnectionStatus ConnSt { get; }
    }
}