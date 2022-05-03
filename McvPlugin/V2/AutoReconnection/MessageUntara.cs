using Mcv.PluginV2.AutoReconnector;

namespace Mcv.PluginV2.AutoReconnection
{
    public class MessageUntara
    {
        public event EventHandler<SystemInfoEventArgs> SystemInfoReiceved;
        public void Set(string message, InfoType type)
        {
            SystemInfoReiceved?.Invoke(this, new SystemInfoEventArgs(message, type));
        }
    }
}
