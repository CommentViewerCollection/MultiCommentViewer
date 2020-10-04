using SitePlugin;
using System.Threading.Tasks;

namespace BigoSitePlugin
{
    internal class BigoMessageMethods : IMessageMethods
    {
        public Task AfterCommentAdded()
        {
            return Task.CompletedTask;
        }
    }
}
