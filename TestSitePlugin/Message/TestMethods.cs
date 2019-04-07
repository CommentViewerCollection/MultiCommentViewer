using System.Threading.Tasks;
using SitePlugin;

namespace TestSitePlugin
{
    internal class TestMethods:IMessageMethods
    {
        public Task AfterCommentAdded()
        {
            return Task.CompletedTask;
        }
    }
}