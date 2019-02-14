using SitePlugin;
using System.Windows.Controls;

namespace OpenrecSitePlugin
{
    public class OpenrecOptionsTabPage : IOptionsTabPage
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
        private readonly OpenrecOptionsPanel _panel;
        public OpenrecOptionsTabPage(string displayName, OpenrecOptionsPanel panel)
        {
            HeaderText = displayName;
            _panel = panel;
        }
    }
}
