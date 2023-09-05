using Mcv.PluginV2;
using Mcv.PluginV2.Messages;
using System.Collections.Generic;

namespace Mcv.Core.PluginActorMessages;
internal record RemovePlugin(PluginId PluginId);
internal record GetMessage(PluginId PluginId, IGetMessageToPluginV2 Message);
internal record GetDefaultSite;
internal record SetSetToAllPlugin(ISetMessageToPluginV2 Message);
internal record SetSetToAPlugin(PluginId PluginId, ISetMessageToPluginV2 Message);
internal record SetNotifyToAllPlugin(INotifyMessageV2 Message);
internal record SetNotifyToAPlugin(PluginId PluginId, INotifyMessageV2 Message);
internal record GetPluginList;
internal record SetPluginRole(PluginId PluginId, List<string> PluginRole);
internal record AddPlugins(List<IPlugin> Plugins, PluginHost PluginHost);
