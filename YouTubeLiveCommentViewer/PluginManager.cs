using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Plugin;
using SitePlugin;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
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
            foreach (var pluginDir in pluginDirs)
            {
                list.Add(new DirectoryCatalog(pluginDir));
            }
            var aggCat = new AggregateCatalog(list);

            var container = new CompositionContainer(aggCat);
            _plugins = container.GetExports<IPlugin>().Select(p => p.Value).ToList();
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
            foreach (var plugin in _plugins)
            {
                plugin.OnLoaded();
            }
        }
        public void OnClosing()
        {
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
