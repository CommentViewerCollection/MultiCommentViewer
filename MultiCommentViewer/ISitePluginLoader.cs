using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SitePlugin;
using Common;
using System.Windows.Threading;
namespace MultiCommentViewer
{
    public interface ISitePluginLoader
    {
        IEnumerable<ISiteContext> LoadSitePlugins(IOptions options, ILogger logger, IUserStore userStore, Dispatcher dispatcher);
    }
}
