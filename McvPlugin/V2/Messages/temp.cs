using Mcv.PluginV2.Messages;

namespace Mcv.PluginV2;

public record SetException(Exception Ex, string Message, string Details) : ISetMessageToCoreV2;
