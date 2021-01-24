using SitePlugin;
using System.Threading.Tasks;

namespace NicoSitePlugin
{
    internal class NicoMessageMethods : IMessageMethods
    {
        public Task AfterCommentAdded()
        {
            return Task.CompletedTask;
        }
    }
}
