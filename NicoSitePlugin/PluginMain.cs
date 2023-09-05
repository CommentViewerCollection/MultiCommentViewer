using Mcv.PluginV2;
using Mcv.PluginV2.Messages;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;

namespace NicoSitePlugin.V2
{
    class CommentProviderHost
    {
        private readonly IPluginHost _host;
        private readonly ConnectionId _connId;
        private readonly PluginId _pluginId;

        internal void NotifyMetadataUpdated(IMetadata e)
        {
            _host.SetMessageAsync(new SetMetadata(_connId, _pluginId, e));
        }

        internal void NotifyMessageReceived(ISiteMessage message, string userId, string? newNickname)
        {
            _host.SetMessageAsync(new SetMessage(_connId, _pluginId, message, userId, newNickname));
        }
        public CommentProviderHost(IPluginHost host, ConnectionId connId, PluginId pluginId)
        {
            _host = host;
            _connId = connId;
            _pluginId = pluginId;
        }
    }
    class CommentProviderWrapper
    {
        private readonly ICommentProvider _commentProvider;
        private readonly CommentProviderHost _host;

        public CommentProviderWrapper(ICommentProvider commentProvider, CommentProviderHost host)
        {
            _commentProvider = commentProvider;
            _host = host;
            commentProvider.MessageReceived += CommentProvider_MessageReceived;
            commentProvider.MetadataUpdated += CommentProvider_MetadataUpdated;
        }
        ~CommentProviderWrapper()
        {
            _commentProvider.MessageReceived -= CommentProvider_MessageReceived;
            _commentProvider.MetadataUpdated -= CommentProvider_MetadataUpdated;
        }
        private void CommentProvider_MetadataUpdated(object sender, IMetadata e)
        {
            _host.NotifyMetadataUpdated(e);
        }

        private void CommentProvider_MessageReceived(object sender, IMessageContext e)
        {
            _host.NotifyMessageReceived(e.Message, e.UserId, e.NewNickname);
        }

        internal Task ConnectAsync(string input, List<Cookie> cookies)
        {
            return _commentProvider.ConnectAsync(input, cookies);
        }
        internal void Disconnect()
        {
            _commentProvider.Disconnect();
        }
    }
    [Export(typeof(IPlugin))]
    public class PluginMain : IPlugin
    {
        public IPluginHost Host { get; set; } = default!;
        public PluginId Id { get; } = new PluginId(new Guid("852C766E-B60E-4FA9-92FE-387F310C0124"));
        public string Name { get; } = "NicoSitePlugin";
        public List<string> Roles { get; } = new List<string> { "site:nicolive" };
        NicoSiteContext _context;
        private async Task<string> GetUserAgent()
        {
            var res = await Host.RequestMessageAsync(new GetUserAgent()) as ReplyUserAgent;
            return res.UserAgent;
        }
        public async Task SetMessageAsync(ISetMessageToPluginV2 message)
        {
            switch (message)
            {
                case SetLoading _:
                    {
                        var userAgent = await GetUserAgent();
                        _context = new NicoSiteContext(new DataSource(userAgent), new Logger(Host));
                        var res = await Host.RequestMessageAsync(new RequestLoadPluginOptions(Name)) as ReplyPluginOptions;
                        _context.LoadOptions(res.RawOptions);
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
                case SetCreateCommentProvider createCommentProvider:
                    {
                        var provider = _context.CreateCommentProvider();
                        var wrappter = new CommentProviderWrapper(provider, new CommentProviderHost(Host, createCommentProvider.ConnId, Id));
                        if (_connDict.ContainsKey(createCommentProvider.ConnId))
                        {
                            _connDict[createCommentProvider.ConnId] = wrappter;
                        }
                        else
                        {
                            _connDict.Add(createCommentProvider.ConnId, wrappter);
                        }
                    }
                    break;
                case SetDestroyCommentProvider destroyCommentProvider:
                    {
                        _connDict.Remove(destroyCommentProvider.ConnId);
                    }
                    break;
                case SetConnectSite connect:
                    {
                        if (!_connDict.TryGetValue(connect.ConnId, out var wrapper))
                        {
                            return;
                        }
                        var connectionTask = wrapper.ConnectAsync(connect.Input, connect.Cookies);
                        _connectionTaskDict.Add(connect.ConnId, connectionTask);
                        await Host.SetMessageAsync(new NotifySiteConnected(connect.ConnId));
                    }
                    break;
                case SetDisconnectSite disconnect:
                    {
                        if (!_connDict.TryGetValue(disconnect.ConnId, out var wrapper))
                        {
                            return;
                        }
                        wrapper.Disconnect();
                        var connectionTask = _connectionTaskDict[disconnect.ConnId];
                        try
                        {
                            await connectionTask;
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex.Message);
                        }
                        _connectionTaskDict.Remove(disconnect.ConnId);
                        await Host.SetMessageAsync(new NotifySiteDisconnected(disconnect.ConnId));
                    }
                    break;
                default:
                    break;
            }
        }
        private readonly Dictionary<ConnectionId, Task> _connectionTaskDict = new();
        private readonly Dictionary<ConnectionId, CommentProviderWrapper> _connDict = new Dictionary<ConnectionId, CommentProviderWrapper>();

        public async Task SetMessageAsync(INotifyMessageV2 message)
        {
        }

        public async Task<IReplyMessageToPluginV2> RequestMessageAsync(IGetMessageToPluginV2 message)
        {
            switch (message)
            {
                case GetSitePluginDisplayName _:
                    return new ReplySitePluginDisplayName(_context.DisplayName);
                case GetIsValidSiteUrl isValidUrl:
                    return new ReplyIsValidSiteUrl(_context.IsValidInput(isValidUrl.Input));
                case GetSiteDomain _:
                    return new ReplySiteDomain("nicovideo.jp");
                case GetSettingsPanel _:
                    return new AnswerSettingsPanel(_context.TabPanel);
            }
            throw new NotImplementedException();
        }
    }
    class Logger : ILogger
    {
        private readonly IPluginHost _host;

        public string GetExceptions()
        {
            return "";
        }

        public void LogException(Exception ex, string message = "", string detail = "")
        {
            _host.SetMessageAsync(new SetException(ex, message, detail));
        }
        public Logger(IPluginHost host)
        {
            _host = host;
        }
    }
}
