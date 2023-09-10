using Mcv.PluginV2;
using Mcv.YouTubeLiveSitePlugin;

namespace Mcv.MainViewPlugin
{
    class YouTubeMessageProcessor : ILiveSiteMessageProcessor
    {
        public IMcvCommentViewModel? CreateViewModel(ISiteMessage message, ConnectionName connName, IMainViewPluginOptions options, MyUser? user)
        {
            IMcvCommentViewModel? vm = null;
            if (message is not IYouTubeLiveMessage ytMessage)
            {
                return vm;
            }
            switch (ytMessage)
            {
                case IYouTubeLiveComment ytComment:
                    vm = new McvYouTubeLiveCommentViewModel(ytComment, connName, options, user);
                    break;
                case IYouTubeLiveSuperchat ytSuperChat:
                    vm = new McvYouTubeLiveCommentViewModel(ytSuperChat, connName, options, user);
                    break;
                case IYouTubeLivePaidSticker ytPaidSticker:
                    vm = new McvYouTubeLiveCommentViewModel(ytPaidSticker, connName, options, user);
                    break;
                case IYouTubeLiveSponsorshipsGiftPurchaseAnnouncement ytSponsorGift:
                    vm = new McvYouTubeLiveCommentViewModel(ytSponsorGift, connName, options, user);
                    break;
                case IYouTubeLiveMembership ytMembership:
                    vm = new McvYouTubeLiveCommentViewModel(ytMembership, connName, options, user);
                    break;
                case IYouTubeLiveConnected ytConnected:
                    vm = new McvYouTubeLiveCommentViewModel(ytConnected, connName, options, user);
                    break;
                case IYouTubeLiveDisconnected ytDisconnected:
                    vm = new McvYouTubeLiveCommentViewModel(ytDisconnected, connName, options, user);
                    break;
                default:
                    break;
            }
            return vm;
        }

        public bool IsValidMessage(ISiteMessage message)
        {
            return message is IYouTubeLiveMessage ytMessage;
        }
    }
}
