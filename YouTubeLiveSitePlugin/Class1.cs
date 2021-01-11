using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SitePlugin;
namespace YouTubeLiveSitePlugin
{
    public interface IYouTubeCommentProvider : ICommentProvider
    {
        event EventHandler LoggedInStateChanged;
        bool IsLoggedIn { get; }
        Task<bool> PostCommentAsync(string comment);
    }
    public interface IYouTubeCommentViewModel : ICommentViewModel
    {

    }
    public interface IYouTubeSiteContext : ISiteContext
    {

    }
    public interface IYouTubeSiteOptions
    {

    }
}
