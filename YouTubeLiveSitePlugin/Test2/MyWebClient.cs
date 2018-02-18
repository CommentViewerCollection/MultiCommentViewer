using System;
using System.Net;

namespace YouTubeLiveSitePlugin.Test2
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
}
