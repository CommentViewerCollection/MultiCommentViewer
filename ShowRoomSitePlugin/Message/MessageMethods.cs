using SitePlugin;
using System.Threading.Tasks;

namespace ShowRoomSitePlugin
{
    internal class MessageMethods : IMessageMethods
    {
        public Task AfterCommentAdded()
        {
            return Task.CompletedTask;
        }
    }
}
