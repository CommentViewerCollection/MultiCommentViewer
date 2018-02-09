using ryu_s.BrowserCookie;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows;
using Newtonsoft.Json;
using System.Diagnostics;
using System.IO;
using TwitchSitePlugin;
namespace TwitchTestClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            var manager = new ChromeManager();
            var profiles = manager.GetProfiles();
            var cookies = profiles.First().GetCookieCollection("twitch.tv");
            var cc = new CookieContainer();
            cc.Add(cookies);
            var cookieList = Tools.ExtractCookies(cc);
            string apiToken = null;
            foreach (var c in cookieList)
            {
                if(c.Name == "api_token")
                {
                    apiToken = c.Value;
                }
            }
            if(apiToken == null)
            {
                return;
            }
            var headers = new[]
            {
                new KeyValuePair<string,string>("client-id","jzkbprff40iqj646a697cyrvl0zt2m6"),//固定値
                new KeyValuePair<string, string>("twitch-api-token",apiToken),
            };
            var wc = new MyWebClient(cc, headers);
            try
            {
                var data = await wc.DownloadDataTaskAsync("https://api.twitch.tv/v5/users/115620888/emotes?on_site=1");
                var s = Encoding.UTF8.GetString(data);
                var emots = JsonConvert.DeserializeObject<TwitchSitePlugin.LowObject.Emoticons>(s);

                using (var sw = new System.IO.StreamWriter("me.txt", true))
                {
                    sw.WriteLine(s);
                }
            }
            catch (WebException ex)
            {
                if(ex.Response is HttpWebResponse res)
                {
                    var ms = new MemoryStream();
                    var stream = res.GetResponseStream();
                    stream.CopyTo(ms);
                    var  reason = Encoding.UTF8.GetString(ms.ToArray());
                    Debug.WriteLine(reason);
                }
            }
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
}
