using System.Threading.Tasks;
using SitePlugin;

namespace NicoSitePlugin.Next
{
    interface INicoCommentProvider :ICommentProvider
    {
        Task PostCommentAsync(string comment, string mail);
    }
}
