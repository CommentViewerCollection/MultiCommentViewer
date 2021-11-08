using SitePlugin;
using System.Windows.Controls;

namespace MixchSitePlugin
{
    public class MixchOptionsTabPage : IOptionsTabPage
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
        private readonly MixchOptionsPanel _panel;
        public MixchOptionsTabPage(string displayName, MixchOptionsPanel panel)
        {
            HeaderText = displayName;
            _panel = panel;
        }
    }
}
