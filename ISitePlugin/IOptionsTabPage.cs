using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SitePlugin
{
    public interface IOptionsTabPage
    {
        string HeaderText { get; }
        void Apply();
        void Cancel();
        System.Windows.Controls.UserControl TabPagePanel { get; }
    }
}
