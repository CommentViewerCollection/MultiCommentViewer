using Mcv.PluginV2;
using Mcv.PluginV2.Messages;
using ryu_s.BrowserCookie;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Net;

namespace BrowserCookiePlugin;

[Export(typeof(IPlugin))]
public class PluginMain : IPlugin
{
    public IPluginHost Host { get; set; } = default!;
    public PluginId Id { get; } = new PluginId(new Guid("305643BE-1178-4FE0-8C9B-10EDB8D4F38E"));
    public string Name { get; } = "BrowserCookiePlugin";
    public List<string> Roles { get; } = new List<string> { "cookie:chrome", "cookie:firefox", "cookie:edge" };

    private void LoadBrowsers()
    {
        var list = new List<IBrowserProfile>();
        var managers = new List<IBrowserManager>
        {
            new ChromeManager(),
            new ChromeBetaManager(),
            new FirefoxManager(),
            new EdgeManager(),
            new OperaManager(),
            new OperaGxManager(),
        };
        foreach (var manager in managers)
        {
            try
            {
                list.AddRange(manager.GetProfiles());
            }
            catch (Exception ex)
            {
                //_logger.LogException(ex);
            }
        }
        foreach (var profile in list)
        {
            var id = new BrowserProfileId(Guid.NewGuid());
            var info = new ProfileInfo(Id, profile.Type.ToString(), profile.ProfileName, id);
            _profileInfoDict.Add(id, info);
            _profileDict.Add(id, profile);
        }
    }
    private readonly Dictionary<BrowserProfileId, ProfileInfo> _profileInfoDict = new();
    private readonly Dictionary<BrowserProfileId, IBrowserProfile> _profileDict = new();

    public async Task<IReplyMessageToPluginV2> RequestMessage(IGetMessageToPluginV2 message)
    {
        switch (message)
        {
            case GetBrowserProfiles _:
                {
                    return new ReplyBrowserProfiles(_profileInfoDict.Values.ToList());
                }
            case GetCookies getCookies:
                {
                    List<Cookie> cookies = new List<Cookie>();
                    try
                    {
                        var profile = _profileDict[getCookies.BrowserProfileId];
                        cookies = profile.GetCookieCollection(getCookies.Domain);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                    }
                    return new ReplyCookies(cookies);

                }
        }
        throw new Exception("bug");
    }

    public async Task SetMessage(ISetMessageToPluginV2 message)
    {
        switch (message)
        {
            case SetLoading _:
                {
                    LoadBrowsers();
                    await Host.SetMessageAsync(new SetPluginHello(Id, Name, Roles));
                }
                break;
            case SetLoaded _:
                {
                }
                break;
            case SetClosing _:
                {
                }
                break;
        }
    }

    public async Task SetMessage(INotifyMessageV2 message)
    {
    }
}

