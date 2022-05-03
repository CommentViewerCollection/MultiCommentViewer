using Mcv.PluginV2.Messages;


namespace Mcv.PluginV2
{
    public interface IPlugin : IPluginInfo
    {
        IPluginHost Host { get; set; }
        void SetMessage(ISetMessageToPluginV2 message);
        void SetMessage(INotifyMessageV2 message);
        IReplyMessageToPluginV2 RequestMessage(IGetMessageToPluginV2 message);
    }
    public interface IPluginHost
    {
        void SetMessage(ISetMessageToCoreV2 message);
        void SetMessage(INotifyMessageV2 message);
        IReplyMessageToPluginV2 RequestMessage(IGetMessageToCoreV2 message);
    }
}
