using System;
using System.Threading.Tasks;
using System.Net;
using System.Text;
using System.Net.Http;
using System.Collections.Generic;
using SitePluginCommon;

namespace TwicasSitePlugin
{
    class TwicasServer : ServerBase, IDataServer
    {
        public async Task<string> GetAsync(string url, CookieContainer cc)
        {
            var result = await GetInternalAsync(new HttpOptions
            {
                Url = url,
                Cc = cc,
            });
            var str = await result.Content.ReadAsStringAsync();
            return str;
        }
        public async Task<string> GetAsync(string url, string userAgent, CookieContainer cc)
        {
            var result = await GetInternalAsync(new HttpOptions
            {
                Url = url,
                Cc = cc,
                UserAgent = userAgent,
            });
            var str = await result.Content.ReadAsStringAsync();
            return str;
        }
        public Task<string> GetAsync(string url)
        {
            return GetAsync(url, null);
        }
        public async Task<string> PostAsync(string url, Dictionary<string, string> data, CookieContainer cc)
        {
            var content = new FormUrlEncodedContent(data);
            var result = await PostInternalAsync(new HttpOptions
            {
                Url = url,
                Cc = cc,
            }, content);
            var str = await result.Content.ReadAsStringAsync();
            return str;
        }
        public async Task<string> PostMultipartFormdataAsync(string url, Dictionary<string, string> data, CookieContainer cc)
        {
            var content = new MultipartFormDataContent();
            foreach (var kv in data)
            {
                content.Add(new StringContent(kv.Value), kv.Key);
            }
            var result = await PostInternalAsync(new HttpOptions
            {
                Url = url,
                Cc = cc,
            }, content);
            var str = await result.Content.ReadAsStringAsync();
            return str;
        }
    }
}
