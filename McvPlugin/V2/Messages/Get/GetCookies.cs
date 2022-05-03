using System.Net;

namespace Mcv.PluginV2.Messages;

public record GetCookies(BrowserProfileId BrowserProfileId, string Domain) : IGetMessageToPluginV2;
public record ReplyCookies(List<Cookie> Cookies) : IReplyMessageToPluginV2
{
    public string Raw { get; } = "";
}
