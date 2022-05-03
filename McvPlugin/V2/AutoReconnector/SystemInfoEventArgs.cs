namespace Mcv.PluginV2.AutoReconnector
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
