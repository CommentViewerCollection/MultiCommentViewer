using Akka.Actor;
using Mcv.Core;
using Mcv.Core.CoreActorMessages;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Mcv.Core;

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

        var actorSystem = ActorSystem.Create("mcv");
        var deadletterWatchMonitorProps = Props.Create(() => new DeadletterMonitor());
        var deadletterWatchActorRef = actorSystem.ActorOf(deadletterWatchMonitorProps, "DeadLetterMonitoringActor");
        actorSystem.EventStream.Subscribe(deadletterWatchActorRef, typeof(Akka.Event.DeadLetter));

        var monitorActor = actorSystem.ActorOf<UnhandledMessagesMonitorActor>();
        actorSystem.EventStream.Subscribe(monitorActor, typeof(Akka.Event.UnhandledMessage));

        var actor = actorSystem.ActorOf(McvCoreActor.Props(), "coreActor");


        var t = actorSystem.WhenTerminated;
        Handle(t).ContinueWith(t => app.Shutdown());
        actor.Tell(new Initialize());

        ////if (!core.Initialize())
        ////{
        ////    return;
        ////}
        //var task = a.RunAsync();
        //Handle(task);
        app.Run();
    }

    static async Task Handle(Task task)
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
public class DeadletterMonitor : ReceiveActor
{

    public DeadletterMonitor()
    {
        Receive<Akka.Event.DeadLetter>(dl => HandleDeadletter(dl));
    }

    private void HandleDeadletter(Akka.Event.DeadLetter dl)
    {
        Debug.WriteLine($"DeadLetter captured: {dl.Message}, sender: {dl.Sender}, recipient: {dl.Recipient}");
    }
}
public class UnhandledMessagesMonitorActor : ReceiveActor
{
    public UnhandledMessagesMonitorActor()
    {
        Receive<Akka.Event.UnhandledMessage>(m =>
        {
            Debug.WriteLine(m);
        });
    }
}
