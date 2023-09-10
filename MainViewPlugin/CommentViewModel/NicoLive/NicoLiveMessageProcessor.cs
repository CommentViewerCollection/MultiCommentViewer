using Mcv.PluginV2;
using NicoSitePlugin;

namespace Mcv.MainViewPlugin;

class NicoLiveMessageProcessor : ILiveSiteMessageProcessor
{
    public IMcvCommentViewModel? CreateViewModel(ISiteMessage message, ConnectionName connName, IMainViewPluginOptions options, MyUser? user)
    {
        IMcvCommentViewModel? vm = null;
        if (message is INicoMessage nicoMessage)
        {
            switch (nicoMessage)
            {
                case INicoComment nicoComment:
                    vm = new McvNicoCommentViewModel(nicoComment, connName, options, user);
                    break;
                case INicoInfo nicoInfo:
                    vm = new McvNicoCommentViewModel(nicoInfo, connName, options, user);
                    break;
                case INicoGift nicoGift:
                    vm = new McvNicoCommentViewModel(nicoGift, connName, options, user);
                    break;
                case INicoAd nicoAd:
                    vm = new McvNicoCommentViewModel(nicoAd, connName, options, user);
                    break;
                case INicoSpi nicoSpi:
                    vm = new McvNicoCommentViewModel(nicoSpi, connName, options, user);
                    break;
                case INicoEmotion nicoEmotion:
                    vm = new McvNicoCommentViewModel(nicoEmotion, connName, options, user);
                    break;
                case INicoConnected nicoConnected:
                    vm = new McvNicoCommentViewModel(nicoConnected, connName, options, user);
                    break;
                case INicoDisconnected nicoDisconnected:
                    vm = new McvNicoCommentViewModel(nicoDisconnected, connName, options, user);
                    break;
                default:
                    break;
            }
        }
        return vm;
    }

    public bool IsValidMessage(ISiteMessage message)
    {
        return message is INicoMessage _;
    }
}
