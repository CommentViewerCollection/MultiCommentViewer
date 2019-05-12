using SitePlugin;
using System.Threading.Tasks;

namespace WhowatchSitePlugin
{
    internal class WhowatchMessageMethods : IMessageMethods
    {
        public Task AfterCommentAdded()
        {
            return Task.CompletedTask;
        }
    }
}
