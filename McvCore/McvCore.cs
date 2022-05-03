using PluginV2 = Mcv.PluginV2;
using System;
using System.IO;
using System.Threading.Tasks;
using Mcv.PluginV2;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Mcv.PluginV2.Messages;
using McvCore.V1;
using System.Diagnostics;

namespace McvCore;
class McvCore
{
    public event EventHandler? ExitRequested;
    private readonly ConnectionManager _connManager;
    private readonly PluginManager _pluginManager;
    private readonly V1.IUserStoreManager _userStoreManager;
    private static readonly string OptionsPath = Path.Combine("settings", "options.txt");
    private static readonly string MainViewPluginOptionsPath = Path.Combine("settings", "MainViewPlugin.txt");
    private static readonly ILogger _logger = new LoggerTest();
    private IMcvCoreOptions _coreOptions = default!;
    internal void RequestCloseApp()
    {
        _pluginManager.SetMessage(new SetClosing());
        _pluginManager.SetMessage(new PluginV2.Messages.RequestCloseToPlugin());

        _userStoreManager.Save();
        ExitRequested?.Invoke(this, EventArgs.Empty);
    }
    public McvCore()
    {
        var io = new IOTest();
        _pluginManager = new PluginManager();

        _connManager = new ConnectionManager();
        _connManager.ConnectionAdded += ConnManager_ConnectionAdded;
        _connManager.ConnectionRemoved += ConnManager_ConnectionRemoved;
        _connManager.ConnectionStatusChanged += ConnManager_ConnectionStatusChanged;

        _userStoreManager = new V1.UserStoreManager();
        _userStoreManager.UserAdded += UserStoreManager_UserAdded;
    }

    private void ConnManager_ConnectionRemoved(object? sender, ConnectionRemovedEventArgs e)
    {
        _pluginManager.SetMessage(new PluginV2.Messages.NotifyConnectionRemoved(e.ConnId));
    }

    private void ConnManager_ConnectionStatusChanged(object? sender, ConnectionStatusChangedEventArgs e)
    {
        _pluginManager.SetMessage(new PluginV2.Messages.NotifyConnectionStatusChanged(e.ConnStDiff));
    }

