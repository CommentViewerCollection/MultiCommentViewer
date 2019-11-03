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
namespace MixerSitePlugin
{
    /// <summary>
    /// Interaction logic for TabPagePanel.xaml
    /// </summary>
    public partial class TabPagePanel : UserControl
    {
        internal TabPagePanel()
        {
            InitializeComponent();
        }
        internal void SetViewModel(MixerSiteOptionsViewModel vm)
        {
            this.DataContext = vm;
        }
        internal MixerSiteOptionsViewModel GetViewModel()
        {
            return (MixerSiteOptionsViewModel)this.DataContext;
        }
        
    }
}
