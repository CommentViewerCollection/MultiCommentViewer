using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Command;
using System.Windows.Input;
using SitePlugin;
using System.Threading;
using System.Collections.ObjectModel;
using Plugin;
using System.Diagnostics;
using System.Windows.Threading;
using System.Windows.Media;
using System.Reflection;
using System.ComponentModel;
using MultiCommentViewer.Test;
using Common;
using System.Windows.Data;
using System.Text.RegularExpressions;
using CommentViewerCommon;
using SitePluginCommon;
using System.Windows;
using System.Windows.Controls;
using NicoSitePlugin;
using MixchSitePlugin;
using BLiveSitePlugin;

namespace MultiCommentViewer
{
    class ColorPair
    {
        public Color BackColor { get; }
        public Color ForeColor { get; }
        private Color ColorFromArgb(string argb)
        {
            if (argb == null)
                throw new ArgumentNullException("argb");
            var pattern = "#(?<a>[0-9a-fA-F]{2})(?<r>[0-9a-fA-F]{2})(?<g>[0-9a-fA-F]{2})(?<b>[0-9a-fA-F]{2})";
            var match = System.Text.RegularExpressions.Regex.Match(argb, pattern, System.Text.RegularExpressions.RegexOptions.Compiled);

            if (!match.Success)
            {
                throw new ArgumentException("形式が不正");
            }
            else
            {
                var a = byte.Parse(match.Groups["a"].Value, System.Globalization.NumberStyles.HexNumber);
                var r = byte.Parse(match.Groups["r"].Value, System.Globalization.NumberStyles.HexNumber);
                var g = byte.Parse(match.Groups["g"].Value, System.Globalization.NumberStyles.HexNumber);
                var b = byte.Parse(match.Groups["b"].Value, System.Globalization.NumberStyles.HexNumber);
                return Color.FromArgb(a, r, g, b);
            }
        }
        public ColorPair(string backArgb, string foreArgb)
        {
            BackColor = ColorFromArgb(backArgb);
            ForeColor = ColorFromArgb(foreArgb);
        }
        public ColorPair(Color backColor, Color foreColor)
        {
            BackColor = backColor;
            ForeColor = foreColor;
        }
    }
    class ConnectionSerializerLoader
    {
        private readonly string _path;

        public void Save(IEnumerable<ConnectionViewModel> serializers)
        {
            var list = new List<string>();
            foreach (var connection in serializers)
            {
                if (connection.NeedSave)
                {
                    var back = Common.Utils.ColorToArgb(connection.BackColor);
                    var fore = Common.Utils.ColorToArgb(connection.ForeColor);
                    var connectionSerializer = new ConnectionSerializer(connection.Name, connection.SelectedSite.DisplayName, connection.Input, connection.SelectedBrowser.DisplayName, back, fore);
                    var serialized = connectionSerializer.Serialize();
                    list.Add(serialized);
                }
            }
            using (var sw = new System.IO.StreamWriter(_path))
            {
                foreach (var line in list)
                {
                    sw.WriteLine(line);
                }
            }
        }
        public IEnumerable<ConnectionSerializer> Load()
        {
            var connectionSerializerList = new List<ConnectionSerializer>();
            if (System.IO.File.Exists(_path))
            {
                using (var sr = new System.IO.StreamReader(_path))
                {
                    for (string line; (line = sr.ReadLine()) != null;)
                    {
                        var serializer = ConnectionSerializer.Deserialize(line);
                        connectionSerializerList.Add(serializer);
                    }
                }
            }
            return connectionSerializerList;
        }
        public ConnectionSerializerLoader(string path)
        {
            _path = path;
        }
    }
    public class MainViewModel : CommentDataGridViewModelBase
    {
        #region Commands
        public ICommand ActivatedCommand { get; }
        public ICommand LoadedCommand { get; }
        public ICommand MainViewContentRenderedCommand { get; }
        public ICommand MainViewClosingCommand { get; }
        public ICommand ShowOptionsWindowCommand { get; }
        public ICommand ExitCommand { get; }
        public ICommand ShowWebSiteCommand { get; }
        public ICommand ShowDevelopersTwitterCommand { get; }
        public ICommand CheckUpdateCommand { get; }
        public ICommand ShowUserInfoCommand { get; }
        public ICommand ShowUserListCommand { get; }
        public ICommand RemoveSelectedConnectionCommand { get; }
        public ICommand AddNewConnectionCommand { get; }
        public ICommand ClearAllCommentsCommand { get; }
        public ICommand CommentCopyCommand { get; }
        public ICommand OpenUrlCommand { get; }

        #endregion //Commands

        #region Fields
        private readonly Dictionary<IPlugin, PluginMenuItemViewModel> _pluginMenuItemDict = new Dictionary<IPlugin, PluginMenuItemViewModel>();
        private readonly ILogger _logger;
        private IPluginManager _pluginManager;
        private readonly ISitePluginLoader _sitePluginLoader;
        public ISiteContext GetSiteContext(Guid siteContextGuid)
        {
            return _sitePluginLoader.GetSiteContext(siteContextGuid);
        }
        private readonly IBrowserLoader _browserLoader;
        private readonly IIo _io;
        //IEnumerable<ISiteContext> _siteContexts;
        IEnumerable<SiteViewModel> _siteVms;
        IEnumerable<BrowserViewModel> _browserVms;

        Dictionary<ConnectionViewModel, MetadataViewModel> _metaDict = new Dictionary<ConnectionViewModel, MetadataViewModel>();
        ConnectionSerializerLoader _connectionSerializerLoader = new ConnectionSerializerLoader("settings\\connections.txt");
        #endregion //Fields


        #region Methods
        private void Activated()
        {

        }
        private void Loaded()
        {

        }
        private void ClearAllComments()
        {
            try
            {
                //Comments.Clear();
                //個別ユーザのコメントはどうしようか

            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
            }
        }
        private void ShowOptionsWindow()
        {
            try
            {
                var list = new List<IOptionsTabPage>();
                var mainOptionsPanel = new MainOptionsPanel();
                mainOptionsPanel.SetViewModel(new MainOptionsViewModel(_options));
                list.Add(new MainTabPage("一般", mainOptionsPanel));
                foreach (var siteVm in _siteVms)
                {
                    try
                    {
                        var siteContext = _sitePluginLoader.GetSiteContext(siteVm.Guid);
                        var tabPanel = siteContext.TabPanel;
                        if (tabPanel == null)
                            continue;
                        list.Add(tabPanel);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogException(ex);
                        Debug.WriteLine(ex.Message);
                    }
                }
                MessengerInstance.Send(new ShowOptionsViewMessage(list));
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
            }
        }
        private string GetOptionsPath()
        {
            return System.IO.Path.Combine(_options.SettingsDirPath, "options.txt");
        }
        private string GetSiteOptionsPath(string displayName)
        {
            var path = System.IO.Path.Combine(_options.SettingsDirPath, displayName + ".txt");
            return path;
        }
        private IEnumerable<ISiteContext> GetSiteContexts()
        {
            foreach (var siteVm in _siteVms)
            {
                yield return _sitePluginLoader.GetSiteContext(siteVm.Guid);
            }
        }
        IUserStoreManager _userStoreManager;
        private async void ContentRendered()
        {
            //なんか気持ち悪い書き方だけど一応動く。
            //ここでawaitするとそれ以降が実行されないからこうするしかない。

            //2019/04/28 初期化として必須の部分はあえて例外をcatchしないようにした。

            //UserStoreManagerの作成
            _userStoreManager = new UserStoreManager();
            _userStoreManager.UserAdded += UserStoreManager_UserAdded;


            //SitePluginの読み込み
            IEnumerable<(string, Guid)> sitePlugins = null;
            var siteVms = new List<SiteViewModel>();
            try
            {
                sitePlugins = _sitePluginLoader.LoadSitePlugins(_options, _logger, _userStoreManager, GetUserAgent());
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
            }
            foreach (var (displayName, guid) in sitePlugins ?? new List<(string, Guid)>())
            {
                try
                {
                    var path = GetSiteOptionsPath(displayName);
                    var siteContext = _sitePluginLoader.GetSiteContext(guid);
                    siteContext.LoadOptions(path, _io);
                    siteVms.Add(new SiteViewModel(displayName, guid));
                }
                catch (Exception ex)
                {
                    _logger.LogException(ex);
                }
            }
            _siteVms = siteVms;

            _browserVms = _browserLoader.LoadBrowsers().Select(b => new BrowserViewModel(b));
            //もしブラウザが無かったらclass EmptyBrowserProfileを使う。
            if (_browserVms.Count() == 0)
            {
                _browserVms = new List<BrowserViewModel>
                {
                    new BrowserViewModel( new EmptyBrowserProfile()),
                };
            }

            //PluginManagerの作成とプラグインの読み込み・初期化
            _pluginManager = new PluginManager(_options);
            _pluginManager.PluginAdded += PluginManager_PluginAdded;
            try
            {
                _pluginManager.LoadPlugins(new PluginHost(this, _options, _io, _logger));
                _pluginManager.OnLoaded();
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                Debug.WriteLine(ex.Message);
            }

            //前回保存したConnectionの読み込み
            try
            {
                var connectionSerializerList = _connectionSerializerLoader.Load();
                foreach (var serializer in connectionSerializerList)
                {
                    Color backColor;
                    Color foreColor;
                    if (!string.IsNullOrEmpty(serializer.BackColorArgb) && !string.IsNullOrEmpty(serializer.ForeColorArgb))
                    {
                        backColor = Common.Utils.ColorFromArgb(serializer.BackColorArgb);
                        foreColor = Common.Utils.ColorFromArgb(serializer.ForeColorArgb);
                    }
                    else
                    {
                        var colorPair = GetRandomColorPair();
                        backColor = colorPair.BackColor;
                        foreColor = colorPair.ForeColor;
                    }

                    AddNewConnection(serializer.Name, serializer.SiteName, serializer.Url, serializer.BrowserName, true, backColor, foreColor);
                }
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                Debug.WriteLine(ex.Message);
            }

            //アップデートチェック
            //ここの例外をcatchしてしまうとずっとアップデートに気づかない可能性がある
            if (_options.IsAutoCheckIfUpdateExists)
            {
                await CheckIfUpdateExists(true);
            }
        }

