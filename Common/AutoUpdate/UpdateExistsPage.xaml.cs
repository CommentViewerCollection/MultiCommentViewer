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

namespace Common.AutoUpdate
{
    /// <summary>
    /// Interaction logic for UpdateExistsPage.xaml
    /// </summary>
    public partial class UpdateExistsPage : Page
    {
        public Version CurrentVersion { get; }
        public LatestVersionInfo LatestVersionInfo { get; }
        private readonly NavigationService _service;
        private readonly ILogger _logger;
        public UpdateExistsPage(NavigationService service, Version currentVersion, LatestVersionInfo latestVersionInfo, ILogger logger)
        {
            _service = service;
            CurrentVersion = currentVersion;
            LatestVersionInfo = latestVersionInfo;
            _logger = logger;
            InitializeComponent();
            CurrentVersionText.Text = CurrentVersion.ToString();
            LatestVersionText.Text = LatestVersionInfo.Version.ToString();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var view = Window.GetWindow(this);
            view?.Close();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {            
            _service.Navigate(new DownloadPage(_service, CurrentVersion, LatestVersionInfo, _logger));
        }
    }
}
