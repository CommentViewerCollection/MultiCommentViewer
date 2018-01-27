using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SitePlugin;
using System.Windows.Controls;
namespace NicoSitePlugin
{
    public class NicoOptionsTabPage : IOptionsTabPage
    {
        public NicoOptionsTabPage(string displayName, UserControl panel)
        {
            HeaderText = displayName;
            TabPagePanel = panel;
        }

        public string HeaderText { get; }

        public UserControl TabPagePanel { get; }

        public void Apply()
        {
        }

        public void Cancel()
        {
        }
    }
}
