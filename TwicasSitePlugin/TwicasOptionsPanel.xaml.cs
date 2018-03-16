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

namespace TwicasSitePlugin
{
    /// <summary>
    /// Interaction logic for TwicasOptionsPanel.xaml
    /// </summary>
    public partial class TwicasOptionsPanel : UserControl
    {
        public TwicasOptionsPanel()
        {
            InitializeComponent();
        }
        public void SetViewModel(TwicasOptionsViewModel vm)
        {
            this.DataContext = vm;
        }
        public TwicasOptionsViewModel GetViewModel()
        {
            return (TwicasOptionsViewModel)this.DataContext;
        }
    }
}
