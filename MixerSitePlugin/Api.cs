using Newtonsoft.Json;
using SitePlugin;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MixerSitePlugin
{
    static class Api
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="server"></param>
        /// <param name="cc"></param>
        /// <returns></returns>
        /// <exception cref="ParseException"></exception>
        public static async Task<UserInfoBase> GetCurrentUserInfo(IDataServer server, CookieContainer cc)
        {
            var url = "https://mixer.com/api/v1/users/current";
            var headers = new Dictionary<string, string>
            {

            };
            string res = await server.GetWithNoThrowAsync(url, headers, cc);
            UserInfoBase infoBase;
            if (res.Contains("Unauthorized"))
            {
                //{"errorCode":24317,"errorMessage":"Unauthorized"}
                infoBase = new AnonymousUserInfo();
            }
            else
            {
                var obj = Tools.Deserialize<Low.CurrentUser.RootObject>(res);
                infoBase = new CurrentUser(obj);
            }
            return infoBase;
        }
        public static async Task<ChatInfo> GetChatInfo(IDataServer server, long channelId, CookieContainer cc)
        {
            //https://mixer.com/api/v1/chats/160788
            var url = "https://mixer.com/api/v1/chats/" + channelId;
            var headers = new Dictionary<string, string>
            {

            };
            //{"roles":["User"],"authkey":"8zqbrc8HFYjBWqpT","permissions":["chat","connect","poll_vote","whisper"],"endpoints":["wss://chat.mixer.com:443"],"isLoadShed":false}
            var res = await server.GetAsync(url, headers, cc);
            var obj = Tools.Deserialize<Low.Chats.RootObject>(res);
            return new ChatInfo(obj);
        }
        public static async Task<ChannelInfo> GetChannelInfo(IDataServer server, string channelName, CookieContainer cc)
        {
            //https://mixer.com/api/v1/channels/Monstercat?noCount=1
            var url = "https://mixer.com/api/v1/channels/" + channelName;
            var headers = new Dictionary<string, string>
            {

            };
            var res = await server.GetAsync(url, headers, cc);
            var obj = Tools.Deserialize<Low.Channels.RootObject>(res);
            return new ChannelInfo(obj);
        }
    }
    class ChannelInfo
    {
        public long Id { get; }
        public ChannelInfo(Low.Channels.RootObject low)
        {
            Id = low.Id;

        }
    }
    abstract class UserInfoBase
    {
        public bool IsLoggedIn { get; protected set; }
    }
    class AnonymousUserInfo:UserInfoBase
    {
        public AnonymousUserInfo()
        {
            IsLoggedIn = false;
        }
    }
    class ChatInfo
    {
        public string Authkey { get; }
        public string[] Endpoints { get; }
        public bool IsLoadShed { get; }
        public string[] Permissions { get; }
        public string[] Roles { get; }
        public ChatInfo(Low.Chats.RootObject low)
        {
            Authkey=low.Authkey;
            Endpoints=low.Endpoints;
            IsLoadShed=low.IsLoadShed;
            Permissions=low.Permissions;
            Roles=low.Roles;
        }
    }
    class CurrentUser: UserInfoBase
    {
        public long Id { get; set; }
        public long Level { get; set; }
        public long Experience { get; set; }
        public long Sparks { get; set; }
        public string Username { get; set; }
        public CurrentUser(Low.CurrentUser.RootObject low)
        {
            IsLoggedIn = true;
            Id = low.Id;
            Experience = low.Experience;
            Level = low.Level;
            Sparks = low.Sparks;
            Username = low.Username;
        }
    }
}
