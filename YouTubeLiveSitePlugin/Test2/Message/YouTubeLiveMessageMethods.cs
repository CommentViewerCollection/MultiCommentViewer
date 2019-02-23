using SitePlugin;
using System.Threading.Tasks;

namespace YouTubeLiveSitePlugin.Test2
{
    internal class YouTubeLiveMessageMethods : IMessageMethods
    {
        public Task AfterCommentAdded()
        {
            return Task.CompletedTask;
        }
    }
}
