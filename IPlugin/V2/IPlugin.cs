using Mcv.PluginV2.Messages;
using Mcv.PluginV2.Messages.ToCore;
using Mcv.PluginV2.Messages.ToPlugin;


namespace Mcv.PluginV2
{
    public interface IPlugin : IPluginInfo
    {
        IPluginHost Host { get; set; }
        void SetMessage(ISetMessageToPluginV2 message);
        void SetMessage(INotifyMessageV2 message);
        void OnLoaded();
        void OnLoading();
        void OnClosing();
    }
    public interface IPluginHost
    {
        void SetMessage(IMessageToCore message);
        IAnswerMessageToPlugin RequestMessage(IRequestMessageToCore message);
        void SetMessage(Messages.ISetMessageToCoreV2 message);
        Messages.IReplyMessageV2 RequestMessage(Messages.IRequestMessageV2 message);
    }
}
