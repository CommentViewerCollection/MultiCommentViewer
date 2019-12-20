using Common;
using SitePluginCommon.AutoReconnector;
using System;

namespace SitePluginCommon.AutoReconnection
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
