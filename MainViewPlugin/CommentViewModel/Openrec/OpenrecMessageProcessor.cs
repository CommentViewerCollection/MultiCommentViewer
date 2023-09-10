using Mcv.PluginV2;
using OpenrecSitePlugin;

namespace Mcv.MainViewPlugin;

class OpenrecMessageProcessor : ILiveSiteMessageProcessor
{
    public IMcvCommentViewModel? CreateViewModel(ISiteMessage message, ConnectionName connName, IMainViewPluginOptions options, MyUser? user)
    {
        IMcvCommentViewModel? vm = null;
        if (message is IOpenrecMessage opMessage)
        {
            switch (opMessage)
            {
                default:
                    break;
            }
        }
        return vm;
    }
    public bool IsValidMessage(ISiteMessage message)
    {
        return message is IOpenrecMessage _;
    }
}
