using System;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.Collections.Generic;
namespace OpenrecSitePlugin
{
    interface IDataSource
    {
        Task<string> GetAsync(string url);
        Task<string> GetAsync(string url, Dictionary<string, string> headers);
        Task<string> GetAsync(string url, CookieContainer cc);
        Task<byte[]> GetByteArrayAsync(string url, CookieContainer cc);
        Task<string> PostJsonAsync(string url, Dictionary<string,string> headers, string json);
    }
    class DataSource : IDataSource
    {
        public async Task<string> GetAsync(string url, CookieContainer cc)
        {
            if (cc == null) return await GetAsync(url);
            using (var handler = new System.Net.Http.HttpClientHandler { UseCookies = true, CookieContainer = cc })
            using (var client = new System.Net.Http.HttpClient(handler))
            {
                var result = await client.GetStringAsync(url);
                return result;
            }
        }
        public async Task<string> GetAsync(string url)
        {
            using (var handler = new System.Net.Http.HttpClientHandler { UseCookies = false })
            using (var client = new System.Net.Http.HttpClient(handler))
            {
                var result = await client.GetStringAsync(url);
                return result;
            }
        }
        public async Task<string> GetAsync(string url, Dictionary<string,string> headers)
        {
            using (var handler = new System.Net.Http.HttpClientHandler { UseCookies = false })
            using (var client = new System.Net.Http.HttpClient(handler))
            {
                foreach (var kv in headers)
                {
                    client.DefaultRequestHeaders.Add(kv.Key, kv.Value);
                }
                var result = await client.GetStringAsync(url);
                return result;
            }
        }
        public async Task<string> PostJsonAsync(string url, Dictionary<string, string> headers, string json)
        {
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            using (var handler = new System.Net.Http.HttpClientHandler { UseCookies = false })
            using (var client = new System.Net.Http.HttpClient(handler))
            {
                foreach(var kv in headers)
                {
                    client.DefaultRequestHeaders.Add(kv.Key, kv.Value);
                }
                var res = await client.PostAsync(url, content);
                return await res.Content.ReadAsStringAsync();
            }
        }

        public async Task<byte[]> GetByteArrayAsync(string url, CookieContainer cc)
        {
            using (var handler = new System.Net.Http.HttpClientHandler { UseCookies = true, CookieContainer = cc })
            using (var client = new System.Net.Http.HttpClient(handler))
            {
                var result = await client.GetByteArrayAsync(url);
                return result;
            }
        }

        public DataSource()
        {

        }
    }
}
