using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace SitePlugin
{
    public interface ISiteContext
    {
        /// <summary>
        /// 
        /// </summary>
        Guid Guid { get; }
        string DisplayName { get; }
        IOptionsTabPage TabPanel { get; }
        string SaveOptions();
        void LoadOptions(string siteOptionsStr);
        ICommentProvider CreateCommentProvider(ConnectionName connectionName);
    }
}
