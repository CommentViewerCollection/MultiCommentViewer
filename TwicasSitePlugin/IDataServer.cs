using System.Threading.Tasks;
using System.Net;
using System.Collections.Generic;

namespace TwicasSitePlugin
{
    interface IDataServer
    {
        Task<string> GetAsync(string url, CookieContainer cc);
        Task<string> GetAsync(string url);
        Task<string> GetAsync(string url, string userAgent, CookieContainer cc);
        Task<string> PostAsync(string url, Dictionary<string, string> data, CookieContainer cc);
        Task<string> PostMultipartFormdataAsync(string url, Dictionary<string, string> data, CookieContainer cc);
    }
}
