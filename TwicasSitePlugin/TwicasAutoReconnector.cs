using Common;
using SitePluginCommon.AutoReconnection;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace TwicasSitePlugin
{
    /// <summary>
    /// Twicasへの自動再接続装置
    /// Twicasは特殊で、配信外でもチャットが可能。配信外ではLiveIdが無くなるが、次回の配信が始まるまではwebsocketのURLは直近の配信のURLを使用する。
    /// 配信外ではStreamCheckerにはLiveIdの記載が無い。配信が始まるとLiveIdが記載されるため、そのタイミングでコメント取得のwebsocketを切断して新しいURLを取得し直す。
    /// </summary>
    class TwicasAutoReconnector
    {
        private readonly string _broadcasterId;
        private readonly CookieContainer _cc;
        private readonly IDataServer _server;
        private readonly ILogger _logger;
        private readonly WebsocketMessageProvider _p1;
        private readonly MetadataProvider _p2;

        public async Task AutoReconnectAsync()
        {
            _isDisconnectCalled = false;
            while (true)
            {
                var (context, contextRaw) = await API.GetLiveContext(_server, _broadcasterId, _cc);
                var liveId = context.MovieId;
                if (liveId < 0)
                {
                    //過去に一度も配信歴が無い

                    return;
                }
                _p1.Cc = _cc;
                _p1.BroadcasterId = _broadcasterId;
                _p1.LiveId = _currentLiveId ?? liveId;
                _p1.Master = _p2;
                //var p = new WebsocketMessageProvider(new Common.Websocket());
                //p.MessageReceived += (sender, e) => { Debug.WriteLine(e.Raw); };

                //var p2 = new MetadataProvider(_logger, _server, _broadcasterId);

                var reason = await _connectionManager.ConnectAsync(new List<IProvider> { _p1, _p2 });
                if (_newLiveStarted)
                {
                    _newLiveStarted = false;
                    continue;
                }
                else if (_isDisconnectCalled)
                {
                    break;
                }
            }
        }
        bool _isDisconnectCalled;
        ConnectionManager _connectionManager;
        public void Disconnect()
        {
            _isDisconnectCalled = true;
            _connectionManager?.Disconnect();
        }
        public TwicasAutoReconnector(string broadcasterId, CookieContainer cc, IDataServer server, ILogger logger, WebsocketMessageProvider p1, MetadataProvider p2)
        {
            _broadcasterId = broadcasterId;
            _cc = cc;
            _server = server;
            _logger = logger;
            _connectionManager = new ConnectionManager(logger);
            _p1 = p1;
            _p2 = p2;
            p2.LiveIdReceived += P2_LiveIdReceived;
        }
        long? _currentLiveId;
        bool _newLiveStarted;
        private void P2_LiveIdReceived(object sender, long? e)
        {
            var newLiveId = e;
            if (!_currentLiveId.HasValue)
            {
                _currentLiveId = newLiveId;
            }
            else if (!newLiveId.HasValue)
            {
                //配信と配信の間はnewLiveIdがnullになる。
                //この時は前回の配信IDでコメントが取れる。
            }
            else if (_currentLiveId.Value == newLiveId)
            {
                //配信中。特にやること無し。
            }
            else
            {
                //新しい放送が始まった。websocketのURLが変更になるから再接続する。
                _currentLiveId = newLiveId;
                _newLiveStarted = true;
                System.Diagnostics.Debug.WriteLine($"newLiveId={newLiveId}");
                _connectionManager.Disconnect();
            }
        }
    }
}
