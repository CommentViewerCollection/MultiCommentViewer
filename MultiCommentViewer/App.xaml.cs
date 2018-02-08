using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using SitePlugin;
namespace MultiCommentViewer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        ILogger _logger;
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            IOptionsLoader optionsLoader = new Test.OptionsLoaderTest();
            var options = optionsLoader.LoadOptions();
            ISitePluginLoader sitePluginLoader = new Test.SitePluginLoaderTest();
            IBrowserLoader browserLoader = new BrowserLoader();
            IUserStore userStore = new UserStoreTest();

            _logger = new Test.LoggerTest();
            var mainViewModel = new MainViewModel(_logger, options, sitePluginLoader,browserLoader, userStore);
            var resource = Application.Current.Resources;
            var locator = resource["Locator"] as ViewModels.ViewModelLocator;
            locator.Main = mainViewModel;
        }
        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = e.ExceptionObject as Exception;

            _logger.LogException(ex, "UnhandledException");
        }
    }
}
