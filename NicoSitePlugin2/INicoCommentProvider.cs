using SitePlugin;
using System.Threading.Tasks;
using System.Diagnostics;

namespace NicoSitePlugin
{
    interface INicoCommentProvider : ICommentProvider
    {
        Task PostCommentAsync(string comment, bool is184, string color, string size, string position);
    }
}
