using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using YouTubeLiveSitePlugin.Test2;
using Common;
using System.Windows.Media;
using System.Threading.Tasks;
using System.Diagnostics;
using System;

namespace YouTubeLiveCommentViewer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        DynamicOptionsTest options;
        IOTest io;
        private string GetOptionsPath()
        {
            return @"settings\options.txt";
        }
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            io = new IOTest();

            options = new DynamicOptionsTest();
            try
            {
                var s = io.ReadFile(GetOptionsPath());
                options.Deserialize(s);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            var logger = new LoggerTest();
            var siteContext = new YouTubeLiveSiteContext(options, logger);

            var vm = new ViewModel.MainViewModel(siteContext, options, io, logger);
            var resource = Application.Current.Resources;
            var locator = resource["Locator"] as ViewModel.ViewModelLocator;
            locator.Main = vm;
        }
        protected override void OnExit(ExitEventArgs e)
        {
            try
            {
                var s = options.Serialize();
                io.WriteFile(GetOptionsPath(), s);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            base.OnExit(e);
        }
    }
}
