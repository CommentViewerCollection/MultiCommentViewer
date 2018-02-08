using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using Newtonsoft.Json;
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

    internal static class API
    {
        public static async Task<LowObject.Emoticons> GetEmotIcons(IDataServer server, long myUserId, CookieContainer cc)
        {
            var userId = myUserId;
            var cookieList = Tools.ExtractCookies(cc);
            string apiToken = null;
            foreach (var c in cookieList)
            {
                if (c.Name == "api_token")
                {
                    apiToken = c.Value;
                }
            }
            if (apiToken == null)
            {
                throw new NotLoggedInException();
            }
            var headers = new[]
            {
                new KeyValuePair<string,string>("client-id","jzkbprff40iqj646a697cyrvl0zt2m6"),//固定値
                new KeyValuePair<string, string>("twitch-api-token",apiToken),
            };
            var s = await server.GetAsync($"https://api.twitch.tv/v5/users/{userId}/emotes?on_site=1", cc, headers);
            var low = JsonConvert.DeserializeObject<LowObject.Emoticons>(s);
            return low;
        }
        public static async Task<IMe> GetMeAsync(IDataServer server, CookieContainer cc)
        {
            var cookieList = Tools.ExtractCookies(cc);
            string apiToken = null;
            foreach (var c in cookieList)
            {
                if (c.Name == "api_token")
                {
                    apiToken = c.Value;
                }
            }
            if (apiToken == null)
            {
                throw new NotLoggedInException();
            }
            var headers = new[]
            {
                new KeyValuePair<string,string>("client-id","jzkbprff40iqj646a697cyrvl0zt2m6"),//固定値
                new KeyValuePair<string, string>("twitch-api-token",apiToken),
            };
            var s = await server.GetAsync("https://api.twitch.tv/api/me?on_site=1", cc, headers);
            var low = JsonConvert.DeserializeObject<LowObject.Me>(s);
            return new MeTest(low);
        }
    }
    internal interface IDataServer
    {
        Task<string> GetAsync(string url, CookieContainer cc, IEnumerable<KeyValuePair<string, string>> headers);
    }
    internal class TwitchServer : IDataServer
    {
        public async Task<string> GetAsync(string url, CookieContainer cc, IEnumerable<KeyValuePair<string, string>> headers)
        {
            var wc = new MyWebClient(cc, headers);
            var bytes = await wc.DownloadDataTaskAsync(url);
            return Encoding.UTF8.GetString(bytes);
        }
    }
    internal interface IMe
    {
        long Id { get; }
        string ChatOauthToken { get; }
        string Name { get; }
    }
    internal class MeTest : IMe
    {
        public long Id { get; }
        public string ChatOauthToken { get; }
        public string Name { get; }
        public MeTest(LowObject.Me low)
        {
            Id = low.id;
            Name = low.name;
            ChatOauthToken = low.chat_oauth_token;
        }
    }
}
namespace TwitchSitePlugin.LowObject
{
    public class Me
    {
        public long id { get; set; }
        public string login { get; set; }
        public string name { get; set; }
        public bool is_staff { get; set; }
        public bool is_admin { get; set; }
        public bool is_partner { get; set; }
        public bool is_broadcaster { get; set; }
        public string logo { get; set; }
        public bool account_verified { get; set; }
        public string csrf_token { get; set; }
        public object last_broadcast_time { get; set; }
        public object has_turbo { get; set; }
        public bool has_premium { get; set; }
        public bool twitter_connected { get; set; }
        public string chat_oauth_token { get; set; }
    }

    public class Item
    {
        public int id { get; set; }
        public string code { get; set; }
    }

    public class EmoticonSets
    {
        [JsonProperty("0")]
        public List<Item> Item0 { get; set; }
    }

    public class Emoticons
    {
        public EmoticonSets emoticon_sets { get; set; }
    }
}
