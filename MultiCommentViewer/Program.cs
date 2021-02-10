using CommentViewerCommon;
using Common;
using MultiCommentViewer.Test;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace MultiCommentViewer
{
    class Program
    {
        static ILogger _logger;
        [STAThread]
        //static async Task Main(string[] args)
        static void Main()
        {
            _logger = new Common.LoggerTest();
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            var app = new AppNoStartupUri
            {
                ShutdownMode = ShutdownMode.OnExplicitShutdown
            };
            app.InitializeComponent();
            SynchronizationContext.SetSynchronizationContext(new DispatcherSynchronizationContext());

            var p = new Program();
            p.ExitRequested += (sender, e) =>
            {
                app.Shutdown();
            };

            var t = p.StartAsync();
            Handle(t);
            app.Run();
        }
        static async void Handle(Task t)
        {
            try
            {
                await t;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                _logger.LogException(ex);
            }
        }
        private string GetOptionsPath()
        {
            var currentDir = Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName;
            return Path.Combine(currentDir, "settings", "options.txt");
        }
        public async Task StartAsync()
        {
            var io = new Test.IOTest();
            //settingsディレクトリの有無を確認し、無ければ作成する
            const string SettingsDirName = "settings";
            if (!Directory.Exists(SettingsDirName))
            {
                Directory.CreateDirectory(SettingsDirName);
            }
            //OptionsはMainViewModelのContentRendered()で読み込みたい。しかし、その前にConnectionNameWidth等が参照されるため現状ではコンストラクタ以前に読み込む必要がある。
            //実行される順番は
            //ctor->ConnectionNameWidth->Activated->Loaded->ContentRendered
            //理想は、とりあえずViewを表示して、そこに"読み込み中です"みたいな表示を出している間に必要なものを読み込むこと。
            //しかし、それをやるにはViewの位置はデフォルト値になってしまう。それでも良いか。            
            //これ↓が一番いいかも
            //ここでOptionsのインスタンスを作成し、MainViewModelに渡す。とりあえずデフォルト値で初期化させ、ContentRenderedで保存されたOptionsを読み込み差し替える。
            var options = new DynamicOptionsTest();
            try
            {
                var s = io.ReadFile(GetOptionsPath());
                options.Deserialize(s);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
            }
            try
            {
                SendErrorFile();
            }
            catch { }
            ISitePluginLoader sitePluginLoader = new Test.SitePluginLoaderTest();
            IBrowserLoader browserLoader = new BrowserLoader(_logger);
            //var model = new Model(new SitePluginLoaderTest(), options, _logger, io);
            //(IIo io, ILogger logger, IOptions options, ISitePluginLoader sitePluginLoader, IBrowserLoader browserLoader)
            //var viewModel = new MainViewModel(model);
            var viewModel = new MainViewModel(io, _logger, options, sitePluginLoader, browserLoader);
            viewModel.CloseRequested += ViewModel_CloseRequested;

            void windowClosed(object sender, EventArgs e)
            {
                viewModel.RequestClose();

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
                    SendErrorReport(s, GetTitle(), GetVersion());
                }
                catch (Exception ex)
                {
                    //ここで例外が起きても送れない・・・。
                    Debug.WriteLine(ex.Message);
                }
            }

            //SplashScreen splashScreen = new SplashScreen();  //not disposable, but I'm keeping the same structure
            //{
            //    splashScreen.Closed += windowClosed; //if user closes splash screen, let's quit
            //    splashScreen.Show();

            await viewModel.InitializeAsync();

            var mainForm = new MainWindow();
            mainForm.Closed += windowClosed;
            mainForm.DataContext = viewModel;
            mainForm.Show();

            //    splashScreen.Owner = mainForm;
            //    splashScreen.Closed -= windowClosed;
            //    splashScreen.Close();
            //}
        }

        public event EventHandler<EventArgs> ExitRequested;
        void ViewModel_CloseRequested(object sender, EventArgs e)
        {
            OnExitRequested(EventArgs.Empty);
        }

        protected virtual void OnExitRequested(EventArgs e)
        {
            ExitRequested?.Invoke(this, e);
        }
        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = e.ExceptionObject as Exception;

            try
            {
                _logger.LogException(ex, "UnhandledException");
                var s = _logger.GetExceptions();
                using (var sw = new System.IO.StreamWriter("error.txt", true))
                {
                    sw.WriteLine(s);
                }
            }
            catch { }
        }
        private string GetTitle()
        {
            var asm = System.Reflection.Assembly.GetExecutingAssembly();
            var title = asm.GetName().Name;
            return title;
        }
        private string GetVersion()
        {
            var asm = System.Reflection.Assembly.GetExecutingAssembly();
            var ver = asm.GetName().Version;
            var s = $"v{ver.Major}.{ver.Minor}.{ver.Build}";
            return s;
        }
        /// <summary>
        /// エラー情報をサーバに送信する
        /// </summary>
        /// <param name="errorData"></param>
        /// <param name="title"></param>
        /// <param name="version"></param>
        private void SendErrorReport(string errorData, string title, string version)
        {
            if (string.IsNullOrEmpty(errorData))
            {
                return;
            }
            var fileStreamContent = new StreamContent(new System.IO.MemoryStream(Encoding.UTF8.GetBytes(errorData)));
            using (var client = new HttpClient())
            using (var formData = new MultipartFormDataContent())
            {
                client.DefaultRequestHeaders.Add("User-Agent", $"{title} {version}");
                formData.Add(fileStreamContent, "error", title + "_" + version + "_" + "error.txt");
                var t = client.PostAsync("https://int-main.net/upload", formData);
                var response = t.Result;
                if (!response.IsSuccessStatusCode)
                {
                }
                else
                {
                }
            }
        }
        /// <summary>
        /// error.txtがあったらサーバに送信して削除する
        /// </summary>
        private void SendErrorFile()
        {
            if (System.IO.File.Exists("error.txt"))
            {
                string errorContent;
                using (var sr = new System.IO.StreamReader("error.txt"))
                {
                    errorContent = sr.ReadToEnd();
                }
                SendErrorReport(errorContent, GetTitle(), GetVersion());
                System.IO.File.Delete("error.txt");
            }
        }
    }
}