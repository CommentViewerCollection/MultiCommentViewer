using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
namespace NicoSitePlugin.Next
{
    class DataSource : IDataSource
    {
        public async Task<string> Get(string url, CookieContainer cc)
        {
            using (var handler = new HttpClientHandler { UseCookies = true, CookieContainer = cc })
            using (var client = new HttpClient(handler))
            {
                var result = await client.GetStringAsync(url);
                return result;
            }
        }
    }
}
