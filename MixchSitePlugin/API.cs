using Codeplex.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MixchSitePlugin.Low.BanList
{
    public class Item
    {
        public string banned_user_id { get; set; }
    }

    public class Data
    {
        public List<Item> items { get; set; }
    }

    public class RootObject
    {
        public int status { get; set; }
        public Data data { get; set; }
    }
}
namespace MixchSitePlugin
{
    class Me
    {
        public string DisplayName { get; set; }
        public string UserPath { get; set; }
    }
    static class API
    {
        public static async Task<Me> GetMeAsync(IDataSource server, CookieContainer cc)
        {
            var me = new Me();
            var url = "https://www.mixch.tv";
            var res = await server.GetAsync(url, cc);
            var match0 = Regex.Match(res, "<div class=\"l-headerMain__content__usermenu__myIcon__myName[^\"]*?\">\\s*([^<\\s]*)?\\s*</div>");
            if (match0.Success)
            {
                var displayName = match0.Groups[1].Value;
                me.DisplayName = displayName;
            }
            //<div class="l-headerMain__content__usermenu__myIcon__myNameKey js-menu__key">kv510k</div>
            var match1 = Regex.Match(res, "<div class=\"l-headerMain__content__usermenu__myIcon__myNameKey[^\"]*?\">([^<]+)</div>");
            if (match1.Success)
            {
                var userPath = match1.Groups[1].Value;
                me.UserPath = userPath;
            }
            return me;
        }
        public static async Task<MovieInfo> GetMovieInfo(IDataSource dataSource, string liveId, CookieContainer cc)
        {
            //https://public.mixch.tv/external/api/v5/movies/pC8n3HQX5gh
            var url = "https://public.mixch.tv/external/api/v5/movies/" + liveId;
            var ret = await dataSource.GetAsync(url, cc);
            var obj = Tools.Deserialize<Low.External.Movies.RootObject>(ret);
            return new MovieInfo(obj);
        }
        public static async Task<Low.External.Movies.RootObject[]> GetChannelMovies(IDataSource dataSource, string channelId)
        {
            //https://public.mixch.tv/external/api/v5/movies?channel_id=rainbow6jp
            var url = "https://public.mixch.tv/external/api/v5/movies?channel_id=" + channelId;
            var ret = await dataSource.GetAsync(url);
            var obj = Tools.Deserialize<Low.External.Movies.RootObject[]>(ret);
            return obj;
        }
        public static async Task<Low.Movies.RootObject[]> GetMovies(IDataSource dataSource, string channelId)
        {
            var url = $"https://public.mixch.tv/external/api/v5/movies?channel_id={channelId}&sort=onair_status";
            var res = await dataSource.GetAsync(url);
            var obj = Tools.Deserialize<Low.Movies.RootObject[]>(res);
            return obj;
        }
        public static async Task<List<string>> GetBanList(IDataSource dataSource, Context context)
        {
            //var url=$"https://www.mixch.tv/viewapp/api/v3/blacklist/list?movie_id={movieId}&user_type=2&Uuid={context.Uuid}&Token={context.Token}&Random={context.Random}";
            //var res = await dataSource.GetAsync(url);
            //var json = JsonConvert.DeserializeObject<Low.BanList.RootObject>(res);
            //var list = new List<string>();
            //if (json.data!= null && json.data.items != null)
            //{
            //    foreach (var item in json.data.items)
            //    {
            //        list.Add(item.banned_user_id);
            //    }
            //}
            //return list;
            var list = new List<string>();
            var url = "https://apiv5.mixch.tv/api/v5/users/me/blacklists";
            var headers = new Dictionary<string, string>
            {
                { "uuid", context.Uuid },
                {"access-token", context.AccessToken },
            };
            var res = await dataSource.GetAsync(url, headers);
            var d = DynamicJson.Parse(res);
            if (!d.IsDefined("status"))
            {
                return list;
            }
            switch ((int)d.status)
            {
                case 0:
                    var s = (string)d.data.items.ToString();
                    var low = JsonConvert.DeserializeObject<Low.BlackList.RootObject[]>(s);
                    foreach(var item in low)
                    {
                        list.Add(item.Id);
                    }
                    break;
                default:
                    //{"message":"authorization required","status":-4}
                    return list;
            }


            return list;

        }
        public static async Task<Low.WebsocketContext2> GetWebsocketContext2(IDataSource dataSource, string movieId, CookieContainer cc)
        {
            var url = $"https://chat.mixch.tv/socket.io/?movieId={movieId}&EIO=3&transport=polling&t={Tools.Yeast()}";
            var bytes = await dataSource.GetByteArrayAsync(url,cc);
            var str = Tools.Bytes2String(bytes);
            var packet = Packet.Parse(str) as PacketOpen;
            return packet.Context;
        }
        public static async Task<(Low.Chats.RootObject[], string raw)> GetChats(IDataSource dataSource, string liveId, DateTime toCreatedAt, CookieContainer cc)
        {
            //https://public.mixch.tv/external/api/v5/movies/9PgmVnlqtMz/chats?to_created_at=2018-07-24T19:32:50.395Z
            var url = "https://public.mixch.tv/external/api/v5/movies/" + liveId + "/chats?to_created_at=" + toCreatedAt.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
            var res = await dataSource.GetAsync(url, cc);
            var obj = Tools.Deserialize<Low.Chats.RootObject[]>(res);
            return (obj, res);
        }
        /// <summary>
        ///
        /// </summary>
        /// <param name="server"></param>
        /// <param name="liveId"></param>
        /// <param name="comment"></param>
        /// <param name="postTime">投稿日時（JST）</param>
        /// <param name="cc"></param>
        /// <returns></returns>
        public static async Task PostCommentAsync(IDataSource server, string liveId, string comment, DateTime postTime, Context context)
        {
            var headers = new Dictionary<string, string>
            {
                { "uuid", context.Uuid },
                { "access-token", context.AccessToken },
            };
            var url = $"https://apiv5.mixch.tv/api/v5/movies/{liveId}/chats";
            var data = $"{{\"message\":\"{comment}\",\"quality_type\":0,\"messaged_at\":\"{postTime.ToString("yyyy-MM-ddTHH:mm:ss.fff+09:00")}\",\"league_key\":\"\",\"to_user_id\":\"\"}}";
            var res = await server.PostJsonAsync(url, headers, data);
            //{"message":"authorization required","status":-4}
            //{"status":0,"data":{"type":"chat","items":[{"id":213829498,"message":"a_a","quality_type":0,"posted_at":"2018-11-01T02:35:25+09:00","stamp":null,"yell_type":null,"yell":null,"user":{"id":"kv510k","mixch_user_id":137594,"recxuser_id":20487471,"nickname":"たこやき","introduction":"","icon_image_url":"https://hayabusa.io/mixch-image/user/204875/20487471.w90.v1470867009.png?format=png","l_icon_image_url":"https://hayabusa.io/mixch-image/user/204875/20487471.w320.v1470867009.png?format=png","cover_image_url":"","follows":5,"followers":1,"is_premium":false,"premium_start_at":null,"premium_charge_type":null,"is_official":false,"is_fresh":false,"is_warned":false,"is_team":false,"is_league_yell":false,"is_live":false,"live_views":0},"to_user":null,"chat_setting":{"name_color":"#F6A434","is_premium_hidden":false},"is_moderating":false,"has_banned_word":false}]}}
            return;
        }
    }
}
namespace MixchSitePlugin.Low
{
    public class WebsocketContext2
    {
        public string sid { get; set; }
        public List<string> upgrades { get; set; }
        public int pingInterval { get; set; }
        public int pingTimeout { get; set; }
    }
    public class Item
    {
        public string user_id { get; set; }
        public string user_name { get; set; }
        public string user_type { get; set; }
        public string user_key { get; set; }
        public int user_rank { get; set; }
        public string user_icon { get; set; }
        public string chat_id { get; set; }
        public string message { get; set; }
        public Stamp stamp { get; set; }
        public string item { get; set; }
        public int supporter_rank { get; set; }
        public int is_creaters { get; set; }
        public string golds { get; set; }
        public string cre_dt { get; set; }
        public int is_fresh { get; set; }
        public int is_warned { get; set; }
        public int has_banned_word { get; set; }
        public int is_moderator { get; set; }
        public int is_premium { get; set; }
        public int is_premium_hidden { get; set; }
        public string user_color { get; set; }
        public Yell yell { get; set; }
        public string display_dt { get; set; }
        public string del_flg { get; set; }
        public string quality_type { get; set; }
    }
    public class Yell
    {
        public string yell_id { get; set; }
        public string name { get; set; }
        public string label { get; set; }
        public string image_url { get; set; }
        public string points { get; set; }
        public string yells { get; set; }
        public string ticker_seconds { get; set; }
    }
    public class Stamp
    {
        public string stamp_id { get; set; }
        public string group_id { get; set; }
        public string image_url { get; set; }
    }
    public class Data
    {
        public List<Item> items { get; set; }
    }

    public class ChatList
    {
        public int status { get; set; }
        public Data data { get; set; }
    }
}
