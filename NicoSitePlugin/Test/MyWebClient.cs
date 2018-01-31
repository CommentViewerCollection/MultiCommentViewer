using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
namespace NicoSitePlugin.Test
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
