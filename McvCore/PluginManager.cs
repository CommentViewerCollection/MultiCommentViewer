using Akka.Actor;
using Mcv.PluginV2;
using Mcv.PluginV2.Messages;
using McvCore.PluginActorMessages;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace McvCore;
class PluginManagerActor : ReceiveActor
{
    private readonly ConcurrentDictionary<PluginId, IActorRef> _actorDict = new();
    private readonly ConcurrentDictionary<PluginId, IList<string>> _pluginRoleDict = new();
    public static Props Props()
    {
        return Akka.Actor.Props.Create(() => new PluginManagerActor());
    }
    public PluginManagerActor()
    {
        Receive<AddPlugins>(m =>
        {
            foreach (var plugin in m.Plugins)
            {
                AddPlugin(plugin, m.PluginHost);
            }
        });
        Receive<RemovePlugin>(m =>
        {
            RemovePlugin(m.PluginId);
        });
        Receive<SetPluginRole>(m =>
        {
            _pluginRoleDict.TryAdd(m.PluginId, m.PluginRole);
        });
        Receive<GetPluginList>(m =>
        {
            Sender.Tell(GetPluginList());
        });
        Receive<SetNotifyToAPlugin>(m =>
        {
            var target = GetPluginActorById(m.PluginId);
            if (target == null)
            {
                return;
            }
            target.Tell(new NotifyMessageV2(m.Message));
        });
        Receive<SetNotifyToAllPlugin>(m =>
        {
            foreach (var target in GetActors())
            {
                target.Tell(new NotifyMessageV2(m.Message));
            }
        });
        Receive<SetSetToAPlugin>(m =>
        {
            var target = GetPluginActorById(m.PluginId);
            if (target == null)
            {
                return;
            }
            target.Tell(new SetMessageToPluginV2(m.Message));
        });
        Receive<SetSetToAllPlugin>(m =>
        {
            foreach (var target in GetActors())
            {
                target.Tell(new SetMessageToPluginV2(m.Message));
            }
        });
        ReceiveAsync<GetMessage>(async m =>
        {
            Sender.Tell(await RequestMessage(m.PluginId, m.Message));
        });
        Receive<GetDefaultSite>(m =>
        {
            Sender.Tell(GetDefaultSite());
        });
    }
    private static IActorRef CreateActor(IPlugin plugin)
    {
        return Context.ActorOf(PluginActor.Props(plugin).WithDispatcher("akka.actor.synchronized-dispatcher"));
    }
    public void AddPlugin(IPlugin plugin, IPluginHost host)
    {
        var actor = CreateActor(plugin);
        _actorDict.TryAdd(plugin.Id, actor);
        plugin.Host = host;
        actor.Tell(new SetMessageToPluginV2(new SetLoading()));
        //Plugin側がHelloメッセージを送ってくるまで登録はしない。
    }
    internal List<IPluginInfo> GetPluginList()
    {
        //このメソッドはPluginManagerの外部から参照できるから、HelloメッセージによってPluginRoleを登録済みのプラグインのみを返す。
        return _pluginRoleDict.Select(kv => _actorDict[kv.Key]).Cast<IPluginInfo>().ToList();
    }
    private IEnumerable<IActorRef> GetActors()
    {
        return _actorDict.Values;
    }
    internal PluginId? GetDefaultSite()
    {
        return _pluginRoleDict.Where(p => PluginTypeChecker.IsSitePlugin(p.Value)).Select(p => p.Key).FirstOrDefault();
    }
    internal Task<IReplyMessageToPluginV2> RequestMessage(PluginId pluginId, IGetMessageToPluginV2 message)
    {
        var plugin = GetPluginActorById(pluginId);
        if (plugin is null)
        {
            return null;//TODO:
        }
        return plugin.Ask<IReplyMessageToPluginV2>(new GetMessageToPluginV2(message));
    }
    private IActorRef? GetPluginActorById(PluginId pluginId)
    {
        //return _pluginsV2.Find(p => p.Id == pluginId)!;
        if (_actorDict.TryGetValue(pluginId, out var actor))
        {
            return actor;
        }
        else
        {
            return null;
        }
    }
    internal void RemovePlugin(PluginId pluginId)
    {
        try
        {
            _pluginRoleDict.Remove(pluginId, out var _);
            _actorDict.Remove(pluginId, out var _);
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
        }
    }
}
