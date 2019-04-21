using SitePlugin;
using Plugin;
using System.Collections.Generic;
using System.Linq;
using System;
using Common;
using System.Diagnostics;

namespace MultiCommentViewer
{
    public class PluginHost : IPluginHost
    {
        public string SettingsDirPath => _options.SettingsDirPath;

        public double MainViewLeft => _options.MainViewLeft;

        public double MainViewTop => _options.MainViewTop;
        public bool IsTopmost => _options.IsTopmost;
        public string LoadOptions(string path)
        {
            var s = _io.ReadFile(path);
            return s;
        }

        public void SaveOptions(string path, string s)
        {
            _io.WriteFile(path, s);
        }

        public async void PostCommentToAll(string comment)
        {
            foreach(var connection in _vm.Connections)
            {
                try
                {
                    var cp = connection.CommentProvider;
                    await cp.PostCommentAsync(comment);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"PluginHost.PostCommentToAll(string) ConnectionName={connection.Name}, ex={ex.Message}");
                    _logger.LogException(ex);
                }
            }
        }

        public void PostComment(string guid, string comment)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<IConnectionStatus> GetAllConnectionStatus()
        {
            return _vm.Connections.Cast<IConnectionStatus>();
        }

        public IUser GetUser(Guid sitePluginGuid, string userId)
        {
            var siteContext = GetSiteContext(sitePluginGuid);
            return siteContext.GetUser(userId);
        }
        private ISiteContext GetSiteContext(Guid sitePluginGuid)
        {
            return _vm.GetSiteContext(sitePluginGuid);
        }
        private readonly MainViewModel _vm;
        private readonly IOptions _options;
        private readonly IIo _io;
        private readonly ILogger _logger;

        public PluginHost(MainViewModel vm, IOptions options, IIo io, ILogger logger)
        {
            //TODO:MainViewModelを直接受け取るのではなく、interfaceを受け取りたい
            _vm = vm;
            _options = options;
            _io = io;
            _logger = logger;
        }
    }
}

