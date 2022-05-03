namespace Mcv.PluginV2.Messages;

public record RequestSavePluginOptions(string Filename, string PluginOptionsRaw) : ISetMessageToCoreV2;
