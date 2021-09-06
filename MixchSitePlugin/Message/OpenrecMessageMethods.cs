using SitePlugin;
using System.Threading.Tasks;

namespace MixchSitePlugin
{
    internal class MixchMessageMethods : IMessageMethods
    {
        public Task AfterCommentAdded()
        {
            return Task.CompletedTask;
        }
    }
}
