using SitePlugin;

namespace PeriscopeSitePlugin
{
    public class PeriscopeOptionsTabPage : IOptionsTabPage
    {
        public string HeaderText { get; }

        public System.Windows.Controls.UserControl TabPagePanel => _panel;

        public void Apply()
        {
            var optionsVm = _panel.GetViewModel();
            optionsVm.OriginOptions.Set(optionsVm.ChangedOptions);
        }

        public void Cancel()
        {
        }
        private readonly PeriscopeOptionsPanel _panel;
        public PeriscopeOptionsTabPage(string displayName, PeriscopeOptionsPanel panel)
        {
            HeaderText = displayName;
            _panel = panel;
        }
    }
}
