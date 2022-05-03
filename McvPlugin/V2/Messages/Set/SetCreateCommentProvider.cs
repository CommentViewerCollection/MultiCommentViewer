namespace Mcv.PluginV2.Messages;

public record SetCreateCommentProvider(ConnectionId ConnId) : ISetMessageToPluginV2;
public record SetDestroyCommentProvider(ConnectionId ConnId) : ISetMessageToPluginV2;