        private void Closing(CancelEventArgs e)
        {
            try
            {
                _connectionSerializerLoader.Save(Connections);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                Debug.WriteLine(ex.Message);
            }
            foreach (var site in GetSiteContexts())
            {
                try
                {
                    var path = GetSiteOptionsPath(site.DisplayName);
                    site.SaveOptions(path, _io);
                }
                catch (Exception ex)
                {
                    _logger.LogException(ex);
                    Debug.WriteLine(ex.Message);
                }
            }
            try
            {
                _pluginManager?.OnClosing();
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                Debug.WriteLine(ex.Message);
            }

            try
            {
                _sitePluginLoader.Save();
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                Debug.WriteLine(ex.Message);
            }
        }
        private void RemoveSelectedConnection()
        {
            try
            {
                var toRemove = Connections.Where(conn => conn.IsSelected).ToList();
                foreach (var conn in toRemove)
                {
                    Connections.Remove(conn);
                    var meta = _metaDict[conn];
                    _metaDict.Remove(conn);
                    MetaCollection.Remove(meta);
                    OnConnectionDeleted(conn.ConnectionName);
                }
                //TODO:この接続に関連するコメントも全て消したい

            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
            }
        }
        private void SetSystemInfo(string message, InfoType type)
        {
            var context = InfoMessageContext.Create(new InfoMessage
            {
                Text = message,
                SiteType = SiteType.Unknown,
                Type = type,
            }, _options);
            AddComment(context, null);
        }
        private string GetDefaultName(IEnumerable<string> existingNames)
        {
            for (var n = 1; ; n++)
            {
                var testName = "#" + n;
                if (!existingNames.Contains(testName))
                {
                    return testName;
                }
            }
        }
        private SiteViewModel GetSiteViewModelFromName(string siteName)
        {
            if (string.IsNullOrEmpty(siteName))
            {
                return null;
            }
            if (_siteVms == null) return null;
            foreach (var siteViewModel in _siteVms)
            {
                if (siteViewModel.DisplayName == siteName)
                {
                    return siteViewModel;
                }
            }
            return null;
        }
        private BrowserViewModel GetBrowserViewModelFromName(string browserName)
        {
            if (string.IsNullOrEmpty(browserName))
            {
                return null;
            }
            if (_browserVms == null) return null;
            foreach (var browserViewModel in _browserVms)
            {
                if (browserViewModel.DisplayName == browserName)
                {
                    return browserViewModel;
                }
            }
            return null;
        }

        readonly ColorPair[] ConnectionColors = new[]
        {
            new ColorPair("#FFe0efd0", "#FF008000"),
            new ColorPair("#FFEEE8AA", "#FF483D8B"),
            new ColorPair("#FF7FFFD4", "#FF0000FF"),
            new ColorPair("#FFA52A2A", "#FF00FF7F"),
            new ColorPair("#FFAFEEEE", "#FF20B2AA"),
            new ColorPair("#FFFFD700", "#FFDC143C"),

        };
        private void AddNewConnection(string name, string siteName, string url, string browserName, bool needSave, Color backColor, Color foreColor)
        {
            try
            {
                var connectionName = new ConnectionName { Name = name };
                var connection = new ConnectionViewModel(connectionName, _siteVms, _browserVms, _logger, _sitePluginLoader, _options)
                {
                    BackColor = backColor,
                    ForeColor = foreColor,
                };
                connection.Renamed += Connection_Renamed;
                connection.MessageReceived += Connection_MessageReceived;
                connection.MetadataReceived += Connection_MetadataReceived;
                connection.SelectedSiteChanged += Connection_SelectedSiteChanged;
                var site = GetSiteViewModelFromName(siteName);
                if (site != null)
                {
                    connection.SelectedSite = site;
                }
                var browser = GetBrowserViewModelFromName(browserName);
                if (browser != null)
                {
                    connection.SelectedBrowser = browser;
                }
                connection.InputWithNoAutoSiteSelect = url;
                connection.NeedSave = needSave;
                var metaVm = new MetadataViewModel(connectionName);
                _metaDict.Add(connection, metaVm);
                MetaCollection.Add(metaVm);
                Connections.Add(connection);
                OnConnectionAdded(connection);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                Debug.WriteLine(ex.Message);
                Debugger.Break();
            }
        }

        private void AddNewConnection()
        {
            try
            {
                var name = GetDefaultName(Connections.Select(c => c.Name));
                var colorPair = GetRandomColorPair();
                AddNewConnection(name, "", "", "", false, colorPair.BackColor, colorPair.ForeColor);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
            }
        }
        private ColorPair GetRandomColorPair()
        {
            var rnd = new Random();
            var n = rnd.Next(ConnectionColors.Length);
            return ConnectionColors[n];
        }
        /// <summary>
        /// 将来的にSiteContext毎に別のIUserStoreを使い分ける可能性を考えて今のうちに。
        /// </summary>
        //Dictionary<ISiteContext, IUserStore> _dic1 = new Dictionary<ISiteContext, IUserStore>();
        /// <summary>
        /// Connection_SelectedSiteChanged内で値を設定
        /// </summary>
        //Dictionary<ICommentProvider, IUserStore> _dict2 = new Dictionary<ICommentProvider, IUserStore>();
        //Dictionary<ConnectionName, >
        private void Connection_SelectedSiteChanged(object sender, SelectedSiteChangedEventArgs e)
        {
            //SetDict(e.NewValue);

            var connectionVm = sender as ConnectionViewModel;
            Debug.Assert(connectionVm != null);

            try
            {
                if (connectionVm == SelectedConnection)
                {
                    MessengerInstance.Send(new SetPostCommentPanel(connectionVm.CommentPostPanel));
                }
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
            }
        }
        Dictionary<ICommentProvider, UserControl> _rawMessagePostPanelDict = new Dictionary<ICommentProvider, UserControl>();

        private void Connection_Renamed(object sender, RenamedEventArgs e)
        {
            //TODO:プラグインに通知
            Debug.WriteLine($"ConnectionRenamed:{e.OldValue}→{e.NewValue}");
        }

