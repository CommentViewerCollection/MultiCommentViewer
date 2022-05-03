using System.Collections.Generic;

namespace Mcv.PluginV2.Messages
{
    public class NotifyConnectionStatusList : INotifyMessageV2
    {
        public NotifyConnectionStatusList(List<IConnectionStatus> connections)
        {
            Connections = connections;
        }

        public List<IConnectionStatus> Connections { get; }
        public string Raw
        {
            get
            {
                return "";
            }
        }
    }
}
