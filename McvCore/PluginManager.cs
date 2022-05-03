using PluginV0 = Plugin;
using PluginV2 = Mcv.PluginV2;
using SitePlugin;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Mcv.PluginV2.Messages.ToPlugin;
using Mcv.PluginV2.Messages;
using Mcv.PluginV2;
using System.Windows.Media;
using System.Windows;
using System.ComponentModel;

namespace McvCore
{
    //class PluginInfo : PluginV2.IPluginInfo
    //{
    //    public PluginInfo(PluginV0.IPlugin plugin)
    //    {
    //        Id =
    //    }
    //    public PluginInfo(PluginV2.IPlugin plugin)
    //    {
    //        Id = plugin.Id;
    //        Name = plugin.Name;
    //    }

    //    public PluginV2.PluginId Id { get; }
    //    public string Name { get; }
    //}
    interface IPluginManager
    {
        event EventHandler<PluginV2.IPluginInfo> PluginAdded;
        void LoadPlugins(PluginV2.IPluginHost host, string pluginsDir);
        //void SetMessage(ISiteMessage message, IMessageMetadata messageMetadata);
        void OnLoaded();
        //void OnClosing();
        //void OnTopmostChanged(bool isTopmost);
        //event EventHandler<string> PostingCommentReceived;
    }
    class PluginV0HostAdapter : PluginV0.IPluginHost
    {
        private readonly PluginV2.IPluginHost _host;

        public string SettingsDirPath { get => ""; }
        public double MainViewLeft { get => 0; }
        public double MainViewTop { get => 0; }
        public bool IsTopmost { get => false; }

        public IEnumerable<PluginV0.IConnectionStatus> GetAllConnectionStatus()
        {
            throw new NotImplementedException();
        }

        public IUser GetUser(Guid sitePluginGuid, string userId)
        {
            throw new NotImplementedException();
        }

        public string LoadOptions(string path)
        {
            return "";
        }

        public void PostComment(string guid, string comment)
        {
            throw new NotImplementedException();
        }

        public void PostCommentToAll(string comment)
        {
            throw new NotImplementedException();
        }

        public void SaveOptions(string path, string s)
        {
            var pluginName = Path.GetFileNameWithoutExtension(path);
            _host.SetMessage(new Mcv.PluginV2.Messages.RequestSavePluginOptions(pluginName, s));
        }
        public PluginV0HostAdapter(PluginV2.IPluginHost host)
        {
            _host = host;
        }
    }
    class PluginV2Adapter : PluginV2.IPlugin
    {
        public PluginV2.PluginId Id { get; }
        public string Name => _plugin.Name;
        private readonly PluginV0.IPlugin _plugin;
        private PluginV2.IPluginHost _host;

        public PluginV2Adapter(PluginV0.IPlugin plugin, PluginV2.PluginId id)
        {
            _plugin = plugin;
            Id = id;
        }

        public PluginV2.IPluginHost Host
        {
            get => _host;
            set
            {
                if (_host != null) return;
                _plugin.Host = new PluginV0HostAdapter(value);
                _host = value;
            }
        }

        public void OnClosing()
        {
            _plugin.OnClosing();
        }

        public void OnLoaded()
        {
            _plugin.OnLoaded();
        }

        public void OnLoading() { }

        class Abc : IMessageMetadata
        {
            public Color BackColor { get; }
            public Color ForeColor { get; }
            public FontFamily FontFamily { get; }
            public int FontSize { get; }
            public FontWeight FontWeight { get; }
            public FontStyle FontStyle { get; }
            public bool IsNgUser { get; } = false;
            public bool IsSiteNgUser { get; }
            public bool IsFirstComment { get; }
            public bool IsInitialComment { get; } = false;
            public bool Is184 { get; } = true;
            public IUser User { get; }
            public bool IsVisible { get; }
            public bool IsNameWrapping { get; }
            public Guid SiteContextGuid { get; }
            public ISiteOptions SiteOptions { get; }

            public event PropertyChangedEventHandler PropertyChanged;
            public Abc(IUser user)
            {
                User = user;
            }
        }
        class TempUser : IUser
        {
            public string UserId { get; }
            public IEnumerable<IMessagePart> Name { get; set; }
            public string Nickname { get; set; }
            public string ForeColorArgb { get; set; } = "";
            public string BackColorArgb { get; set; } = "";
            public bool IsNgUser { get; set; }

            public event PropertyChangedEventHandler? PropertyChanged;
            public TempUser(string userId, IEnumerable<IMessagePart> name, string nickname, bool isNgUser)
            {
                UserId = userId;
                Name = name;
                Nickname = nickname;
                IsNgUser = isNgUser;
            }
        }

