using SitePlugin;
using System.Threading.Tasks;

namespace TwicasSitePlugin
{
    internal class TwicasMessageMethods : IMessageMethods
    {
        public Task AfterCommentAdded()
        {
            return Task.CompletedTask;
        }
    }
}
