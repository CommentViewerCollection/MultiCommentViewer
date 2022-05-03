namespace Mcv.PluginV2.Messages
{
    public class NotifyConnectionStatusChanged : INotifyMessageV2
    {
        public NotifyConnectionStatusChanged(IConnectionStatusDiff connStDiff)
        {
            ConnStDiff = connStDiff;
        }

        public IConnectionStatusDiff ConnStDiff { get; }
    }
    public class RequestChangeConnectionStatus : ISetMessageToCoreV2
    {
        public RequestChangeConnectionStatus(IConnectionStatusDiff connStDiff)
        {
            ConnStDiff = connStDiff;
        }

        public IConnectionStatusDiff ConnStDiff { get; }
    }
}
