using Mcv.PluginV2;
using MildomSitePlugin;

namespace Mcv.MainViewPlugin;

class MildomMessageProcessor : ILiveSiteMessageProcessor
{
    public IMcvCommentViewModel? CreateViewModel(ISiteMessage message, ConnectionName connName, IMainViewPluginOptions options, MyUser? user)
    {
        IMcvCommentViewModel? vm = null;
        if (message is IMildomMessage milMessage)
        {
            switch (milMessage)
            {
                case IMildomComment milComment:
                    vm = new McvMildomCommentViewModel(milComment, connName, options, user);
                    break;
                case IMildomJoinRoom milJoin:
                    vm = new McvMildomCommentViewModel(milJoin, connName, options, user);
                    break;
                default:
                    break;
            }
        }
        return vm;
    }

    public bool IsValidMessage(ISiteMessage message)
    {
        return message is IMildomMessage _;
    }
}
