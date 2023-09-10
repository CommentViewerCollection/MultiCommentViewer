using Mcv.PluginV2;
using TwicasSitePlugin;

namespace Mcv.MainViewPlugin;

class TwicasMessageProcessor : ILiveSiteMessageProcessor
{
    public IMcvCommentViewModel? CreateViewModel(ISiteMessage message, ConnectionName connName, IMainViewPluginOptions options, MyUser? user)
    {
        IMcvCommentViewModel? vm = null;
        if (message is ITwicasMessage twMessage)
        {
            switch (twMessage)
            {
                case ITwicasComment twComment:
                    vm = new TwicasCommentViewModel(twComment, connName, options, user);
                    break;
                default:
                    break;
            }
        }
        return vm;
    }
    public bool IsValidMessage(ISiteMessage message)
    {
        return message is ITwicasMessage _;
    }
}
