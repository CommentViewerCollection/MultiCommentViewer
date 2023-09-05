using System.Windows.Controls;

namespace NicoSitePlugin
{
    /// <summary>
    /// Interaction logic for NicoOptionsPanel.xaml
    /// </summary>
    public partial class NicoOptionsPanel : UserControl
    {
        public NicoOptionsPanel()
        {
            InitializeComponent();
        }
        public void SetViewModel(NicoSiteOptionsViewModel vm)
        {
            this.DataContext = vm;
        }
        public NicoSiteOptionsViewModel GetViewModel()
        {
            return (NicoSiteOptionsViewModel)this.DataContext;
        }
    }
}
