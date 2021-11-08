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

namespace MixchSitePlugin
{
    /// <summary>
    /// Interaction logic for MixchOptionsPanel.xaml
    /// </summary>
    public partial class MixchOptionsPanel : UserControl
    {
        public MixchOptionsPanel()
        {
            InitializeComponent();
        }
        public void SetViewModel(MixchOptionsViewModel vm)
        {
            this.DataContext = vm;
        }
        public MixchOptionsViewModel GetViewModel()
        {
            return (MixchOptionsViewModel)this.DataContext;
        }
    }
}
