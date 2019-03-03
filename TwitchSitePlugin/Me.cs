using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using Newtonsoft.Json;
using System.Net.Http;

namespace TwitchSitePlugin
{

    [Serializable]
    public class TwitchException : Exception
    {
        public TwitchException() { }
        public TwitchException(string message) : base(message) { }
        public TwitchException(string message, Exception inner) : base(message, inner) { }
        protected TwitchException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

    [Serializable]
    public class NotLoggedInException : TwitchException
    {
        public NotLoggedInException() { }
        public NotLoggedInException(string message) : base(message) { }
        public NotLoggedInException(string message, Exception inner) : base(message, inner) { }
        protected NotLoggedInException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

    [Serializable]
    public class ParseException : Exception
    {
        public string Raw { get; }
        public ParseException(string raw)
        {
            Raw = raw;
        }
        public ParseException(string raw, Exception inner) : base("", inner)
        {
            Raw = raw;
        }
    }
    internal interface IUserInfo
    {
        string DisplayName { get; }
        string Id { get; }
        string Name { get; }
        string Type { get; }
        string Bio { get; }
        DateTimeOffset CreatedAt { get; }
        DateTimeOffset UpdatedAt { get; }
        string Logo { get; }
    }
    internal interface IChannelInfo
    {
        bool Mature { get; }
        string Status { get; }
        string BroadcasterLanguage { get; }
        string DisplayName { get; }
        string Game { get; }
        string Language { get; }
        string Id { get; }
        string Name { get; }
        DateTimeOffset CreatedAt { get; }
        DateTimeOffset UpdatedAt { get; }
        bool Partner { get; }
        string Logo { get; }
        string VideoBanner { get; }
        string ProfileBanner { get; }
        string ProfileBannerBackgroundColor { get; }
        string Url { get; }
        long Views { get; }
        long Followers { get; }
        string BroadcasterType { get; }
        string Description { get; }
        bool PrivateVideo { get; }
        bool PrivacyOptionsEnabled { get; }
    }
    public class Stream
    {
        public string LiveId { get; }
        public string UserId { get; }
        public string Username { get; }
        public string GameId { get; }
        public string Type { get; }
        public string Title { get; }
        public long ViewerCount { get; }
        public DateTime StartedAt { get; }
        public string Language { get; }
        public string ThumbnailUrl { get; }
        public Stream(Low.Streams.Datum lowObject)
        {
            LiveId = lowObject.Id;
            UserId = lowObject.UserId;
            Username = lowObject.UserName;
            GameId = lowObject.GameId;
            Type = lowObject.Type;
            Title = lowObject.Title;
            ViewerCount = lowObject.ViewerCount;
            StartedAt = lowObject.StartedAt.LocalDateTime;
            Language = lowObject.Language;
            ThumbnailUrl = lowObject.ThumbnailUrl;

        }
    }
    internal static class API
    {
        /// <summary>
        /// 現在放送中の番組の情報を取得する
        /// </summary>
        /// <param name="server"></param>
        /// <param name="channel"></param>
        /// <returns></returns>
        public static async Task<Stream> GetStreamAsync(IDataServer server, string channel)
        {
            //var url = "https://api.twitch.tv/kraken/streams/" + channel;
            //var headers = new Dictionary<string, string>
            //{
            //    {"client-id","jzkbprff40iqj646a697cyrvl0zt2m6" },//固定値
            //    {"Accept","application/vnd.twitchtv.v3+json" },
            //};
            //var s = await server.GetAsync(url, headers);

            var url = "https://api.twitch.tv/helix/streams?user_login=" + channel;
            var headers = new Dictionary<string, string>
            {
                {"client-id","jzkbprff40iqj646a697cyrvl0zt2m6" },//固定値
                //{"Accept","application/vnd.twitchtv.v3+json" },
            };
            var s = await server.GetAsync(url, headers);
            //配信していないとき
            //{"data":[],"pagination":{}}

            var lowObject = Tools.Deserialize<Low.Streams.RootObject>(s);
            if(lowObject.Data.Length > 0)
            {
                var ret = new Stream(lowObject.Data[0]);
                return ret;
            }
            else
            {
                return null;
            }
        }
        /// <summary>
        /// ユーザの表示名からID等を取れる
        /// TwitchではUser=Channel（多分）。他のAPIでは表示名ではなくChannelIDが必要なため、このAPIで先にIDを取得する
        /// </summary>
        /// <param name="server"></param>
        /// <param name="username"></param>
        /// <returns></returns>
        public static async Task<IUserInfo> GetChannelInfoByName(IDataServer server, string username)
        {
            var headers = new Dictionary<string,string>
            {
                {"client-id","jzkbprff40iqj646a697cyrvl0zt2m6" },//固定値
                {"Accept","application/vnd.twitchtv.v5+json" },
            };
            var s = await server.GetAsync($"https://api.twitch.tv/kraken/users?login=" + username, headers);
            var low = Tools.Deserialize<LowObject.UserInfo.RootObject>(s);
            return low.Users[0];
        }
        public static async Task<IChannelInfo> GetChannelInfo(IDataServer server, string channelId)
        {
            var headers = new Dictionary<string, string>
            {
                { "client-id","jzkbprff40iqj646a697cyrvl0zt2m6" },//固定値
            };
            var s = await server.GetAsync($"https://api.twitch.tv/kraken/channels/" + channelId, headers);
            var low = Tools.Deserialize<LowObject.ChannelInfo.RootObject>(s);
            return low;
        }
    }
    public interface IDataServer
    {
        Task<string> GetAsync(string url, Dictionary<string, string> headers);
        Task<string> GetAsync(string url, Dictionary<string, string> headers, CookieContainer cc);
    }
    public class TwitchServer : IDataServer
    {
        public async Task<string> GetAsync(string url, Dictionary<string, string> headers)
        {
            using (var client = new HttpClient())
            {
                foreach (var kv in headers)
                {
                    client.DefaultRequestHeaders.Add(kv.Key, kv.Value);
                }
                var result = await client.GetStringAsync(url);
                return result;
            }
        }
        public async Task<string> GetAsync(string url, Dictionary<string, string> headers, CookieContainer cc)
        {
            using (var handler = new HttpClientHandler { UseCookies = true, CookieContainer = cc })
            using (var client = new HttpClient(handler))
            {
                foreach (var kv in headers)
                {
                    client.DefaultRequestHeaders.Add(kv.Key, kv.Value);
                }
                var result = await client.GetStringAsync(url);
                return result;
            }
        }
        public async Task<string> GetAsync(string url)
        {
            using (var client = new HttpClient())
            {
                var result = await client.GetStringAsync(url);
                return result;
            }
        }
        public async Task<string> PostAsync(string url, Dictionary<string, string> data, CookieContainer cc)
        {
            var content = new FormUrlEncodedContent(data);
            using (var handler = new HttpClientHandler { UseCookies = true, CookieContainer = cc })
            using (var client = new HttpClient(handler))
            {
                var result = await client.PostAsync(url, content);
                var resBody = await result.Content.ReadAsStringAsync();
                return resBody;
            }
        }
    }
}
namespace TwitchSitePlugin.LowObject.ChannelInfo
{
    public partial class RootObject:IChannelInfo
    {
        [JsonProperty("mature")]
        public bool Mature { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("broadcaster_language")]
        public string BroadcasterLanguage { get; set; }

