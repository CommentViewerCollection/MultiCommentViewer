using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Diagnostics;
using System.Windows.Threading;
using Plugin;
using System.ComponentModel.Composition;
using SitePlugin;

namespace OpenrecYoyakuPlugin
{
    static class MessageItemsExtensions
    {
        public static string ToText(this IEnumerable<IMessagePart> parts)
        {
            string s = "";
            if (parts != null)
            {
                foreach (var part in parts)
                {
                    if (part is IMessageText text)
                    {
                        s += text;
                    }
                }
            }
            return s;
        }
    }
    [Export(typeof(IPlugin))]
    public class PluginBody : IPlugin
    {
        private DynamicOptions _options = new DynamicOptions();

        public string Name
        {
            get
            {
                return "予約管理プラグイン";
            }
        }
        public string Description
        {
            get
            {
                return "";
            }
        }
        public IPluginHost Host { get; set; }


        public void OnCommentReceived(ICommentData data)
        {
            if (!_options.IsEnabled || data.IsNgUser)
                return;

            vm.SetComment(data);
        }
        public void OnMessageReceived(IMessage message, IMessageMetadata messageMetadata)
        {
            if (!_options.IsEnabled || messageMetadata.IsNgUser)
                return;

            if (message is IMessageComment comment)
            {
                var name = comment.NameItems.ToText();
                var text = comment.CommentItems.ToText();
                vm.SetComment(comment.UserId, name, text);
            }
        }
        SettingsViewModel vm;
        private Dispatcher _dispatcher;
        public void OnLoaded()
        {
            _dispatcher = Dispatcher.CurrentDispatcher;
            try
            {
                var s = Host.LoadOptions(GetSettingsFilePath());
                _options.Deserialize(s);
            }
            catch (System.IO.FileNotFoundException) { }
            vm = new SettingsViewModel(Host, _options, _dispatcher);
        }

        public void OnClosing()
        {
            var op = new DynamicOptions();
            var s = _options.Serialize();
            Host.SaveOptions(GetSettingsFilePath(), s);
        }
        public void Run()
        {
        }

        public void ShowSettingView()
        {
            var left = Host.MainViewLeft;
            var top = Host.MainViewTop;
            var view = new SettingsView();
            view.Left = left;
            view.Top = top;
            view.DataContext = vm;
            view.Show();
        }

        public string GetSettingsFilePath()
        {
            var dir = Host.SettingsDirPath;
            return Path.Combine(dir, $"{Name}.xml");
        }

        public void OnTopmostChanged(bool isTopmost)
        {
            //if (_settingsView != null)
            //{
            //    _settingsView.Topmost = isTopmost;
            //}
        }

        public PluginBody()
        {

        }
    }
}
