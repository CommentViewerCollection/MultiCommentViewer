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
using System.Diagnostics;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using TwitchSitePlugin;
namespace TwitchMessageProviderTestClient
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
    public class MainViewModel : ViewModelBase
    {
        private string _channelName;

        public string ChannelName
        {
            get => _channelName;
            set
            {
                _channelName = value;
                RaisePropertyChanged();
            }
        }
        public ICommand ConnectCommand { get; }
        public MainViewModel()
        {
            ConnectCommand = new RelayCommand(Connect);
        }
        private MessageProvider _messageProvider;
        private async void Connect()
        {
            _messageProvider = new MessageProvider();// "justinfan12345", ChannelName);
            _messageProvider.Opened += _messageProvider_Opened;
            _messageProvider.Received += CommentProvider_Received;
            await _messageProvider.ReceiveAsync();

        }

        private async void _messageProvider_Opened(object sender, EventArgs e)
        {
            var name = "justinfan12345";
            await _messageProvider.SendAsync("CAP REQ :twitch.tv/tags twitch.tv/commands");
            await _messageProvider.SendAsync("PASS SCHMOOPIIE");
            await _messageProvider.SendAsync($"NICK {name}");
            await _messageProvider.SendAsync($"USER {name} 8 * :{name}");
        }

        private async void CommentProvider_Received(object sender, Result result)
        {
            if(!Enum.TryParse("RPL_" + result.Command, out Command command))
            {
                using(var sw = new System.IO.StreamWriter("command.txt", true))
                {
                    sw.WriteLine("RPL_" + result.Command + ",");
                }
            }
            Debug.Write(result.Prefix);
            Debug.Write(" ");
            Debug.Write(result.Command);
            Debug.Write(" ");
            foreach(var param in result.Params)
            {
                Debug.Write(param);
                Debug.Write(" ");
            }
            Debug.WriteLine("");

            if(result.Command == "PING")
            {
                await _messageProvider.SendAsync("PONG");
            }
            else if (result.Command == "376")
            {
                await _messageProvider.SendAsync("JOIN #" + _channelName);
            }
        }
    }
    //class CommentProvider
    //{
    //    public event EventHandler<Result> Received;
    //    WebSocket _ws;
    //    TaskCompletionSource<object> _tcs;
    //    public Task ReceiveAsync()
    //    {
    //        _tcs = new TaskCompletionSource<object>();
    //        var cookies = new List<KeyValuePair<string, string>>();
    //        _ws = new WebSocket("wss://irc-ws.chat.twitch.tv/", "", cookies);
    //        _ws.MessageReceived += _ws_MessageReceived;
    //        _ws.Opened += _ws_Opened;
    //        _ws.Error += _ws_Error;
    //        _ws.Closed += _ws_Closed;
    //        _ws.Open();
    //        return _tcs.Task;
    //    }

    //    private void _ws_Closed(object sender, EventArgs e)
    //    {
    //        _tcs.SetResult(null);
    //    }

    //    //public static string GetRandomGuestUsername()
    //    //{
    //    //    //return "justinfan" + Math.floor(8e4 * Math.random() + 1e3)
    //    //    //var random = new Random();
    //    //    //random.Next()
    //    //    return "justinfan12345";

    //    //}

    //    private void _ws_Error(object sender, SuperSocket.ClientEngine.ErrorEventArgs e)
    //    {
    //    }

    //    private async void _ws_Opened(object sender, EventArgs e)
    //    {
    //        await SendAsync("CAP REQ :twitch.tv/tags twitch.tv/commands");
    //        await SendAsync("PASS SCHMOOPIIE");
    //        await SendAsync("NICK " + _name);
    //        await SendAsync($"USER {_name} 8 * :{_name}");
    //    }

    //    public async Task SendAsync(string s)
    //    {
    //        await Task.Yield();
    //        _ws.Send(s + "\r\n");
    //    }

    //    private async void _ws_MessageReceived(object sender, MessageReceivedEventArgs e)
    //    {
    //        var result = Tools.Parse(e.Message);
    //        if(result.Command == "376")
    //        {
    //            await SendAsync("JOIN #" + _channelName);
    //        }
    //        Received?.Invoke(this, result);
    //    }

    //    public void Disconnect()
    //    {
    //        _ws.Close();
    //    }
    //    private string _name;
    //    private string _channelName;
    //    public CommentProvider(string name, string channelName)
    //    {
    //        _name = name;
    //        _channelName = channelName;
    //    }
    //}

}
