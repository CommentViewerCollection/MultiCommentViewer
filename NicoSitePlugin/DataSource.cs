using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
namespace NicoSitePlugin
{
    public class DataSource : IDataSource
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

        public async Task<string> GetAsync(string url)
        {
            using (var client = new HttpClient())
            {
                var result = await client.GetStringAsync(url);
                return result;
            }
        }
    }
}
