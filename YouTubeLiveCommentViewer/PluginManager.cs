using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Plugin;
using SitePlugin;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Diagnostics;
using System.Linq;
using System.IO;

namespace YouTubeLiveCommentViewer
{
    public class PluginManager : IPluginManager
    {
        public event EventHandler<IPlugin> PluginAdded;
        //public event EventHandler<IPlugin> PluginRemoved;

        private IEnumerable<IPlugin> _plugins;
        public void LoadPlugins(IPluginHost host)
        {
            var dir = _options.PluginDir;
            var pluginDirs = Directory.GetDirectories(dir);
            var list = new List<DirectoryCatalog>();
            var def = new ImportDefinition(d => d.ContractName == typeof(IPlugin).FullName, "", ImportCardinality.ExactlyOne, false, false);
            var plugins = new List<IPlugin>();
            foreach (var pluginDir in pluginDirs)
            {
                var files = Directory.GetFiles(pluginDir).Where(s => s.EndsWith("Plugin.dll"));//ファイル名がPlugin.dllで終わるアセンブリだけ探す
                foreach(var file in files)
                {
                    try
                    {
                        var catalog = new AssemblyCatalog(file);
                        var con = new CompositionContainer(catalog);
                        var plugin = con.GetExport<IPlugin>().Value;
                        plugins.Add(plugin);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                    }
                }
                list.Add(new DirectoryCatalog(pluginDir));
            }
            _plugins = plugins;
            foreach (var plugin in _plugins)
            {
                plugin.Host = host;
                PluginAdded?.Invoke(this, plugin);
            }
        }
        public void SetComments(ICommentViewModel comment)
        {
            var pluginCommentData = new CommentData
            {
                Comment = GetString(comment.MessageItems),
                IsNgUser = false,
                Nickname = GetString(comment.NameItems),
            };
            foreach (var plugin in _plugins)
            {
                plugin.OnCommentReceived(pluginCommentData);
            }
        }
        private string GetString(IEnumerable<IMessagePart> items, string separator = "")
        {
            var textItems = items.Where(s => s is IMessageText).Cast<IMessageText>().Select(s => s.Text);
            var message = string.Join("", textItems);
            return message;
        }
        public void ShowSettingsView()
        {

        }
        public void OnLoaded()
        {
            if(_plugins == null)
            {
                throw new InvalidOperationException("最初にLoadPlugins()を実行すること");
            }
            foreach (var plugin in _plugins)
            {
                plugin.OnLoaded();
            }
        }
        public void OnClosing()
        {
            if (_plugins == null)
                return;
            foreach (var plugin in _plugins)
            {
                try
                {
                    plugin.OnClosing();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }
        }

        public void ForeachPlugin(Action<IPlugin> p)
        {
            foreach (var plugin in _plugins)
            {
                try
                {
                    p(plugin);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }
        }

        private readonly IOptions _options;
        public PluginManager(IOptions options)
        {
            _options = options;
        }
    }
}
