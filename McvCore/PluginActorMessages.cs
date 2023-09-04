using Mcv.PluginV2.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace McvCore.PluginActorMessages;
internal record GetMessageToPluginV2(IGetMessageToPluginV2 Message);

internal record NotifyMessageV2(INotifyMessageV2 Message);

internal record SetMessageToPluginV2(ISetMessageToPluginV2 Message);
