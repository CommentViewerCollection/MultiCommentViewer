using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SitePlugin;
using Plugin;
using ryu_s.BrowserCookie;
using System.Windows.Controls;
using System.Diagnostics;
using System.Threading;
using GalaSoft.MvvmLight;
using System.Windows.Media;
using System.Windows;
using Common;
namespace MultiCommentViewer.Test
{

    public class TestSiteContext : ISiteContext
    {
        public Guid Guid { get { return new Guid("609B4057-A5CE-49BA-A30F-211C4DFE838E"); } }

        public string DisplayName { get { return "テスト"; } }

        public IOptionsTabPage TabPanel
        {
            get
            {
                var panel = new TestSiteOptionsPagePanel();
                panel.SetViewModel(new TestSiteOptionsViewModel(_siteOptions));
                return new TestOptionsTabPage(DisplayName, panel);
            }
        }

        public ICommentProvider CreateCommentProvider(ConnectionName connectionName)
        {
            return new TestSiteCommentProvider(connectionName,_options, _siteOptions);
        }
        private TestSiteOptions _siteOptions;
        public void LoadOptions(string siteOptionsStr, IIo io)
        {
            _siteOptions = new TestSiteOptions();
        }

        public void SaveOptions(string path, IIo io)
        {
        }
        private readonly IOptions _options;
        private readonly ILogger _logger;
        public TestSiteContext(IOptions options, ILogger logger)
        {
            _options = options;
            _logger = logger;
        }
    }
    public class TestSiteOptionsViewModel:ViewModelBase
    {
        public bool IsCheckBox { get { return changed.IsCheckBox; } set { changed.IsCheckBox = value; } }
        public string TextBoxText { get { return changed.TextBoxText; } set { changed.TextBoxText = value; } }
        private readonly TestSiteOptions _origin;
        private readonly TestSiteOptions changed;
        public TestSiteOptions OriginOptions { get { return _origin; } }
        public TestSiteOptions ChangedOptions { get { return changed; } }
        public TestSiteOptionsViewModel(TestSiteOptions siteOptions)
        {
            _origin = siteOptions;
            changed = siteOptions.Clone();
        }
    }
    public class TestSiteOptions
    {
        public bool IsCheckBox { get; set; }
        public string TextBoxText { get; set; }
        public TestSiteOptions()
        {
            IsCheckBox = false;
            TextBoxText = "test";
        }

        internal TestSiteOptions Clone()
        {
            return (TestSiteOptions)this.MemberwiseClone();
        }

        internal void Set(TestSiteOptions changedOptions)
        {
            var properties = changedOptions.GetType().GetProperties();
            foreach (var property in properties)
            {
                if (property.SetMethod != null)
                {
                    property.SetValue(this, property.GetValue(changedOptions));
                }
            }
        }
    }
    public class TestOptionsTabPage : IOptionsTabPage
    {
        public string HeaderText { get; }

        private readonly TestSiteOptionsPagePanel _tabPagePanel;
        public UserControl TabPagePanel { get { return _tabPagePanel; } }

        public void Apply()
        {
            //TODO:なんかもっとすっきりとした実装にしたい
            var optionsVm = _tabPagePanel.GetViewModel();
            optionsVm.OriginOptions.Set(optionsVm.ChangedOptions);
        }

        public void Cancel()
        {
        }
        public TestOptionsTabPage(string displayName, TestSiteOptionsPagePanel panel)
        {
            HeaderText = displayName;
            _tabPagePanel = panel;
        }
    }
    public class MessageTextTest : IMessageText
    {
        public string Text { get; }
        public MessageTextTest(string text)
        {
            Text = text;
        }
    }
    public class TestSiteCommentProvider : ICommentProvider
    {
        private bool _canConnect=true;
        public bool CanConnect
        {
            get { return _canConnect; }
            set
            {
                _canConnect = value;
                CanConnectChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        private bool _canDisconnect = false;
        public bool CanDisconnect
        {
            get { return _canDisconnect; }
            set
            {
                _canDisconnect = value;
                CanDisconnectChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler<List<ICommentViewModel>> InitialCommentsReceived;
        public event EventHandler<ICommentViewModel> CommentReceived;
        public event EventHandler<IMetadata> MetadataUpdated;
        public event EventHandler CanConnectChanged;
        public event EventHandler CanDisconnectChanged;

        private CancellationTokenSource _cts;
        public async Task ConnectAsync(string input, IBrowserProfile browserProfile)
        {
            CanConnect = false;
            CanDisconnect = true;
            if(_cts != null)
            {
                Debugger.Break();                                
            }
            //Debug.Assert(_cts == null);
            _cts = new CancellationTokenSource();
            _metaTimer.Interval = 10 * 1000;
            _metaTimer.Elapsed += _metaTimer_Elapsed;
            _metaTimer.Enabled = true;

            try
            {
                while (!_cts.IsCancellationRequested)
                {
                    var name = new List<IMessagePart>
                {
                    new MessageTextTest(RandomString(2)),
                };
                    var message = new List<IMessagePart>
                {
                    new MessageTextTest(RandomString()),
                };
                    var comment = new TestSiteCommentViewModel(ConnectionName, name, message, _options, SiteOptions);
                    CommentReceived?.Invoke(this, comment );
                    await Task.Delay(500);
                }
            }
            finally
            {
                CanConnect = true;
                CanDisconnect = false;
                _metaTimer.Enabled = false;
            }
        }

        private void _metaTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            MetadataUpdated?.Invoke(this, new Metadata
            {
                Title = RandomString(),
                Active = RandomNum(2).ToString(), 
                 CurrentViewers = "-",
                  Elapsed = "-",
                  TotalViewers = "-",
                IsLive = true,

            });
        }

        private readonly System.Timers.Timer _metaTimer = new System.Timers.Timer();
        public ConnectionName ConnectionName { get; set; }
        public TestSiteOptions SiteOptions { get; set; }
        private readonly IOptions _options;
        public TestSiteCommentProvider(ConnectionName connectionName, IOptions options, TestSiteOptions siteOptions)
        {
            ConnectionName = connectionName;
            _options = options;
            SiteOptions = siteOptions;
        }
        public TestSiteCommentProvider()
        {

        }


        public void Disconnect()
        {
            if(_cts != null)
            {
                _cts.Cancel();
            }
        }

        public IEnumerable<ICommentViewModel> GetUserComments(IUser user)
        {
            throw new NotImplementedException();
        }

        public Task PostCommentAsync(string text)
        {
            throw new NotImplementedException();
        }
        private static Random random = new Random();
        public static string RandomString()
        {
            return RandomString(random.Next(2, 40));
        }
        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789\n";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
        public static int RandomNum(int length)
        {
            const string chars = "0123456789";
            var str = new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
            return int.Parse(str);
        }
    }
    class Metadata : IMetadata
    {
        public string Title { get; set; }

        public string Elapsed { get; set; }

        public string CurrentViewers { get; set; }

        public string Active { get; set; }

        public string TotalViewers { get; set; }

        public bool? IsLive { get; set; }
    }
}
