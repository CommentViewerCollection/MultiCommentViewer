using System;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.Collections.Generic;
using SitePluginCommon;

namespace BLiveSitePlugin
{
    interface IDataSource
    {
        Task<string> GetAsync(string url);
        Task<string> GetAsync(string url, Dictionary<string, string> headers);
        Task<string> GetAsync(string url, CookieContainer cc);
        Task<byte[]> GetByteArrayAsync(string url, CookieContainer cc);
        Task<string> PostJsonAsync(string url, Dictionary<string,string> headers, string json);
    }
    class DataSource : ServerBase, IDataSource
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
        public Task<string> GetAsync(string url)
        {
            return GetAsync(url, (CookieContainer)null);
        }
        public async Task<string> GetAsync(string url, Dictionary<string,string> headers)
        {
            var result = await GetInternalAsync(new HttpOptions
            {
                Url = url,
                Headers = headers,
            });
            var str = await result.Content.ReadAsStringAsync();
            return str;
        }
        public async Task<string> PostJsonAsync(string url, Dictionary<string, string> headers, string json)
        {
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            var result = await PostInternalAsync(new HttpOptions
            {
                Url = url,
                Headers = headers,
            }, content);
            var str = await result.Content.ReadAsStringAsync();
            return str;
        }

        public async Task<byte[]> GetByteArrayAsync(string url, CookieContainer cc)
        {
            var result = await GetInternalAsync(new HttpOptions
            {
                Url = url,
                Cc = cc,
            });
            var arr = await result.Content.ReadAsByteArrayAsync();
            return arr;
        }

        public DataSource()
        {

        }
    }
}
