using Mcv.PluginV2;
using ShowRoomSitePlugin;

namespace Mcv.MainViewPlugin;

class ShowRoomMessageProcessor : ILiveSiteMessageProcessor
{
    public IMcvCommentViewModel? CreateViewModel(ISiteMessage message, ConnectionName connName, IMainViewPluginOptions options, MyUser? user)
    {
        IMcvCommentViewModel? vm = null;
        if (message is IShowRoomMessage showMessage)
        {
            switch (showMessage)
            {
                default:
                    break;
            }
        }
        return vm;
    }
    public bool IsValidMessage(ISiteMessage message)
    {
        return message is IShowRoomMessage _;
    }
}
