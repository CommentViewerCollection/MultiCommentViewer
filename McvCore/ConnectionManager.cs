using System;
using System.Collections.Generic;
using Mcv.PluginV2;
using System.Linq;

namespace McvCore
{
    class ConnectionAddedEventArgs : EventArgs
    {
        public ConnectionAddedEventArgs(IConnectionStatus connSt)
        {
            ConnSt = connSt;
        }

        public IConnectionStatus ConnSt { get; }
    }
    class ConnectionRemovedEventArgs : EventArgs
    {
        public ConnectionRemovedEventArgs(ConnectionId connId)
        {
            ConnId = connId;
        }

        public ConnectionId ConnId { get; }
    }
    class ConnectionStatusChangedEventArgs : EventArgs
    {
        public ConnectionStatusChangedEventArgs(IConnectionStatusDiff connStDiff)
        {
            ConnStDiff = connStDiff;
        }

        public IConnectionStatusDiff ConnStDiff { get; }
    }
    class ConnectionManager
    {
        //List<IConnectionStatus> _connections = new List<IConnectionStatus>();
        private readonly Dictionary<ConnectionId, IConnectionStatus> _connDict = new Dictionary<ConnectionId, IConnectionStatus>();
        private readonly SitePluginManager _sitePluginManager;
        private readonly BrowserManager _browserManager;

        public event EventHandler<ConnectionAddedEventArgs>? ConnectionAdded;
        public event EventHandler<ConnectionRemovedEventArgs>? ConnectionRemoved;
        public event EventHandler<ConnectionStatusChangedEventArgs>? ConnectionStatusChanged;
        public void AddConnection(SiteId defaultSite, BrowserProfileId defaultBrowser)
        {
            var defaultName = GetDefaultName(_connDict.Values.Select(c => c.Name));
            var connId = new ConnectionId(Guid.NewGuid());
            var conn = new Connection(connId, defaultSite, defaultBrowser, _sitePluginManager, _browserManager)
            {
                Name = defaultName,
            };
            _connDict.Add(connId, conn);
            ConnectionAdded?.Invoke(this, new ConnectionAddedEventArgs(conn));
        }
        private static string GetDefaultName(IEnumerable<string> existingNames)
        {
            for (var n = 1; ; n++)
            {
                var testName = "#" + n;
                if (!existingNames.Contains(testName))
                {
                    return testName;
                }
            }
        }

        internal void ChangeConnectionStatus(IConnectionStatusDiff connStDiff)
        {
            var conn = GetConnection(connStDiff.Id);
            var result = conn.SetDiff(connStDiff);
            ConnectionStatusChanged?.Invoke(this, new ConnectionStatusChangedEventArgs(result));
        }
        private IConnectionStatus GetConnection(ConnectionId id)
        {
            return _connDict[id];
        }
        public ConnectionManager(SitePluginManager sitePluginManager, BrowserManager browserManager)
        {
            _sitePluginManager = sitePluginManager;
            _browserManager = browserManager;
        }

        internal IConnectionStatus GetConnectionStatus(ConnectionId connId)
        {
            return GetConnection(connId);
        }
        internal List<IConnectionStatus> GetConnectionStatusList()
        {
            return _connDict.Values.ToList();
        }

        internal void RemoveConnection(ConnectionId connId)
        {
            if (!_connDict.TryGetValue(connId, out var conn))
            {
                //TODO:そんなConnectionIdは存在しませんエラー
                return;
            }
            if (conn.IsConnected)
            {
                //TODO:切断処理

            }
            _connDict.Remove(connId);
            ConnectionRemoved?.Invoke(this, new ConnectionRemovedEventArgs(connId));
        }
    }

}
