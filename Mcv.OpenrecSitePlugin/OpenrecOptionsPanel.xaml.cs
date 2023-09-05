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

namespace OpenrecSitePlugin
{
    /// <summary>
    /// Interaction logic for OpenrecOptionsPanel.xaml
    /// </summary>
    public partial class OpenrecOptionsPanel : UserControl
    {
        public OpenrecOptionsPanel()
        {
            InitializeComponent();
        }
        public void SetViewModel(OpenrecOptionsViewModel vm)
        {
            this.DataContext = vm;
        }
        public OpenrecOptionsViewModel GetViewModel()
        {
            return (OpenrecOptionsViewModel)this.DataContext;
        }
    }
}
