using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BLiveSitePlugin
{
    /// <summary>
    /// Interaction logic for BLiveOptionsPanel.xaml
    /// </summary>
    public partial class BLiveOptionsPanel : UserControl
    {
        public BLiveOptionsPanel()
        {
            InitializeComponent();
        }
        public void SetViewModel(BLiveOptionsViewModel vm)
        {
            this.DataContext = vm;
        }
        public BLiveOptionsViewModel GetViewModel()
        {
            return (BLiveOptionsViewModel)this.DataContext;
        }
    }
}
