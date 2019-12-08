using Common;
using SitePlugin;
using System.Threading.Tasks;

namespace MildomSitePlugin
{
    internal class MildomMessageMethods : IMessageMethods
    {
        public Task AfterCommentAdded()
        {
            return Task.CompletedTask;
        }
    }
}
