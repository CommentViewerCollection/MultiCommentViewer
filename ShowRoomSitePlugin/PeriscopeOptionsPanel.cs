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

namespace ShowRoomSitePlugin
{
    /// <summary>
    /// Interaction logic for TwicasOptionsPanel.xaml
    /// </summary>
    public partial class ShowRoomOptionsPanel : UserControl
    {
        public ShowRoomOptionsPanel()
        {
            InitializeComponent();
        }
        internal void SetViewModel(ShowRoomSiteOptionsViewModel vm)
        {
            this.DataContext = vm;
        }
        internal ShowRoomSiteOptionsViewModel GetViewModel()
        {
            return (ShowRoomSiteOptionsViewModel)this.DataContext;
        }
    }
}
