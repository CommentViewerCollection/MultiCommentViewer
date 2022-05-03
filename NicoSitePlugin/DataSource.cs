using Mcv.PluginV2;
using System.Net;
using System.Threading.Tasks;
namespace NicoSitePlugin
{
    public class DataSource : ServerBase, IDataSource
    {
        private readonly string _userAgent;

        public async Task<string> GetAsync(string url, CookieContainer cc)
        {
            var result = await GetInternalAsync(new HttpOptions
            {
                Url = url,
                Cc = cc,
                UserAgent = _userAgent,
            }, false);
            var str = await result.Content.ReadAsStringAsync();
            return str;
        }

        public Task<string> GetAsync(string url)
        {
            return GetAsync(url, null);
        }
        public DataSource(string userAgent)
        {
            _userAgent = userAgent;
        }
    }
}
