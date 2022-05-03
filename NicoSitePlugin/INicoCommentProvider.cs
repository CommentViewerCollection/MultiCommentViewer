using System.Threading.Tasks;
using Mcv.PluginV2;

namespace NicoSitePlugin
{
    interface INicoCommentProvider : ICommentProvider
    {
        Task PostCommentAsync(string comment, bool is184, string color, string size, string position);
    }
}
