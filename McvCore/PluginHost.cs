using Mcv.PluginV2;
using Mcv.PluginV2.Messages;
using System.Threading.Tasks;

namespace Mcv.Core;
class PluginHost : IPluginHost
{
    private readonly McvCoreActor _core;

    public Task<IReplyMessageToPluginV2> RequestMessageAsync(IGetMessageToCoreV2 message)
    {
        return _core.RequestMessageAsync(message);
    }
    public async Task SetMessageAsync(ISetMessageToCoreV2 message)
    {
        await _core.SetMessageAsync(message);
    }
    public Task SetMessageAsync(INotifyMessageV2 message)
    {
        return _core.SetMessageAsync(message);
    }
    public Task SetMessageAsync(PluginId target, ISetMessageToPluginV2 message)
    {
        return _core.SetMessageAsync(target, message);
    }
    public PluginHost(McvCoreActor core)
    {
        _core = core;
    }
}
