using Mcv.PluginV2.Messages;


namespace Mcv.PluginV2
{
    public interface IPlugin : IPluginInfo
    {
        IPluginHost Host { get; set; }
        Task SetMessageAsync(ISetMessageToPluginV2 message);
        Task SetMessageAsync(INotifyMessageV2 message);
        Task<IReplyMessageToPluginV2> RequestMessageAsync(IGetMessageToPluginV2 message);
    }
    public interface IPluginHost
    {
        Task SetMessageAsync(ISetMessageToCoreV2 message);
        Task SetMessageAsync(INotifyMessageV2 message);
        Task<IReplyMessageToPluginV2> RequestMessageAsync(IGetMessageToCoreV2 message);
    }
}
