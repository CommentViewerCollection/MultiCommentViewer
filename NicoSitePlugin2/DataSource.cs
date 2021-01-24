using SitePluginCommon;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
namespace NicoSitePlugin
{
    public class DataSource : ServerBase, IDataSource
    {
        public async Task<string> GetAsync(string url, CookieContainer cc)
        {
            var result = await GetInternalAsync(new HttpOptions
            {
                Url = url,
                Cc = cc,
                UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/87.0.4280.88 Safari/537.36",
            }, false);
            var str = await result.Content.ReadAsStringAsync();
            return str;
        }

        public Task<string> GetAsync(string url)
        {
            return GetAsync(url, null);
        }
    }
}