    internal void SetMessage(ISetMessageToCoreV2 m)
    {
        switch (m)
        {
            case SetPluginHello pluginHello:
                {
                    var success = true;
                    try
                    {
                        _pluginManager.SetPluginRole(pluginHello.PluginId, pluginHello.PluginRole);
                        if (PluginTypeChecker.IsSitePlugin(pluginHello.PluginRole))
                        {
                            _userStoreManager.SetUserStore(pluginHello.PluginId, new V1.SQLiteUserStore(GetSettingsFilePath("users_" + pluginHello.PluginName + ".db"), _logger));
                        }
                        else if (PluginTypeChecker.IsBrowserPlugin(pluginHello.PluginRole))
                        {

                        }
                        //新たに追加されたプラグインに対して次の情報を通知する
                        //・読み込み済みのプラグイン
                        //・Connection
                        _pluginManager.SetMessage(pluginHello.PluginId, new NotifyPluginInfoList(_pluginManager.GetPluginList().Where(p => p.Id != pluginHello.PluginId).ToList()));

                        _pluginManager.SetMessage(pluginHello.PluginId, new NotifyConnectionStatusList(_connManager.GetConnectionStatusList()));
                    }
                    catch (Exception)
                    {
                        success = false;
                    }
                    if (!success)
                    {
                        //rollback
                        _pluginManager.RemovePlugin(pluginHello.PluginId);
                    }
                    _pluginManager.SetMessage(pluginHello.PluginId, new SetLoaded());
                    _pluginManager.SetMessage(new NotifyPluginAdded(pluginHello.PluginId, pluginHello.PluginName, pluginHello.PluginRole));
                }
                break;
            case SetMetadata metadata:
                {
                    _pluginManager.SetMessage(new NotifyMetadataUpdated(metadata.ConnId, metadata.SiteId, metadata.Metadata));
                }
                break;
            case SetMessage setMessage:
                {
                    var userId = setMessage.UserId;
                    if (userId is not null)
                    {
                        var user = _userStoreManager.GetUser(setMessage.SiteId, userId);
                        if (setMessage.NewNickname is not null)
                        {
                            user.Nickname = setMessage.NewNickname;
                        }

                        //初コメかどうか。McvCoreでやること？
                        _userCommentCountManager.AddCommentCount(setMessage.ConnId, userId);
                        var isFirstComment = _userCommentCountManager.IsFirstComment(setMessage.ConnId, userId);

                        //TODO:NameとNicknameもプラグインに渡したい
                        IEnumerable<IMessagePart>? username = user.Name;// Common.MessagePartFactory.CreateMessageItems("");
                        var nickname = user.Nickname;
                        var isNgUser = user.IsNgUser;
                        _pluginManager.SetMessage(new NotifyMessageReceived(setMessage.ConnId, setMessage.SiteId, setMessage.Message, userId, username, nickname, isNgUser));
                    }
                    else
                    {
                        //InfoMessageとかがUserId==nullになるからこれが必要。
                        //他にも配信サイトのメッセージでもUserIdが無いものもある。
                        _pluginManager.SetMessage(new NotifyMessageReceived(setMessage.ConnId, setMessage.SiteId, setMessage.Message, null, null, null, false));
                    }
                }
                break;
            case RequestChangeConnectionStatus connStDiffMsg:
                ChangeConnectionStatus(connStDiffMsg.ConnStDiff);
                break;
            case RequestAddConnection _:
                AddConnection();
                break;
            case RequestShowSettingsPanel reqShowSettingsPanel:
                ShowPluginSettingsPanel(reqShowSettingsPanel.PluginId);
                break;
            case RequestRemoveConnection removeConn:
                RemoveConnection(removeConn.ConnId);
                break;
            case RequestSavePluginOptions savePluginOptions:
                SavePluginOptions(savePluginOptions.Filename, savePluginOptions.PluginOptionsRaw);
                break;
            case SetCloseApp _:
                RequestCloseApp();
                break;
            case SetDirectMessage directMsg:
                _pluginManager.SetMessage(directMsg.Target, directMsg.Message);
                break;
        }
    }
    internal void SetMessage(INotifyMessageV2 message)
    {
        switch (message)
        {
            case NotifySiteConnected connected:
                _pluginManager.SetMessage(new NotifyConnectionStatusChanged(new ConnectionStatusDiff(connected.ConnId) { CanConnect = false, CanDisconnect = true }));
                break;
            case NotifySiteDisconnected disconnected:
                _pluginManager.SetMessage(new PluginV2.Messages.NotifyConnectionStatusChanged(new ConnectionStatusDiff(disconnected.ConnId) { CanConnect = true, CanDisconnect = false }));
                break;
        }
    }
    internal IReplyMessageToPluginV2 RequestMessage(IGetMessageToCoreV2 message)
    {
        switch (message)
        {
            case RequestLoadPluginOptions reqPluginOptions:
                {
                    var rawOptions = LoadPluginOptionsRaw(reqPluginOptions.PluginName);
                    return new ReplyPluginOptions(rawOptions);
                }
            case GetLegacyOptions _:
                {
                    var rawOptions = LoadLegacyOptionsRaw() ?? "";
                    return new ReplyLegacyOptions(rawOptions);
                }
            case GetConnectionStatus reqConnSt:
                {
                    var connSt = GetConnectionStatus(reqConnSt.ConnId);
                    return new ReplyConnectionStatus(connSt);
                }
            case GetAppName _:
                {
                    var appName = GetAppName();
                    return new ReplyAppName(appName);
                }
            case GetAppVersion _:
                {
                    var appVersion = GetAppVersion();
                    return new ReplyAppVersion(appVersion);
                }
            case GetAppSolutionConfiguration _:
                {
                    var appSolutionConfiguration = GetAppSolutionConfiguration();
                    return new ReplyAppSolutionConfiguration(appSolutionConfiguration);
                }
            case GetUserAgent _:
                {
                    var userAgent = GetUserAgent();
                    return new ReplyUserAgent(userAgent);
                }
            case GetDirectMessage directMsg:
                {
                    return _pluginManager.RequestMessage(directMsg.Target, directMsg.Message);
                }
        }
        throw new Exception("bug");
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
    internal void ShowPluginSettingsPanel(PluginId pluginId)
    {
        _pluginManager.SetMessage(pluginId, new PluginV2.Messages.RequestShowSettingsPanelToPlugin());
    }

    private void ConnManager_ConnectionAdded(object? sender, ConnectionAddedEventArgs e)
    {
        _pluginManager.SetMessage(new NotifyConnectionAdded(e.ConnSt));
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
    private string GetSettingsFilePath(string filename)
    {
        return Path.Combine(_coreOptions.SettingsDirPath, filename);
    }
    internal bool Initialize()
    {
        try
        {
            var testResult = CheckIfCanReadWrite();
        }
        catch (Exception)
        {
            MessageBox.Show("ファイルの読み書き権限無し", "マルチコメビュ起動エラー");
            return false;
        }
        if (File.Exists(OptionsPath) && !File.Exists(MainViewPluginOptionsPath))
        {
            File.Copy(OptionsPath, MainViewPluginOptionsPath);
        }
        //旧バージョンから移行する処理
        if (File.Exists(Path.Combine(OptionsPath, "users_YouTubeLive.db")))
        {
            File.Move(Path.Combine(OptionsPath, "users_YouTubeLive.db"), Path.Combine(OptionsPath, "users_YouTubeLiveSitePlugin.db"));
        }
        if (File.Exists(Path.Combine(OptionsPath, "users_Twitch.db")))
        {
            File.Move(Path.Combine(OptionsPath, "users_Twitch.db"), Path.Combine(OptionsPath, "users_TwitchSitePlugin.db"));
        }


        _coreOptions = LoadOptions(OptionsPath, _logger);

        _coreOptions.PluginDir = "plugins";

        var pluginHost = new PluginHost(this);

        var plugins = PluginLoader.LoadPlugins(_coreOptions.PluginDir);
        _pluginManager.AddPlugins(plugins, pluginHost);

        //var options = LoadOptions(GetOptionsPath(), logger);
        //_sitePluginOptions = LoadSitePluginOptions(GetSitePluginOptionsPath(), _logger);
        //_pluginManager.LoadSitePlugins(_sitePluginOptions, _logger, GetUserAgent());
        //foreach (var site in _pluginManager.GetSitePlugins())
        //{
        //    //TODO:DisplayNameでファイル名を付けておきながらSiteTypeで識別している。
        //    var userStore = new V1.SQLiteUserStore(coreOptions.SettingsDirPath + "\\" + "users_" + site.Name + ".db", _logger);
        //    _userStoreManager.SetUserStore(site.SiteId, userStore);
        //}


        //_browserManager.LoadBrowserProfiles();

        return true;
    }
    private void UserStoreManager_UserAdded(object? sender, McvUser e)
    {
        throw new NotImplementedException();
    }

    private static IMcvCoreOptions LoadOptions(string optionsPath, ILogger logger)
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

    internal async Task RunAsync()
    {
        while (true)
        {
            await Task.Delay(200);
        }
    }

    internal void AddConnection()
    {
        var defaultSite = _pluginManager.GetDefaultSite();
        if (defaultSite is null)
        {
            Debug.WriteLine("siteが無い");
            return;
        }
        var connId = _connManager.AddConnection(defaultSite);

        _userCommentCountManager.AddConnection(connId);
    }
    private readonly UserCommentCountManager _userCommentCountManager = new();

    internal void RemoveConnection(ConnectionId connId)
    {
        _connManager.RemoveConnection(connId);
    }
    internal static string GetAppVersion()
    {
        var asm = System.Reflection.Assembly.GetExecutingAssembly();
        var ver = asm.GetName().Version!;
        var s = $"{ver.Major}.{ver.Minor}.{ver.Build}";
        return s;
    }
    internal static string GetAppName()
    {
        var asm = System.Reflection.Assembly.GetExecutingAssembly();
        var title = asm.GetName().Name!;
        return title;
    }
    internal static string GetAppSolutionConfiguration()
    {
        string s;
#if BETA
            s = "ベータ版";
#elif ALPHA
            s = "アルファ版";
#elif DEBUG
        s = "DEBUG";
#endif
        return s;
    }
    private static string GetUserAgent()
    {
        return $"{GetAppName()}/{GetAppVersion()} contact-> twitter.com/kv510k";
    }
    private string GetSitePluginOptionsPath()
    {
        return Path.Combine(AppContext.BaseDirectory, "settings", "options.txt");
    }

    internal void ChangeConnectionStatus(IConnectionStatusDiff connStDiff)
    {
        var connId = connStDiff.Id;
        if (connStDiff.SelectedSite is not null)
        {
            var before = _connManager.GetConnectionStatus(connId).SelectedSite;
            var after = connStDiff.SelectedSite;
            _pluginManager.SetMessage(before, new SetDestroyCommentProvider(connId));
            _pluginManager.SetMessage(after, new SetCreateCommentProvider(connId));
        }
        _connManager.ChangeConnectionStatus(connStDiff);
    }
    internal static string? LoadLegacyOptionsRaw()
    {
        var optionsPath = Path.Combine("settings", "options.txt");
        var io = new V1.IOTest();
        var s = io.ReadFile(optionsPath);
        return s;
    }
    internal static string? LoadPluginOptionsRaw(string pluginName)
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

    internal void SetMessage(PluginId target, ISetMessageToPluginV2 message)
    {
        _pluginManager.SetMessage(target, message);
    }
}
