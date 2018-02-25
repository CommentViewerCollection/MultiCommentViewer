using SitePlugin;
using Plugin;

namespace YouTubeLiveCommentViewer.ViewModel
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

        private readonly MainViewModel _vm;
        private readonly IOptions _options;
        private readonly IIo _io;
        public PluginHost(MainViewModel vm, IOptions options, IIo io)
        {
            _vm = vm;
            _options = options;
            _io = io;
        }
    }
}
