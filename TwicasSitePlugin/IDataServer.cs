using System.Threading.Tasks;
using System.Net;

namespace TwicasSitePlugin
{
    interface IDataServer
    {
        Task<string> GetAsync(string url, CookieContainer cc);
        Task<string> GetAsync(string url);
        Task<string> GetAsync(string url, string userAgent, CookieContainer cc);
    }
}