        [JsonProperty("display_name")]
        public string DisplayName { get; set; }

        [JsonProperty("game")]
        public string Game { get; set; }

        [JsonProperty("language")]
        public string Language { get; set; }

        [JsonProperty("_id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("created_at")]
        public DateTimeOffset CreatedAt { get; set; }

        [JsonProperty("updated_at")]
        public DateTimeOffset UpdatedAt { get; set; }

        [JsonProperty("partner")]
        public bool Partner { get; set; }

        [JsonProperty("logo")]
        public string Logo { get; set; }

        [JsonProperty("video_banner")]
        public string VideoBanner { get; set; }

        [JsonProperty("profile_banner")]
        public string ProfileBanner { get; set; }

        [JsonProperty("profile_banner_background_color")]
        public string ProfileBannerBackgroundColor { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("views")]
        public long Views { get; set; }

        [JsonProperty("followers")]
        public long Followers { get; set; }

        [JsonProperty("broadcaster_type")]
        public string BroadcasterType { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("private_video")]
        public bool PrivateVideo { get; set; }

        [JsonProperty("privacy_options_enabled")]
        public bool PrivacyOptionsEnabled { get; set; }
    }
}
namespace TwitchSitePlugin.LowObject.UserInfo
{
    public partial class RootObject
    {
        [JsonProperty("_total")]
        public long Total { get; set; }

        [JsonProperty("users")]
        public List<User> Users { get; set; }
    }

    public partial class User : IUserInfo
    {
        [JsonProperty("display_name")]
        public string DisplayName { get; set; }

        [JsonProperty("_id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("bio")]
        public string Bio { get; set; }

        [JsonProperty("created_at")]
        public DateTimeOffset CreatedAt { get; set; }

        [JsonProperty("updated_at")]
        public DateTimeOffset UpdatedAt { get; set; }

        [JsonProperty("logo")]
        public string Logo { get; set; }
    }
}
