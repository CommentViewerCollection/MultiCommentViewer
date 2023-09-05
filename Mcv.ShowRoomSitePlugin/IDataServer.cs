using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
namespace ShowRoomSitePlugin
{
    public interface IDataServer
    {
        //Task<string> GetAsync(string url, CookieContainer cc);
        Task<string> GetAsync(string url);
        Task<string> GetAsync(string url, Dictionary<string, string> headers, CookieContainer cc);
        //Task<string> GetAsync(string url, string userAgent, CookieContainer cc);
        //Task<string> PostAsync(string url, Dictionary<string, string> data, CookieContainer cc);
        Task<string> PostJsonAsync(string url, Dictionary<string,string> headers, string json, CookieContainer cc);
    }
}
