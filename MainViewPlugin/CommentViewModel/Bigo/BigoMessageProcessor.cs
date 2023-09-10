using BigoSitePlugin;
using Mcv.PluginV2;

namespace Mcv.MainViewPlugin
{
    class BigoMessageProcessor : ILiveSiteMessageProcessor
    {
        public IMcvCommentViewModel? CreateViewModel(ISiteMessage message, ConnectionName connName, IMainViewPluginOptions options, MyUser? user)
        {
            IMcvCommentViewModel? vm = null;
            if (message is IBigoMessage bigoMessage)
            {
                switch (bigoMessage)
                {
                    case IBigoComment bigoComment:
                        vm = new McvBigoCommentViewModel(bigoComment, connName, options, user);
                        break;
                    case IBigoGift bigoGift:
                        vm = new McvBigoGiftViewModel(bigoGift, connName, options, user);
                        break;
                    default:
                        break;
                }
            }
            return vm;
        }

        public bool IsValidMessage(ISiteMessage message)
        {
            return message is IBigoMessage _;
        }
    }
}