        private void OnConnectionAdded(ConnectionViewModel connection)
        {
            //TODO:プラグインに通知
            Debug.WriteLine($"ConnectionAdded:{connection.ConnectionName.Guid}");

            var context = connection.GetCurrent();
            //SetDict(context);

            if (SelectedConnection == null)
            {
                SelectedConnection = connection;
            }
        }
        private bool IsNicoGuid(Guid guid)
        {
            return new Guid("5A477452-FF28-4977-9064-3A4BC7C63252").Equals(guid);
        }
        private bool IsMildomGuid(Guid guid)
        {
            return new Guid("DBBA654F-0A5D-41CC-8153-5DB2D5869BCF").Equals(guid);
        }
        private bool IsTwitchGuid(Guid guid)
        {
            return new Guid("22F7824A-EA1B-411E-85CA-6C9E6BE94E39").Equals(guid);
        }
        private bool IsMirrativGuid(Guid guid)
        {
            return new Guid("6DAFA768-280D-4E70-8494-FD5F31812EF5").Equals(guid);
        }
        //private void SetDict(ConnectionContext context)
        //{
        //    var newSiteContext = context.SiteContext;
        //    var newCommentProvider = context.CommentProvider;
        //    var userStore = _dic1[newSiteContext];
        //    if (!_dict2.ContainsKey(newCommentProvider))
        //    {
        //        _dict2.Add(newCommentProvider, userStore);
        //    }
        //}
        private void OnConnectionDeleted(ConnectionName connectionName)
        {
            //TODO:プラグインに通知
            Debug.WriteLine($"ConnectionDeleted:{connectionName.Guid}");
        }
        string Name
        {
            get { return "MultiCommentViewer"; }
        }
        string Fullname
        {
            get { return $""; }
        }
        string AppDirName
        {
            get
            {
#if BETA
                return Name + "_Beta";
#elif Alpha
                return Name + "_Alpha";
#else
                return Name;
#endif
            }
        }
        private async Task CheckIfUpdateExists(bool isAutoCheck)
        {
            //新しいバージョンがあるか確認
            Common.AutoUpdate.LatestVersionInfo latestVersionInfo;
            string name = AppDirName;
            try
            {
                latestVersionInfo = await Common.AutoUpdate.Tools.GetLatestVersionInfo(name, GetUserAgent());
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                if (!isAutoCheck)
                {
                    SetSystemInfo("サーバに障害が発生している可能性があります。しばらく経ってから再度試してみて下さい。", InfoType.Error);
                }
                return;
            }
            try
            {
                var asm = System.Reflection.Assembly.GetExecutingAssembly();
                var myVer = asm.GetName().Version;
                if (myVer < latestVersionInfo.Version)
                {
                    //新しいバージョンがあった
                    MessengerInstance.Send(new Common.AutoUpdate.ShowUpdateDialogMessage(true, myVer, latestVersionInfo, _logger, GetUserAgent()));
                }
                else
                {
                    //自動チェックの時は、アップデートが無ければ何も表示しない
                    if (!isAutoCheck)
                    {
                        //アップデートはありません
                        MessengerInstance.Send(new Common.AutoUpdate.ShowUpdateDialogMessage(false, myVer, latestVersionInfo, _logger, GetUserAgent()));
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
            }
        }
        #endregion //Methods
        public event EventHandler<EventArgs> CloseRequested;
        public void RequestClose()
        {
            OnCloseRequested(EventArgs.Empty);
        }

        protected virtual void OnCloseRequested(EventArgs e)
        {
            CloseRequested?.Invoke(this, e);
        }

        public async Task InitializeAsync()
        {
            //use this to test the exception handling
            //throw new NotImplementedException();
            await Task.CompletedTask;
        }
        private void AddComment(IMessageContext messageContext, IConnectionStatus connectionName)
        {
            //if (cvm is IInfoCommentViewModel info && info.Type > _options.ShowingInfoLevel)
            //{
            //    return;
            //}

            IMcvCommentViewModel mcvCvm = null;
            if (messageContext.Message is IInfoMessage infoMessage && _options.ShowingInfoLevel >= infoMessage.Type)
            {
                mcvCvm = new InfoCommentViewModel(infoMessage, messageContext.Metadata, messageContext.Methods, connectionName, _options);
            }
            else if (messageContext.Message is WhowatchSitePlugin.IWhowatchMessage whowatchMessage)
            {
                if (whowatchMessage is WhowatchSitePlugin.IWhowatchComment comment)
                {
                    mcvCvm = new McvWhowatchCommentViewModel(comment, messageContext.Metadata, messageContext.Methods, connectionName, _options);
                }
                else if (whowatchMessage is WhowatchSitePlugin.IWhowatchItem item)
                {
                    mcvCvm = new McvWhowatchCommentViewModel(item, messageContext.Metadata, messageContext.Methods, connectionName, _options);
                }
                else if (whowatchMessage is WhowatchSitePlugin.IWhowatchConnected connected)
                {
                    mcvCvm = new McvWhowatchCommentViewModel(connected, messageContext.Metadata, messageContext.Methods, connectionName, _options);
                }
                else if (whowatchMessage is WhowatchSitePlugin.IWhowatchDisconnected disconnected)
                {
                    mcvCvm = new McvWhowatchCommentViewModel(disconnected, messageContext.Metadata, messageContext.Methods, connectionName, _options);
                }
            }
            else if (messageContext.Message is YouTubeLiveSitePlugin.IYouTubeLiveMessage youtubeMessage)
            {
                if (youtubeMessage is YouTubeLiveSitePlugin.IYouTubeLiveComment comment)
                {
                    mcvCvm = new McvYouTubeLiveCommentViewModel(comment, messageContext.Metadata, messageContext.Methods, connectionName, _options);
                }
                else if (youtubeMessage is YouTubeLiveSitePlugin.IYouTubeLiveSuperchat item)
                {
                    mcvCvm = new McvYouTubeLiveCommentViewModel(item, messageContext.Metadata, messageContext.Methods, connectionName, _options);
                }
                else if (youtubeMessage is YouTubeLiveSitePlugin.IYouTubeLivePaidSticker sticker)
                {
                    mcvCvm = new McvYouTubeLiveCommentViewModel(sticker, messageContext.Metadata, messageContext.Methods, connectionName, _options);
                }
                else if (youtubeMessage is YouTubeLiveSitePlugin.IYouTubeLiveSponsorshipsGiftPurchaseAnnouncement GiftPurchaseAnnouncement)
                {
                    mcvCvm = new McvYouTubeLiveCommentViewModel(GiftPurchaseAnnouncement, messageContext.Metadata, messageContext.Methods, connectionName, _options);
                }
                else if (youtubeMessage is YouTubeLiveSitePlugin.IYouTubeLiveMembership member)
                {
                    mcvCvm = new McvYouTubeLiveCommentViewModel(member, messageContext.Metadata, messageContext.Methods, connectionName, _options);
                }
                else if (youtubeMessage is YouTubeLiveSitePlugin.IYouTubeLiveConnected connected)
                {
                    mcvCvm = new McvYouTubeLiveCommentViewModel(connected, messageContext.Metadata, messageContext.Methods, connectionName, _options);
                }
                else if (youtubeMessage is YouTubeLiveSitePlugin.IYouTubeLiveDisconnected disconnected)
                {
                    mcvCvm = new McvYouTubeLiveCommentViewModel(disconnected, messageContext.Metadata, messageContext.Methods, connectionName, _options);
                }
            }
            else if (messageContext.Message is MirrativSitePlugin.IMirrativMessage mirrativMessage)
            {
                if (mirrativMessage is MirrativSitePlugin.IMirrativComment comment)
                {
                    mcvCvm = new McvMirrativCommentViewModel(comment, messageContext.Metadata, messageContext.Methods, connectionName, _options);
                }
                else if (mirrativMessage is MirrativSitePlugin.IMirrativJoinRoom join)
                {
                    mcvCvm = new McvMirrativCommentViewModel(join, messageContext.Metadata, messageContext.Methods, connectionName, _options);
                }
                else if (mirrativMessage is MirrativSitePlugin.IMirrativItem item)
                {
                    mcvCvm = new McvMirrativCommentViewModel(item, messageContext.Metadata, messageContext.Methods, connectionName, _options);
                }
                else if (mirrativMessage is MirrativSitePlugin.IMirrativConnected connected)
                {
                    mcvCvm = new McvMirrativCommentViewModel(connected, messageContext.Metadata, messageContext.Methods, connectionName, _options);
                }
                else if (mirrativMessage is MirrativSitePlugin.IMirrativDisconnected disconnected)
                {
                    mcvCvm = new McvMirrativCommentViewModel(disconnected, messageContext.Metadata, messageContext.Methods, connectionName, _options);
                }
            }
            else if (messageContext.Message is TwitchSitePlugin.ITwitchMessage twitchMessage)
            {
                if (twitchMessage is TwitchSitePlugin.ITwitchComment comment)
                {
                    mcvCvm = new TwitchCommentViewModel(comment, messageContext.Metadata, messageContext.Methods, connectionName, _options);
                }
                if (twitchMessage is TwitchSitePlugin.ITwitchNotice notice)
                {
                    mcvCvm = new TwitchCommentViewModel(notice, messageContext.Metadata, messageContext.Methods, connectionName, _options);
                }
            }
            else if (messageContext.Message is OpenrecSitePlugin.IOpenrecMessage openrecMessage)
            {
                if (openrecMessage is OpenrecSitePlugin.IOpenrecComment comment)
                {
                    mcvCvm = new OpenrecCommentViewModel(comment, messageContext.Metadata, messageContext.Methods, connectionName, _options);
                }
                else if (openrecMessage is OpenrecSitePlugin.IOpenrecStamp stamp)
                {
                    mcvCvm = new OpenrecCommentViewModel(stamp, messageContext.Metadata, messageContext.Methods, connectionName, _options);
                }
                else if (openrecMessage is OpenrecSitePlugin.IOpenrecYell yell)
                {
                    mcvCvm = new OpenrecCommentViewModel(yell, messageContext.Metadata, messageContext.Methods, connectionName, _options);
                }
                else if (openrecMessage is OpenrecSitePlugin.IOpenrecConnected connected)
                {
                    mcvCvm = new OpenrecCommentViewModel(connected, messageContext.Metadata, messageContext.Methods, connectionName, _options);
                }
                else if (openrecMessage is OpenrecSitePlugin.IOpenrecDisconnected disconnected)
                {
                    mcvCvm = new OpenrecCommentViewModel(disconnected, messageContext.Metadata, messageContext.Methods, connectionName, _options);
                }
            }
            else if (messageContext.Message is BLiveSitePlugin.IBLiveMessage bliveMessage)
            {
                if (bliveMessage is BLiveSitePlugin.IBLiveComment comment)
                {
                    mcvCvm = new BLiveCommentViewModel(comment, messageContext.Metadata, messageContext.Methods, connectionName, _options);
                }
                else if (bliveMessage is BLiveSitePlugin.IBLiveStamp stamp)
                {
                    mcvCvm = new BLiveCommentViewModel(stamp, messageContext.Metadata, messageContext.Methods, connectionName, _options);
                }
                else if (bliveMessage is BLiveSitePlugin.IBLiveYell yell)
                {
                    mcvCvm = new BLiveCommentViewModel(yell, messageContext.Metadata, messageContext.Methods, connectionName, _options);
                }
                else if (bliveMessage is BLiveSitePlugin.IBLiveConnected connected)
                {
                    mcvCvm = new BLiveCommentViewModel(connected, messageContext.Metadata, messageContext.Methods, connectionName, _options);
                }
                else if (bliveMessage is BLiveSitePlugin.IBLiveDisconnected disconnected)
                {
                    mcvCvm = new BLiveCommentViewModel(disconnected, messageContext.Metadata, messageContext.Methods, connectionName, _options);
                }
            }
            else if (messageContext.Message is MixchSitePlugin.IMixchMessage mixchMessage)
            {
                mcvCvm = new MixchCommentViewModel(mixchMessage, messageContext.Metadata, messageContext.Methods, connectionName, _options);
            }
            else if (messageContext.Message is LineLiveSitePlugin.ILineLiveMessage lineliveMessage)
            {
                if (lineliveMessage is LineLiveSitePlugin.ILineLiveComment comment)
                {
                    mcvCvm = new LineLiveCommentViewModel(comment, messageContext.Metadata, messageContext.Methods, connectionName, _options);
                }
                else if (lineliveMessage is LineLiveSitePlugin.ILineLiveItem item)
                {
                    mcvCvm = new LineLiveCommentViewModel(item, messageContext.Metadata, messageContext.Methods, connectionName, _options);
                }
            }
            else if (messageContext.Message is NicoSitePlugin.INicoMessage nicoMessage)
            {
                if (nicoMessage is NicoSitePlugin.INicoComment comment)
                {
                    mcvCvm = new NicoCommentViewModel(comment, messageContext.Metadata, messageContext.Methods, connectionName, _options);
                }
                else if (nicoMessage is NicoSitePlugin.INicoAd ad)
                {
                    mcvCvm = new NicoCommentViewModel(ad, messageContext.Metadata, messageContext.Methods, connectionName, _options);
                }
                else if (nicoMessage is NicoSitePlugin.INicoGift item)
                {
                    mcvCvm = new NicoCommentViewModel(item, messageContext.Metadata, messageContext.Methods, connectionName, _options);
                }
                else if (nicoMessage is NicoSitePlugin.INicoSpi spi)
                {
                    mcvCvm = new NicoCommentViewModel(spi, messageContext.Metadata, messageContext.Methods, connectionName, _options);
                }
                else if (nicoMessage is NicoSitePlugin.INicoEmotion emotion && messageContext.Metadata.SiteOptions is INicoSiteOptions nicoSiteOptions)
                {
                    if (nicoSiteOptions.IsShowEmotion)
                    {
                        mcvCvm = new NicoCommentViewModel(emotion, messageContext.Metadata, messageContext.Methods, connectionName, _options);
                    }
                }
                else if (nicoMessage is NicoSitePlugin.INicoInfo info)
                {
                    mcvCvm = new NicoCommentViewModel(info, messageContext.Metadata, messageContext.Methods, connectionName, _options);
                }
                else if (nicoMessage is NicoSitePlugin.INicoConnected connected)
                {
                    mcvCvm = new NicoCommentViewModel(connected, messageContext.Metadata, messageContext.Methods, connectionName, _options);
                }
                else if (nicoMessage is NicoSitePlugin.INicoDisconnected disconnected)
                {
                    mcvCvm = new NicoCommentViewModel(disconnected, messageContext.Metadata, messageContext.Methods, connectionName, _options);
                }
            }
            else if (messageContext.Message is TwicasSitePlugin.ITwicasMessage twicasMessage)
            {
                if (twicasMessage is TwicasSitePlugin.ITwicasComment comment)
                {
                    mcvCvm = new TwicasCommentViewModel(comment, messageContext.Metadata, messageContext.Methods, connectionName);
                }
                else if (twicasMessage is TwicasSitePlugin.ITwicasItem item)
                {
                    mcvCvm = new TwicasCommentViewModel(item, messageContext.Metadata, messageContext.Methods, connectionName);
                }
                else if (twicasMessage is TwicasSitePlugin.ITwicasConnected connected)
                {
                    mcvCvm = new TwicasCommentViewModel(connected, messageContext.Metadata, messageContext.Methods, connectionName);
                }
                else if (twicasMessage is TwicasSitePlugin.ITwicasDisconnected disconnected)
                {
                    mcvCvm = new TwicasCommentViewModel(disconnected, messageContext.Metadata, messageContext.Methods, connectionName);
                }
            }
            else if (messageContext.Message is PeriscopeSitePlugin.IPeriscopeMessage periscopeMessage)
            {
                if (periscopeMessage is PeriscopeSitePlugin.IPeriscopeComment comment)
                {
                    mcvCvm = new PeriscopeCommentViewModel(comment, messageContext.Metadata, messageContext.Methods, connectionName, _options);
                }
                else if (periscopeMessage is PeriscopeSitePlugin.IPeriscopeConnected connected)
                {
                    mcvCvm = new PeriscopeCommentViewModel(connected, messageContext.Metadata, messageContext.Methods, connectionName, _options);
                }
                else if (periscopeMessage is PeriscopeSitePlugin.IPeriscopeDisconnected disconnected)
                {
                    mcvCvm = new PeriscopeCommentViewModel(disconnected, messageContext.Metadata, messageContext.Methods, connectionName, _options);
                }
                else if (periscopeMessage is PeriscopeSitePlugin.IPeriscopeJoin join)
                {
                    mcvCvm = new PeriscopeCommentViewModel(join, messageContext.Metadata, messageContext.Methods, connectionName, _options);
                }
                else if (periscopeMessage is PeriscopeSitePlugin.IPeriscopeLeave leave)
                {
                    mcvCvm = new PeriscopeCommentViewModel(leave, messageContext.Metadata, messageContext.Methods, connectionName, _options);
                }
            }
            else if (messageContext.Message is ShowRoomSitePlugin.IShowRoomMessage showRoomMessage)
            {
                if (showRoomMessage is ShowRoomSitePlugin.IShowRoomComment comment)
                {
                    mcvCvm = new ShowRoomCommentViewModel(comment, messageContext.Metadata, messageContext.Methods, connectionName, _options);
                }
                else if (showRoomMessage is ShowRoomSitePlugin.IShowRoomConnected connected)
                {
                    mcvCvm = new ShowRoomCommentViewModel(connected, messageContext.Metadata, messageContext.Methods, connectionName, _options);
                }
                else if (showRoomMessage is ShowRoomSitePlugin.IShowRoomDisconnected disconnected)
                {
                    mcvCvm = new ShowRoomCommentViewModel(disconnected, messageContext.Metadata, messageContext.Methods, connectionName, _options);
                }
                else if (showRoomMessage is ShowRoomSitePlugin.IShowRoomJoin join)
                {
                    mcvCvm = new ShowRoomCommentViewModel(join, messageContext.Metadata, messageContext.Methods, connectionName, _options);
                }
                else if (showRoomMessage is ShowRoomSitePlugin.IShowRoomLeave leave)
                {
                    mcvCvm = new ShowRoomCommentViewModel(leave, messageContext.Metadata, messageContext.Methods, connectionName, _options);
                }
            }
            else if (messageContext.Message is MildomSitePlugin.IMildomMessage mildomMessage)
            {
                if (mildomMessage is MildomSitePlugin.IMildomComment comment)
                {
                    mcvCvm = new McvMildomCommentViewModel(comment, messageContext.Metadata, messageContext.Methods, connectionName, _options);
                }
                else if (mildomMessage is MildomSitePlugin.IMildomJoinRoom join)
                {
                    mcvCvm = new McvMildomCommentViewModel(join, messageContext.Metadata, messageContext.Methods, connectionName, _options);
                }
                else if (mildomMessage is MildomSitePlugin.IMildomGift gift)
                {
                    mcvCvm = new McvMildomCommentViewModel(gift, messageContext.Metadata, messageContext.Methods, connectionName, _options);
                }
            }
            else if (messageContext.Message is BigoSitePlugin.IBigoMessage bigoMessage)
            {
                switch (bigoMessage)
                {
                    case BigoSitePlugin.IBigoComment bigoComment:
                        mcvCvm = new McvBigoCommentViewModel(bigoComment, messageContext.Metadata, messageContext.Methods, connectionName, _options);
                        break;
                    case BigoSitePlugin.IBigoGift bigoGift:
                        mcvCvm = new McvBigoGiftViewModel(bigoGift, messageContext.Metadata, messageContext.Methods, connectionName, _options);
                        break;
                }
            }
            else if (messageContext.Message is TestSitePlugin.ITestMessage testMessage)
            {
                if (testMessage is TestSitePlugin.ITestComment comment)
                {
                    mcvCvm = new TestCommentViewModel(comment, messageContext.Metadata, messageContext.Methods, connectionName);
                }
            }
            if (mcvCvm != null)
            {
                if (_dispatcher != null)
                {
                    _dispatcher.Invoke(() =>
                    {
                        AddComment(mcvCvm);
                    });
                }
                else
                {
                    AddComment(mcvCvm);
                }
            }
        }

        private void AddComment(IMcvCommentViewModel mcvCvm)
        {
            if (_options.IsAddingNewCommentTop)
            {
                _comments.Insert(0, mcvCvm);
            }
            else
            {
                _comments.Add(mcvCvm);
            }
        }
        #region EventHandler
        private async void Connection_MessageReceived(object sender, IMessageContext e)
        {
            Debug.Assert(e != null);
            if (e == null) return;
            var connectionViewModel = sender as ConnectionViewModel;
            Debug.Assert(connectionViewModel != null);
            var methods = e.Methods;
            try
            {
                //TODO:Comments.AddRange()が欲しい
                await _dispatcher.BeginInvoke((Action)(() =>
                {
                    var comment = e;
                    //if (!_userDict.TryGetValue(comment.UserId, out UserViewModel uvm))
                    //{
                    //    var user = _userStore.GetUser(comment.UserId);
                    //    uvm = new UserViewModel(user, _options);
                    //    _userDict.Add(comment.UserId, uvm);
                    //}
                    //comment.User = uvm.User;
                    AddComment(comment, connectionViewModel);
                    //uvm.Comments.Add(comment);
                }), DispatcherPriority.Normal);
                //プラグインに渡すのはIInfoMessage以外全て
                if (!(e.Message is IInfoMessage))
                {
                    _pluginManager.SetMessage(e.Message, e.Metadata);
                }
                await methods.AfterCommentAdded();
            }
            catch (TaskCanceledException) { }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                _logger.LogException(ex, "", $"{e.ToString()}");
            }
        }
        bool IsComment(MessageType type)
        {
            return !(type == MessageType.SystemInfo || type == MessageType.BroadcastInfo);
        }
        private void Connection_MetadataReceived(object sender, IMetadata e)
        {
            if (sender is ConnectionViewModel connection)
            {
                try
                {
                    var metaVm = _metaDict[connection];
                    if (e.Title != null)
                        metaVm.Title = e.Title;
                    if (e.Active != null)
                        metaVm.Active = e.Active;
                    if (e.CurrentViewers != null)
                        metaVm.CurrentViewers = e.CurrentViewers;
                    if (e.TotalViewers != null)
                        metaVm.TotalViewers = e.TotalViewers;
                    if (e.Elapsed != null)
                        metaVm.Elapsed = e.Elapsed;
                    if (e.Others != null)
                    {
                        metaVm.Others = e.Others;
                    }
                }
                catch (KeyNotFoundException ex)
                {
                    //{"Ex":{"Message":"指定されたキーはディレクトリ内に存在しませんでした。","StackTrace":"   場所 System.Collections.Generic.Dictionary`2.get_Item(TKey key)\r\n   場所 MultiCommentViewer.MainViewModel.Connection_MetadataReceived(Object sender, IMetadata e) 場所 K:\\Programming\\workspace\\MultiCommentViewer\\MultiCommentViewer\\ViewModels\\MainViewModel.cs:行 550","Timestamp":"2018/11/24 18:28:49","InnerError":null},"Title":"","Detail":""},
                    var keys = _metaDict.Select(kv => kv.Key.Name).Aggregate((a, b) => a + "," + b);
                    _logger.LogException(ex, "バグ", $"keys=\"{keys}\" find=\"{connection.Name}\"");
                    Debug.Fail("");
                }
                catch (Exception ex)
                {
                    _logger.LogException(ex);
                }
            }
        }
        #endregion //EventHandler




        #region Properties
        public ObservableCollection<MetadataViewModel> MetaCollection { get; } = new ObservableCollection<MetadataViewModel>();
        public ObservableCollection<PluginMenuItemViewModel> PluginMenuItemCollection { get; } = new ObservableCollection<PluginMenuItemViewModel>();
        private readonly ObservableCollection<IMcvCommentViewModel> _comments = new ObservableCollection<IMcvCommentViewModel>();
        public ObservableCollection<ConnectionViewModel> Connections { get; } = new ObservableCollection<ConnectionViewModel>();
        public ICollectionViewLiveShaping ConnectedConnections { get; }

        private ConnectionViewModel _selectedConnection;
        public ConnectionViewModel SelectedConnection
        {
            get { return _selectedConnection; }
            set
            {
                _selectedConnection = value;
                if (_selectedConnection == null)
                {
                    MessengerInstance.Send(new SetPostCommentPanel(null));
                }
                else
                {
                    MessengerInstance.Send(new SetPostCommentPanel(_selectedConnection.CommentPostPanel));
                }
                RaisePropertyChanged();
            }
        }
        private string GetVersionNumber()
        {
            var asm = System.Reflection.Assembly.GetExecutingAssembly();
            var ver = asm.GetName().Version;
            var s = $"{ver.Major}.{ver.Minor}.{ver.Build}";
            return s;
        }
        private string GetAppName()
        {
            var asm = System.Reflection.Assembly.GetExecutingAssembly();
            var title = asm.GetName().Name;
            return title;
        }
        private string GetUserAgent()
        {
            return $"{GetAppName()}/{GetVersionNumber()} contact-> twitter.com/kv510k";
        }
        public string Title
        {
            get
            {
                var s = $"{GetAppName()} v{GetVersionNumber()}";
#if BETA
                s += " (ベータ版)";
#elif ALPHA
                s += " (アルファ版)";
#elif DEBUG
                s += " (DEBUG)";
#endif
                return s;
            }
        }
        public bool Topmost
        {
            get { return _options.IsTopmost; }
            set { _options.IsTopmost = value; }
        }
        public double MainViewHeight
        {
            get { return _options.MainViewHeight; }
            set { _options.MainViewHeight = value; }
        }
        public double MainViewWidth
        {
            get { return _options.MainViewWidth; }
            set { _options.MainViewWidth = value; }
        }
        public double MainViewLeft
        {
            get { return _options.MainViewLeft; }
            set { _options.MainViewLeft = value; }
        }
        public double MainViewTop
        {
            get { return _options.MainViewTop; }
            set { _options.MainViewTop = value; }
        }
        public double ConnectionViewHeight
        {
            get { return _options.ConnectionViewHeight; }
            set { _options.ConnectionViewHeight = value; }
        }
        public double MetadataViewHeight
        {
            get { return _options.MetadataViewHeight; }
            set { _options.MetadataViewHeight = value; }
        }
        public double MetadataViewConnectionNameColumnWidth
        {
            get { return _options.MetadataViewConnectionNameColumnWidth; }
            set { _options.MetadataViewConnectionNameColumnWidth = value; }
        }
        public double MetadataViewTitleColumnWidth
        {
            get { return _options.MetadataViewTitleColumnWidth; }
            set { _options.MetadataViewTitleColumnWidth = value; }
        }
        public double MetadataViewElapsedColumnWidth
        {
            get { return _options.MetadataViewElapsedColumnWidth; }
            set { _options.MetadataViewElapsedColumnWidth = value; }
        }
        public double MetadataViewCurrentViewersColumnWidth
        {
            get { return _options.MetadataViewCurrentViewersColumnWidth; }
            set { _options.MetadataViewCurrentViewersColumnWidth = value; }
        }
        public double MetadataViewTotalViewersColumnWidth
        {
            get { return _options.MetadataViewTotalViewersColumnWidth; }
            set { _options.MetadataViewTotalViewersColumnWidth = value; }
        }
        public double MetadataViewActiveColumnWidth
        {
            get { return _options.MetadataViewActiveColumnWidth; }
            set { _options.MetadataViewActiveColumnWidth = value; }
        }
        public double MetadataViewOthersColumnWidth
        {
            get { return _options.MetadataViewOthersColumnWidth; }
            set { _options.MetadataViewOthersColumnWidth = value; }
        }


        //public Brush HorizontalGridLineBrush
        //{
        //    get { return new SolidColorBrush(_options.HorizontalGridLineColor); }
        //}
        //public Brush VerticalGridLineBrush
        //{
        //    get { return new SolidColorBrush(_options.VerticalGridLineColor); }
        //}
        //public double ConnectionNameWidth
        //{
        //    get { return _options.ConnectionNameWidth; }
        //    set { _options.ConnectionNameWidth = value; }
        //}
        //public bool IsShowConnectionName
        //{
        //    get { return _options.IsShowConnectionName; }
        //    set { _options.IsShowConnectionName = value; }
        //}
        //public int ConnectionNameDisplayIndex
        //{
        //    get { return _options.ConnectionNameDisplayIndex; }
        //    set { _options.ConnectionNameDisplayIndex = value; }
        //}
        //public double ThumbnailWidth
        //{
        //    get { return _options.ThumbnailWidth; }
        //    set { _options.ThumbnailWidth = value; }
        //}
        //public bool IsShowThumbnail
        //{
        //    get { return _options.IsShowThumbnail; }
        //    set { _options.IsShowThumbnail = value; }
        //}
        //public int ThumbnailDisplayIndex
        //{
        //    get { return _options.ThumbnailDisplayIndex; }
        //    set { _options.ThumbnailDisplayIndex = value; }
        //}
        //public double CommentIdWidth
        //{
        //    get { return _options.CommentIdWidth; }
        //    set { _options.CommentIdWidth = value; }
        //}
        //public bool IsShowCommentId
        //{
        //    get { return _options.IsShowCommentId; }
        //    set { _options.IsShowCommentId = value; }
        //}
        //public int CommentIdDisplayIndex
        //{
        //    get { return _options.CommentIdDisplayIndex; }
        //    set { _options.CommentIdDisplayIndex = value; }
        //}
        //public double UsernameWidth
        //{
        //    get { return _options.UsernameWidth; }
        //    set { _options.UsernameWidth = value; }
        //}
        //public bool IsShowUsername
        //{
        //    get { return _options.IsShowUsername; }
        //    set { _options.IsShowUsername = value; }
        //}
        //public int UsernameDisplayIndex
        //{
        //    get { return _options.UsernameDisplayIndex; }
        //    set { _options.UsernameDisplayIndex = value; }
        //}

        //public double MessageWidth
        //{
        //    get { return _options.MessageWidth; }
        //    set { _options.MessageWidth = value; }
        //}
        //public bool IsShowMessage
        //{
        //    get { return _options.IsShowMessage; }
        //    set { _options.IsShowMessage = value; }
        //}
        //public int MessageDisplayIndex
        //{
        //    get { return _options.MessageDisplayIndex; }
        //    set { _options.MessageDisplayIndex = value; }
        //}

        //public double InfoWidth
        //{
        //    get { return _options.InfoWidth; }
        //    set { _options.InfoWidth = value; }
        //}
        //public bool IsShowInfo
        //{
        //    get { return _options.IsShowInfo; }
        //    set { _options.IsShowInfo = value; }
        //}
        //public int InfoDisplayIndex
        //{
        //    get { return _options.InfoDisplayIndex; }
        //    set { _options.InfoDisplayIndex = value; }
        //}
        //public Color SelectedRowBackColor
        //{
        //    get { return _options.SelectedRowBackColor; }
        //    set { _options.SelectedRowBackColor = value; }
        //}
        //public Color SelectedRowForeColor
        //{
        //    get { return _options.SelectedRowForeColor; }
        //    set { _options.SelectedRowForeColor = value; }
        //}
        public bool IsShowMetaConnectionName
        {
            get => _options.IsShowMetaConnectionName;
            set => _options.IsShowMetaConnectionName = value;
        }
        public int MetadataViewConnectionNameDisplayIndex
        {
            get => _options.MetadataViewConnectionNameDisplayIndex;
            set => _options.MetadataViewConnectionNameDisplayIndex = value;
        }

        public bool IsShowMetaTitle
        {
            get => _options.IsShowMetaTitle;
            set => _options.IsShowMetaTitle = value;
        }
        public int MetadataViewTitleDisplayIndex
        {
            get => _options.MetadataViewTitleDisplayIndex;
            set => _options.MetadataViewTitleDisplayIndex = value;
        }

        public bool IsShowMetaElapse
        {
            get => _options.IsShowMetaElapse;
            set => _options.IsShowMetaElapse = value;
        }
        public int MetadataViewElapsedDisplayIndex
        {
            get => _options.MetadataViewElapsedDisplayIndex;
            set => _options.MetadataViewElapsedDisplayIndex = value;
        }

        public bool IsShowMetaCurrentViewers
        {
            get => _options.IsShowMetaCurrentViewers;
            set => _options.IsShowMetaCurrentViewers = value;
        }
        public int MetadataViewCurrentViewersDisplayIndex
        {
            get => _options.MetadataViewCurrentViewersDisplayIndex;
            set => _options.MetadataViewCurrentViewersDisplayIndex = value;
        }

        public bool IsShowMetaTotalViewers
        {
            get => _options.IsShowMetaTotalViewers;
            set => _options.IsShowMetaTotalViewers = value;
        }
        public int MetadataViewTotalViewersDisplayIndex
        {
            get => _options.MetadataViewTotalViewersDisplayIndex;
            set => _options.MetadataViewTotalViewersDisplayIndex = value;
        }

        public bool IsShowMetaActive
        {
            get => _options.IsShowMetaActive;
            set => _options.IsShowMetaActive = value;
        }
        public int MetadataViewActiveDisplayIndex
        {
            get => _options.MetadataViewActiveDisplayIndex;
            set => _options.MetadataViewActiveDisplayIndex = value;
        }

        public bool IsShowMetaOthers
        {
            get => _options.IsShowMetaOthers;
            set => _options.IsShowMetaOthers = value;
        }
        public int MetadataViewOthersDisplayIndex
        {
            get => _options.MetadataViewOthersDisplayIndex;
            set => _options.MetadataViewOthersDisplayIndex = value;
        }

        public bool ContainsUrl
        {
            get
            {
                return !string.IsNullOrEmpty(GetUrlFromSelectedComment());
            }
        }
        private string GetUrlFromSelectedComment()
        {
            var selectedComment = SelectedComment;
            if (selectedComment == null)
            {
                return null;
            }
            var message = selectedComment.MessageItems.ToText();
            if (message == null)
            {
                return null;
            }
            var match = Regex.Match(message, "(https?://([\\w-]+.)+[\\w-]+(?:/[\\w- ./?%&=]))?");
            if (match.Success)
            {
                return match.Groups[1].Value;
            }
            return null;
        }
        private void OpenUrl()
        {
            var url = GetUrlFromSelectedComment();
            Process.Start(url);
            SetSystemInfo("open: " + url, InfoType.Debug);
        }
        private void CopyComment()
        {
            var message = SelectedComment.MessageItems.ToText();
            try
            {
                System.Windows.Clipboard.SetText(message);
            }
            catch (System.Runtime.InteropServices.COMException) { }
            SetSystemInfo("copy: " + message, InfoType.Debug);
        }
        #region ConnectionsView
        #region ConnectionsViewSelection
        public int ConnectionsViewSelectionDisplayIndex
        {
            get { return _options.ConnectionsViewSelectionDisplayIndex; }
            set { _options.ConnectionsViewSelectionDisplayIndex = value; }
        }
        public double ConnectionsViewSelectionWidth
        {
            get { return _options.ConnectionsViewSelectionWidth; }
            set { _options.ConnectionsViewSelectionWidth = value; }
        }
        public bool IsShowConnectionsViewSelection
        {
            get { return _options.IsShowConnectionsViewSelection; }
            set { _options.IsShowConnectionsViewSelection = value; }
        }
        #endregion
        #region ConnectionsViewSite
        public int ConnectionsViewSiteDisplayIndex
        {
            get { return _options.ConnectionsViewSiteDisplayIndex; }
            set { _options.ConnectionsViewSiteDisplayIndex = value; }
        }
        public double ConnectionsViewSiteWidth
        {
            get { return _options.ConnectionsViewSiteWidth; }
            set { _options.ConnectionsViewSiteWidth = value; }
        }
        public bool IsShowConnectionsViewSite
        {
            get { return _options.IsShowConnectionsViewSite; }
            set { _options.IsShowConnectionsViewSite = value; }
        }
        #endregion
        #region ConnectionsViewConnectionName
        public int ConnectionsViewConnectionNameDisplayIndex
        {
            get { return _options.ConnectionsViewConnectionNameDisplayIndex; }
            set { _options.ConnectionsViewConnectionNameDisplayIndex = value; }
        }
        public double ConnectionsViewConnectionNameWidth
        {
            get { return _options.ConnectionsViewConnectionNameWidth; }
            set { _options.ConnectionsViewConnectionNameWidth = value; }
        }
        public bool IsShowConnectionsViewConnectionName
        {
            get { return _options.IsShowConnectionsViewConnectionName; }
            set { _options.IsShowConnectionsViewConnectionName = value; }
        }
        #endregion
        #region ConnectionsViewInput
        public int ConnectionsViewInputDisplayIndex
        {
            get { return _options.ConnectionsViewInputDisplayIndex; }
            set { _options.ConnectionsViewInputDisplayIndex = value; }
        }
        public double ConnectionsViewInputWidth
        {
            get { return _options.ConnectionsViewInputWidth; }
            set { _options.ConnectionsViewInputWidth = value; }
        }
        public bool IsShowConnectionsViewInput
        {
            get { return _options.IsShowConnectionsViewInput; }
            set { _options.IsShowConnectionsViewInput = value; }
        }
        #endregion
        #region ConnectionsViewBrowser
        public int ConnectionsViewBrowserDisplayIndex
        {
            get { return _options.ConnectionsViewBrowserDisplayIndex; }
            set { _options.ConnectionsViewBrowserDisplayIndex = value; }
        }
        public double ConnectionsViewBrowserWidth
        {
            get { return _options.ConnectionsViewBrowserWidth; }
            set { _options.ConnectionsViewBrowserWidth = value; }
        }
        public bool IsShowConnectionsViewBrowser
        {
            get { return _options.IsShowConnectionsViewBrowser; }
            set { _options.IsShowConnectionsViewBrowser = value; }
        }
        #endregion
        #region ConnectionsViewConnection
        public int ConnectionsViewConnectionDisplayIndex
        {
            get { return _options.ConnectionsViewConnectionDisplayIndex; }
            set { _options.ConnectionsViewConnectionDisplayIndex = value; }
        }
        public double ConnectionsViewConnectionWidth
        {
            get { return _options.ConnectionsViewConnectionWidth; }
            set { _options.ConnectionsViewConnectionWidth = value; }
        }
        public bool IsShowConnectionsViewConnection
        {
            get { return _options.IsShowConnectionsViewConnection; }
            set { _options.IsShowConnectionsViewConnection = value; }
        }
        #endregion
        #region ConnectionsViewDisconnection
        public int ConnectionsViewDisconnectionDisplayIndex
        {
            get { return _options.ConnectionsViewDisconnectionDisplayIndex; }
            set { _options.ConnectionsViewDisconnectionDisplayIndex = value; }
        }
        public double ConnectionsViewDisconnectionWidth
        {
            get { return _options.ConnectionsViewDisconnectionWidth; }
            set { _options.ConnectionsViewDisconnectionWidth = value; }
        }
        public bool IsShowConnectionsViewDisconnection
        {
            get { return _options.IsShowConnectionsViewDisconnection; }
            set { _options.IsShowConnectionsViewDisconnection = value; }
        }
        #endregion
        #region ConnectionsViewSave
        public int ConnectionsViewSaveDisplayIndex
        {
            get { return _options.ConnectionsViewSaveDisplayIndex; }
            set { _options.ConnectionsViewSaveDisplayIndex = value; }
        }
        public double ConnectionsViewSaveWidth
        {
            get { return _options.ConnectionsViewSaveWidth; }
            set { _options.ConnectionsViewSaveWidth = value; }
        }
        public bool IsShowConnectionsViewSave
        {
            get { return _options.IsShowConnectionsViewSave; }
            set { _options.IsShowConnectionsViewSave = value; }
        }
        #endregion
        #region ConnectionsViewLoggedinUsername
        public int ConnectionsViewLoggedinUsernameDisplayIndex
        {
            get { return _options.ConnectionsViewLoggedinUsernameDisplayIndex; }
            set { _options.ConnectionsViewLoggedinUsernameDisplayIndex = value; }
        }
        public double ConnectionsViewLoggedinUsernameWidth
        {
            get { return _options.ConnectionsViewLoggedinUsernameWidth; }
            set { _options.ConnectionsViewLoggedinUsernameWidth = value; }
        }
        public bool IsShowConnectionsViewLoggedinUsername
        {
            get { return _options.IsShowConnectionsViewLoggedinUsername; }
            set { _options.IsShowConnectionsViewLoggedinUsername = value; }
        }
        #endregion
        #region ConnectionsViewConnectionBackground
        public int ConnectionsViewConnectionBackgroundDisplayIndex
        {
            get { return _options.ConnectionsViewConnectionBackgroundDisplayIndex; }
            set { _options.ConnectionsViewConnectionBackgroundDisplayIndex = value; }
        }
        public double ConnectionsViewConnectionBackgroundWidth
        {
            get { return _options.ConnectionsViewConnectionBackgroundWidth; }
            set { _options.ConnectionsViewConnectionBackgroundWidth = value; }
        }
        public bool IsShowConnectionsViewConnectionBackground
        {
            get { return _options.IsEnabledSiteConnectionColor && _options.SiteConnectionColorType == SiteConnectionColorType.Connection; }
        }
        #endregion
        #region ConnectionsViewConnectionForeground
        public int ConnectionsViewConnectionForegroundDisplayIndex
        {
            get { return _options.ConnectionsViewConnectionForegroundDisplayIndex; }
            set { _options.ConnectionsViewConnectionForegroundDisplayIndex = value; }
        }
        public double ConnectionsViewConnectionForegroundWidth
        {
            get { return _options.ConnectionsViewConnectionForegroundWidth; }
            set { _options.ConnectionsViewConnectionForegroundWidth = value; }
        }
        public bool IsShowConnectionsViewConnectionForeground
        {
            get { return _options.IsEnabledSiteConnectionColor && _options.SiteConnectionColorType == SiteConnectionColorType.Connection; }
        }
        #endregion
        #endregion

        public double ConnectionColorColumnWidth
        {
            get
            {
                if (_options.IsEnabledSiteConnectionColor && _options.SiteConnectionColorType == SiteConnectionColorType.Connection)
                {
                    return 100;
                }
                else
                {
                    return 0;
                }
            }
        }
        public System.Windows.Controls.DataGridGridLinesVisibility GridLinesVisibility
        {
            get
            {
                if (_options.IsShowHorizontalGridLine && _options.IsShowVerticalGridLine)
                    return System.Windows.Controls.DataGridGridLinesVisibility.All;
                else if (_options.IsShowHorizontalGridLine)
                    return System.Windows.Controls.DataGridGridLinesVisibility.Horizontal;
                else if (_options.IsShowVerticalGridLine)
                    return System.Windows.Controls.DataGridGridLinesVisibility.Vertical;
                else
                    return System.Windows.Controls.DataGridGridLinesVisibility.None;
            }
        }
        public Brush TitleForeground => new SolidColorBrush(_options.TitleForeColor);
        public Brush TitleBackground => new SolidColorBrush(_options.TitleBackColor);
        public Brush ViewBackground => new SolidColorBrush(_options.ViewBackColor);
        public Brush WindowBorderBrush => new SolidColorBrush(_options.WindowBorderColor);
        public Brush SystemButtonForeground => new SolidColorBrush(_options.SystemButtonForeColor);
        public Brush SystemButtonBackground => new SolidColorBrush(_options.SystemButtonBackColor);
        public Brush SystemButtonBorderBrush => new SolidColorBrush(_options.SystemButtonBorderColor);
        public Brush SystemButtonMouseOverBackground => new SolidColorBrush(_options.SystemButtonMouseOverBackColor);
        public Brush SystemButtonMouseOverForeground => new SolidColorBrush(_options.SystemButtonMouseOverForeColor);
        public Brush SystemButtonMouseOverBorderBrush => new SolidColorBrush(_options.SystemButtonMouseOverBorderColor);
        public Brush MenuBackground => new SolidColorBrush(_options.MenuBackColor);
        public Brush MenuForeground => new SolidColorBrush(_options.MenuForeColor);
        public Brush MenuItemCheckMarkBrush => new SolidColorBrush(_options.MenuItemCheckMarkColor);
        public Brush MenuItemMouseOverBackground => new SolidColorBrush(_options.MenuItemMouseOverBackColor);
        public Brush MenuItemMouseOverForeground => new SolidColorBrush(_options.MenuItemMouseOverForeColor);
        public Brush MenuItemMouseOverBorderBrush => new SolidColorBrush(_options.MenuItemMouseOverBorderColor);
        public Brush MenuItemMouseOverCheckMarkBrush => new SolidColorBrush(_options.MenuItemMouseOverCheckMarkColor);
        public Brush MenuSeparatorBackground => new SolidColorBrush(_options.MenuSeparatorBackColor);
        public Brush MenuPopupBorderBrush => new SolidColorBrush(_options.MenuPopupBorderColor);
        public Brush ButtonBackground => new SolidColorBrush(_options.ButtonBackColor);
        public Brush ButtonForeground => new SolidColorBrush(_options.ButtonForeColor);
        public Brush ButtonBorderBrush => new SolidColorBrush(_options.ButtonBorderColor);
        public Brush CommentListBackground => new SolidColorBrush(_options.CommentListBackColor);
        public Brush CommentListBorderBrush => new SolidColorBrush(_options.CommentListBorderColor);
        public Brush CommentListHeaderBackground => new SolidColorBrush(_options.CommentListHeaderBackColor);
        public Brush CommentListHeaderForeground => new SolidColorBrush(_options.CommentListHeaderForeColor);
        public Brush CommentListHeaderBorderBrush => new SolidColorBrush(_options.CommentListHeaderBorderColor);
        public Brush CommentListSeparatorBrush => new SolidColorBrush(_options.CommentListSeparatorColor);
        public Brush ConnectionListBackground => new SolidColorBrush(_options.CommentListBackColor);
        public Brush ConnectionListHeaderBackground => new SolidColorBrush(_options.CommentListHeaderBackColor);
        public Brush ConnectionListHeaderForeground => new SolidColorBrush(_options.CommentListHeaderForeColor);
        public Brush ConnectionListHeaderBorderBrush => new SolidColorBrush(_options.CommentListHeaderBorderColor);
        public Brush ConnectionListBorderBrush => new SolidColorBrush(_options.CommentListBorderColor);
        public Brush ConnectionListSeparatorBrush => new SolidColorBrush(Colors.Yellow);
        public Brush ConnectionListRowBackground => new SolidColorBrush(_options.ConnectionListRowBackColor);
        public Brush ContextMenuBackground => new SolidColorBrush(_options.MenuBackColor);
        public Brush ContextMenuForeground => new SolidColorBrush(_options.MenuForeColor);
        public Brush ContextMenuBorderBrush => new SolidColorBrush(_options.MenuPopupBorderColor);
        public Brush ScrollBarBorderBrush => new SolidColorBrush(_options.ScrollBarBorderColor);
        public Brush ScrollBarThumbBackground => new SolidColorBrush(_options.ScrollBarThumbBackColor);
        public Brush ScrollBarThumbMouseOverBackground => new SolidColorBrush(_options.ScrollBarThumbMouseOverBackColor);
        public Brush ScrollBarThumbPressedBackground => new SolidColorBrush(_options.ScrollBarThumbPressedBackColor);
        public Brush ScrollBarBackground => new SolidColorBrush(_options.ScrollBarBackColor);
        public Brush ScrollBarButtonBackground => new SolidColorBrush(_options.ScrollBarButtonBackColor);
        public Brush ScrollBarButtonForeground => new SolidColorBrush(_options.ScrollBarButtonForeColor);
        public Brush ScrollBarButtonBorderBrush => new SolidColorBrush(_options.ScrollBarButtonBorderColor);
        public Brush ScrollBarButtonDisabledBackground => new SolidColorBrush(_options.ScrollBarButtonDisabledBackColor);
        public Brush ScrollBarButtonDisabledForeground => new SolidColorBrush(_options.ScrollBarButtonDisabledForeColor);
        public Brush ScrollBarButtonDisabledBorderBrush => new SolidColorBrush(_options.ScrollBarButtonDisabledBorderColor);
        public Brush ScrollBarButtonMouseOverBackground => new SolidColorBrush(_options.ScrollBarButtonMouseOverBackColor);
        public Brush ScrollBarButtonMouseOverForeground => new SolidColorBrush(_options.ScrollBarButtonMouseOverForeColor);
        public Brush ScrollBarButtonMouseOverBorderBrush => new SolidColorBrush(_options.ScrollBarButtonMouseOverBorderColor);
        public Brush ScrollBarButtonPressedBackground => new SolidColorBrush(_options.ScrollBarButtonPressedBackColor);
        public Brush ScrollBarButtonPressedForeground => new SolidColorBrush(_options.ScrollBarButtonPressedForeColor);
        public Brush ScrollBarButtonPressedBorderBrush => new SolidColorBrush(_options.ScrollBarButtonPressedBorderColor);

        private readonly Color _myColor = new Color { A = 0xFF, R = 45, G = 45, B = 48 };
        public double RawMessagePostPanelHeight
        {
            get
            {
#if DEBUG
                return 50;
#else
                return 0;
#endif
            }
        }
        #endregion //Properties

        public MainViewModel() : base(new DynamicOptionsTest())
        {
            if ((bool)(DesignerProperties.IsInDesignModeProperty.GetMetadata(typeof(System.Windows.DependencyObject)).DefaultValue))
            {
                _siteVms = new List<SiteViewModel> { };// new SiteViewModel("DesignSite", Guid.NewGuid()) };
                _browserVms = new List<BrowserViewModel>();// { new BrowserViewModel()}
                AddNewConnection("test", "YouTubeLive", "https://google.com", "Chrome", false, Colors.Blue, Colors.Red);
                SetSystemInfo("test", InfoType.Notice);
            }
            else
            {
                throw new NotSupportedException();
            }
        }
        [GalaSoft.MvvmLight.Ioc.PreferredConstructor]
        public MainViewModel(IIo io, ILogger logger, IOptions options, ISitePluginLoader sitePluginLoader, IBrowserLoader browserLoader)
            : base(options)
        {
            _io = io;
            _logger = logger;
            _sitePluginLoader = sitePluginLoader;
            _browserLoader = browserLoader;

            Comments = CollectionViewSource.GetDefaultView(_comments);
            //ConnectedConnections = CollectionViewSource.GetDefaultView(Connections);
            //ConnectedConnections.Filter = x =>
            //{
            //    return x is ConnectionViewModel connVm && connVm.CanDisconnect;
            //};
            var view = new CollectionViewSource { Source = Connections }.View;
            view.Filter = obj =>
            {
                if (!(obj is ConnectionViewModel connVm))
                {
                    return false;
                }
                return connVm.CanDisconnect;
                //return true;
            };
            ConnectedConnections = view as ICollectionViewLiveShaping;
            if (ConnectedConnections.CanChangeLiveFiltering)
            {
                //ConnectedConnections.LiveFilteringProperties.Add("CanDisconnect");
                ConnectedConnections.LiveFilteringProperties.Add(nameof(ConnectionViewModel.CanDisconnect));
                ConnectedConnections.IsLiveFiltering = true;
            }

            MainViewContentRenderedCommand = new RelayCommand(ContentRendered);
            MainViewClosingCommand = new RelayCommand<CancelEventArgs>(Closing);
            ShowOptionsWindowCommand = new RelayCommand(ShowOptionsWindow);
            ExitCommand = new RelayCommand(Exit);
            ShowWebSiteCommand = new RelayCommand(ShowWebSite);
            ShowDevelopersTwitterCommand = new RelayCommand(ShowDevelopersTwitter);
            CheckUpdateCommand = new RelayCommand(CheckUpdate);
            AddNewConnectionCommand = new RelayCommand(AddNewConnection);
            RemoveSelectedConnectionCommand = new RelayCommand(RemoveSelectedConnection);
            ClearAllCommentsCommand = new RelayCommand(ClearAllComments);
            ShowUserInfoCommand = new RelayCommand(ShowUserInfo);
            ShowUserListCommand = new RelayCommand(ShowUserList);
            ActivatedCommand = new RelayCommand(Activated);
            LoadedCommand = new RelayCommand(Loaded);
            CommentCopyCommand = new RelayCommand(CopyComment);
            OpenUrlCommand = new RelayCommand(OpenUrl);

            _options.PropertyChanged += (s, e) =>
            {
                switch (e.PropertyName)
                {
                    case nameof(_options.MainViewLeft):
                        RaisePropertyChanged(nameof(MainViewLeft));
                        break;
                    case nameof(_options.MainViewTop):
                        RaisePropertyChanged(nameof(MainViewTop));
                        break;
                    case nameof(_options.MainViewHeight):
                        RaisePropertyChanged(nameof(MainViewHeight));
                        break;
                    case nameof(_options.MainViewWidth):
                        RaisePropertyChanged(nameof(MainViewWidth));
                        break;
                    case nameof(_options.IsShowThumbnail):
                        RaisePropertyChanged(nameof(IsShowThumbnail));
                        break;
                    case nameof(_options.IsShowUsername):
                        RaisePropertyChanged(nameof(IsShowUsername));
                        break;
                    case nameof(_options.IsShowConnectionName):
                        RaisePropertyChanged(nameof(IsShowConnectionName));
                        break;
                    case nameof(_options.IsShowCommentId):
                        RaisePropertyChanged(nameof(IsShowCommentId));
                        break;
                    case nameof(_options.IsShowMessage):
                        RaisePropertyChanged(nameof(IsShowMessage));
                        break;
                    case nameof(_options.IsShowPostTime):
                        RaisePropertyChanged(nameof(IsShowPostTime));
                        break;
                    case nameof(_options.IsShowInfo):
                        RaisePropertyChanged(nameof(IsShowInfo));
                        break;

                    case nameof(_options.TitleBackColor):
                        RaisePropertyChanged(nameof(TitleBackground));
                        break;
                    case nameof(_options.TitleForeColor):
                        RaisePropertyChanged(nameof(TitleForeground));
                        break;
                    case nameof(_options.ViewBackColor):
                        RaisePropertyChanged(nameof(ViewBackground));
                        break;
                    case nameof(_options.WindowBorderColor):
                        RaisePropertyChanged(nameof(WindowBorderBrush));
                        break;
                    case nameof(_options.SystemButtonBackColor):
                        RaisePropertyChanged(nameof(SystemButtonBackground));
                        break;
                    case nameof(_options.SystemButtonForeColor):
                        RaisePropertyChanged(nameof(SystemButtonForeground));
                        break;
                    case nameof(_options.SystemButtonBorderColor):
                        RaisePropertyChanged(nameof(SystemButtonBorderBrush));
                        break;
                    case nameof(_options.SystemButtonMouseOverBackColor):
                        RaisePropertyChanged(nameof(SystemButtonMouseOverBackground));
                        break;
                    case nameof(_options.SystemButtonMouseOverForeColor):
                        RaisePropertyChanged(nameof(SystemButtonMouseOverForeground));
                        break;
                    case nameof(_options.SystemButtonMouseOverBorderColor):
                        RaisePropertyChanged(nameof(SystemButtonMouseOverBorderBrush));
                        break;

                    case nameof(_options.MenuBackColor):
                        RaisePropertyChanged(nameof(MenuBackground));
                        RaisePropertyChanged(nameof(ContextMenuBackground));
                        break;
                    case nameof(_options.MenuForeColor):
                        RaisePropertyChanged(nameof(MenuForeground));
                        RaisePropertyChanged(nameof(ContextMenuForeground));
                        break;
                    case nameof(_options.MenuPopupBorderColor):
                        RaisePropertyChanged(nameof(MenuPopupBorderBrush));
                        break;
                    case nameof(_options.MenuSeparatorBackColor):
                        RaisePropertyChanged(nameof(MenuSeparatorBackground));
                        break;
                    case nameof(_options.MenuItemCheckMarkColor):
                        RaisePropertyChanged(nameof(MenuItemCheckMarkBrush));
                        break;
                    case nameof(_options.MenuItemMouseOverBackColor):
                        RaisePropertyChanged(nameof(MenuItemMouseOverBackground));
                        break;
                    case nameof(_options.MenuItemMouseOverForeColor):
                        RaisePropertyChanged(nameof(MenuItemMouseOverForeground));
                        break;
                    case nameof(_options.MenuItemMouseOverBorderColor):
                        RaisePropertyChanged(nameof(MenuItemMouseOverBorderBrush));
                        break;
                    case nameof(_options.MenuItemMouseOverCheckMarkColor):
                        RaisePropertyChanged(nameof(MenuItemMouseOverCheckMarkBrush));
                        break;


                    case nameof(_options.ButtonBackColor):
                        RaisePropertyChanged(nameof(ButtonBackground));
                        break;
                    case nameof(_options.ButtonForeColor):
                        RaisePropertyChanged(nameof(ButtonForeground));
                        break;
                    case nameof(_options.ButtonBorderColor):
                        RaisePropertyChanged(nameof(ButtonBorderBrush));
                        break;
                    case nameof(_options.CommentListBackColor):
                        RaisePropertyChanged(nameof(CommentListBackground));
                        RaisePropertyChanged(nameof(ConnectionListBackground));
                        break;
                    case nameof(_options.CommentListHeaderBackColor):
                        RaisePropertyChanged(nameof(CommentListHeaderBackground));
                        RaisePropertyChanged(nameof(ConnectionListHeaderBackground));
                        break;
                    case nameof(_options.CommentListHeaderForeColor):
                        RaisePropertyChanged(nameof(CommentListHeaderForeground));
                        RaisePropertyChanged(nameof(ConnectionListHeaderForeground));
                        break;
                    case nameof(_options.CommentListHeaderBorderColor):
                        RaisePropertyChanged(nameof(CommentListHeaderBorderBrush));
                        RaisePropertyChanged(nameof(ConnectionListHeaderBorderBrush));
                        break;
                    case nameof(_options.CommentListBorderColor):
                        RaisePropertyChanged(nameof(CommentListBorderBrush));
                        RaisePropertyChanged(nameof(ConnectionListBorderBrush));
                        break;
                    case nameof(_options.CommentListSeparatorColor):
                        RaisePropertyChanged(nameof(CommentListSeparatorBrush));
                        RaisePropertyChanged(nameof(ConnectionListSeparatorBrush));
                        break;
                    //case nameof(_options.ConnectionListBackColor):
                    //    RaisePropertyChanged(nameof(ConnectionListBackground));
                    //    break;
                    //case nameof(_options.ConnectionListHeaderBackColor):
                    //    RaisePropertyChanged(nameof(ConnectionListHeaderBackground));
                    //    break;
                    //case nameof(_options.ConnectionListHeaderForeColor):
                    //    RaisePropertyChanged(nameof(ConnectionListHeaderForeground));
                    //    break;
                    case nameof(_options.ConnectionListRowBackColor):
                        RaisePropertyChanged(nameof(ConnectionListRowBackground));
                        break;

                    case nameof(_options.ScrollBarBackColor):
                        RaisePropertyChanged(nameof(ScrollBarBackground));
                        break;
                    case nameof(_options.ScrollBarBorderColor):
                        RaisePropertyChanged(nameof(ScrollBarBorderBrush));
                        break;
                    case nameof(_options.ScrollBarThumbBackColor):
                        RaisePropertyChanged(nameof(ScrollBarThumbBackground));
                        break;
                    case nameof(_options.ScrollBarThumbMouseOverBackColor):
                        RaisePropertyChanged(nameof(ScrollBarThumbMouseOverBackground));
                        break;
                    case nameof(_options.ScrollBarThumbPressedBackColor):
                        RaisePropertyChanged(nameof(ScrollBarThumbPressedBackground));
                        break;


                    case nameof(_options.ScrollBarButtonBackColor):
                        RaisePropertyChanged(nameof(ScrollBarButtonBackground));
                        break;
                    case nameof(_options.ScrollBarButtonForeColor):
                        RaisePropertyChanged(nameof(ScrollBarButtonForeground));
                        break;
                    case nameof(_options.ScrollBarButtonBorderColor):
                        RaisePropertyChanged(nameof(ScrollBarButtonBorderBrush));
                        break;


                    case nameof(_options.ScrollBarButtonDisabledBackColor):
                        RaisePropertyChanged(nameof(ScrollBarButtonDisabledBackground));
                        break;
                    case nameof(_options.ScrollBarButtonDisabledForeColor):
                        RaisePropertyChanged(nameof(ScrollBarButtonDisabledForeground));
                        break;
                    case nameof(_options.ScrollBarButtonDisabledBorderColor):
                        RaisePropertyChanged(nameof(ScrollBarButtonDisabledBorderBrush));
                        break;

                    case nameof(_options.ScrollBarButtonMouseOverBackColor):
                        RaisePropertyChanged(nameof(ScrollBarButtonMouseOverBackground));
                        break;
                    case nameof(_options.ScrollBarButtonPressedBackColor):
                        RaisePropertyChanged(nameof(ScrollBarButtonPressedBackground));
                        break;
                    case nameof(_options.ScrollBarButtonPressedBorderColor):
                        RaisePropertyChanged(nameof(ScrollBarButtonPressedBorderBrush));
                        break;

                    case nameof(_options.IsEnabledSiteConnectionColor):
                    case nameof(_options.SiteConnectionColorType):
                        RaisePropertyChanged(nameof(ConnectionColorColumnWidth));
                        RaisePropertyChanged(nameof(IsShowConnectionsViewConnectionBackground));
                        RaisePropertyChanged(nameof(IsShowConnectionsViewConnectionForeground));
                        break;
                    case nameof(_options.IsTopmost):
                        _pluginManager.OnTopmostChanged(_options.IsTopmost);
                        RaisePropertyChanged(nameof(Topmost));
                        break;

                    case nameof(_options.IsShowHorizontalGridLine):
                        break;
                    case nameof(_options.HorizontalGridLineColor):
                        RaisePropertyChanged(nameof(HorizontalGridLineBrush));
                        break;
                    case nameof(_options.IsShowVerticalGridLine):
                        break;
                    case nameof(_options.VerticalGridLineColor):
                        RaisePropertyChanged(nameof(VerticalGridLineBrush));
                        break;

                    case nameof(_options.IsShowMetaConnectionName):
                        RaisePropertyChanged(nameof(IsShowMetaConnectionName));
                        break;
                    case nameof(_options.IsShowMetaTitle):
                        RaisePropertyChanged(nameof(IsShowMetaTitle));
                        break;
                    case nameof(_options.IsShowMetaElapse):
                        RaisePropertyChanged(nameof(IsShowMetaElapse));
                        break;
                    case nameof(_options.IsShowMetaCurrentViewers):
                        RaisePropertyChanged(nameof(IsShowMetaCurrentViewers));
                        break;
                    case nameof(_options.IsShowMetaTotalViewers):
                        RaisePropertyChanged(nameof(IsShowMetaTotalViewers));
                        break;
                    case nameof(_options.IsShowMetaActive):
                        RaisePropertyChanged(nameof(IsShowMetaActive));
                        break;
                    case nameof(_options.IsShowMetaOthers):
                        RaisePropertyChanged(nameof(IsShowMetaOthers));
                        break;
                }
            };
            RaisePropertyChanged(nameof(Topmost));
        }

        private async void PluginManager_PluginAdded(object sender, IPlugin e)
        {
            try
            {
                await _dispatcher.BeginInvoke((Action)(() =>
                {
                    var vm = new PluginMenuItemViewModel(e, _options);
                    _pluginMenuItemDict.Add(e, vm);
                    PluginMenuItemCollection.Add(vm);
                }), DispatcherPriority.Normal);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
            }
        }
        private readonly Dictionary<string, UserViewModel> _userViewModelDict = new Dictionary<string, UserViewModel>();
        private readonly ObservableCollection<UserViewModel> _userViewModels = new ObservableCollection<UserViewModel>();
        private async void UserStoreManager_UserAdded(object sender, IUser e)
        {
            //IUserStore.UserAddedに配信サイトかSiteContextを識別するものが必要かも
            await _dispatcher.BeginInvoke((Action)(() =>
            {
                try
                {
                    var uvm = CreateUserViewModel(e);
                    _userViewModelDict.Add(e.UserId, uvm);
                    _userViewModels.Add(uvm);
                }
                catch (Exception ex)
                {
                    _logger.LogException(ex);
                }
            }), DispatcherPriority.Normal);
        }
        private UserViewModel CreateUserViewModel(IUser user)
        {
            var uvm = new UserViewModel(user, _options);
            return uvm;
        }
        public void ShowUserInfo(string userId)
        {
            if (!_userViewModelDict.TryGetValue(userId, out var uvm))
            {
                Debug.WriteLine($"{nameof(_userViewModelDict)}にuserId={userId}が存在しない");
                return;
            }
            var view = new CollectionViewSource { Source = _comments }.View;
            view.Filter = obj =>
            {
                if (!(obj is IMcvCommentViewModel cvm))
                {
                    return false;
                }
                return cvm.UserId == userId;
            };
            uvm.Comments = view;
            MessengerInstance.Send(new ShowUserViewMessage(uvm));
        }
        private void ShowUserInfo()
        {
            var current = SelectedComment;
            try
            {
                Debug.Assert(current != null);
                Debug.Assert(current is IMcvCommentViewModel);

                var userId = current.UserId;
                if (string.IsNullOrEmpty(userId))
                {
                    Debug.WriteLine("UserIdがnull");
                    return;
                }
                ShowUserInfo(userId);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                _logger.LogException(ex);
            }
        }
        private void ShowUserList()
        {
            MessengerInstance.Send(new ShowUserListViewMessage(_userViewModels, this, _options));
        }
        private async void CheckUpdate()
        {
            try
            {
                await CheckIfUpdateExists(false);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
            }
        }
        private void ShowDevelopersTwitter()
        {
            try
            {
                System.Diagnostics.Process.Start("https://twitter.com/kv510k");
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
            }
        }
        private void ShowWebSite()
        {
            try
            {
                System.Diagnostics.Process.Start("https://ryu-s.github.io/app/multicommentviewer");
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
            }
        }
        private void Exit()
        {
            this.RequestClose();
        }
    }
}
