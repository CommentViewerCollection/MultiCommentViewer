using Newtonsoft.Json;
using ShowRoomSitePlugin;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace ShowRoom.Api
{
    internal class CommentLog
    {
        public List<T1> Comments { get; private set; }
        public static async Task<CommentLog> GetCommentLogAsync(IDataServer server, string roomId)
        {
            var url = $"https://www.showroom-live.com/api/live/comment_log?room_id={roomId}";
            var res = await server.GetAsync(url);
            dynamic d = JsonConvert.DeserializeObject(res);
            var list = new List<T1>();
            foreach (var message in d.comment_log)
            {
                var name = (string)message.name;
                var createdAt = (long)message.created_at;
                var comment = (string)message.comment;
                var userId = (long)message.user_id;
                var t1 = new T1()
                {
                    UserName = name,
                    Comment = comment,
                    CreatedAt = createdAt,
                    UserId = userId,
                };
                list.Add(t1);
            }
            return new CommentLog
            {
                Comments = list,
            };
        }
    }
    internal class Status
    {
        public string RoomName { get; private set; }
        public bool IsLive { get; private set; }
        public string RoomId { get; private set; }
        public long? StartedAt { get; private set; }
        public long LiveId { get; private set; }
        public string BroadcastKey { get; private set; }
        public string BroadcastHost { get; private set; }
        public static async Task<Status> GetStatusAsync(IDataServer server, string roomUrlKey)
        {
            //https://www.showroom-live.com/api/room/status?room_url_key=227a44548082
            var url = $"https://www.showroom-live.com/api/room/status?room_url_key={roomUrlKey}";
            var res = await server.GetAsync(url);
            dynamic d = JsonConvert.DeserializeObject(res);
            var status = Parse(d);
            return status;
        }
        private static Status Parse(dynamic d)
        {
            var isLive = (bool)d.is_live;
            var roomName = (string)d.room_name;
            var roomId = (string)d.room_id;
            var startedAt = (long?)d.started_at;
            var liveId = (long)d.live_id;
            var broadcastKey = (string)d.broadcast_key;
            var broadcastHost = (string)d.broadcast_host;
            return new Status
            {
                IsLive = isLive,
                LiveId = liveId,
                RoomId = roomId,
                RoomName = roomName,
                StartedAt = startedAt,
                BroadcastKey = broadcastKey,
                BroadcastHost = broadcastHost,
            };
        }
    }
}
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