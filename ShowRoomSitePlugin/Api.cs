using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace ShowRoomSitePlugin
{
    internal static class Api
    {
        public static async Task<LiveInfo> GetLiveInfo(IDataServer server, string room_id)
        {
            if (string.IsNullOrEmpty(room_id)) throw new ArgumentNullException(nameof(room_id));

            var url = "https://www.showroom-live.com/api/live/live_info?room_id=" + room_id;
            var res = await server.GetAsync(url);
            var obj = Tools.Deserialize<Low.LiveInfo.RootObject>(res);
            return new LiveInfo(obj);
        }
    }
    internal class LiveInfo
    {
        public string BcsvrHost { get; }
        public string BcsvrKey { get; }
        public long LiveStatus { get; }
        public string RoomName { get; }
        public LiveInfo(Low.LiveInfo.RootObject low)
        {
            BcsvrHost = low.BcsvrHost;
            BcsvrKey = low.BcsvrKey;
            LiveStatus = low.LiveStatus;
            RoomName = low.RoomName;
        }
    }
}