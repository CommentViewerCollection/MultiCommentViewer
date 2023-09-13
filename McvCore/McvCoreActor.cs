using PluginV2 = Mcv.PluginV2;
using System;
using System.IO;
using System.Threading.Tasks;
using Mcv.PluginV2;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Mcv.PluginV2.Messages;
using Mcv.Core.V1;
using System.Diagnostics;
using Akka.Actor;
using System.Threading;
using Mcv.Core.PluginActorMessages;
using Mcv.Core.CoreActorMessages;

namespace Mcv.Core;

class McvCoreActor : ReceiveActor
{
    private readonly ConnectionManager _connManager;
    private readonly IActorRef _pluginManager;
    private readonly V1.IUserStoreManager _userStoreManager;
    private static readonly string OptionsPath = Path.Combine("settings", "options.txt");
    private static readonly string MainViewPluginOptionsPath = Path.Combine("settings", "MainViewPlugin.txt");
    private static readonly ILogger _logger = new LoggerTest();
    private IMcvCoreOptions _coreOptions = default!;
    private void SetMessageToPluginManager(ISetMessageToPluginV2 message)
    {
        _pluginManager.Tell(new SetSetToAllPlugin(message));
    }
    private void SetMessageToPluginManager(PluginId pluginId, ISetMessageToPluginV2 message)
    {
        _pluginManager.Tell(new SetSetToAPlugin(pluginId, message));
    }
    private void SetMessageToPluginManager(INotifyMessageV2 message)
    {
        _pluginManager.Tell(new SetNotifyToAllPlugin(message));
    }
    private void SetMessageToPluginManager(PluginId pluginId, INotifyMessageV2 message)
    {
        _pluginManager.Tell(new SetNotifyToAPlugin(pluginId, message));
    }
    private Task<IReplyMessageToPluginV2> GetMessageToPluginManagerAsync(PluginId pluginId, IGetMessageToPluginV2 message)
    {
        return _pluginManager.Ask<IReplyMessageToPluginV2>(new GetMessage(pluginId, message));
    }
    private Task<List<IPluginInfo>> GetPluginListAsync()
    {
        return _pluginManager.Ask<List<IPluginInfo>>(new GetPluginList(), CancellationToken.None);
    }
    private void RemovePlugin(PluginId pluginId)
    {
        _pluginManager.Tell(new RemovePlugin(pluginId));
    }
    private void SetPluginRole(PluginId pluginId, List<string> pluginRole)
    {
        _pluginManager.Tell(new SetPluginRole(pluginId, pluginRole));
    }
    private void AddPlugins(List<IPlugin> plugins, PluginHost pluginHost)
    {
        _pluginManager.Tell(new AddPlugins(plugins, pluginHost));
    }
    internal void RequestCloseApp()
    {
        SetMessageToPluginManager(new SetClosing());

        _userStoreManager.Save();

        //ここで直接Context.System.Terminate()したいけど、できないから自分にメッセージを送る
        _self.Tell(new SystemShutDown());
    }
    private readonly IActorRef _self;
    public McvCoreActor()
    {
        _self = Self;
        var io = new IOTest();
        _pluginManager = Context.ActorOf(PluginManagerActor.Props());

        _connManager = new ConnectionManager();
        _connManager.ConnectionAdded += ConnManager_ConnectionAdded;
        _connManager.ConnectionRemoved += ConnManager_ConnectionRemoved;
        _connManager.ConnectionStatusChanged += ConnManager_ConnectionStatusChanged;

        _userStoreManager = new V1.UserStoreManager();
        _userStoreManager.UserAdded += UserStoreManager_UserAdded;

        Receive<Initialize>(_ =>
        {
            Initialize();
        });
        Receive<SystemShutDown>(_ =>
        {
            Context.System.Terminate();
        });
    }

    private void ConnManager_ConnectionRemoved(object? sender, ConnectionRemovedEventArgs e)
    {
        SetMessageToPluginManager(new NotifyConnectionRemoved(e.ConnId));
    }

    private void ConnManager_ConnectionStatusChanged(object? sender, ConnectionStatusChangedEventArgs e)
    {
        SetMessageToPluginManager(new NotifyConnectionStatusChanged(e.ConnStDiff));
    }

