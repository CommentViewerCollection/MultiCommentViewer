using SitePlugin;
using System.Threading.Tasks;

namespace OpenrecSitePlugin
{
    internal class OpenrecMessageMethods : IMessageMethods
    {
        public Task AfterCommentAdded()
        {
            return Task.CompletedTask;
        }
    }
}
