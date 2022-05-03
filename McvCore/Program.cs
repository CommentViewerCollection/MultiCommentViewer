using Mcv.Core;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using System.Diagnostics;

namespace McvCore
{
    class Program
    {
        [STAThread]
        static void Main()
        {
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

            //pluginsディレクトリが無ければ作成する
            Directory.CreateDirectory("plugins");

            //settingsディレクトリが無ければ作成する
            Directory.CreateDirectory("settings");

            //var actorSystem = ActorSystem.Create("mcv");
            //var core = actorSystem.ActorOf(Props.Create(() => new CoreActor(app)).WithDispatcher("akka.actor.synchronized-dispatcher"));
            //core.Tell(new LoadPlugins());

            var core = new McvCore();
            core.ExitRequested += (s, e) =>
            {
                app.Shutdown();
            };

            if (!core.Initialize())
            {
                return;
            }
            var task = core.RunAsync();
            Handle(task);
            app.Run();
        }

        static async void Handle(Task task)
        {
            try
            {
                await Task.Yield();
                await task;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        public event EventHandler<EventArgs>? ExitRequested;
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
            if (ex is not null)
            {
                Debug.WriteLine(ex.Message);
            }

        }
    }

}
