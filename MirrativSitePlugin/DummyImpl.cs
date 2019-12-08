using Common;
using System.Collections.Generic;
using System.Threading.Tasks;
using SitePluginCommon.AutoReconnection;

namespace MirrativSitePlugin
{
    class DummyImpl : IDummy
    {
        private readonly IDataServer _server;
        private readonly string _input;
        private readonly ILogger _logger;
        private readonly IMirrativSiteOptions _siteOptions;
        private readonly MessageProvider2 _p1;
        private readonly MetadataProvider2 _p2;

        public async Task<bool> CanConnectAsync()
        {
            var input = _input;
            if (Tools.IsValidUserId(input))
            {
                return true;
            }
            else if (Tools.IsValidLiveId(input))
            {
                var liveId = Tools.ExtractLiveId(input);
                var liveInfo = await Api.GetLiveInfo(_server, liveId);
                return liveInfo.IsLive;
            }
            else
            {
                return false;
            }
        }

        public async Task<IEnumerable<IProvider>> GenerateGroupAsync()
        {
            var input = _input;
            string liveId;
            if (Tools.IsValidUserId(input))
            {
                var userId = Tools.ExtractUserId(input);
                liveId = await GetLiveIdAsync(userId);//TODO:
                                                      //GetLiveIdAsync()を実行中にユーザがDisconnect()するとliveIdがnullになる
                if (string.IsNullOrEmpty(liveId))
                {
                    //エラーメッセージ
                    return new List<IProvider>();
                }
            }
            else if (Tools.IsValidLiveId(input))
            {
                liveId = Tools.ExtractLiveId(input);
            }
            else
            {
                //エラーメッセージ
                return new List<IProvider>();
            }
            var liveInfo = await Api.GetLiveInfo(_server, liveId);
            var broadcastKey = liveInfo.BcsvrKey;
            //var p1 = new MessageProvider2(new WebSocket("wss://online.mirrativ.com/"), _logger);
            //p1.MessageReceived += P1_MessageReceived;
            //p1.MetadataUpdated += P1_MetadataUpdated;
            var p1 = _p1;
            p1.BroadcastKey = broadcastKey;
            //var p2 = new MetadataProvider2(_server, _siteOptions);
            //p2.MetadataUpdated += P2_MetadataUpdated;
            var p2 = _p2;
            p2.LiveId = liveId;
            return new List<IProvider>
            {
                p1,
                p2,
            };
        }

        private async Task<string> GetLiveIdAsync(string userId)
        {
            var userProfile = await Api.GetUserProfileAsync(_server, userId);
            if (!string.IsNullOrEmpty(userProfile.OnLiveLiveId))
            {
                return userProfile.OnLiveLiveId;
            }
            else
            {
                return null;
            }
        }
        public DummyImpl(IDataServer server, string input, ILogger logger, IMirrativSiteOptions siteOptions, MessageProvider2 p1, MetadataProvider2 p2)
        {
            _server = server;
            _input = input;
            _logger = logger;
            _siteOptions = siteOptions;
            _p1 = p1;
            _p2 = p2;
        }
    }
}
