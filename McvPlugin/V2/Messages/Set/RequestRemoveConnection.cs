namespace Mcv.PluginV2.Messages
{
    public class RequestRemoveConnection : ISetMessageToCoreV2
    {
        public RequestRemoveConnection(ConnectionId connId)
        {
            ConnId = connId;
        }

        public ConnectionId ConnId { get; }
    }
    public class NotifyConnectionRemoved : INotifyMessageV2
    {
        public NotifyConnectionRemoved(ConnectionId connId)
        {
            ConnId = connId;
        }

        public ConnectionId ConnId { get; }
        public string Raw
        {
            get
            {
                return "";
            }
        }
    }
}
