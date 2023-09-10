using Mcv.PluginV2;
using WhowatchSitePlugin;

namespace Mcv.MainViewPlugin;

class WhowatchMessageProcessor : ILiveSiteMessageProcessor
{
    public IMcvCommentViewModel? CreateViewModel(ISiteMessage message, ConnectionName connName, IMainViewPluginOptions options, MyUser? user)
    {
        IMcvCommentViewModel? vm = null;
        if (message is IWhowatchMessage wwMessage)
        {
            switch (wwMessage)
            {
                case IWhowatchComment wwComment:
                    vm = new McvWhowatchCommentViewModel(wwComment, connName, options, user);
                    break;
                default:
                    break;
            }
        }
        return vm;
    }
    public bool IsValidMessage(ISiteMessage message)
    {
        return message is IWhowatchMessage _;
    }
}