using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace PeriscopeSitePlugin
{
    internal class AccessVideo
    {
        public AccessVideo(Low.AccessVideo.RootObject obj)
        {

        }
    }
    internal static class Api
    {
        public static async Task<AccessVideo> GetAccessVideoAsync(IDataServer server, string broadcast_id, CookieContainer cc)
        {
            if (string.IsNullOrEmpty(broadcast_id)) throw new ArgumentNullException(nameof(broadcast_id));

            var url = "https://proxsee.pscp.tv/api/v2/accessVideo";
            var json = "{\"broadcast_id\":\"" + broadcast_id + "\",\"replay_redirect\":false}";
            var headers = new Dictionary<string, string>
            {
                { "X-Periscope-Csrf", "Periscope" },
            };
            var res = await server.PostJsonAsync(url, headers, json, cc);
            var obj = Tools.Deserialize<Low.AccessVideo.RootObject>(res);
            return new AccessVideo(obj);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="server"></param>
        /// <param name="broadcast_id"></param>
        /// <returns></returns>
        public static async Task<(AccessVideoPublic, BroadcastInfo)> GetAccessVideoPublicAsync(IDataServer server, string broadcast_id)
        {
            if (string.IsNullOrEmpty(broadcast_id)) throw new ArgumentNullException(nameof(broadcast_id));

            var url = "https://proxsee.pscp.tv/api/v2/accessVideoPublic?broadcast_id=" + broadcast_id;
            var res = await server.GetAsync(url);
            var obj = Tools.Deserialize<Low.AccessVideoPublic.RootObject>(res);
            return (new AccessVideoPublic(obj), new BroadcastInfo(obj));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="server"></param>
        /// <param name="chat_token"></param>
        /// <returns></returns>
        public static async Task<AccessChatPublic> GetAccessChatPublicAsync(IDataServer server, string chat_token)
        {
            var url = "https://proxsee.pscp.tv/api/v2/accessChatPublic?chat_token=" + chat_token;
            var headers = new Dictionary<string, string>
            {
                {"X-Periscope-User-Agent", "PeriscopeWeb/App (f8d11936a3dbe9921be2fdec89d5bb1e7448bb6d) Chrome/74.0.3729.75 (Windows;10)" },
            };
            var res = await server.GetAsync(url, headers, null);
            var obj = Tools.Deserialize<Low.AccessChatPublic.RootObject>(res);
            return new AccessChatPublic(obj);
        }
    }
    internal class AccessVideoPublic
    {
        public string ChatToken { get; }
        public AccessVideoPublic(Low.AccessVideoPublic.RootObject obj)
        {
            ChatToken = obj.ChatToken;
        }
    }
    internal class BroadcastInfo
    {
        public DateTime Start { get; }
        public string State { get; }
        public BroadcastInfo(Low.AccessVideoPublic.RootObject obj)
        {
            Start = obj.Broadcast.Start.LocalDateTime;
            State = obj.Broadcast.State;

        }
    }
    internal class AccessChatPublic
    {
        public string AccessToken { get; }
        public string Endpoint { get; }
        public string ReplayAccessToken { get; }
        public string RoomId { get; }
        public string SignerKey { get; }
        public AccessChatPublic(Low.AccessChatPublic.RootObject obj)
        {
            AccessToken = obj.AccessToken;
            Endpoint = obj.Endpoint;
            ReplayAccessToken = obj.ReplayAccessToken;
            SignerKey = obj.SignerKey;
            RoomId = obj.RoomId;
        }
    }
}