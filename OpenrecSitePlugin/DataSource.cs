using System;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
namespace OpenrecSitePlugin
{
    interface IDataSource
    {
        Task<string> GetAsync(string url);
        Task<string> GetAsync(string url, CookieContainer cc);
        Task<byte[]> GetByteArrayAsync(string url, CookieContainer cc);
    }
    class DataSource : IDataSource
    {
        public async Task<string> GetAsync(string url, CookieContainer cc)
        {
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
        public async Task<string> PostAsync(string url)
        {
            throw new NotImplementedException();
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
