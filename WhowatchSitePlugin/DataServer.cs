using SitePluginCommon;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace WhowatchSitePlugin
{
    internal class DataServer : ServerBase, IDataServer
    {
        public Task<string> GetAsync(string url)
        {
            return GetAsync(url, null);
        }
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
        public async Task<string> PostAsync(string url, Dictionary<string, string> headers, Dictionary<string, string> data, CookieContainer cc)
        {
            var content = new FormUrlEncodedContent(data);
            var result = await PostInternalAsync(new HttpOptions
            {
                Url = url,
                Cc = cc,
                Headers = headers,
            }, content);
            var str = await result.Content.ReadAsStringAsync();
            return str;
        }
    }
}
