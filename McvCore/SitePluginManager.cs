using Common;
using Mcv.PluginV2;
using Mcv.PluginV2.Messages.ToCore;
using ryu_s.BrowserCookie;
using SitePlugin;
using SitePluginCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace McvCore
{
    class SitePluginAdapter
    {
        private readonly ISiteContext _siteContext;
        public SitePluginAdapter(PluginId pluginId, SiteId siteId, ISiteContext siteContext)
        {
            _siteContext = siteContext;
            Id = pluginId;
            SiteId = siteId;
        }
        public SiteId SiteId { get; }
        public ICommentProvider CreateCommentProvider()
        {
            return _siteContext.CreateCommentProvider();
        }
        public IOptionsTabPage GetPanel()
        {
            return _siteContext.TabPanel;
        }
        public string Name => _siteContext.DisplayName;
        public PluginId Id { get; }
    }
    class SitePluginAddedEventArgs : EventArgs
    {
        public SitePluginAddedEventArgs(SiteId sitePluginId, string sitePluginDisplayName)
        {
            SitePluginId = sitePluginId;
            SitePluginDisplayName = sitePluginDisplayName;
        }

        public SiteId SitePluginId { get; }
        public string SitePluginDisplayName { get; }
    }
    class SitePluginHost
    {
        private readonly McvCore _mcvCore;

        public SitePluginHost(McvCore mcvCore)
        {
            _mcvCore = mcvCore;
        }
        internal void SetMessage(Mcv.PluginV2.Messages.ToCore.NotifyMessageReceived toCoreMessage)
        {
            _mcvCore.SetMessage(toCoreMessage);
        }
        internal void SetMessage(Mcv.PluginV2.Messages.ToCore.NotifyMetadataUpdated toCoreMessage)
        {
            _mcvCore.SetMessage(toCoreMessage);
        }
    }
    class SitePluginManager
    {
        public event EventHandler<SitePluginAddedEventArgs>? SitePluginAdded;
        public void LoadSitePlugins(ICommentOptions options, ILogger logger, string userAgent)
        {
            var userStoreManager = new SitePluginCommon.UserStoreManager();
            var list = new List<ISiteContext>
            {
                new YouTubeLiveSitePlugin.Test2.YouTubeLiveSiteContext(options, new YouTubeLiveSitePlugin.Test2.YouTubeLiveServer(), logger, userStoreManager),
                new OpenrecSitePlugin.OpenrecSiteContext(options, logger, userStoreManager),
                new MixchSitePlugin.MixchSiteContext(options, logger, userStoreManager),
                new TwitchSitePlugin.TwitchSiteContext(options,new TwitchSitePlugin.TwitchServer(), logger, userStoreManager),
                new NicoSitePlugin.NicoSiteContext(options,new NicoSitePlugin.DataSource(userAgent), logger, userStoreManager),
                new TwicasSitePlugin.TwicasSiteContext(options,logger, userStoreManager),
                new WhowatchSitePlugin.WhowatchSiteContext(options, logger, userStoreManager),
                new MirrativSitePlugin.MirrativSiteContext(options,new MirrativSitePlugin.MirrativServer(), logger, userStoreManager),
                new ShowRoomSitePlugin.ShowRoomSiteContext(options,new ShowRoomSitePlugin.ShowRoomServer(), logger,userStoreManager),
                new MildomSitePlugin.MildomSiteContext(options, new MildomSitePlugin.MildomServer(),logger, userStoreManager),
                new BigoSitePlugin.BigoSiteContext(options, new BigoSitePlugin.BigoServer(), logger, userStoreManager),
            };
            foreach (var site in list)
            {
                site.Init();
                site.LoadOptions(GetSiteOptionsPath(site.DisplayName, options.SettingsDirPath), _io);
                var id = new SiteId(site.Guid);
                _dict.Add(id, new SitePluginAdapter(new PluginId(site.Guid), id, site));
                SitePluginAdded?.Invoke(this, new SitePluginAddedEventArgs(id, site.DisplayName));
            }
        }
        private static string GetSiteOptionsPath(string displayName, string settingsDirPath)
        {
            var path = System.IO.Path.Combine(settingsDirPath, displayName + ".txt");
            return path;
        }
        internal SiteId GetDefaultSite()
        {
            var first = _dict.Select(kv => kv.Key).ToList()[0];
            return first;
        }

        private readonly Dictionary<SiteId, SitePluginAdapter> _dict = new Dictionary<SiteId, SitePluginAdapter>();
        public void CreateCommentProvider(ConnectionId connId, SiteId siteId)
        {
            var context = _dict[siteId];
            var provider = context.CreateCommentProvider();
            var wrapper = new CommentProviderWrapper(provider, new CommentProviderHost(this, siteId, connId));
            if (_commentProviderDict.ContainsKey(connId))
            {
                _commentProviderDict[connId] = wrapper;
            }
            else
            {
                _commentProviderDict.Add(connId, wrapper);
            }
        }
        public void Connect(ConnectionId connId, string input, ryu_s.BrowserCookie.IBrowserProfile browserProfile)
        {
            var provider = _commentProviderDict[connId];
            var t = provider.ConnectAsync(input, browserProfile);
        }
        public void Disconnect(ConnectionId connId)
        {
            var provider = _commentProviderDict[connId];
            provider.Disconnect();
        }

        private readonly Dictionary<ConnectionId, CommentProviderWrapper> _commentProviderDict = new Dictionary<ConnectionId, CommentProviderWrapper>();
        private readonly SitePluginHost _host;
        private readonly IIo _io;

        public SitePluginManager(SitePluginHost host, IIo io)
        {
            _host = host;
            _io = io;
        }

        internal List<SitePluginAdapter> GetSitePlugins()
        {
            return _dict.Values.ToList();
        }

        internal void SetMessage(Mcv.PluginV2.Messages.ToCore.NotifyMessageReceived toCoreMessage)
        {
            _host.SetMessage(toCoreMessage);
        }

        internal List<(SiteId, IOptionsTabPage)> GetSettingsPanels()
        {
            var dd = _dict.Select(kv => (kv.Key, kv.Value.GetPanel())).ToList();
            return dd;
        }

        internal void SetMessage(Mcv.PluginV2.Messages.ToCore.NotifyMetadataUpdated toCoreMessage)
        {
            _host.SetMessage(toCoreMessage);
        }
    }
    class CommentProviderHost
    {
        private readonly SitePluginManager _mcvCore;
        private readonly SiteId _siteId;
        private readonly ConnectionId _connId;

        public CommentProviderHost(SitePluginManager mcvCore, SiteId siteId, ConnectionId connId)
        {
            _mcvCore = mcvCore;
            _siteId = siteId;
            _connId = connId;
        }

        internal void NotifyMetadataUpdated(IMetadata e)
        {
            _mcvCore.SetMessage(new Mcv.PluginV2.Messages.ToCore.NotifyMetadataUpdated(_connId, _siteId, e));
        }

        internal void NotifyMessageReceived(SitePlugin.ISiteMessage message, string? userId)
        {
            _mcvCore.SetMessage(new Mcv.PluginV2.Messages.ToCore.NotifyMessageReceived(_connId, _siteId, message, userId));
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
            _host.NotifyMessageReceived(e.Message, e.Metadata.User?.UserId);
        }

        internal Task ConnectAsync(string input, IBrowserProfile browserProfile)
        {
            return _commentProvider.ConnectAsync(input, browserProfile);
        }
        internal void Disconnect()
        {
            _commentProvider.Disconnect();
        }
    }

}
