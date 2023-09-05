using Mcv.PluginV2.Messages;

namespace Mcv.Core.PluginActorMessages;
internal record GetMessageToPluginV2(IGetMessageToPluginV2 Message);

internal record NotifyMessageV2(INotifyMessageV2 Message);

internal record SetMessageToPluginV2(ISetMessageToPluginV2 Message);
