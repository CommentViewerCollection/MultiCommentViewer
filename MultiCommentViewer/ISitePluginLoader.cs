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
        IEnumerable<(string displayName, Guid guid)> LoadSitePlugins(ICommentOptions options, ILogger logger);
        /// <summary>
        /// 終了処理
        /// 終了処理的な名前にしたい
        /// </summary>
        void Save();
        ISiteContext GetSiteContext(Guid guid);
        ICommentProvider CreateCommentProvider(Guid guid);
        Guid GetValidSiteGuid(string input);
        System.Windows.Controls.UserControl GetCommentPostPanel(Guid guid, ICommentProvider commentProvider);
    }
}
