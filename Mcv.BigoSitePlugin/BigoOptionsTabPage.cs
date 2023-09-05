using Mcv.PluginV2;
using System.Windows.Controls;
namespace BigoSitePlugin
{
    public class BigoOptionsTabPage : IOptionsTabPage
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
        private readonly BigoOptionsPanel _panel;
        public BigoOptionsTabPage(string displayName, BigoOptionsPanel panel)
        {
            HeaderText = displayName;
            _panel = panel;
        }
    }
}
