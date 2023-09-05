using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LineLiveSitePlugin
{
    /// <summary>
    /// Interaction logic for TwicasOptionsPanel.xaml
    /// </summary>
    public partial class LineLiveOptionsPanel : UserControl
    {
        public LineLiveOptionsPanel()
        {
            InitializeComponent();
        }
        internal void SetViewModel(LineLiveSiteOptionsViewModel vm)
        {
            this.DataContext = vm;
        }
        internal LineLiveSiteOptionsViewModel GetViewModel()
        {
            return (LineLiveSiteOptionsViewModel)this.DataContext;
        }
    }
}
