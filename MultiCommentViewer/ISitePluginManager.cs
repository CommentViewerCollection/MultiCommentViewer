using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SitePlugin;
namespace MultiCommentViewer
{
    public interface ISitePluginManager
    {
        void LoadSitePlugins(IOptions options);

    }
}
