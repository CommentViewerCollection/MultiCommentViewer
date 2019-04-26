using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;
using System.Diagnostics;
using System.Windows.Threading;
using Plugin;
using System.ComponentModel.Composition;
using SitePlugin;
using Common;

namespace OpenrecYoyakuPlugin
{
    [Export(typeof(IPlugin))]
    public class PluginBody : IPlugin
    {
        private IOptions _options;

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
        }
        public void OnMessageReceived(IMessage message, IMessageMetadata messageMetadata)
        {
            if (!_options.IsEnabled || messageMetadata.IsNgUser || messageMetadata.IsInitialComment)
                return;

            if (message is IMessageComment comment)
            {
                var name = comment.NameItems.ToText();
                var text = comment.CommentItems.ToText();
                _model.SetComment(comment.UserId, name, text, messageMetadata.User, messageMetadata.SiteContextGuid);
            }
        }
        SettingsViewModel _vm;
        private Dispatcher _dispatcher;
        protected virtual IOptions LoadOptions()
        {
            var options = new DynamicOptions();
            try
            {
                var s = Host.LoadOptions(GetSettingsFilePath());
                options.Deserialize(s);
            }
            catch (System.IO.FileNotFoundException) { }
            return options;
        }
        public void OnLoaded()
        {
            _dispatcher = Dispatcher.CurrentDispatcher;
            _options = LoadOptions();
            _model = CreateModel();
            _model.UsersListChanged += Model_UsersListChanged;
            _vm = CreateSettingsViewModel();
            LoadRegisteredUsers();
        }
        private string GetPath()
        {
            var dir = Host.SettingsDirPath;
            var path = Path.Combine(dir, $"{Name}_users.txt");
            return path;
        }
        private void LoadRegisteredUsers()
        {
            var path = GetPath();
            var s = Host.LoadOptions(path);
            var lines = s.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            foreach(var line in lines)
            {
                var arr = line.Split('\t');
                if (arr.Length != 5) continue;
                try
                {
                    var userId = arr[0];
                    var name = arr[1];
                    var date = DateTime.Parse(arr[2]);
                    var hasCalled = arr[3] == "True";
                    var guid = new Guid(arr[4]);
                    var user = Host.GetUser(guid, userId);
                    _model.AddUser(new User(user)
                    {
                        Date = date,
                        HadCalled = hasCalled,
                        Id = userId,
                        Name = name,
                        SitePluginGuid = guid,
                    });
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }
        }

        private void Model_UsersListChanged(object sender, EventArgs e)
        {
            var users = _model.RegisteredUsers.ToArray();
            var s = "";
            foreach(var user in users)
            {
                s += $"{user.Id}\t{user.Name}\t{user.Date}\t{user.HadCalled}\t{user.SitePluginGuid}" + Environment.NewLine;
            }
            var path = GetPath();
            Host.SaveOptions(path, s);
        }

        protected virtual SettingsViewModel CreateSettingsViewModel()
        {
            return new SettingsViewModel(_model, _dispatcher);
        }
        protected virtual Model CreateModel()
        {
            return new Model(_options, Host);
        }

        public void OnClosing()
        {
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
            var view = new SettingsView
            {
                Left = left,
                Top = top,
                DataContext = _vm
            };
            view.Show();
        }

        public string GetSettingsFilePath()
        {
            var dir = Host.SettingsDirPath;
            return Path.Combine(dir, $"{Name}.txt");
        }

        public void OnTopmostChanged(bool isTopmost)
        {
            if (_vm != null)
            {
                _vm.Topmost = isTopmost;
            }
        }
        Model _model;
        public PluginBody()
        {
        }
    }
}
