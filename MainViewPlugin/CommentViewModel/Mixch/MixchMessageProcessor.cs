using Mcv.PluginV2;
using MixchSitePlugin;

namespace Mcv.MainViewPlugin;

class MixchMessageProcessor : ILiveSiteMessageProcessor
{
    public IMcvCommentViewModel? CreateViewModel(ISiteMessage message, ConnectionName connName, IMainViewPluginOptions options, MyUser? user)
    {
        IMcvCommentViewModel? vm = null;
        if (message is IMixchMessage mixMessage)
        {
            switch (mixMessage)
            {
                default:
                    break;
            }
        }
        return vm;
    }
    public bool IsValidMessage(ISiteMessage message)
    {
        return message is IMixchMessage _;
    }
}
