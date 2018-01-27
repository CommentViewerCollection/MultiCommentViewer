using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Plugin;
using SitePlugin;
namespace MultiCommentViewer
{
    public interface IPluginLoader
    {
        IEnumerable<IPlugin> LoadPlugins();
    }
    public interface IPluginManager
    {
        Task LoadPlugins();
        Task SetComments(List<ICommentViewModel> comments);

        event EventHandler<string> postingCommentReceived;
    }
}
