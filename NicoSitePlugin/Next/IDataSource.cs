using System.Net;
using System.Threading.Tasks;

namespace NicoSitePlugin.Next
{
    interface IDataSource
    {
        Task<string> Get(string url, CookieContainer cc);
    }
}
