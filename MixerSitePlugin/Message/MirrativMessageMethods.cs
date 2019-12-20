using Common;
using SitePlugin;
using System.Threading.Tasks;

namespace MixerSitePlugin
{
    internal class MixerMessageMethods : IMessageMethods
    {
        public Task AfterCommentAdded()
        {
            return Task.CompletedTask;
        }
    }
}
