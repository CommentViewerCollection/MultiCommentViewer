using PluginV2 = Mcv.PluginV2;
using System;
using System.IO;
using System.Threading.Tasks;
using Mcv.PluginV2.Messages.ToCore;
using Mcv.PluginV2;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace McvCore
{
    class McvCore
    {
        public event EventHandler? ExitRequested;
        private readonly ConnectionManager _connManager;
        private readonly PluginManager _pluginManager;
        private readonly SitePluginManager _sitePluginManager;
        private readonly V1.IUserStoreManager _userStoreManager;
        internal void RequestCloseApp()
        {
            _pluginManager.OnClosing();
            _pluginManager.SetMessage(new PluginV2.Messages.RequestCloseToPlugin());


            ExitRequested?.Invoke(this, EventArgs.Empty);
        }

        private readonly BrowserManager _browserManager;
        public McvCore()
        {
            var io = new Common.IOTest();
            _sitePluginManager = new SitePluginManager(new SitePluginHost(this), io);
            _sitePluginManager.SitePluginAdded += SitePluginManager_SitePluginAdded;

            _browserManager = new BrowserManager();
            _browserManager.BrowserAdded += BrowserManager_BrowserAdded;

            _connManager = new ConnectionManager(_sitePluginManager, _browserManager);
            _connManager.ConnectionAdded += ConnManager_ConnectionAdded;
            _connManager.ConnectionRemoved += ConnManager_ConnectionRemoved;
            _connManager.ConnectionStatusChanged += ConnManager_ConnectionStatusChanged;

            _pluginManager = new PluginManager();
            _pluginManager.PluginAdded += PluginManager_PluginAdded;

            _userStoreManager = new V1.UserStoreManager();
            _userStoreManager.UserAdded += UserStoreManager_UserAdded;
        }

        private void ConnManager_ConnectionRemoved(object sender, ConnectionRemovedEventArgs e)
        {
            _pluginManager.SetMessage(new PluginV2.Messages.NotifyConnectionRemoved(e.ConnId));
        }

        private void ConnManager_ConnectionStatusChanged(object sender, ConnectionStatusChangedEventArgs e)
        {
            _pluginManager.SetMessage(new PluginV2.Messages.NotifyConnectionStatusChanged(e.ConnStDiff));
        }

        internal void SetMessage(NotifyMessageReceived m)
        {
            var userId = m.UserId;
            var user = _userStoreManager.GetUser(m.SiteId, userId);
            //TODO:NameとNicknameもプラグインに渡したい
            IEnumerable<SitePlugin.IMessagePart> username = user.Name;// Common.MessagePartFactory.CreateMessageItems("");
            var nickname = user.Nickname;
            var isNgUser = user.IsNgUser;
            _pluginManager.SetMessage(new PluginV2.Messages.NotifyMessageReceived(m.ConnId, m.SiteId, m.Message, m.UserId, username, nickname, isNgUser));
        }

        internal void SetMessage(NotifyMetadataUpdated m)
        {
            _pluginManager.SetMessage(new PluginV2.Messages.NotifyMetadataUpdated(m.ConnId, m.SiteId, m.Metadata));
        }



        private void BrowserManager_BrowserAdded(object sender, BrowserAddedEventArgs e)
        {
            _pluginManager.SetMessage(new PluginV2.Messages.NotifyBrowserAdded(e.BrowserProfileId, e.BrowserDisplayName, e.ProfileDisplayName));
        }

        internal void SavePluginOptions(string pluginName, string pluginOptionsRaw)
        {
            try
            {
                var io = new V1.IOTest();
                io.WriteFile(Path.Combine("settings", pluginName + ".txt"), pluginOptionsRaw);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
            }
        }

        private void SitePluginManager_SitePluginAdded(object sender, SitePluginAddedEventArgs e)
        {
            _pluginManager.SetMessage(new PluginV2.Messages.NotifySiteAdded(e.SitePluginId, e.SitePluginDisplayName));
        }

        internal void ShowPluginSettingsPanel(PluginId pluginId)
        {
            _pluginManager.SetMessage(pluginId, new PluginV2.Messages.RequestShowSettingsPanelToPlugin());
        }

        private void ConnManager_ConnectionAdded(object sender, ConnectionAddedEventArgs e)
        {
            _pluginManager.SetMessage(new Mcv.PluginV2.Messages.NotifyConnectionAdded(e.ConnSt));
        }
        enum ReadWriteTestResult
        {
            Ok,
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="UnauthorizedAccessException"></exception>
        private static ReadWriteTestResult CheckIfCanReadWrite()
        {
            var filename = "test.txt";
            using (var sw = new StreamWriter(filename))
            {
                sw.Write("ok");
            }
            using (var sr = new StreamReader(filename))
            {
                var _ = sr.ReadToEnd();
            }
            File.Delete(filename);
            return ReadWriteTestResult.Ok;
        }
        /// <summary>
        /// 本プログラムの実行時に必要になる主要なファイルが存在するか確認する
        /// </summary>
        /// <returns></returns>
        private static bool CheckIfLibraryExists()
        {
#if DEBUG
            string dir = "";
#else
            string dir = "dll";
#endif
            string[] files = { "plugin.dll" };
            foreach (var filename in files)
            {
                var path = Path.Combine(dir, filename);
                if (!File.Exists(path))
                {
                    return false;
                }
            }
            return true;
        }
        private static readonly string OptionsPath = Path.Combine("settings", "options.txt");
        private static readonly string MainViewPluginOptionsPath = Path.Combine("settings", "MainViewPlugin.txt");
        private static readonly Common.ILogger _logger = new Common.LoggerTest();
        internal bool Initialize()
        {
            try
            {
                var testResult = CheckIfCanReadWrite();
            }
            catch (Exception ex)
            {
                MessageBox.Show("ファイルの読み書き権限無し", "マルチコメビュ起動エラー");
                return false;
            }
            if (!CheckIfLibraryExists())
            {
                MessageBox.Show("必要なライブラリが無い", "マルチコメビュ起動エラー");
                return false;
            }
            if (File.Exists(OptionsPath) && !File.Exists(MainViewPluginOptionsPath))
            {
                File.Copy(OptionsPath, MainViewPluginOptionsPath);
            }


            var coreOptions = LoadOptions(OptionsPath, _logger);

            coreOptions.PluginDir = "plugins";

            var pluginHost = new PluginHost(this);

            _pluginManager.LoadPlugins(pluginHost, coreOptions.PluginDir);

            //var options = LoadOptions(GetOptionsPath(), logger);
            var sitePluginOptions = LoadSitePluginOptions(GetSitePluginOptionsPath(), _logger);
            _sitePluginManager.LoadSitePlugins(sitePluginOptions, _logger, GetUserAgent());
            foreach (var site in _sitePluginManager.GetSitePlugins())
            {
                //TODO:DisplayNameでファイル名を付けておきながらSiteTypeで識別している。
                var userStore = new V1.SQLiteUserStore(coreOptions.SettingsDirPath + "\\" + "users_" + site.Name + ".db", _logger);
                _userStoreManager.SetUserStore(site.SiteId, userStore);
            }


            _browserManager.LoadBrowserProfiles();

            _pluginManager.OnLoaded();


            return true;
        }

        private void UserStoreManager_UserAdded(object sender, SitePlugin.IUser e)
        {
            throw new NotImplementedException();
        }

        private static IMcvCoreOptions LoadOptions(string optionsPath, Common.ILogger logger)
        {
            var options = new McvCoreOptions();
            try
            {
                var io = new V1.IOTest();
                var s = io.ReadFile(optionsPath);
                options.Deserialize(s);
            }
            catch (Exception ex)
            {
                logger.LogException(ex);
            }
            return options;
        }
        private static SitePlugin.ICommentOptions LoadSitePluginOptions(string optionsPath, Common.ILogger logger)
        {
            var options = new V1.SitePluginOptions();
            try
            {
                var io = new V1.IOTest();
                var s = io.ReadFile(optionsPath);
                options.Deserialize(s);
            }
            catch (Exception ex)
            {
                logger.LogException(ex);
            }
            return options;
        }

        private void PluginManager_PluginAdded(object sender, IPluginInfo e)
        {
            //新たに追加されたプラグインに対して次の情報を通知する
            //・読み込み済みのプラグイン
            //・Connection
            _pluginManager.SetMessage(e.Id, new PluginV2.Messages.NotifyPluginInfoList(_pluginManager.GetPluginList().Where(p => p.Id != e.Id).ToList()));

            _pluginManager.SetMessage(e.Id, new PluginV2.Messages.NotifyConnectionStatusList(_connManager.GetConnectionStatusList()));

            //読み込み済みのプラグインに対して追加されたプラグインの情報を通知する


        }

        internal async Task RunAsync()
        {
            while (true)
            {
                await Task.Delay(200);
            }
        }

        internal void AddConnection()
        {
            var defaultSite = _sitePluginManager.GetDefaultSite();
            BrowserProfileId defaultBrowser = _browserManager.GetDefaultBrowser();//TODO:
            _connManager.AddConnection(defaultSite, defaultBrowser);
        }
        internal void RemoveConnection(ConnectionId connId)
        {
            _connManager.RemoveConnection(connId);
        }
        private string GetVersionNumber()
        {
            var asm = System.Reflection.Assembly.GetExecutingAssembly();
            var ver = asm.GetName().Version;
            var s = $"{ver.Major}.{ver.Minor}.{ver.Build}";
            return s;
        }
        internal string GetAppName()
        {
            var asm = System.Reflection.Assembly.GetExecutingAssembly();
            var title = asm.GetName().Name;
            return title;
        }
        private string GetUserAgent()
        {
            return $"{GetAppName()}/{GetVersionNumber()} contact-> twitter.com/kv510k";
        }
        private string GetSitePluginOptionsPath()
        {
            var currentDir = Directory.GetParent(System.Reflection.Assembly.GetExecutingAssembly().Location).FullName;
            return Path.Combine(currentDir, "settings", "options.txt");
        }

        internal void ChangeConnectionStatus(IConnectionStatusDiff connStDiff)
        {
            _connManager.ChangeConnectionStatus(connStDiff);
        }

        internal List<(SiteId, SitePlugin.IOptionsTabPage)> GetSettingsPanels()
        {
            var panels = _sitePluginManager.GetSettingsPanels();
            return panels;
        }

        internal string LoadLegacyOptionsRaw()
        {
            var optionsPath = Path.Combine("settings", "options.txt");
            var io = new V1.IOTest();
            var s = io.ReadFile(optionsPath);
            return s;
        }
        internal string LoadPluginOptionsRaw(string pluginName)
        {
            var optionsPath = Path.Combine("settings", pluginName + ".txt");
            var io = new V1.IOTest();
            var s = io.ReadFile(optionsPath);
            return s;
        }

        internal IConnectionStatus GetConnectionStatus(ConnectionId connId)
        {
            return _connManager.GetConnectionStatus(connId);
        }
    }

}
