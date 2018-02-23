using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using YouTubeLiveSitePlugin.Test2;
using Common;
using System.Windows.Media;

namespace YouTubeLiveCommentViewer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var options = new DynamicOptionsTest();
            var logger = new LoggerTest();
            var siteContext = new YouTubeLiveSiteContext(options, logger);
            var io = new IOTest();
            var vm = new ViewModel.MainViewModel(siteContext, options, io);
            var resource = Application.Current.Resources;
            var locator = resource["Locator"] as ViewModel.ViewModelLocator;
            locator.Main = vm;
        }
    }
}
