using SitePlugin;
using System.Windows.Controls;

namespace BLiveSitePlugin
{
    public class BLiveOptionsTabPage : IOptionsTabPage
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
        private readonly BLiveOptionsPanel _panel;
        public BLiveOptionsTabPage(string displayName, BLiveOptionsPanel panel)
        {
            HeaderText = displayName;
            _panel = panel;
        }
    }
}
