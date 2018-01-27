using System.Collections.Generic;
using Plugin;
namespace MultiCommentViewer.Test
{
    public class PluginLoaderTest:IPluginLoader
    {
        public IEnumerable<IPlugin> LoadPlugins()
        {
            return new List<IPlugin>
            {

            };
        }
    }

}
