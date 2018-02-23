using System;
using Plugin;
using SitePlugin;
namespace YouTubeLiveCommentViewer
{
    public interface IPluginManager
    {
        event EventHandler<IPlugin> PluginAdded;
        void LoadPlugins(IPluginHost host);
        void SetComments(ICommentViewModel comments);
        void OnLoaded();
        void OnClosing();
        //event EventHandler<string> PostingCommentReceived;
    }
}
