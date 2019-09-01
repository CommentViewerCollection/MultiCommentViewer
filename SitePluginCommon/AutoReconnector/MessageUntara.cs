using Common;
using System;

namespace SitePluginCommon.AutoReconnector
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
