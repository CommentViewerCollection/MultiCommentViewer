namespace Mcv.PluginV2.AutoReconnection
{
    public class NewAutoReconnector
    {
        private readonly ConnectionManager _connectionManager;
        private readonly IDummy _dummy;
        private readonly MessageUntara _messageUntara;
        private readonly ILogger _logger;
        public async Task AutoReconnectAsync()
        {
            _isDisconnectedByUser = false;
            while (true)
            {
                var group = await _dummy.GenerateGroupAsync();
                try
                {
                    var reason = await _connectionManager.ConnectAsync(group);
                }
                catch (Exception ex)
                {
                    _logger.LogException(ex);
                    //_messageUntara.Set()
                }
                if (_isDisconnectedByUser)
                {
                    break;
                }
                if (!await _dummy.CanConnectAsync())
                {
                    //放送IDを入力してその配信が終了した
                    //fatal error
                    break;
                }
            }
        }
        bool _isDisconnectedByUser;
        public void Disconnect()
        {
            _isDisconnectedByUser = true;
            _connectionManager.Disconnect();
        }
        public NewAutoReconnector(ConnectionManager connectionManager, IDummy dummy, MessageUntara messageUntara, ILogger logger)
        {
            _connectionManager = connectionManager;
            _dummy = dummy;
            _messageUntara = messageUntara;
            _logger = logger;
        }
    }
}
