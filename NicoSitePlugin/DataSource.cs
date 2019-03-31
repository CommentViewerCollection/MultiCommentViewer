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
            });
            var str = await result.Content.ReadAsStringAsync();
            return str;
        }

        public Task<string> GetAsync(string url)
        {
            return GetAsync(url, null);
        }
    }
}
