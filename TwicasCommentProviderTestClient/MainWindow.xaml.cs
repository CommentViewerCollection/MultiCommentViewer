using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using TwicasSitePlugin;
using Common;
using SitePlugin;
using System.Diagnostics;
namespace TwicasCommentProviderTestClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }
    }
    class Logger : ILogger
    {
        public string GetExceptions()
        {
            throw new NotImplementedException();
        }

        public void LogException(Exception ex, string message = "", string detail = "")
        {
            Debug.WriteLine(ex.Message);
        }
    }
    public class MainViewModel:ViewModelBase
    {
        public ICommand ConnectCommand { get; }
        public ICommand DisconnectCommand { get; }
        bool _canConnect;
        bool _canDisconnect;
        public bool CanConnect
        {
            get { return _canConnect; }
            set
            {
                _canConnect = value;
                RaisePropertyChanged();
            }
        }
        public bool CanDisconnect
        {
            get { return _canDisconnect; }
            set
            {
                _canDisconnect = value;
                RaisePropertyChanged();
            }
        }
        public string BroadcasterId { get; set; }
        private async void Connect()
        {
            CanConnect = false;
            CanDisconnect = true;
            try
            {
                Debug.WriteLine("connected");
                _messageProvider = new MessageProvider(new TwicasServer(), null, new Logger());
                _messageProvider.Received += MessageProvider_Received;
                _messageProvider.MetaReceived += MessageProvider_MetaReceived;
//                await _messageProvider.ConnectAsync(BroadcasterId);
                Debug.WriteLine("disconnected");
            }
            finally
            {
                CanConnect = true;
                CanDisconnect = false;
            }
        }

        private void MessageProvider_MetaReceived(object sender, SitePlugin.IMetadata e)
        {

        }

        private void MessageProvider_Received(object sender, IEnumerable<ICommentData> e)
        {
            foreach(var data in e)
            {
                var list = new List<string>();
                foreach(var item in data.Message)
                {
                    if(item is IMessageText text)
                    {
                        if(text.Text.Contains("<" ) || text.Text.Contains("&"))
                        {
                            using (var sw = new System.IO.StreamWriter("a.txt", true))
                            {
                                sw.WriteLine(text.Text);
                            }
                        }
                        list.Add(text.Text);
                    }
                    else if(item is IMessageImage image)
                    {
                        list.Add(image.Url);
                    }
                }
                Debug.WriteLine("{");
                foreach(var line in list)
                {
                    Debug.WriteLine("\t" + line);
                }
                Debug.WriteLine("}");
            }
        }

        private void Disconnect()
        {
            if(_messageProvider != null)
            {
                _messageProvider.Disconnect();
            }
        }
        MessageProvider _messageProvider;
        public MainViewModel()
        {
            ConnectCommand = new RelayCommand(Connect);
            DisconnectCommand = new RelayCommand(Disconnect);
            CanConnect = true;
            CanDisconnect = false;
        }
    }
}
