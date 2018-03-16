using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SitePlugin;
using System.Windows.Controls;
namespace NicoSitePlugin.Next
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
