using Mcv.PluginV2;
using System;
using System.Threading.Tasks;

namespace Mcv.YouTubeLiveSitePlugin
{
    public interface IYouTubeCommentProvider : ICommentProvider
    {
        event EventHandler LoggedInStateChanged;
        bool IsLoggedIn { get; }
        Task<bool> PostCommentAsync(string comment);
    }
}
