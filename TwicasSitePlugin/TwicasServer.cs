using System;
using System.Threading.Tasks;
using System.Net;
using System.Text;

namespace TwicasSitePlugin
{
    class MyWebClient : System.Net.WebClient
    {
        protected override WebRequest GetWebRequest(Uri address)
        {
            var request = base.GetWebRequest(address);
            if (request is HttpWebRequest http)
            {
                http.CookieContainer = _cc;
            }
            return request;
        }
        private readonly CookieContainer _cc;
        public MyWebClient(CookieContainer cc)
        {
            _cc = cc;
        }
    }
    class TwicasServer : IDataServer
    {
        public async Task<string> GetAsync(string url, CookieContainer cc)
        {
            var wc = new MyWebClient(cc);
            var data = await wc.DownloadDataTaskAsync(url);
            return Encoding.UTF8.GetString(data);
        }

        public Task<string> GetAsync(string url)
        {
            return GetAsync(url, null);
        }
    }
}
