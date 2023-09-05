using Akka.Actor;
using Mcv.PluginV2;
using Mcv.Core.PluginActorMessages;

namespace Mcv.Core;

class PluginActor : ReceiveActor
{
    public PluginActor(IPlugin plugin)
    {
        ReceiveAsync<GetMessageToPluginV2>(async m =>
        {
            var reply = await plugin.RequestMessageAsync(m.Message);
            Sender.Tell(reply);
        });
        ReceiveAsync<NotifyMessageV2>(async m =>
        {
            await plugin.SetMessageAsync(m.Message);
        });
        ReceiveAsync<SetMessageToPluginV2>(async m =>
        {
            await plugin.SetMessageAsync(m.Message);
        });
    }
    public static Props Props(IPlugin plugin)
    {
        return Akka.Actor.Props.Create(() => new PluginActor(plugin));
    }
}
