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
using System.Text;
using System.Net;
using System.Diagnostics;
using Common;
using System.Net.Http;

namespace MultiCommentViewer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        ILogger _logger;
        Test.DynamicOptionsTest options;
        IIo io;
        private string GetOptionsPath()
        {
            var currentDir = Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName;
            return Path.Combine(currentDir, "settings", "options.txt");
        }
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            _logger = new Test.LoggerTest();
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            
            io = new Test.IOTest();

            //OptionsはMainViewModelのContentRendered()で読み込みたい。しかし、その前にConnectionNameWidth等が参照されるため現状ではコンストラクタ以前に読み込む必要がある。
            //実行される順番は
            //ctor->ConnectionNameWidth->Activated->Loaded->ContentRendered
            //理想は、とりあえずViewを表示して、そこに"読み込み中です"みたいな表示を出している間に必要なものを読み込むこと。
            //しかし、それをやるにはViewの位置はデフォルト値になってしまう。それでも良いか。            
            //これ↓が一番いいかも
            //ここでOptionsのインスタンスを作成し、MainViewModelに渡す。とりあえずデフォルト値で初期化させ、ContentRenderedで保存されたOptionsを読み込み差し替える。
            options = new Test.DynamicOptionsTest();
            try
            {
                var s = io.ReadFile(GetOptionsPath());
                options.Deserialize(s);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
            }

            ISitePluginLoader sitePluginLoader = new Test.SitePluginLoaderTest();
            IBrowserLoader browserLoader = new BrowserLoader(_logger);
            IUserStore userStore = new UserStoreTest();

            
            var mainViewModel = new MainViewModel(io, _logger, options, sitePluginLoader,browserLoader, userStore);
            var resource = Application.Current.Resources;
            var locator = resource["Locator"] as ViewModels.ViewModelLocator;
            locator.Main = mainViewModel;
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
                _logger.LogException(ex);
            }
            try
            {
                var s = _logger.GetExceptions();
                SendErrorReport(s);
            }catch(Exception ex)
            {
                //ここで例外が起きても送れない・・・。
                Debug.WriteLine(ex.Message);
            }
            base.OnExit(e);
        }
        private string GetUserAgent()
        {
            var asm = System.Reflection.Assembly.GetExecutingAssembly();
            var ver = asm.GetName().Version;
            var title = asm.GetName().Name;
            var s = $"{title} v{ver.Major}.{ver.Minor}.{ver.Build}";
            return s;
        }
        private void SendErrorReport(string errorData)
        {
            if (string.IsNullOrEmpty(errorData))
            {
                return;
            }
            var fileStreamContent = new StreamContent(new System.IO.MemoryStream(Encoding.UTF8.GetBytes(errorData)));
            using (var client = new HttpClient())
            using (var formData = new MultipartFormDataContent())
            {
                client.DefaultRequestHeaders.Add("User-Agent", GetUserAgent());
                formData.Add(fileStreamContent, "error", "MultiCommentViewer" + "_" + "error.txt");
                var t = client.PostAsync("http://int-main.net/upload", formData);
                var response = t.Result;
                if (!response.IsSuccessStatusCode)
                {
                }
                else
                {
                }
            }
        }
        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = e.ExceptionObject as Exception;

            _logger.LogException(ex, "UnhandledException");
        }
    }
}
