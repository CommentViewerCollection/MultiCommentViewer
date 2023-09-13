using System.Collections.Generic;
using Mcv.PluginV2;
using System;
using Mcv.PluginV2.Messages;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace Mcv.MainViewPlugin
{
    class ConnectionAddedEventArgs : EventArgs
    {
        public ConnectionAddedEventArgs(IConnectionStatus connSt, BrowserProfileId browser)
        {
            ConnSt = connSt;
            Browser = browser;
        }

        public IConnectionStatus ConnSt { get; }
        public BrowserProfileId Browser { get; }
    }
    class ConnectionRemovedEventArgs : EventArgs
    {
        public ConnectionRemovedEventArgs(ConnectionId connId)
        {
            ConnId = connId;
        }

        public ConnectionId ConnId { get; }
    }
    class ConnectionStatusChangedEventArgs : EventArgs
    {
        public ConnectionStatusChangedEventArgs(IConnectionStatusDiff connStDiff)
        {
            ConnStDiff = connStDiff;
        }

        public IConnectionStatusDiff ConnStDiff { get; }
    }
    class SiteAddedEventArgs : EventArgs
    {
        public SiteAddedEventArgs(PluginId siteId, string siteDisplayName)
        {
            SiteId = siteId;
            SiteDisplayName = siteDisplayName;
        }

        public PluginId SiteId { get; }
        public string SiteDisplayName { get; }
    }
    class BrowserAddedEventArgs : EventArgs
    {
        public BrowserAddedEventArgs(BrowserProfileId browserProfileId, string browserDisplayName, string? profileDisplayName)
        {
            BrowserProfileId = browserProfileId;
            BrowserDisplayName = browserDisplayName;
            ProfileDisplayName = profileDisplayName;
        }

        public BrowserProfileId BrowserProfileId { get; }
        public string BrowserDisplayName { get; }
        public string? ProfileDisplayName { get; }
    }
    record BrowserRemovedEventArgs(BrowserProfileId BrowserProfileId);
    class MessageReceivedEventArgs : EventArgs
    {

        public MessageReceivedEventArgs(NotifyMessageReceived messageReceived, MyUser? user)
        {
            MessageReceived = messageReceived;
            User = user;
        }

        public NotifyMessageReceived MessageReceived { get; }
        public MyUser? User { get; }
    }
    class MetadataUpdatedEventArgs : EventArgs
    {
        public MetadataUpdatedEventArgs(NotifyMetadataUpdated metadataUpdated)
        {
            MetadataUpdated = metadataUpdated;
        }

        public NotifyMetadataUpdated MetadataUpdated { get; }
    }
    class PluginAddedEventArgs : EventArgs
    {
        public PluginAddedEventArgs(PluginId pluginId, string pluginName)
        {
            PluginId = pluginId;
            PluginName = pluginName;
        }

        public PluginId PluginId { get; }
        public string PluginName { get; }
    }
    class SelectedSiteChangedEventArgs : EventArgs
    {
        public SelectedSiteChangedEventArgs(ConnectionId connId, PluginId siteId)
        {
            ConnId = connId;
            SiteId = siteId;
        }

        public ConnectionId ConnId { get; }
        public PluginId SiteId { get; }
    }
    class UserAddedEventArgs : EventArgs
    {
        public UserAddedEventArgs(MyUser user)
        {
            User = user;
        }

        public MyUser User { get; }
    }
    class UserRemovedEventArgs : EventArgs
    {
        public UserRemovedEventArgs(string userId)
        {
            UserId = userId;
        }

        public string UserId { get; }
    }
    class IAdapter : IConnectionNameHost
    {
        public event EventHandler<ConnectionAddedEventArgs> ConnectionAdded;
        public event EventHandler<ConnectionRemovedEventArgs> ConnectionRemoved;
        public event EventHandler<ConnectionStatusChangedEventArgs> ConnectionStatusChanged;
        public event EventHandler<SiteAddedEventArgs> SiteAdded;
        public event EventHandler<BrowserAddedEventArgs> BrowserAdded;
        public event EventHandler<BrowserRemovedEventArgs> BrowserRemoved;
        public event EventHandler<MessageReceivedEventArgs> MessageReceived;
        public event EventHandler<MetadataUpdatedEventArgs> MetadataUpdated;
        public event PropertyChangedEventHandler? PropertyChanged;
        public event EventHandler<SelectedSiteChangedEventArgs> SelectedSiteChanged;
        public event EventHandler<UserAddedEventArgs> UserAdded;
        public event EventHandler<UserRemovedEventArgs> UserRemoved;

        private readonly BrowserProfileId _emptyBrowserProfileId = new(Guid.NewGuid());
        private readonly UserStore _userStore = new();
        internal MyUser GetUser(string userId)
        {
            return _userStore.GetUser(userId);
        }
        internal List<MyUser> GetAllUsers()
        {
            return _userStore.GetAllUsers();
        }
        static readonly string _userDataDbPath = "MainViewPlugin_Users.db";
        internal async Task LoadUserStoreAsync()
        {
            var res = await _host.RequestMessageAsync(new GetPluginSettingsDirPath(_userDataDbPath)) as ReplyPluginSettingsDirPath;
            if (res is null)
            {
                throw new NotImplementedException();
            }
            _userStore.Load(res.PluginSettingsDirPath);
        }
        internal async Task SaveUserStoreAsync()
        {
            var res = await _host.RequestMessageAsync(new GetPluginSettingsDirPath(_userDataDbPath)) as ReplyPluginSettingsDirPath;
            if (res is null)
            {
                throw new NotImplementedException();
            }
            _userStore.Save(res.PluginSettingsDirPath);
        }
        internal void OnConnectionRemoved(ConnectionId connId)
        {
            ConnectionRemoved?.Invoke(this, new ConnectionRemovedEventArgs(connId));
        }

        public event EventHandler<PluginAddedEventArgs> PluginAdded;

        internal void OnConnectionStatusChanged(IConnectionStatusDiff connStDiff)
        {
            ConnectionStatusChanged?.Invoke(this, new ConnectionStatusChangedEventArgs(connStDiff));
        }

        internal void OnMessageReceived(NotifyMessageReceived messageReceived, MyUser? user)
        {
            MessageReceived?.Invoke(this, new MessageReceivedEventArgs(messageReceived, user));
        }

        internal void OnPluginAdded(IPluginInfo pluginInfo)
        {
            PluginAdded?.Invoke(this, new PluginAddedEventArgs(pluginInfo.Id, pluginInfo.Name));
        }
        internal void OnPluginAdded(PluginId pluginId, string pluginName)
        {
            PluginAdded?.Invoke(this, new PluginAddedEventArgs(pluginId, pluginName));
        }

        internal void OnMetadataUpdated(NotifyMetadataUpdated metadataUpdated)
        {
            MetadataUpdated?.Invoke(this, new MetadataUpdatedEventArgs(metadataUpdated));
        }


        #region Properties
        //public bool IsPixelScrolling { get { return _options.IsPixelScrolling; } set { _options.IsPixelScrolling = value; } }
        //public Color HorizontalGridLineColor { get { return _options.HorizontalGridLineColor; } set { _options.HorizontalGridLineColor = value; } }
        //public Color VerticalGridLineColor { get { return _options.VerticalGridLineColor; } set { _options.VerticalGridLineColor = value; } }
        //public double ConnectionNameWidth { get { return _options.ConnectionNameWidth; } set { _options.ConnectionNameWidth = value; } }


        //public bool IsShowConnectionName { get { return _options.IsShowConnectionName; } set { _options.IsShowConnectionName = value; } }
        private readonly ConcurrentDictionary<PluginId, string> _sitePlugins = new();
        internal void OnSiteAdded(PluginId siteId, string siteDisplayName)
        {
            if (_sitePlugins.TryAdd(siteId, siteDisplayName))
            {
                SiteAdded?.Invoke(this, new SiteAddedEventArgs(siteId, siteDisplayName));
            }
            else
            {
                //bug
            }
        }

        //public int ConnectionNameDisplayIndex { get { return _options.ConnectionNameDisplayIndex; } set { _options.ConnectionNameDisplayIndex = value; } }

        internal void OnConnectionAdded(IConnectionStatus connSt)
        {
            var browser = GetDefaultBrowser();
            ConnectionAdded?.Invoke(this, new ConnectionAddedEventArgs(connSt, browser));
        }
        private BrowserProfileId GetDefaultBrowser()
        {
            return _browserProfileDict.Values.ToList()[0].ProfileId;
        }
        internal void AddEmptyBrowserProfile()
        {
            AddBrowserProfile(_emptyBrowserProfileInfo);
        }

        //public double ThumbnailWidth { get { return _options.ThumbnailWidth; } set { _options.ThumbnailWidth = value; } }
        //public bool IsShowThumbnail { get { return _options.IsShowThumbnail; } set { _options.IsShowThumbnail = value; } }
        //public int ThumbnailDisplayIndex { get { return _options.ThumbnailDisplayIndex; } set { _options.ThumbnailDisplayIndex = value; } }
        //public double CommentIdWidth { get { return _options.CommentIdWidth; } set { _options.CommentIdWidth = value; } }
        //public bool IsShowCommentId { get { return _options.IsShowCommentId; } set { _options.IsShowCommentId = value; } }
        //public int CommentIdDisplayIndex { get { return _options.CommentIdDisplayIndex; } set { _options.CommentIdDisplayIndex = value; } }
        //public double UsernameWidth { get { return _options.UsernameWidth; } set { _options.UsernameWidth = value; } }
        //public bool IsShowUsername { get { return _options.IsShowUsername; } set { _options.IsShowUsername = value; } }
        //public int UsernameDisplayIndex { get { return _options.UsernameDisplayIndex; } set { _options.UsernameDisplayIndex = value; } }
        //public double MessageWidth { get { return _options.MessageWidth; } set { _options.MessageWidth = value; } }
        //public bool IsShowMessage { get { return _options.IsShowMessage; } set { _options.IsShowMessage = value; } }
        //public int MessageDisplayIndex { get { return _options.MessageDisplayIndex; } set { _options.MessageDisplayIndex = value; } }
        //public double PostTimeWidth { get { return _options.PostTimeWidth; } set { _options.PostTimeWidth = value; } }
        //public bool IsShowPostTime { get { return _options.IsShowPostTime; } set { _options.IsShowPostTime = value; } }
        //public int PostTimeDisplayIndex { get { return _options.PostTimeDisplayIndex; } set { _options.PostTimeDisplayIndex = value; } }
        //public double InfoWidth { get { return _options.InfoWidth; } set { _options.InfoWidth = value; } }
        //public bool IsShowInfo { get { return _options.IsShowInfo; } set { _options.IsShowInfo = value; } }
        //public int InfoDisplayIndex { get { return _options.InfoDisplayIndex; } set { _options.InfoDisplayIndex = value; } }
        //public Color SelectedRowBackColor { get { return _options.SelectedRowBackColor; } set { _options.SelectedRowBackColor = value; } }
        //public Color SelectedRowForeColor { get { return _options.SelectedRowForeColor; } set { _options.SelectedRowForeColor = value; } }
        //public bool IsShowHorizontalGridLine { get { return _options.IsShowHorizontalGridLine; } set { _options.IsShowHorizontalGridLine = value; } }
        //public bool IsShowVerticalGridLine { get { return _options.IsShowVerticalGridLine; } set { _options.IsShowVerticalGridLine = value; } }
        //public Color TitleForeColor { get { return _options.TitleForeColor; } set { _options.TitleForeColor = value; } }
        //public Color TitleBackColor { get { return _options.TitleBackColor; } set { _options.TitleBackColor = value; } }
        //public Color ViewBackColor { get { return _options.ViewBackColor; } set { _options.ViewBackColor = value; } }
        //public Color WindowBorderColor { get { return _options.WindowBorderColor; } set { _options.WindowBorderColor = value; } }
        //public Color SystemButtonForeColor { get { return _options.SystemButtonForeColor; } set { _options.SystemButtonForeColor = value; } }
        //public Color SystemButtonBackColor { get { return _options.SystemButtonBackColor; } set { _options.SystemButtonBackColor = value; } }
        //public Color SystemButtonBorderColor { get { return _options.SystemButtonBorderColor; } set { _options.SystemButtonBorderColor = value; } }
        //public Color SystemButtonMouseOverBackColor { get { return _options.SystemButtonMouseOverBackColor; } set { _options.SystemButtonMouseOverBackColor = value; } }
        //public Color SystemButtonMouseOverBorderColor { get { return _options.SystemButtonMouseOverBorderColor; } set { _options.SystemButtonMouseOverBorderColor = value; } }
        //public Color SystemButtonMouseOverForeColor { get { return _options.SystemButtonMouseOverForeColor; } set { _options.SystemButtonMouseOverForeColor = value; } }
        //public Color MenuBackColor { get { return _options.MenuBackColor; } set { _options.MenuBackColor = value; } }
        //public Color MenuForeColor { get { return _options.MenuForeColor; } set { _options.MenuForeColor = value; } }
        //public Color MenuItemCheckMarkColor { get { return _options.MenuItemCheckMarkColor; } set { _options.MenuItemCheckMarkColor = value; } }
        //public Color MenuItemMouseOverBackColor { get { return _options.MenuItemMouseOverBackColor; } set { _options.MenuItemMouseOverBackColor = value; } }
        //public Color MenuItemMouseOverForeColor { get { return _options.MenuItemMouseOverForeColor; } set { _options.MenuItemMouseOverForeColor = value; } }
        //public Color MenuItemMouseOverBorderColor { get { return _options.MenuItemMouseOverBorderColor; } set { _options.MenuItemMouseOverBorderColor = value; } }
        //public Color MenuItemMouseOverCheckMarkColor { get { return _options.MenuItemMouseOverCheckMarkColor; } set { _options.MenuItemMouseOverCheckMarkColor = value; } }
        //public Color MenuSeparatorBackColor { get { return _options.MenuSeparatorBackColor; } set { _options.MenuSeparatorBackColor = value; } }
        //public Color MenuPopupBorderColor { get { return _options.MenuPopupBorderColor; } set { _options.MenuPopupBorderColor = value; } }
        //public Color CommentListBackColor { get { return _options.CommentListBackColor; } set { _options.CommentListBackColor = value; } }
        //public Color ButtonForeColor { get { return _options.ButtonForeColor; } set { _options.ButtonForeColor = value; } }
        //public Color ButtonBorderColor { get { return _options.ButtonBorderColor; } set { _options.ButtonBorderColor = value; } }
        //public Color ButtonBackColor { get { return _options.ButtonBackColor; } set { _options.ButtonBackColor = value; } }
        //public Color CommentListHeaderBackColor { get { return _options.CommentListHeaderBackColor; } set { _options.CommentListHeaderBackColor = value; } }
        //public Color CommentListBorderColor { get { return _options.CommentListBorderColor; } set { _options.CommentListBorderColor = value; } }
        //public Color CommentListHeaderForeColor { get { return _options.CommentListHeaderForeColor; } set { _options.CommentListHeaderForeColor = value; } }
        //public Color CommentListHeaderBorderColor { get { return _options.CommentListHeaderBorderColor; } set { _options.CommentListHeaderBorderColor = value; } }
        //public Color CommentListSeparatorColor { get { return _options.CommentListSeparatorColor; } set { _options.CommentListSeparatorColor = value; } }
        //public Color ConnectionListRowBackColor { get { return _options.ConnectionListRowBackColor; } set { _options.ConnectionListRowBackColor = value; } }
        //public Color ScrollBarBorderColor { get { return _options.ScrollBarBorderColor; } set { _options.ScrollBarBorderColor = value; } }
        //public Color ScrollBarThumbBackColor { get { return _options.ScrollBarThumbBackColor; } set { _options.ScrollBarThumbBackColor = value; } }
        //public Color ScrollBarThumbMouseOverBackColor { get { return _options.ScrollBarThumbMouseOverBackColor; } set { _options.ScrollBarThumbMouseOverBackColor = value; } }
        //public Color ScrollBarThumbPressedBackColor { get { return _options.ScrollBarThumbPressedBackColor; } set { _options.ScrollBarThumbPressedBackColor = value; } }
        //public Color ScrollBarBackColor { get { return _options.ScrollBarBackColor; } set { _options.ScrollBarBackColor = value; } }
        //public Color ScrollBarButtonBackColor { get { return _options.ScrollBarButtonBackColor; } set { _options.ScrollBarButtonBackColor = value; } }
        //public Color ScrollBarButtonForeColor { get { return _options.ScrollBarButtonForeColor; } set { _options.ScrollBarButtonForeColor = value; } }
        //public Color ScrollBarButtonBorderColor { get { return _options.ScrollBarButtonBorderColor; } set { _options.ScrollBarButtonBorderColor = value; } }
        //public Color ScrollBarButtonDisabledBackColor { get { return _options.ScrollBarButtonDisabledBackColor; } set { _options.ScrollBarButtonDisabledBackColor = value; } }
        //public Color ScrollBarButtonDisabledForeColor { get { return _options.ScrollBarButtonDisabledForeColor; } set { _options.ScrollBarButtonDisabledForeColor = value; } }
        //public Color ScrollBarButtonDisabledBorderColor { get { return _options.ScrollBarButtonDisabledBorderColor; } set { _options.ScrollBarButtonDisabledBorderColor = value; } }
        //public Color ScrollBarButtonMouseOverBackColor { get { return _options.ScrollBarButtonMouseOverBackColor; } set { _options.ScrollBarButtonMouseOverBackColor = value; } }
        //public Color ScrollBarButtonMouseOverForeColor { get { return _options.ScrollBarButtonMouseOverForeColor; } set { _options.ScrollBarButtonMouseOverForeColor = value; } }
        //public Color ScrollBarButtonMouseOverBorderColor { get { return _options.ScrollBarButtonMouseOverBorderColor; } set { _options.ScrollBarButtonMouseOverBorderColor = value; } }
        //public Color ScrollBarButtonPressedBackColor { get { return _options.ScrollBarButtonPressedBackColor; } set { _options.ScrollBarButtonPressedBackColor = value; } }
        //public Color ScrollBarButtonPressedForeColor { get { return _options.ScrollBarButtonPressedForeColor; } set { _options.ScrollBarButtonPressedForeColor = value; } }
        //public Color ScrollBarButtonPressedBorderColor { get { return _options.ScrollBarButtonPressedBorderColor; } set { _options.ScrollBarButtonPressedBorderColor = value; } }
        //public int MetadataViewConnectionNameDisplayIndex { get { return _options.MetadataViewConnectionNameDisplayIndex; } set { _options.MetadataViewConnectionNameDisplayIndex = value; } }
        //public bool IsShowMetaConnectionName { get { return _options.IsShowMetaConnectionName; } set { _options.IsShowMetaConnectionName = value; } }
        //public bool IsShowMetaTitle { get { return _options.IsShowMetaTitle; } set { _options.IsShowMetaTitle = value; } }
        //public int MetadataViewTitleDisplayIndex { get { return _options.MetadataViewTitleDisplayIndex; } set { _options.MetadataViewTitleDisplayIndex = value; } }
        //public bool IsShowMetaElapse { get { return _options.IsShowMetaElapse; } set { _options.IsShowMetaElapse = value; } }
        //public int MetadataViewElapsedDisplayIndex { get { return _options.MetadataViewElapsedDisplayIndex; } set { _options.MetadataViewElapsedDisplayIndex = value; } }
        //public bool IsShowMetaCurrentViewers { get { return _options.IsShowMetaCurrentViewers; } set { _options.IsShowMetaCurrentViewers = value; } }
        //public int MetadataViewCurrentViewersDisplayIndex { get { return _options.MetadataViewCurrentViewersDisplayIndex; } set { _options.MetadataViewCurrentViewersDisplayIndex = value; } }
        //public bool IsShowMetaTotalViewers { get { return _options.IsShowMetaTotalViewers; } set { _options.IsShowMetaTotalViewers = value; } }
        //public int MetadataViewTotalViewersDisplayIndex { get { return _options.MetadataViewTotalViewersDisplayIndex; } set { _options.MetadataViewTotalViewersDisplayIndex = value; } }
        //public bool IsShowMetaActive { get { return _options.IsShowMetaActive; } set { _options.IsShowMetaActive = value; } }
        //public int MetadataViewActiveDisplayIndex { get { return _options.MetadataViewActiveDisplayIndex; } set { _options.MetadataViewActiveDisplayIndex = value; } }
        //public bool IsShowMetaOthers { get { return _options.IsShowMetaOthers; } set { _options.IsShowMetaOthers = value; } }
        //public int MetadataViewOthersDisplayIndex { get { return _options.MetadataViewOthersDisplayIndex; } set { _options.MetadataViewOthersDisplayIndex = value; } }
        //public int ConnectionsViewSelectionDisplayIndex { get { return _options.ConnectionsViewSelectionDisplayIndex; } set { _options.ConnectionsViewSelectionDisplayIndex = value; } }
        //public double ConnectionsViewSelectionWidth { get { return _options.ConnectionsViewSelectionWidth; } set { _options.ConnectionsViewSelectionWidth = value; } }
        //public bool IsShowConnectionsViewSelection { get { return _options.IsShowConnectionsViewSelection; } set { _options.IsShowConnectionsViewSelection = value; } }
        //public int ConnectionsViewSiteDisplayIndex { get { return _options.ConnectionsViewSiteDisplayIndex; } set { _options.ConnectionsViewSiteDisplayIndex = value; } }
        //public double ConnectionsViewSiteWidth { get { return _options.ConnectionsViewSiteWidth; } set { _options.ConnectionsViewSiteWidth = value; } }
        //public bool IsShowConnectionsViewSite { get { return _options.IsShowConnectionsViewSite; } set { _options.IsShowConnectionsViewSite = value; } }
        //public int ConnectionsViewConnectionNameDisplayIndex { get { return _options.ConnectionsViewConnectionNameDisplayIndex; } set { _options.ConnectionsViewConnectionNameDisplayIndex = value; } }
        //public double ConnectionsViewConnectionNameWidth { get { return _options.ConnectionsViewConnectionNameWidth; } set { _options.ConnectionsViewConnectionNameWidth = value; } }
        //public bool IsShowConnectionsViewConnectionName { get { return _options.IsShowConnectionsViewConnectionName; } set { _options.IsShowConnectionsViewConnectionName = value; } }
        //public int ConnectionsViewInputDisplayIndex { get { return _options.ConnectionsViewInputDisplayIndex; } set { _options.ConnectionsViewInputDisplayIndex = value; } }

        internal void RequestCloseApp()
        {
            _host.SetMessageAsync(new SetCloseApp());
        }

        //public double ConnectionsViewInputWidth { get { return _options.ConnectionsViewInputWidth; } set { _options.ConnectionsViewInputWidth = value; } }
        //public bool IsShowConnectionsViewInput { get { return _options.IsShowConnectionsViewInput; } set { _options.IsShowConnectionsViewInput = value; } }
        //public int ConnectionsViewBrowserDisplayIndex { get { return _options.ConnectionsViewBrowserDisplayIndex; } set { _options.ConnectionsViewBrowserDisplayIndex = value; } }
        //public double ConnectionsViewBrowserWidth { get { return _options.ConnectionsViewBrowserWidth; } set { _options.ConnectionsViewBrowserWidth = value; } }
        //public bool IsShowConnectionsViewBrowser { get { return _options.IsShowConnectionsViewBrowser; } set { _options.IsShowConnectionsViewBrowser = value; } }
        //public int ConnectionsViewConnectionDisplayIndex { get { return _options.ConnectionsViewConnectionDisplayIndex; } set { _options.ConnectionsViewConnectionDisplayIndex = value; } }
        //public double ConnectionsViewConnectionWidth { get { return _options.ConnectionsViewConnectionWidth; } set { _options.ConnectionsViewConnectionWidth = value; } }
        //public bool IsShowConnectionsViewConnection { get { return _options.IsShowConnectionsViewConnection; } set { _options.IsShowConnectionsViewConnection = value; } }
        //public int ConnectionsViewDisconnectionDisplayIndex { get { return _options.ConnectionsViewDisconnectionDisplayIndex; } set { _options.ConnectionsViewDisconnectionDisplayIndex = value; } }
        //public double ConnectionsViewDisconnectionWidth { get { return _options.ConnectionsViewDisconnectionWidth; } set { _options.ConnectionsViewDisconnectionWidth = value; } }
        //public bool IsShowConnectionsViewDisconnection { get { return _options.IsShowConnectionsViewDisconnection; } set { _options.IsShowConnectionsViewDisconnection = value; } }
        //public int ConnectionsViewSaveDisplayIndex { get { return _options.ConnectionsViewSaveDisplayIndex; } set { _options.ConnectionsViewSaveDisplayIndex = value; } }
        //public double ConnectionsViewSaveWidth { get { return _options.ConnectionsViewSaveWidth; } set { _options.ConnectionsViewSaveWidth = value; } }
        //public bool IsShowConnectionsViewSave { get { return _options.IsShowConnectionsViewSave; } set { _options.IsShowConnectionsViewSave = value; } }
        //public int ConnectionsViewLoggedinUsernameDisplayIndex { get { return _options.ConnectionsViewLoggedinUsernameDisplayIndex; } set { _options.ConnectionsViewLoggedinUsernameDisplayIndex = value; } }



        //public double ConnectionsViewLoggedinUsernameWidth { get { return _options.ConnectionsViewLoggedinUsernameWidth; } set { _options.ConnectionsViewLoggedinUsernameWidth = value; } }
        //public bool IsShowConnectionsViewLoggedinUsername { get { return _options.IsShowConnectionsViewLoggedinUsername; } set { _options.IsShowConnectionsViewLoggedinUsername = value; } }
        //public int ConnectionsViewConnectionBackgroundDisplayIndex { get { return _options.ConnectionsViewConnectionBackgroundDisplayIndex; } set { _options.ConnectionsViewConnectionBackgroundDisplayIndex = value; } }
        //public double ConnectionsViewConnectionBackgroundWidth { get { return _options.ConnectionsViewConnectionBackgroundWidth; } set { _options.ConnectionsViewConnectionBackgroundWidth = value; } }
        //public bool IsEnabledSiteConnectionColor { get { return _options.IsEnabledSiteConnectionColor; } set { _options.IsEnabledSiteConnectionColor = value; } }
        //public SiteConnectionColorType SiteConnectionColorType { get { return _options.SiteConnectionColorType; } set { _options.SiteConnectionColorType = value; } }
        //public int ConnectionsViewConnectionForegroundDisplayIndex { get { return _options.ConnectionsViewConnectionForegroundDisplayIndex; } set { _options.ConnectionsViewConnectionForegroundDisplayIndex = value; } }
        //public double ConnectionsViewConnectionForegroundWidth { get { return _options.ConnectionsViewConnectionForegroundWidth; } set { _options.ConnectionsViewConnectionForegroundWidth = value; } }
        //public bool IsTopmost { get { return _options.IsTopmost; } set { _options.IsTopmost = value; } }
        //public double MainViewHeight { get { return _options.MainViewHeight; } set { _options.MainViewHeight = value; } }
        //public double MainViewWidth { get { return _options.MainViewWidth; } set { _options.MainViewWidth = value; } }
        //public double MainViewLeft { get { return _options.MainViewLeft; } set { _options.MainViewLeft = value; } }
        //public double MainViewTop { get { return _options.MainViewTop; } set { _options.MainViewTop = value; } }
        //public double ConnectionViewHeight { get { return _options.ConnectionViewHeight; } set { _options.ConnectionViewHeight = value; } }
        //public double MetadataViewHeight { get { return _options.MetadataViewHeight; } set { _options.MetadataViewHeight = value; } }
        //public double MetadataViewConnectionNameColumnWidth { get { return _options.MetadataViewConnectionNameColumnWidth; } set { _options.MetadataViewConnectionNameColumnWidth = value; } }
        //public double MetadataViewTitleColumnWidth { get { return _options.MetadataViewTitleColumnWidth; } set { _options.MetadataViewTitleColumnWidth = value; } }
        //public double MetadataViewElapsedColumnWidth { get { return _options.MetadataViewElapsedColumnWidth; } set { _options.MetadataViewElapsedColumnWidth = value; } }
        //public double MetadataViewCurrentViewersColumnWidth { get { return _options.MetadataViewCurrentViewersColumnWidth; } set { _options.MetadataViewCurrentViewersColumnWidth = value; } }
        //public double MetadataViewTotalViewersColumnWidth { get { return _options.MetadataViewTotalViewersColumnWidth; } set { _options.MetadataViewTotalViewersColumnWidth = value; } }
        //public double MetadataViewActiveColumnWidth { get { return _options.MetadataViewActiveColumnWidth; } set { _options.MetadataViewActiveColumnWidth = value; } }
        //public double MetadataViewOthersColumnWidth { get { return _options.MetadataViewOthersColumnWidth; } set { _options.MetadataViewOthersColumnWidth = value; } }


        #endregion //Properties
        //public bool CanConnect(ConnectionId id)
        //{
        //    return true;
        //}

        //public bool CanDisconnect(ConnectionId id)
        //{
        //    return false;
        //}

        //public void RequestAddConnection()
        //{
        //    //_parent.Tell(new ToCore.RequestAddNormalConnection());
        //    _mainActor.RequestAddNormalConnection();
        //}


        //public void InputChanged(ConnectionId connId, string value)
        //{
        //    //TODO:適切なサイトに変更する
        //    //_parent.Tell(new ToCore.RequestChangeConnectionStatus(id, new NormalConnectionDiff { Input = value }));
        //    _mainActor.RequestChangeConnectionStatus(connId, new NormalConnectionDiff { Input = value });
        //}
        //public event EventHandler<ConnectionAddedEventArgs>? ConnectionAdded;
        //public event EventHandler<SitePluginAddedEventArgs>? SiteAdded;
        //public event EventHandler<BrowserAddedEventArgs>? BrowserAdded;
        //public event EventHandler<SitePluginRemovedEventArgs>? SiteRemoved;
        //public event EventHandler<BrowserRemovedEventArgs>? BrowserRemoved;
        //public event EventHandler<ConnectionStatusChangedEventArgs>? ConnectionStatusChanged;
        //public event EventHandler<ConnectionRemovedEventArgs>? ConnectionRemoved;
        //public event EventHandler<MessageReceivedEventArgs>? MessageReceived;
        //public event EventHandler<SitePluginSettingsReceivedEventArgs>? SitePluginSettingsReceived;

        //private readonly Dictionary<ConnectionId, ConnectionNameViewModel> _nameDict = new();
        //public void OnConnectionAdded(ConnectionId connId, IConnectionStatus connSt)
        //{
        //    var name = new ConnectionNameViewModel(connId, connSt.Name);
        //    _nameDict.Add(connId, name);
        //    if (connSt is INormalConnection normal)
        //    {
        //        ConnectionAdded?.Invoke(this, new ConnectionAddedEventArgs(connId, normal));
        //    }
        //}
        //public void OnConnectionStatusChanged(ConnectionId id, IConnectionStatusDiff diff)
        //{
        //    Debug.WriteLine("Adapter::OnConnectionStatusChanged()");
        //    if (diff is INormalConnectionDiff normal)
        //    {
        //        if (diff.Name != null)
        //        {
        //            var name = _nameDict[id];
        //            name.Name = diff.Name;
        //        }
        //        ConnectionStatusChanged?.Invoke(this, new ConnectionStatusChangedEventArgs(id, normal));
        //    }
        //}

        //public void OnSiteAdded(SiteId sitePluginId, string name)
        //{
        //    SiteAdded?.Invoke(this, new SitePluginAddedEventArgs(sitePluginId, name));
        //}

        //public void OnBrowserAdded(BrowserProfileId PluginId, string name, string? profileName)
        //{
        //    BrowserAdded?.Invoke(this, new BrowserAddedEventArgs(PluginId, name, profileName));
        //}

        //public void OnSiteRemoved(SiteId sitePluginId)
        //{
        //    SiteRemoved?.Invoke(this, new SitePluginRemovedEventArgs(sitePluginId));
        //}

        //public void OnBrowserRemoved(BrowserProfileId PluginId)
        //{
        //    BrowserRemoved?.Invoke(this, new BrowserRemovedEventArgs(PluginId));
        //}

        //public void NameChanged(ConnectionId id, string value)
        //{
        //    //_parent.Tell(new ToCore.RequestChangeConnectionStatus(id, new NormalConnectionDiff { Name = value }));
        //    _mainActor.RequestChangeConnectionStatus(id, new NormalConnectionDiff { Name = value });
        //}

        //public void SiteChanged(ConnectionId connId, SiteId siteId)
        //{
        //    //_parent.Tell(new ToCore.RequestChangeConnectionStatus(connId, new NormalConnectionDiff { SelectedSite = siteId }));
        //    _mainActor.RequestChangeConnectionStatus(connId, new NormalConnectionDiff { SelectedSite = siteId });
        //}

        //public void BrowserChanged(ConnectionId connId, BrowserProfileId id2)
        //{
        //    //_parent.Tell(new ToCore.RequestChangeConnectionStatus(id1, new NormalConnectionDiff { SelectedBrowser = id2 }));
        //    _mainActor.RequestChangeConnectionStatus(connId, new NormalConnectionDiff { SelectedBrowser = id2 });
        //}

        //public void Connect(ConnectionId connId)
        //{
        //    //_parent.Tell(new ToCore.RequestChangeConnectionStatus(id, new NormalConnectionDiff { IsConnected = true }));
        //    _mainActor.RequestChangeConnectionStatus(connId, new NormalConnectionDiff { IsConnected = true });
        //}

        //public void Disconnect(ConnectionId connId)
        //{
        //    //_parent.Tell(new ToCore.RequestChangeConnectionStatus(id, new NormalConnectionDiff { IsConnected = false }));
        //    _mainActor.RequestChangeConnectionStatus(connId, new NormalConnectionDiff { IsConnected = false });
        //}

        //public void RemoveConnections(List<ConnectionId> selectedConnections)
        //{
        //    foreach (var id in selectedConnections)
        //    {
        //        //_parent.Tell(new ToCore.RequestRemoveConnection(id));
        //        _mainActor.RequestRemoveConnection(id);
        //    }
        //}

        //public void OnConnectionRemoved(ConnectionId connId)
        //{
        //    _nameDict.Remove(connId);
        //    ConnectionRemoved?.Invoke(this, new ConnectionRemovedEventArgs(connId));
        //}
        //public void OnMessageReceived(ConnectionId connId, ISiteMessage message, SiteMessageParser siteMessageParser, IAdapter adapter)
        //{
        //    var connName = _nameDict[connId];
        //    var vm = siteMessageParser.Parse(message, connName, adapter);
        //    if (vm != null)
        //    {
        //        MessageReceived?.Invoke(this, new MessageReceivedEventArgs(vm));
        //    }
        //    else
        //    {
        //        Debug.WriteLine($"unsupported message={message}");
        //    }
        //}

        //public void RequestCloseApp()
        //{
        //    //_parent.Tell(new ToCore.RequestCloseApp());
        //    _mainActor.RequestCloseApp();
        //}

        //public void RequestSitePluginSettings()
        //{
        //    _mainActor.RequestSitePluginSettings();
        //}

        //public void OnSitePluginSettingsReceived(List<IPluginSettings> pluginSettingsList)
        //{
        //    SitePluginSettingsReceived?.Invoke(this, new SitePluginSettingsReceivedEventArgs(pluginSettingsList));
        //}

        //public void OnSettingsApplied(List<IPluginSettingsDiff> modifiedData)
        //{

        //    //_parent.Tell(new ToCore.RequestUpdateSettingsList(modifiedData));
        //    //_parent.Tell()をしてもReceiveAsync<ToCore.RequestSitePluginSettings>を処理中の状態だからmailboxを読みにいかない。
        //    //だからMainViewActorのmailboxを経由せずにcoreにメッセージを送らないといけない。
        //    _mainActor.RequestUpdateSettingsList(modifiedData);
        //}
        internal void RemoveConnections(List<ConnectionId> selectedConnections)
        {
            foreach (var connId in selectedConnections)
            {
                _host.SetMessageAsync(new RequestRemoveConnection(connId));
            }
        }
        internal void RequestAddConnection()
        {
            _host.SetMessageAsync(new RequestAddConnection());
        }

        internal void RequestChangeConnectionStatus(ConnectionStatusDiff connectionStatusDiff)
        {
            _host.SetMessageAsync(new RequestChangeConnectionStatus(connectionStatusDiff));
        }
        public void SetConnectionName(ConnectionId connId, string newConnectionName)
        {
            _host.SetMessageAsync(new RequestChangeConnectionStatus(new ConnectionStatusDiff(connId)
            {
                Name = newConnectionName,
            }));
        }
        public async Task<List<(PluginId, IOptionsTabPage)>> RequestSettingsPanels()
        {
            var list = new List<(PluginId, IOptionsTabPage)>();
            var sites = _sitePlugins.ToArray();
            foreach (var (site, _) in sites)
            {
                if (await _host.RequestMessageAsync(new GetDirectMessage(site, new GetSettingsPanel())) is not AnswerSettingsPanel reply)
                {
                    throw new Exception("bug");
                }
                list.Add((site, reply.Panel));
            }
            return list;
        }
        public async Task<string> GetConnectionName(ConnectionId connId)
        {
            var reply = await _host.RequestMessageAsync(new GetConnectionStatus(connId)) as ReplyConnectionStatus;
            if (reply is null)
            {
                throw new Exception("bug");
            }
            return reply.ConnSt.Name;
        }
        public async Task<string> GetAppName()
        {
            var reply = await _host.RequestMessageAsync(new GetAppName()) as ReplyAppName;
            if (reply is null)
            {
                throw new Exception("bug");
            }
            return reply.AppName;
        }
        public async Task<string> GetVersion()
        {
            var reply = await _host.RequestMessageAsync(new GetAppVersion()) as ReplyAppVersion;
            if (reply is null)
            {
                throw new Exception("bug");
            }
            return reply.AppVersion;
        }
        public async Task<string> GetAppSolutionConfiguration()
        {
            var reply = await _host.RequestMessageAsync(new GetAppSolutionConfiguration()) as ReplyAppSolutionConfiguration;
            if (reply is null)
            {
                throw new Exception("bug");
            }
            return reply.AppSolutionConfiguration;
        }

        internal void RequestShowSettingsPanel(PluginId pluginId)
        {
            _host.SetMessageAsync(new RequestShowSettingsPanel(pluginId));
        }
        internal async Task<string> GetSitePluginDisplayName(PluginId pluginId)
        {
            var res = await _host.RequestMessageAsync(new GetDirectMessage(pluginId, new GetSitePluginDisplayName())) as ReplySitePluginDisplayName;
            if (res is null)
            {
                throw new Exception("bug");
            }
            return res.DisplayName;
        }

        internal async void AfterInputChanged(ConnectionId connId, string input)
        {
            var sites = _sitePlugins.ToArray();
            foreach (var (site, _) in sites)
            {
                var reply = await _host.RequestMessageAsync(new GetDirectMessage(site, new GetIsValidSiteUrl(input))) as ReplyIsValidSiteUrl;
                if (reply is null)
                {
                    throw new Exception("bug");
                }
                if (reply.IsValid)
                {
                    //SelectedSiteChanged?.Invoke(this, new SelectedSiteChangedEventArgs(connId, site));
                    await _host.SetMessageAsync(new RequestChangeConnectionStatus(new ConnectionStatusDiff(connId) { SelectedSite = site }));
                    break;
                }
            }
        }

        internal async Task SetConnectSite(PluginId selectedSite, ConnectionId connId, string input, BrowserProfileId browserProfileId)
        {
            var resDomain = await _host.RequestMessageAsync(new GetDirectMessage(selectedSite, new GetSiteDomain(connId))) as ReplySiteDomain;
            if (resDomain is null)
            {
                throw new Exception("bug");
            }
            var profile = _browserProfileDict[browserProfileId];
            var res = await _host.RequestMessageAsync(new GetDirectMessage(profile.PluginId, new GetCookies(browserProfileId, resDomain.Domain))) as ReplyCookies;
            if (res is null)
            {
                throw new Exception("bug");
            }
            await _host.SetMessageAsync(new SetDirectMessage(selectedSite, new SetConnectSite(connId, input, res.Cookies)));
        }
        internal void SetDisconectSite(PluginId selectedSite, ConnectionId connId)
        {
            _host.SetMessageAsync(new SetDirectMessage(selectedSite, new SetDisconnectSite(connId)));
        }
        internal void AddBrowserProfile(ProfileInfo browserProfileInfo)
        {
            if (_browserProfileDict.ContainsKey(_emptyBrowserProfileId))
            {
                _browserProfileDict.Remove(_emptyBrowserProfileId, out var _);
                BrowserRemoved?.Invoke(this, new BrowserRemovedEventArgs(_emptyBrowserProfileId));
            }
            _browserProfileDict.TryAdd(browserProfileInfo.ProfileId, browserProfileInfo);

            BrowserAdded?.Invoke(this, new BrowserAddedEventArgs(browserProfileInfo.ProfileId, browserProfileInfo.BrowserName, browserProfileInfo.ProfileName));
        }
        internal void RemoveBrowserProfile(BrowserProfileId browserProfileId)
        {
            _browserProfileDict.Remove(browserProfileId, out var _);
            if (_browserProfileDict.Count == 0)
            {
                _browserProfileDict.TryAdd(_emptyBrowserProfileId, _emptyBrowserProfileInfo);
                BrowserAdded?.Invoke(this, new BrowserAddedEventArgs(_emptyBrowserProfileId, _emptyBrowserProfileInfo.BrowserName, _emptyBrowserProfileInfo.ProfileName));
            }
            BrowserRemoved?.Invoke(this, new BrowserRemovedEventArgs(browserProfileId));
        }
        //空にしない。ブラウザが無い時は_emptyBrowserViewModelを必ず入れる。
        private readonly ConcurrentDictionary<BrowserProfileId, ProfileInfo> _browserProfileDict = new();
        public IMainViewPluginOptions Options { get; }
        private readonly IPluginHost _host;

        //private readonly IMainViewActor _mainActor;

        public IAdapter(IPluginHost host, IMainViewPluginOptions options)
        {
            Options = options;
            _host = host;
            _emptyBrowserProfileInfo = new ProfileInfo(new PluginId(Guid.NewGuid()), "(未選択)", null, _emptyBrowserProfileId);
            _userStore.UserAdded += UserStore_UserAdded;
        }
        private readonly ProfileInfo _emptyBrowserProfileInfo;
        private void UserStore_UserAdded(object? sender, UserAddedEventArgs e)
        {
            UserAdded?.Invoke(this, e);
        }
    }
}