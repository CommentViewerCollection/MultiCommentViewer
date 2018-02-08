using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ryu_s.BrowserCookie;
using SitePlugin;
using System.Net;
using System.Text;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
namespace TwitchSitePlugin
{

    class TwitchCommentProvider : ICommentProvider
    {
        private bool _canConnect;
        public bool CanConnect
        {
            get { return _canConnect; }
            set
            {
                if (_canConnect == value)
                    return;
                _canConnect = value;
                CanConnectChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        
        private bool _canDisconnect;
        public bool CanDisconnect
        {
            get { return _canDisconnect; }
            set
            {
                if (_canDisconnect == value)
                    return;
                _canDisconnect = value;
                CanDisconnectChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler<List<ICommentViewModel>> InitialCommentsReceived;
        public event EventHandler<ICommentViewModel> CommentReceived;
        public event EventHandler<IMetadata> MetadataUpdated;
        public event EventHandler CanConnectChanged;
        public event EventHandler CanDisconnectChanged;

        private string GetChannelName(string input)
        {
            return Tools.GetChannelName(input);
        }
        private string _channelName;
        private MessageProvider _provider;
        //private CookieContainer _cc;
        public async Task ConnectAsync(string input, IBrowserProfile browserProfile)
        {
            CanConnect = false;
            CanDisconnect = true;
            try
            {
                try
                {
                    _channelName = GetChannelName(input);
                }
                catch (Exception ex)
                {
                    _logger.LogException(ex);
                }
                var cc = new CookieContainer();

                try
                {
                    var cookies = browserProfile.GetCookieCollection("twitch.tv");
                    cc.Add(cookies);
                }
                catch (Exception ex)
                {
                    _logger.LogException(ex);
                }
                _me = null;
                try
                {
                    _me = await API.GetMeAsync(_server, cc);
                }
                catch (Exception ex)
                {
                    _logger.LogException(ex);
                }
                if (_me != null)
                {
                    _emotIcons = await API.GetEmotIcons(_server, _me.Id, cc);
                }

                
                _provider = new MessageProvider();
                _provider.Opened += Provider_Opened;
                _provider.Received += Provider_Received;
                await _provider.ReceiveAsync();
            }
            finally
            {
                CanConnect = true;
                CanDisconnect = false;
            }
        }
        IMe _me;
        LowObject.Emoticons _emotIcons;
        private async void Provider_Received(object sender, Result e)
        {
            switch (e.Command)
            {
                case "GLOBALUSERSTATE":
                    await _provider.SendAsync($"JOIN " + _channelName);
                    break;
                case "PRIVMSG":
                    var cvm = new TwitchCommentViewModel(_connectionName, _options, _siteOptions, e, _emotIcons);
                    CommentReceived?.Invoke(this, cvm);
                    break;
            }
        }

        private async void Provider_Opened(object sender, EventArgs e)
        {
            if (IsLoggedIn())
            {
                await _provider.SendAsync("CAP REQ :twitch.tv/tags twitch.tv/commands");
                await _provider.SendAsync($"PASS oauth:{_me.ChatOauthToken}");
                await _provider.SendAsync($"NICK {_me.Name}");
                await _provider.SendAsync($"USER {_me.Name} 8 * :{_me.Name}");
            }
            else
            {
                var name = Tools.GetRandomGuestUsername();
                await _provider.SendAsync("CAP REQ :twitch.tv/tags twitch.tv/commands");
                await _provider.SendAsync($"PASS SCHMOOPIIE");
                await _provider.SendAsync($"NICK {name}");
                await _provider.SendAsync($"USER {name} 8 * :{name}");
            }
        }
        private bool IsLoggedIn()
        {
            return _me != null;
        }

        public void Disconnect()
        {
            _provider.Disconnect();
        }

        public List<ICommentViewModel> GetUserComments(IUser user)
        {
            throw new NotImplementedException();
        }

        public async Task PostCommentAsync(string text)
        {
            var s = $"PRIVMSG {_channelName} :{text}";
        }
        private readonly IDataServer _server;
        private readonly ILogger _logger;
        private readonly ConnectionName _connectionName;
        private readonly IOptions _options;
        private readonly TwitchSiteOptions _siteOptions;
        public TwitchCommentProvider(ConnectionName connectionName, IDataServer server, ILogger logger, IOptions options, TwitchSiteOptions siteOptions)
        {
            _connectionName = connectionName;
            _server = server;
            _logger = logger;
            _options = options;
            _siteOptions = siteOptions;

            CanConnect = true;
            CanDisconnect = false;
        }
    }
    class MyWebClient : WebClient
    {
        protected override WebRequest GetWebRequest(Uri address)
        {
            var req = base.GetWebRequest(address);
            if (req is HttpWebRequest http)
            {
                http.CookieContainer = _cc;
                foreach (var header in _headers)
                {
                    http.Headers.Add(header.Key, header.Value);
                }
            }
            return req;
        }
        private readonly CookieContainer _cc;
        readonly IEnumerable<KeyValuePair<string, string>> _headers;
        public MyWebClient(CookieContainer cc, IEnumerable<KeyValuePair<string, string>> headers)
        {
            _cc = cc;
            _headers = headers;
        }
    }
    class MessageText : IMessageText
    {
        public string Text { get; }
        public MessageText(string text)
        {
            Text = text;
        }
        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (obj is MessageText text)
            {
                return this.Text.Equals(text.Text);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Text.GetHashCode();
        }
    }
    public class MessageImage : IMessageImage
    {
        public int? Width { get; set; }

        public int? Height { get; set; }

        public string Url { get; set; }

        public string Alt { get; set; }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (obj is MessageImage image)
            {
                return this.Url.Equals(image.Url) && this.Alt.Equals(image.Alt);
            }
            return false;
        }
        public override int GetHashCode()
        {
            return Url.GetHashCode() ^ Alt.GetHashCode();
        }
    }
}
