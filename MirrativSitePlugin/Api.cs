using Mcv.PluginV2;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MirrativSitePlugin
{
    class CurrentUser
    {
        public bool IsLoggedIn => !string.IsNullOrEmpty(UserId);
        public string UserId { get; set; }
        public string Name { get; set; }
    }
    static class Api
    {
        public static async Task<CurrentUser> GetCurrentUserAsync(IDataServer server, CookieContainer cc)
        {
            var currentUser = new CurrentUser();
            var url = "https://www.mirrativ.com/";
            var res = await server.GetAsync(url, null, cc);
            var match = Regex.Match(res, "window\\.Mirrativ\\.currentUser\\s*=\\s*({[^;]+});");
            if (match.Success)
            {
                var raw = match.Groups[1].Value;
                var low = JsonConvert.DeserializeObject<Low.CurrentUser.RootObject>(raw);
                currentUser.UserId = low.UserId.ToString();
                currentUser.Name = low.Name;
            }
            return currentUser;
        }
        public static async Task<ILiveInfo> PollLiveAsync(IDataServer server, string liveId)
        {
            //https://www.mirrativ.com/api/live/live_polling?live_id=qnLD1dkSheKqylMKKD5hOA
            var url = "https://www.mirrativ.com/api/live/live_polling?live_id=" + liveId;
            var res = await server.GetAsync(url, null);
            var obj = Tools.Deserialize<Low.LiveInfo.RootObject>(res);
            return new LiveInfo(obj);
        }
        public static async Task<List<Message>> GetLiveComments(IDataServer server, string liveId)
        {
            var url = "https://www.mirrativ.com/api/live/live_comments?live_id=" + liveId;
            var res = await server.GetAsync(url, null);
            var obj = Tools.Deserialize<Low.LiveComments.RootObject>(res);
            var list = new List<Message>();
            foreach (var c in obj.Comments)
            {
                list.Add(new Message
                {
                    Comment = c.CommentComment,
                    CreatedAt = long.Parse(c.CreatedAt),
                    Id = c.Id,
                    Type = MessageType.Comment,
                    UserId = c.UserId,
                    Username = c.UserName,
                });
            }
            //時系列的に降順になっているから昇順に直す
            list.Reverse();
            return list;
        }
        public static async Task<ILiveInfo> GetLiveInfo(IDataServer server, string liveId)
        {
            if (string.IsNullOrEmpty(liveId))
            {
                throw new ArgumentNullException(nameof(liveId));
            }
            var url = "https://www.mirrativ.com/api/live/live?live_id=" + liveId;
            var res = await server.GetAsync(url, null);
            var obj = Tools.Deserialize<Low.LiveInfo.RootObject>(res);
            return new LiveInfo(obj);
        }
        public static async Task<UserProfile> GetUserProfileAsync(IDataServer server, string userId)
        {
            var url = "https://www.mirrativ.com/api/user/profile?user_id=" + userId;
            var res = await server.GetAsync(url, null);
            var obj = Tools.Deserialize<Low.UserProfile.RootObject>(res);
            return new UserProfile(obj);
        }
    }
    class UserProfile
    {
        public string Name { get; set; }
        public string OnLiveLiveId { get; set; }
        public UserProfile(Low.UserProfile.RootObject lowObject)
        {
            OnLiveLiveId = lowObject.Onlive?.LiveId;
            Name = lowObject.Name;
        }
    }
}
