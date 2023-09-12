using LineLiveSitePlugin;
using Mcv.PluginV2;

namespace Mcv.MainViewPlugin;

class LineLiveMessageProcessor : ILiveSiteMessageProcessor
{
    public IMcvCommentViewModel? CreateViewModel(ISiteMessage message, ConnectionName connName, IMainViewPluginOptions options, MyUser? user)
    {
        IMcvCommentViewModel? vm = null;
        if (message is ILineLiveMessage llMessage)
        {
            switch (llMessage)
            {
                case ILineLiveComment llComment:
                    vm = new LineLiveCommentViewModel(llComment, connName, options, user);
                    break;
                default:
                    break;
            }
        }
        return vm;
    }
    public bool IsValidMessage(ISiteMessage message)
    {
        return message is ILineLiveMessage _;
    }
}
