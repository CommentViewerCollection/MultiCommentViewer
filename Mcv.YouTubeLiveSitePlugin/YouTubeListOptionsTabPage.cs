using Mcv.PluginV2;
using System.Windows.Controls;

namespace Mcv.YouTubeLiveSitePlugin
{
    public class YouTubeListOptionsTabPage : IOptionsTabPage
    {
        public string HeaderText { get; }

        public UserControl TabPagePanel => _panel;

        public void Apply()
        {
            var optionsVm = _panel.GetViewModel();
            optionsVm.OriginOptions.Set(optionsVm.ChangedOptions);
        }

        public void Cancel()
        {
        }
        private readonly YouTubeLiveOptionsPanel _panel;
        public YouTubeListOptionsTabPage(string displayName, YouTubeLiveOptionsPanel panel)
        {
            HeaderText = displayName;
            _panel = panel;
        }
    }
}
