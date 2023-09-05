using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Mcv.PluginV2;
using Mcv.PluginV2.AutoReconnection;

namespace MildomSitePlugin
{
    class DummyImpl : IDummy
    {
        private readonly IDataServer _server;
        private readonly string _input;
        private readonly List<Cookie> _cookies;
        private readonly ILogger _logger;
        private readonly IMildomSiteOptions _siteOptions;
        private readonly MessageProvider _p1;
        //private readonly MetadataProvider2 _p2;

        public async Task<bool> CanConnectAsync()
        {


            return await Task.FromResult(true);
        }

        public async Task<IEnumerable<IProvider>> GenerateGroupAsync()
        {
            var myUserInfo = Tools.GetUserInfoFromCookie(_cookies);
            var roomId = Tools.ExtractRoomId(_input);
            _p1.MyInfo = myUserInfo;
            _p1.RoomId = roomId.Value.ToString();

            await Task.Yield();
            return new List<IProvider>
            {
                _p1,
            };
        }

        public DummyImpl(IDataServer server, string input, List<Cookie> cookies, ILogger logger, IMildomSiteOptions siteOptions, MessageProvider p1)
        {
            _server = server;
            _input = input;
            _cookies = cookies;
            _logger = logger;
            _siteOptions = siteOptions;
            _p1 = p1;
            //_p2 = p2;
        }
    }
}
