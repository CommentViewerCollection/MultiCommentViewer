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
        public string UserId { get; set; }
    }
    static class API
    {
        public static async Task<Me> GetMeAsync(IDataSource server, CookieContainer cc)
        {
            var me = new Me();
            var url = "https://mixch.tv/mypage";
            var res = await server.GetAsync(url, cc);
            var match0 = Regex.Match(res, "<p class=\"name\">\\s*([^<\\s]*)?\\s*</p>\\s*<p class=\"id\">");
            if (match0.Success)
            {
                var displayName = match0.Groups[1].Value;
                me.DisplayName = displayName;
            }
            var match1 = Regex.Match(res, "<p class=\"id\">\\s*ID\\s*:\\s*([0-9]+)");
            if (match1.Success)
            {
                me.UserId = match1.Groups[1].Value;
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
        public static async Task<(Low.Chats.RootObject[], string raw)> GetChats(IDataSource dataSource, string liveId, DateTime toCreatedAt, CookieContainer cc)
        {
            //https://public.mixch.tv/external/api/v5/movies/9PgmVnlqtMz/chats?to_created_at=2018-07-24T19:32:50.395Z
            var url = "https://public.mixch.tv/external/api/v5/movies/" + liveId + "/chats?to_created_at=" + toCreatedAt.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
            var res = await dataSource.GetAsync(url, cc);
            var obj = Tools.Deserialize<Low.Chats.RootObject[]>(res);
            return (obj, res);
        }
    }
}
