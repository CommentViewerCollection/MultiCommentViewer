using System;
using System.Threading.Tasks;
using System.Net;
using System.Text;
using System.Net.Http;
using System.Collections.Generic;

namespace TwicasSitePlugin
{
    class TwicasServer : IDataServer
    {
        public async Task<string> GetAsync(string url, CookieContainer cc)
        {
            using (var handler = new HttpClientHandler { UseCookies = true, CookieContainer = cc })
            using (var client = new HttpClient(handler))
            {
                var result = await client.GetStringAsync(url);
                return result;
            }
        }
        public async Task<string> GetAsync(string url, string userAgent, CookieContainer cc)
        {
            using (var handler = new HttpClientHandler { UseCookies = true, CookieContainer = cc })
            using (var client = new HttpClient(handler))
            {
                client.DefaultRequestHeaders.Add("User-Agent", userAgent);
                var result = await client.GetStringAsync(url);
                return result;
            }
        }
        public async Task<string> GetAsync(string url)
        {
            using(var client = new HttpClient())
            {
                var result = await client.GetStringAsync(url);
                return result;
            }
        }
        public async Task<string> PostAsync(string url, Dictionary<string,string> data, CookieContainer cc)
        {
            var content = new FormUrlEncodedContent(data);
            using (var handler = new HttpClientHandler { UseCookies = true, CookieContainer = cc })
            using (var client = new HttpClient(handler))
            {
                var result = await client.PostAsync(url, content);
                var resBody = await result.Content.ReadAsStringAsync();
                return resBody;
            }
        }
    }
}
