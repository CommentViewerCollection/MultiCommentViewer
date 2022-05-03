using Mcv.PluginV2;
using Mcv.PluginV2.Messages;

namespace McvCore;

class PluginHost : IPluginHost
{
    private readonly McvCore _core;

    public IReplyMessageToPluginV2 RequestMessage(IGetMessageToCoreV2 message)
    {
        return _core.RequestMessage(message);
    }
    public void SetMessage(ISetMessageToCoreV2 message)
    {
        _core.SetMessage(message);
    }
    public void SetMessage(INotifyMessageV2 message)
    {
        _core.SetMessage(message);
    }
    public void SetMessage(PluginId target, ISetMessageToPluginV2 message)
    {
        _core.SetMessage(target, message);
    }
    public PluginHost(McvCore core)
    {
        _core = core;
    }
}
