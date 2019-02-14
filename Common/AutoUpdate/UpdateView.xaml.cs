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
using System.Windows.Shapes;

namespace Common.AutoUpdate
{
    /// <summary>
    /// Interaction logic for UpdateView.xaml
    /// </summary>
    public partial class UpdateView : Window
    {
        public bool IsUpdateExists { get; set; }
        public Version CurrentVersion { get; set; }
        public LatestVersionInfo LatestVersionInfo { get; set; }
        public ILogger Logger { get; set; }
        public UpdateView()
        {
            InitializeComponent();
        }
        bool _isShown = false;
        protected override void OnContentRendered(EventArgs e)
        {
            if (_isShown)
                return;
            _isShown = true;
            
            if (IsUpdateExists)
            {
                myFrame.Visibility = Visibility.Visible;
                notUpdateFrame.Visibility = Visibility.Collapsed;
                myFrame.NavigationService.Navigate(new UpdateExistsPage(myFrame.NavigationService, CurrentVersion, LatestVersionInfo, Logger));
            }
            else
            {
                myFrame.Visibility = Visibility.Collapsed;
                notUpdateFrame.Visibility = Visibility.Visible;
                notUpdateFrame.NavigationService.Navigate(new UpdateNotExistsPage());
            }
            
            base.OnContentRendered(e);
        }
    }
}