    internal async Task SetMessageAsync(ISetMessageToCoreV2 m)
    {
        Debug.WriteLine($"McvCoreActor::SetMessageAsync(): {m}");
        switch (m)
        {
            case SetPluginHello pluginHello:
                {
                    var success = true;
                    try
                    {
                        //Roleを登録した時点でPluginManagerの内部ではそのプラグインが登録され、PluginListで取得できるようになる。
                        var pluginList = await GetPluginListAsync();
                        SetPluginRole(pluginHello.PluginId, pluginHello.PluginRole);
                        if (PluginTypeChecker.IsSitePlugin(pluginHello.PluginRole))
                        {
                            var store = new V1.SQLiteUserStore(GetSettingsFilePath("users_" + pluginHello.PluginName + ".db"), _logger);
                            store.Load();
                            _userStoreManager.SetUserStore(pluginHello.PluginId, store);
                        }
                        else if (PluginTypeChecker.IsBrowserPlugin(pluginHello.PluginRole))
                        {

                        }
                        //新たに追加されたプラグインに対して次の情報を通知する
                        //・読み込み済みのプラグイン
                        //・Connection

                        SetMessageToPluginManager(pluginHello.PluginId, new NotifyPluginInfoList(pluginList.Where(p => p.Id != pluginHello.PluginId).ToList()));

                        SetMessageToPluginManager(pluginHello.PluginId, new NotifyConnectionStatusList(_connManager.GetConnectionStatusList()));
                    }
                    catch (Exception)
                    {
                        success = false;
                    }
                    if (!success)
                    {
                        //rollback
                        RemovePlugin(pluginHello.PluginId);
                    }
                    SetMessageToPluginManager(pluginHello.PluginId, new SetLoaded());
                    SetMessageToPluginManager(new NotifyPluginAdded(pluginHello.PluginId, pluginHello.PluginName, pluginHello.PluginRole));
                }
                break;
            case SetMetadata metadata:
                {
                    SetMessageToPluginManager(new NotifyMetadataUpdated(metadata.ConnId, metadata.SiteId, metadata.Metadata));
                }
                break;
            case SetMessage setMessage:
                {
                    var userId = setMessage.UserId;
                    if (userId is not null)
                    {
                        var user = _userStoreManager.GetUser(setMessage.SiteId, userId);
                        if (setMessage.Username is not null)
                        {
                            user.Name = setMessage.Username;
                        }
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
                        SetMessageToPluginManager(new NotifyMessageReceived(setMessage.ConnId, setMessage.SiteId, setMessage.Message, userId, username, nickname, isNgUser, setMessage.IsInitialComment));
                    }
                    else
                    {
                        //InfoMessageとかがUserId==nullになるからこれが必要。
                        //他にも配信サイトのメッセージでもUserIdが無いものもある。
                        SetMessageToPluginManager(new NotifyMessageReceived(setMessage.ConnId, setMessage.SiteId, setMessage.Message, null, null, null, false, setMessage.IsInitialComment));
                    }
                }
                break;
            case RequestChangeConnectionStatus connStDiffMsg:
                ChangeConnectionStatus(connStDiffMsg.ConnStDiff);
                break;
            case RequestAddConnection _:
                await AddConnection();
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
                SetMessageToPluginManager(directMsg.Target, directMsg.Message);
                break;
        }
    }
    internal async Task SetMessageAsync(INotifyMessageV2 message)
    {
        switch (message)
        {
            case NotifySiteConnected connected:
                SetMessageToPluginManager(new NotifyConnectionStatusChanged(new ConnectionStatusDiff(connected.ConnId) { CanConnect = false, CanDisconnect = true }));
                break;
            case NotifySiteDisconnected disconnected:
                SetMessageToPluginManager(new NotifyConnectionStatusChanged(new ConnectionStatusDiff(disconnected.ConnId) { CanConnect = true, CanDisconnect = false }));
                break;
            default:
                break;
        }
        await Task.CompletedTask;
    }
    internal async Task<IReplyMessageToPluginV2> RequestMessageAsync(IGetMessageToCoreV2 message)
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
                    return await GetMessageToPluginManagerAsync(directMsg.Target, directMsg.Message);
                }
            case GetPluginSettingsDirPath pluginSettingsDirPath:
                {
                    return new ReplyPluginSettingsDirPath(GetSettingsFilePath(pluginSettingsDirPath.FilePath));
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
        SetMessageToPluginManager(pluginId, new RequestShowSettingsPanelToPlugin());
    }

    private void ConnManager_ConnectionAdded(object? sender, ConnectionAddedEventArgs e)
    {
        SetMessageToPluginManager(new NotifyConnectionAdded(e.ConnSt));
    }
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    /// <exception cref="UnauthorizedAccessException"></exception>
    private static bool CheckIfCanReadWrite()
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
        return true;
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
        AddPlugins(plugins, pluginHost);

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
    internal async Task AddConnection()
    {
        var defaultSite = await GetDefaultSite();
        if (defaultSite is null)
        {
            Debug.WriteLine("siteが無い");
            return;
        }
        var connId = _connManager.AddConnection(defaultSite);

        _userCommentCountManager.AddConnection(connId);
    }

    private Task<PluginId> GetDefaultSite()
    {
        return _pluginManager.Ask<PluginId>(new GetDefaultSite());
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
            SetMessageToPluginManager(before, new SetDestroyCommentProvider(connId));
            SetMessageToPluginManager(after, new SetCreateCommentProvider(connId));
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

    internal async Task SetMessageAsync(PluginId target, ISetMessageToPluginV2 message)
    {
        SetMessageToPluginManager(target, message);
        await Task.CompletedTask;
    }
    public static Props Props()
    {
        return Akka.Actor.Props.Create(() => new McvCoreActor()).WithDispatcher("akka.actor.synchronized-dispatcher");
    }
}

