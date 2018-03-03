using System.Windows.Controls;
namespace YouTubeLiveSitePlugin.Test2
{
    /// <summary>
    /// Interaction logic for YouTubeLiveOptionsPanel.xaml
    /// </summary>
    public partial class YouTubeLiveOptionsPanel : UserControl
    {
        public YouTubeLiveOptionsPanel()
        {
            InitializeComponent();
        }
        public void SetViewModel(YouTubeLiveOptionsViewModel vm)
        {
            this.DataContext = vm;
        }
        public YouTubeLiveOptionsViewModel GetViewModel()
        {
            return (YouTubeLiveOptionsViewModel)this.DataContext;
        }
    }
}
