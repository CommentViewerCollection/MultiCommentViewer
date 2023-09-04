using Mcv.PluginV2.Messages;


namespace Mcv.PluginV2
{
    public interface IPlugin : IPluginInfo
    {
        IPluginHost Host { get; set; }
        Task SetMessage(ISetMessageToPluginV2 message);
        Task SetMessage(INotifyMessageV2 message);
        Task<IReplyMessageToPluginV2> RequestMessage(IGetMessageToPluginV2 message);
    }
    public interface IPluginHost
    {
        Task SetMessageAsync(ISetMessageToCoreV2 message);
        Task SetMessageAsync(INotifyMessageV2 message);
        Task<IReplyMessageToPluginV2> RequestMessageAsync(IGetMessageToCoreV2 message);
    }
}
