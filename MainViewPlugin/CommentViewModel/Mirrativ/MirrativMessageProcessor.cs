using Mcv.PluginV2;
using MirrativSitePlugin;

namespace Mcv.MainViewPlugin;

class MirrativMessageProcessor : ILiveSiteMessageProcessor
{
    public IMcvCommentViewModel? CreateViewModel(ISiteMessage message, ConnectionName connName, IMainViewPluginOptions options, MyUser? user)
    {
        IMcvCommentViewModel? vm = null;
        if (message is IMirrativMessage mirMeesage)
        {
            switch (mirMeesage)
            {
                case IMirrativComment mirComment:
                    vm = new McvMirrativCommentViewModel(mirComment, connName, options, user);
                    break;
                case IMirrativJoinRoom mirJoin:
                    vm = new McvMirrativCommentViewModel(mirJoin, connName, options, user);
                    break;
                case IMirrativConnected mirConnected:
                    vm = new McvMirrativCommentViewModel(mirConnected, connName, options, user);
                    break;
                case IMirrativDisconnected mirDisconnected:
                    vm = new McvMirrativCommentViewModel(mirDisconnected, connName, options, user);
                    break;
                default:
                    break;
            }
        }
        return vm;
    }

    public bool IsValidMessage(ISiteMessage message)
    {
        return message is IMirrativMessage _;
    }
}
