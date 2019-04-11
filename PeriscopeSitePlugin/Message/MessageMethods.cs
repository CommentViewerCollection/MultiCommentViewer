using SitePlugin;
using System.Threading.Tasks;

namespace PeriscopeSitePlugin
{
    internal class MessageMethods : IMessageMethods
    {
        public Task AfterCommentAdded()
        {
            return Task.CompletedTask;
        }
    }
}
