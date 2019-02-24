using SitePlugin;
using System.Threading.Tasks;

namespace TwitchSitePlugin
{
    internal class TwitchMessageMethods : IMessageMethods
    {
        public Task AfterCommentAdded()
        {
            return Task.CompletedTask;
        }
    }
}
