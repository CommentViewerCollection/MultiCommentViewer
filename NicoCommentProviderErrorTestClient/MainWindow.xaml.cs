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
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using NicoSitePlugin;
using NicoSitePlugin.Old;
using NicoSitePlugin.Test2;
using Common;
using System.Diagnostics;
using System.Threading;
using System.Collections.ObjectModel;

namespace NicoCommentProviderErrorTestClient
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
    public class RoomViewModel:ViewModelBase
    {
        public ICommand ReceiveThreadCode0Command { get; }
        private void ReceiveThreadCode0()
        {
            _socket.ReceiveStr(new List<string>
            {
                "<thread resultcode=\"0\" thread=\"1621586179\" last_res=\"715\" ticket=\"0x11361600\" revision=\"1\" server_time=\"1518747947\"/>",
                "<chat thread=\"1621586179\" no=\"616\" vpos=\"154535\" date=\"1518747575\" date_usec=\"218368\" mail=\"184\" user_id=\"0mB1dBAnUjDsXYTFpDM2Jgm2wys\" premium=\"1\" anonymity=\"1\">なにこれきも</chat>",

            });
        }

        public ICommand ReceiveThreadCode1Command { get; }
        private void ReceiveThreadCode1()
        {
            _socket.ReceiveStr(new List<string>
            {
                "<thread resultcode=\"1\" thread=\"1621586179\" last_res=\"715\" ticket=\"0x11361600\" revision=\"1\" server_time=\"1518747947\"/>",
            });
        }
        public ICommand DisconnectUnexpectedCommand { get; }
        private void DisconnectUnexpected()
        {
            _socket.DisconnectUnexpected();
        }
        public string Addr { get; }
        public int Port { get; }
        public string Thread { get; }
        public string RoomName { get; }

        private TestStreamSocket _socket;
        public TestStreamSocket Socket
        {
            set
            {
                _socket = value;
            }
        }

        private string _status;
        public string Status
        {
            get { return _status; }
            set
            {
                if (_status == value) return;
                _status = value;
                RaisePropertyChanged();
            }
        }
        public RoomViewModel(RoomInfo roomInfo)
        {
            ReceiveThreadCode0Command = new RelayCommand(ReceiveThreadCode0);
            ReceiveThreadCode1Command = new RelayCommand(ReceiveThreadCode1);
            Addr = roomInfo.Addr;
            Port = roomInfo.Port;
            Thread = roomInfo.Thread;
            RoomName = roomInfo.RoomLabel;
        }
    }
    public class MainViewModel
    {
        public ObservableCollection<RoomViewModel> Rooms { get; } = new ObservableCollection<RoomViewModel>();
        public ICommand ConnectCommand { get; }
        public ICommand DisconnectCommand { get; }
        public ICommand AddNewRoomCommand { get; }
        //public ICommand ReceiveThreadCommand { get; }
        //public ICommand DisconnectUnexpectedCommand { get; }

        private RoomInfo CreateNewRoom()
        {
            var newRoom = new RoomInfo(new MsTest("a", 0, "123"), "room");
            return newRoom;
        }
        Dictionary<RoomInfo, RoomViewModel> _roomVmDict = new Dictionary<RoomInfo, RoomViewModel>();
        private void AddNewRoom()
        {
            var newRoom = CreateNewRoom();
            var vm = new RoomViewModel(newRoom);
            Rooms.Add(vm);
            _roomVmDict.Add(newRoom, vm);
            _commentProvider.Add(new List<RoomInfo> { newRoom });
        }
        //private void ReceiveThread()
        //{
        //    _streamSocket.ReceiveThread();
        //}
        //private void DisconnectUnexpected()
        //{
        //    _streamSocket.DisconnectUnexpected();
        //}
        private async void Connect()
        {
            Debug.WriteLine("connected");
            await _commentProvider.ReceiveAsync();
            Debug.WriteLine("disconnected");
        }

        private void _commentProvider_InitialCommentsReceived(object sender, List<string> e)
        {
        }

        private void _commentProvider_CommentReceived(object sender, CommentContext e)
        {
            Debug.WriteLine("commentReceived: " + e.Chat.Raw);
        }

        private void Disconnect()
        {
            _commentProvider.Disconnect();
        }
        TestCommentProvider _commentProvider;
        public MainViewModel()
        {
            _commentProvider = new TestCommentProvider();
            _commentProvider.CommentReceived += _commentProvider_CommentReceived;
            _commentProvider.InitialCommentsReceived += _commentProvider_InitialCommentsReceived;
            _commentProvider.RoomStatusChanged += _commentProvider_RoomStatusChanged;
            _commentProvider.RoomAdded += _commentProvider_RoomAdded;
            ConnectCommand = new RelayCommand(Connect);
            DisconnectCommand = new RelayCommand(Disconnect);
            AddNewRoomCommand = new RelayCommand(AddNewRoom);
            //ReceiveThreadCommand = new RelayCommand(ReceiveThread);
            //DisconnectUnexpectedCommand = new RelayCommand(DisconnectUnexpected);
        }

        private void _commentProvider_RoomAdded(object sender, RoomAddedEventArgs e)
        {
            var vm = _roomVmDict[e.RoomInfo];
            vm.Socket = e.Socket as TestStreamSocket;
        }

        private void _commentProvider_RoomStatusChanged(object sender, RoomStatusChangedEventArgs e)
        {
            var vm = _roomVmDict[e.RoomInfo];
            vm.Status = e.Status;
        }
    }
    class RoomAddedEventArgs:EventArgs
    {
        public RoomInfo RoomInfo { get; set; }
        public IStreamSocket Socket { get; set; }
    }
    class TestCommentProvider : CommentProvider
    {
        public event EventHandler<RoomAddedEventArgs> RoomAdded;
        public event EventHandler<RoomStatusChangedEventArgs> RoomStatusChanged;
        private void OnRoomStatusChanged(RoomInfo info, string status)
        {
            RoomStatusChanged?.Invoke(this, new RoomStatusChangedEventArgs { RoomInfo = info, Status = status });
        }
        protected override IStreamSocket CreateStreamSocket(string addr, int port)
        {
            return new TestStreamSocket();
        }
        protected override void OnRoomAdded(RoomInfo info, IStreamSocket socket)
        {
            OnRoomStatusChanged(info, "Receiving");
            RoomAdded?.Invoke(this, new RoomAddedEventArgs { RoomInfo = info, Socket = socket });
            base.OnRoomAdded(info, socket);
        }
        protected override void OnReceiveSuccessThread(RoomInfo info)
        {
            OnRoomStatusChanged(info, "resultcode=0");
            base.OnReceiveSuccessThread(info);
        }
        protected override void OnReceiveFailedThread(RoomInfo info)
        {
            OnRoomStatusChanged(info, "resultcode=1");
            base.OnReceiveFailedThread(info);
        }
        protected override void OnUnexpectedDisconnected(RoomInfo info)
        {
            OnRoomStatusChanged(info, "unexpected disconnect");
            base.OnUnexpectedDisconnected(info);
        }
        protected override void OnUnexpectedDisconnectedAndExceptionThrown(RoomInfo info)
        {
            OnRoomStatusChanged(info, "exception");
            base.OnUnexpectedDisconnectedAndExceptionThrown(info);
        }
        protected override void OnReconnecting(RoomInfo info)
        {
            OnRoomStatusChanged(info, "reconnecting");
            base.OnReconnecting(info);
        }
        protected override void OnDisconnected(RoomInfo info)
        {
            OnRoomStatusChanged(info, "disconnected");
            base.OnDisconnected(info);
        }
        protected override void OnRemoved(RoomInfo info)
        {
            OnRoomStatusChanged(info, "removed");
            base.OnRemoved(info);
        }
        public TestCommentProvider():base(0, new Logger())
        {
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
    public class TestStreamSocket : IStreamSocket
    {
        public event EventHandler Connected;
        public event EventHandler<List<string>> Received;
        private CancellationTokenSource _cts;
        public Task ConnectAsync()
        {
            _cts = new CancellationTokenSource();
            return Task.CompletedTask;
        }

        public void Disconnect()
        {
            if(_cts != null)
            {
                _cts.Cancel();
            }
        }

        public void Dispose()
        {
        }

        public async Task ReceiveAsync()
        {
            while (!_cts.IsCancellationRequested)
            {
                await Task.Delay(100);
            }
        }

        public async Task SendAsync(string s)
        {
            Debug.WriteLine("sent:" + s);
        }
        public void ReceiveThread()
        {
            Received?.Invoke(this, new List<string>
            {
                "<thread resultcode=\"0\" thread=\"1621586179\" last_res=\"715\" ticket=\"0x11361600\" revision=\"1\" server_time=\"1518747947\"/>",
                "<chat thread=\"1621586179\" no=\"616\" vpos=\"154535\" date=\"1518747575\" date_usec=\"218368\" mail=\"184\" user_id=\"0mB1dBAnUjDsXYTFpDM2Jgm2wys\" premium=\"1\" anonymity=\"1\">なにこれきも</chat>",
            });
        }
        public void ReceiveStr(List<string> list)
        {
            Received?.Invoke(this, list);
        }
        public void DisconnectUnexpected()
        {
            if (_cts != null)
            {
                _cts.Cancel();
            }
        }
        public TestStreamSocket()
        {

        }
    }
}
