using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MirrativSitePlugin
{
    class LiveInfo : ILiveInfo
    {
        public string Broadcastkey { get; }
        public string BcsvrKey { get; }
        public int BroadcastPort { get; }
        public long CreatedAt { get; }
        public long HeartbeatedAt { get; }
        public bool IsLive { get; }
        public bool IsPrivate { get; }
        public string LiveId { get; }
        public string Title { get; }
        public long OnlineUserNum { get; }
        public long TotalViewerNum { get; }
        public long StartedAt { get; }
        public LiveInfo(Low.LiveInfo.RootObject low)
        {
            Broadcastkey = low.BroadcastKey;
            BcsvrKey = low.BcsvrKey;
            BroadcastPort = (int)low.BroadcastPort;
            CreatedAt = low.CreatedAt;
            HeartbeatedAt = long.Parse(low.HeartbeatedAt);
            IsLive = low.IsLive == 1;
            IsPrivate = low.IsPrivate == 1;
            LiveId = low.LiveId;
            Title = low.Title;
            OnlineUserNum = low.OnlineUserNum;
            TotalViewerNum = low.TotalViewerNum;
            StartedAt = low.StartedAt;
        }
    }
}
