using Common;
using SitePlugin;
using System.Threading.Tasks;

namespace MirrativSitePlugin
{
    internal class MirrativMessageMethods : IMessageMethods
    {
        public Task AfterCommentAdded()
        {
            return Task.CompletedTask;
        }
    }
}
