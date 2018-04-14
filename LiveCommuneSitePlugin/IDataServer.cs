using System.Net;
using System.Threading.Tasks;

namespace LiveCommuneSitePlugin
{
    interface IDataServer
    {
        Task<string> GetAsync(string url, CookieContainer cc);
        Task<string> GetAsync(string url);
        Task<string> GetAsync(string url, string userAgent, CookieContainer cc);
    }
}
