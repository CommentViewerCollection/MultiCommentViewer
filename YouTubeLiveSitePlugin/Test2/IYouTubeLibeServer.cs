using System.Threading.Tasks;
using System.Net;

namespace YouTubeLiveSitePlugin.Test2
{
    internal interface IYouTubeLibeServer
    {
        Task<string> GetAsync(string url);
        Task<string> GetEnAsync(string url);
        Task<string> PostAsync(string url, string data, CookieContainer cc);
    }
}
