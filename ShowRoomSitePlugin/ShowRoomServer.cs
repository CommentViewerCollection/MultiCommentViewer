using SitePluginCommon;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
namespace ShowRoomSitePlugin
{
    public class ShowRoomServer : ServerBase, IDataServer
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
        public async Task<string> GetAsync(string url, Dictionary<string, string> headers, CookieContainer cc)
        {
            var result = await GetInternalAsync(new HttpOptions
            {
                Url = url,
                Cc = cc,
                Headers = headers,
            });
            var str = await result.Content.ReadAsStringAsync();
            return str;
        }
        public async Task<string> GetAsync(string url)
        {
            using (var client = new HttpClient())
            {
                var result = await client.GetStringAsync(url);
                return result;
            }
        }
        public async Task<string> PostJsonAsync(string url, Dictionary<string, string> headers, string json, CookieContainer cc)
        {
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            var result = await PostInternalAsync(new HttpOptions
            {
                Url = url,
                Cc = cc,
                Headers = headers,
            }, content);
            var str = await result.Content.ReadAsStringAsync();
            return str;
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
    }
}

