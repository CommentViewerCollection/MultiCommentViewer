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
using System.Text.RegularExpressions;
using System.Windows.Navigation;
using System.Windows.Shapes;
using NicoSitePlugin.Test2;
using System.Net;
using ryu_s.BrowserCookie;
using NicoSitePlugin.Old;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Threading;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Common;

namespace NicoCommentProviderTestClient
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
        private bool bottom = true;
        //private bool neverTouch = true;
        private void DataGrid_ScrollChanged(object sender, RoutedEventArgs e)
        {
            //if (_addingCommentToTop)
            //    return;
            if (sender == null)
                return;
            ScrollViewer scrollViewer;
            if (sender is DataGrid dataGrid)
            {
                scrollViewer = dataGrid.GetScrollViewer();
            }
            else if (sender is ScrollViewer)
            {
                scrollViewer = sender as ScrollViewer;
            }
            else
            {
                return;
            }
            var a = e as ScrollChangedEventArgs;


            //2017/09/11
            //ExtentHeightは表示されていない部分も含めた全てのコンテントの高さ。
            //ScrollChangedが呼び出されたのにExtentHeightChangeが0ということはアイテムが追加されていないのにも関わらずスクロールがあった。
            //それはユーザが手動でスクロールした場合のみ起こること。
            if (a.ExtentHeightChange == 0)
            {
                //ユーザが手動でスクロールした
                bottom = scrollViewer.IsBottom();
                //neverTouch = false;
            }

            //2017/09/11全体の高さが表示部に収まる間はスクロールがBottomにあるとみなすと、表示部に収まらなくなった瞬間にもBottomにあると判定されて、最初のスクロールが上手くいくかも。

            //if (bottom && a.ExtentHeightChange != 0)
            if (bottom && Test(a))
            {
                scrollViewer.ScrollToBottom();
            }
        }
        private bool Test(ScrollChangedEventArgs e)
        {
            return e.ViewportHeightChange > 0 || e.ExtentHeightChange > 0 || e.ViewportHeightChange < 0 || e.ExtentHeightChange < 0;
        }
    }
    public static class DataGridBehavior
    {
        public static ScrollViewer GetScrollViewer(this DataGrid dataGrid)
        {
            return dataGrid.Template.FindName("DG_ScrollViewer", dataGrid) as ScrollViewer;
        }
    }
    public static class ScrollViewerBehavior
    {
        public static bool IsBottom(this ScrollViewer sv)
        {
            //var b = (sv.VerticalOffset * 1.01) > sv.ScrollableHeight;
            var b = (sv.VerticalOffset >= sv.ScrollableHeight
                || sv.ExtentHeight < sv.ViewportHeight);
            return b;
        }
    }
    public class MainViewModel:ViewModelBase
    {
        public ObservableCollection<RoomViewModel> RoomCollection { get; } = new ObservableCollection<RoomViewModel>();
        public ObservableCollection<CommentViewModel> Comments { get; } = new ObservableCollection<CommentViewModel>();
        private string _addr;
        public string Addr
        {
            get { return _addr; }
            set
            {
                _addr = value;
                RaisePropertyChanged();
            }
        }
        private string _thread;
        public string Thread
        {
            get { return _thread; }
            set
            {
                _thread = value;
                RaisePropertyChanged();
            }
        }
        private int _port;
        public int Port
        {
            get { return _port; }
            set
            {
                _port = value;
                RaisePropertyChanged();
            }
        }
        private string _roomName;
        public string RoomName
        {
            get { return _roomName; }
            set
            {
                _roomName = value;
                RaisePropertyChanged();
            }
        }
        private string _input;
        public string Input
        {
            get { return _input; }
            set
            {
                _input = value;
                RaisePropertyChanged();
            }
        }
        public ICommand AddCommand { get; }
        public ICommand MainViewContentRenderedCommand { get; }
        public ICommand MainViewClosingCommand { get; }
        private async void MainViewContentRendered()
        {
            try
            {
                await _provider.ReceiveAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }
        private void MainViewClosing()
        {

        }
        public ICommand GetPsCommand { get; }
        private async void GetPs()
        {
            string live_id;
            var match = Regex.Match(Input, "(lv\\d+)");
            if (!match.Success)
                return;
            live_id = match.Groups[1].Value;
            var manager = new ryu_s.BrowserCookie.ChromeManager();
            var profile = manager.GetProfiles()[0];
            var cookies = profile.GetCookieCollection("nicovideo.jp");
            var cc = new CookieContainer();
            cc.Add(cookies);
            var res = await API.GetPlayerStatusAsync(new DataSource(),live_id, cc);
            if (!res.Success)
            {
                Debug.WriteLine(res.Error.Code.ToString());
                return;
            }
            var ps = res.PlayerStatus;
            Addr = ps.Ms.Addr;
            Port = ps.Ms.Port;
            Thread = ps.Ms.Thread;
            RoomName = ps.RoomLabel;
            Input = "";
        }
        private void Add()
        {
            try
            {
                if (string.IsNullOrEmpty(Addr) || string.IsNullOrEmpty(Thread) || string.IsNullOrEmpty(RoomName) || Port == 0)
                    return;
                var ms = new MsTest(Addr, Port, Thread);
                var roomInfo = new RoomInfo(ms, RoomName);
                _provider.Add(new List<RoomInfo> { roomInfo });
                RoomCollection.Add(new RoomViewModel(roomInfo));
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }
        private readonly CommentProvider _provider;
        private readonly Dispatcher _dispatcher;
        private readonly ILogger _logger;

        public MainViewModel()
        {
            _logger = new Logger();
            _dispatcher = Dispatcher.CurrentDispatcher;
            _provider = new CommentProvider(100, _logger);
            _provider.CommentReceived += _provider_CommentReceived;
            _provider.InitialCommentsReceived += _provider_InitialCommentsReceived;
            AddCommand = new RelayCommand(Add);
            GetPsCommand = new RelayCommand(GetPs);
            MainViewContentRenderedCommand = new RelayCommand(MainViewContentRendered);
            MainViewClosingCommand = new RelayCommand(MainViewClosing);
        }
        private void _provider_InitialCommentsReceived(object sender, List<string> e)
        {
            foreach (var comment in e)
            {
                Debug.WriteLine(comment);
            }
        }
        bool isDisconnectOffered;
        private async void _provider_CommentReceived(object sender, CommentContext context)
        {   
            await _dispatcher.BeginInvoke((Action)(() =>
            {
                Comments.Add(new CommentViewModel(context));
            }), DispatcherPriority.Normal);
            if (context.Chat.text == "/disconnect" && !isDisconnectOffered)
            {
                isDisconnectOffered = true;
                await Task.Delay(1 * 60 * 1000);
                _provider.Disconnect();
            }
        }
    }
    public class Logger : ILogger
    {
        public string GetExceptions()
        {
            throw new NotImplementedException();
        }

        public void LogException(Exception ex, string message = "", string detail = "")
        {
            Debug.WriteLine("{");
            Debug.WriteLine("\t" + ex.Message);
            Debug.WriteLine("\t" + ex.StackTrace);
            Debug.WriteLine("\t" + message);
            Debug.WriteLine("\t" + detail);
            Debug.WriteLine("}");
        }
    }
    public class CommentViewModel
    {
        public string ThreadId { get; }
        public string RoomName { get; }
        public string Message { get; }
        public CommentViewModel(CommentContext context)
        {
            ThreadId = context.RoomInfo.Thread;
            RoomName = context.RoomInfo.RoomLabel;
            Message = context.Chat.text;
        }
    }
    public class RoomViewModel
    {
        public string RoomName { get; }
        public string Addr { get; }
        public int Port { get; }
        public string Thread { get; }
        public RoomViewModel(RoomInfo room)
        {
            RoomName = room.RoomLabel;
            Addr = room.Addr;
            Port = room.Port;
            Thread = room.Thread;
        }
    }
}
