using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using SitePlugin;
using System.IO;
using System.Reflection;
using Common;
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
            _logger = new Test.LoggerTest();
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            var currentDir = Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName;
            var optionsPath = Path.Combine(currentDir, "options.txt");
            IIo io = new Test.IOTest();

            //OptionsはMainViewModelのContentRendered()で読み込みたい。しかし、その前にConnectionNameWidth等が参照されるため現状ではコンストラクタ以前に読み込む必要がある。
            //実行される順番は
            //ctor->ConnectionNameWidth->Activated->Loaded->ContentRendered
            //理想は、とりあえずViewを表示して、そこに"読み込み中です"みたいな表示を出している間に必要なものを読み込むこと。
            //しかし、それをやるにはViewの位置はデフォルト値になってしまう。それでも良いか。            
            //これ↓が一番いいかも
            //ここでOptionsのインスタンスを作成し、MainViewModelに渡す。とりあえずデフォルト値で初期化させ、ContentRenderedで保存されたOptionsを読み込み差し替える。
            IOptionsSerializer optionsLoader = new Test.OptionsLoaderTest();

            var options = optionsLoader.Load(optionsPath, io);
            
            ISitePluginLoader sitePluginLoader = new Test.SitePluginLoaderTest();
            IBrowserLoader browserLoader = new BrowserLoader();
            IUserStore userStore = new UserStoreTest();

            
            var mainViewModel = new MainViewModel(optionsPath, io, _logger, optionsLoader,options, sitePluginLoader,browserLoader, userStore);
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
