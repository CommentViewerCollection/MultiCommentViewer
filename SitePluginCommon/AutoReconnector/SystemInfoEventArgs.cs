using Common;
using System;

namespace SitePluginCommon.AutoReconnector
{
    public class SystemInfoEventArgs : EventArgs
    {
        public string Message { get; }
        public InfoType Type { get; }
        public SystemInfoEventArgs(string message, InfoType type)
        {
            Message = message;
            Type = type;
        }
    }
}
