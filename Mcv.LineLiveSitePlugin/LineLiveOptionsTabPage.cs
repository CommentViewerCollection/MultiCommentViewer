using Mcv.PluginV2;

namespace LineLiveSitePlugin
{
    public class LineLiveOptionsTabPage : IOptionsTabPage
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
        private readonly LineLiveOptionsPanel _panel;
        public LineLiveOptionsTabPage(string displayName, LineLiveOptionsPanel panel)
        {
            HeaderText = displayName;
            _panel = panel;
        }
    }
}
