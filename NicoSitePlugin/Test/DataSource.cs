using System.Net;
using System.Threading.Tasks;
namespace NicoSitePlugin.Test
{
    public class DataSource : IDataSource
    {
        public async Task<string> Get(string url, CookieContainer cc)
        {
            var wc = new MyWebClient(cc);
            //return wc.DownloadStringTaskAsync(url);
            var bytes = await wc.DownloadDataTaskAsync(url);
            return System.Text.Encoding.UTF8.GetString(bytes, 0, bytes.Length);
        }
    }
}
