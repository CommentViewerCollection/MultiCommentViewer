using System;
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
}
