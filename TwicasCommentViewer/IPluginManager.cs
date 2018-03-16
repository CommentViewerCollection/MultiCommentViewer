using System;
using Plugin;
using SitePlugin;
namespace TwicasCommentViewer
{
    public interface IPluginManager
    {
        event EventHandler<IPlugin> PluginAdded;
        void LoadPlugins(IPluginHost host);
        void SetComments(ICommentViewModel comments);
        void OnLoaded();
        void OnClosing();
        void ForeachPlugin(Action<IPlugin> p);
        //event EventHandler<string> PostingCommentReceived;
    }
}
