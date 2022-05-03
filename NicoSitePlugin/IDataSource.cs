using System.Net;
using System.Threading.Tasks;

namespace NicoSitePlugin
{
    public interface IDataSource
    {
        Task<string> GetAsync(string url, CookieContainer cc);
        Task<string> GetAsync(string url);
    }
}
