using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace MixerSitePlugin
{
    public interface IDataServer
    {
        Task<string> GetAsync(string url, Dictionary<string, string> headers);
        Task<string> GetAsync(string url, Dictionary<string, string> headers, CookieContainer cc);
        Task<string> GetWithNoThrowAsync(string url, Dictionary<string, string> headers, CookieContainer cc);
    }
}
