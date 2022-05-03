using Mcv.PluginV2.Messages.ToCore;
using SitePlugin;
using System.Collections.Generic;

namespace Mcv.PluginV2.Messages
{



    public class NotifyMessageReceived : INotifyMessageV2
    {
        public NotifyMessageReceived(ConnectionId connId, SiteId siteId, SitePlugin.ISiteMessage message, string userId, IEnumerable<IMessagePart> username, string nickname, bool isNgUser)
        {
            ConnectionId = connId;
            SiteId = siteId;
            Message = message;
            UserId = userId;
            Username = username;
            Nickname = nickname;
            IsNgUser = isNgUser;
        }

        public ConnectionId ConnectionId { get; }
        public SiteId SiteId { get; }
        public SitePlugin.ISiteMessage Message { get; }
        public string UserId { get; }
        public IEnumerable<IMessagePart> Username { get; }
        public string Nickname { get; }
        public bool IsNgUser { get; }
    }
    public class NotifyMetadataUpdated : INotifyMessageV2
    {
        public NotifyMetadataUpdated(ConnectionId connId, SiteId siteId, IMetadata e)
        {
            ConnId = connId;
            SiteId = siteId;
            Metadata = e;
        }

        public ConnectionId ConnId { get; }
        public SiteId SiteId { get; }
        public IMetadata Metadata { get; }
    }
}
namespace Mcv.PluginV2.Messages.ToCore
{
    public interface IMessageToCore { }

    public class NotifyMessageReceived : IMessageToCore
    {
        public NotifyMessageReceived(ConnectionId connId, SiteId siteId, SitePlugin.ISiteMessage message, string userId)
        {
            ConnId = connId;
            SiteId = siteId;
            Message = message;
            UserId = userId;
        }

        public ConnectionId ConnId { get; }
        public SiteId SiteId { get; }
        public SitePlugin.ISiteMessage Message { get; }
        public string UserId { get; }
    }
    public class NotifyMetadataUpdated : IMessageToCore
    {
        public NotifyMetadataUpdated(ConnectionId connId, SiteId siteId, IMetadata e)
        {
            ConnId = connId;
            SiteId = siteId;
            Metadata = e;
        }

        public ConnectionId ConnId { get; }
        public SiteId SiteId { get; }
        public IMetadata Metadata { get; }
    }
    public class RequestCloseApp : IMessageToCore { }
}
namespace Mcv.PluginV2.Messages.ToCore
{
    public interface IRequestMessageToCore { }
    public class RequestSettingsPanels : IRequestMessageToCore
    {

    }
    public class RequestUserAgent : IRequestMessageToCore { }
}
namespace Mcv.PluginV2.Messages.ToPlugin
{
    public interface IAnswerMessageToPlugin { }
    public class AnswerSettingsPanels : IAnswerMessageToPlugin
    {
        public AnswerSettingsPanels(List<(SiteId, IOptionsTabPage)> panels)
        {
            Panels = panels;
        }

        public List<(SiteId, IOptionsTabPage)> Panels { get; }
    }
    public class AnswerUserAgent
    {
        private static string GetVersionNumber()
        {
            var asm = System.Reflection.Assembly.GetExecutingAssembly();
            var ver = asm.GetName().Version;
            var s = $"{ver.Major}.{ver.Minor}.{ver.Build}";
            return s;
        }
        private static string GetAppName()
        {
            var asm = System.Reflection.Assembly.GetExecutingAssembly();
            var title = asm.GetName().Name;
            return title;
        }
        public string UserAgent
        {
            get
            {
                return $"{GetAppName()}/{GetVersionNumber()} contact-> twitter.com/kv510k";
            }
        }
    }
}
namespace Mcv.PluginV2.Messages
{
    //この形式なら往復のメッセージを同じ名前空間にできる
    public interface ISetMessageToCoreV2 { }
    public interface ISetMessageToPluginV2 { }
    public interface INotifyMessageV2 { }
    public interface IRequestMessageV2 { }
    public interface IReplyMessageV2 { }

    public class RequestLegacyOptions : IRequestMessageV2 { }
    public class ReplyLegacyOptions : IReplyMessageV2
    {
        public ReplyLegacyOptions(string rawOptions)
        {
            RawOptions = rawOptions;
        }

        public string RawOptions { get; }
    }

    public class RequestLoadPluginOptions : IRequestMessageV2
    {
        public RequestLoadPluginOptions(string pluginName)
        {
            PluginName = pluginName;
        }

        public string PluginName { get; }
    }
    public class ReplyPluginOptions : IReplyMessageV2
    {
        public ReplyPluginOptions(string rawOptions)
        {
            RawOptions = rawOptions;
        }

        public string RawOptions { get; }
    }

    public class RequestSavePluginOptions : ISetMessageToCoreV2
    {
        public RequestSavePluginOptions(string filename, string pluginOptionsRaw)
        {
            Filename = filename;
            PluginOptionsRaw = pluginOptionsRaw;
        }

        public string Filename { get; }
        public string PluginOptionsRaw { get; }
    }

    public class RequestAppName : IRequestMessageV2
    {
        public RequestAppName() { }
    }
    public class ReplyAppName : IReplyMessageV2
    {
        public ReplyAppName(string appName)
        {
            AppName = appName;
        }

        public string AppName { get; }
    }
    public class RequestConnectionStatus : IRequestMessageV2
    {
        public RequestConnectionStatus(ConnectionId connId)
        {
            ConnId = connId;
        }

        public ConnectionId ConnId { get; }
    }
    public class ReplyConnectionStatus : IReplyMessageV2
    {
        public ReplyConnectionStatus(IConnectionStatus connSt)
        {
            ConnSt = connSt;
        }

        public IConnectionStatus ConnSt { get; }
    }

    public class SetConnectionStatus : ISetMessageToCoreV2
    {
        public SetConnectionStatus(ConnectionId connId, IConnectionStatusDiff diff)
        {
            ConnId = connId;
            ConnStDiff = diff;
        }

        public ConnectionId ConnId { get; }
        public IConnectionStatusDiff ConnStDiff { get; }
    }
    public class RequestShowSettingsPanel : ISetMessageToCoreV2
    {
        public RequestShowSettingsPanel(PluginId pluginId)
        {
            PluginId = pluginId;
        }

        public PluginId PluginId { get; }
    }

    public class NotifyConnectionStatusList : INotifyMessageV2
    {
        public NotifyConnectionStatusList(List<IConnectionStatus> connections)
        {
            Connections = connections;
        }

        public List<IConnectionStatus> Connections { get; }
    }
    public class NotifyPluginInfoList : INotifyMessageV2
    {
        public NotifyPluginInfoList(List<IPluginInfo> plugins)
        {
            Plugins = plugins;
        }

        public List<IPluginInfo> Plugins { get; }
    }
    public class RequestShowSettingsPanelToPlugin : ISetMessageToPluginV2
    {

    }
    public class RequestCloseToPlugin : ISetMessageToPluginV2 { }
    public class RequestRemoveConnection : ISetMessageToCoreV2
    {
        public RequestRemoveConnection(ConnectionId connId)
        {
            ConnId = connId;
        }

        public ConnectionId ConnId { get; }
    }
    public class NotifyConnectionRemoved : INotifyMessageV2
    {
        public NotifyConnectionRemoved(ConnectionId connId)
        {
            ConnId = connId;
        }

        public ConnectionId ConnId { get; }
    }
}