        public void SetMessage(INotifyMessageV2 message)
        {
            if (message is Mcv.PluginV2.Messages.NotifyMessageReceived messageReceived)
            {
                _plugin.OnMessageReceived(messageReceived.Message, new Abc(new TempUser(messageReceived.UserId, messageReceived.Username, messageReceived.Nickname, messageReceived.IsNgUser)));
            }
        }
        public void SetMessage(ISetMessageToPluginV2 message)
        {
            switch (message)
            {
                case RequestShowSettingsPanelToPlugin showSettingsPanel:
                    _plugin.ShowSettingView();
                    break;
            }
        }
    }
    class PluginManager : IPluginManager
    {
        private static bool IsValidPluginFileName(string filePath)
        {
            return filePath.EndsWith("Plugin.dll") && filePath != "Plugin.dll" && filePath != "SitePlugin.dll";
        }
        public event EventHandler<PluginV2.IPluginInfo>? PluginAdded;
        //public event EventHandler<PluginV0.IPlugin> PluginRemoved;
        private static List<PluginV2.IPlugin> LoadPluginsInternal(string pluginsDir)
        {
            var pluginDirs = Directory.GetDirectories(pluginsDir);
            var plugins = new List<PluginV2.IPlugin>();
            foreach (var pluginDir in pluginDirs)
            {
                var files = Directory.GetFiles(pluginDir).Where(s => IsValidPluginFileName(s));
                foreach (var filePath in files)
                {
                    try
                    {
                        var plugin = LoadPluginsV2(filePath);
                        if (plugin == null) continue;
                        plugins.Add(plugin);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                    }
                }
            }
            return plugins;
        }
        public void LoadPlugins(PluginV2.IPluginHost host, string pluginsDir)
        {
            _pluginsV2 = LoadPluginsInternal(pluginsDir);
            foreach (var plugin in _pluginsV2)
            {
                plugin.Host = host;
                plugin.OnLoading();
                PluginAdded?.Invoke(this, plugin);
            }
        }

        internal List<PluginV2.IPluginInfo> GetPluginList()
        {
            return _pluginsV2.Cast<PluginV2.IPluginInfo>().ToList();
        }

        private List<PluginV2.IPlugin>? _pluginsV2;
        public static PluginV2.IPlugin? LoadPluginsV2(string file)
        {
            var catalog = new AssemblyCatalog(file);
            var con = new CompositionContainer(catalog);
            var t = new ImportDefinition(ed => true, "", ImportCardinality.ZeroOrMore, false, true);
            var dd = con.GetExports(t);
            foreach (var a in dd.ToList())//リストにしているけど、0個か1個しかない。
            {
                if (a.Value is PluginV0.IPlugin v0)
                {
                    return new PluginV2Adapter(v0, PluginV2.PluginId.New());
                }
                else if (a.Value is PluginV2.IPlugin v2)
                {
                    return v2;
                }
            }
            return null;
        }
        public void OnLoaded()
        {
            if (_pluginsV2 == null)
            {
                throw new InvalidOperationException("最初にLoadPlugins()を実行すること");
            }
            foreach (var plugin in _pluginsV2)
            {
                plugin.OnLoaded();
            }
        }
        internal void OnClosing()
        {
            if (_pluginsV2 == null)
            {
                throw new InvalidOperationException("最初にLoadPlugins()を実行すること");
            }
            foreach (var plugin in _pluginsV2)
            {
                plugin.OnClosing();
            }
        }
        internal void SetMessage(INotifyMessageV2 message)
        {
            if (_pluginsV2 == null)
            {
                throw new InvalidOperationException("最初にLoadPlugins()を実行すること");
            }
            foreach (var plugin in _pluginsV2)
            {
                plugin.SetMessage(message);
            }
        }
        internal void SetMessage(PluginId targetId, INotifyMessageV2 message)
        {
            if (_pluginsV2 == null)
            {
                throw new InvalidOperationException("最初にLoadPlugins()を実行すること");
            }
            var target = GetPlugin(targetId);
            if (target == null)
            {
                return;
            }
            target.SetMessage(message);
        }
        internal void SetMessage(PluginId targetId, ISetMessageToPluginV2 message)
        {
            if (_pluginsV2 == null)
            {
                throw new InvalidOperationException("最初にLoadPlugins()を実行すること");
            }
            var target = GetPlugin(targetId);
            if (target == null)
            {
                return;
            }
            target.SetMessage(message);
        }
        internal void SetMessage(ISetMessageToPluginV2 message)
        {
            if (_pluginsV2 == null)
            {
                throw new InvalidOperationException("最初にLoadPlugins()を実行すること");
            }
            foreach (var plugin in _pluginsV2)
            {
                plugin.SetMessage(message);
            }
        }

        private IPlugin? GetPlugin(PluginId targetId)
        {
            return _pluginsV2?.Find(p => p.Id == targetId);
        }



        //public void OnClosing()
        //{
        //    if (_pluginsV2 == null)
        //        return;
        //    foreach (var plugin in _pluginsV2)
        //    {
        //        try
        //        {
        //            plugin.OnClosing();
        //        }
        //        catch (Exception ex)
        //        {
        //            Debug.WriteLine(ex.Message);
        //        }
        //    }
        //}

        //public void ForeachPlugin(Action<PluginV0.IPlugin> p)
        //{
        //    foreach (var plugin in _pluginsV2)
        //    {
        //        try
        //        {
        //            p(plugin);
        //        }
        //        catch (Exception ex)
        //        {
        //            Debug.WriteLine(ex.Message);
        //        }
        //    }
        //}

        //public void SetMessage(ISiteMessage message, IMessageMetadata messageMetadata)
        //{
        //    foreach (var plugin in _pluginsV2)
        //    {
        //        plugin.OnMessageReceived(message, messageMetadata);
        //    }
        //}

        //public void OnTopmostChanged(bool isTopmost)
        //{
        //    foreach (var plugin in _pluginsV2)
        //    {
        //        plugin.OnTopmostChanged(isTopmost);
        //    }
        //}

        //private readonly IMcvCoreOptions _options;
        public PluginManager()
        {
            //_options = options;
        }
    }
    interface IMcvCoreOptions
    {
        string PluginDir { get; set; }
        string SettingsDirPath { get; set; }
    }
}
