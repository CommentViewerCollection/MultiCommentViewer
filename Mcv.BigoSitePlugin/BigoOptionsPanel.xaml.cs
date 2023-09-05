using System.Windows.Controls;
namespace BigoSitePlugin
{
    /// <summary>
    /// Interaction logic for BigoOptionsPanel.xaml
    /// </summary>
    public partial class BigoOptionsPanel : UserControl
    {
        public BigoOptionsPanel()
        {
            InitializeComponent();
        }
        public void SetViewModel(BigoOptionsViewModel vm)
        {
            this.DataContext = vm;
        }
        public BigoOptionsViewModel GetViewModel()
        {
            return (BigoOptionsViewModel)this.DataContext;
        }
    }
}
