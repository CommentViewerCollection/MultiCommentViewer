using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace MirrativSitePlugin
{
    public interface IDataServer
    {
        Task<string> GetAsync(string url, Dictionary<string, string> headers);
        Task<string> GetAsync(string url, Dictionary<string, string> headers, CookieContainer cc);
    }
}
