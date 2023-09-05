using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace WhowatchSitePlugin
{
    public interface IDataServer
    {
        Task<string> GetAsync(string url, CookieContainer cc);
        Task<string> GetAsync(string url);
        Task<string> PostAsync(string url, Dictionary<string, string> headers, Dictionary<string, string> data, CookieContainer cc);
    }
}
