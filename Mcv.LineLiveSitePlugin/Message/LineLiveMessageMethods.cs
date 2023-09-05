using SitePlugin;
using System.Threading.Tasks;

namespace LineLiveSitePlugin
{
    internal class LineLiveMessageMethods : IMessageMethods
    {
        public Task AfterCommentAdded()
        {
            return Task.CompletedTask;
        }
    }
}
