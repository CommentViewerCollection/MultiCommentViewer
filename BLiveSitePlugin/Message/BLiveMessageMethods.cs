using SitePlugin;
using System.Threading.Tasks;

namespace BLiveSitePlugin
{
    internal class BLiveMessageMethods : IMessageMethods
    {
        public Task AfterCommentAdded()
        {
            return Task.CompletedTask;
        }
    }
}
