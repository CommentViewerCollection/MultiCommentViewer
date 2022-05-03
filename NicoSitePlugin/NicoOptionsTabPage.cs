using System.Windows.Controls;
using Mcv.PluginV2;

namespace NicoSitePlugin
{
    public class NicoOptionsTabPage : IOptionsTabPage
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
        private readonly NicoOptionsPanel _panel;
        public NicoOptionsTabPage(string displayName, NicoOptionsPanel panel)
        {
            HeaderText = displayName;
            _panel = panel;
        }
    }
}
