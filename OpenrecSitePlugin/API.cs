using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace OpenrecSitePlugin.Low.BanList
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
namespace OpenrecSitePlugin
{
    static class API
    {
        public static async Task<List<string>> GetBanList(IDataSource dataSource, string movieId, Context context)
        {
            var url=$"https://www.openrec.tv/viewapp/api/v3/blacklist/list?movie_id={movieId}&user_type=2&Uuid={context.Uuid}&Token={context.Token}&Random={context.Random}";
            var res = await dataSource.GetAsync(url);
            var json = JsonConvert.DeserializeObject<Low.BanList.RootObject>(res);
            var list = new List<string>();
            if (json.data!= null && json.data.items != null)
            {
                foreach (var item in json.data.items)
                {
                    list.Add(item.banned_user_id);
                }
            }
            return list;
        }
        public static async Task<List<Low.Item>> GetChatList(IDataSource dataSource, string movieId, Context context)
        {
            var url = $"https://www.openrec.tv/viewapp/api/v3/chat/list?movie_id={movieId}&Uuid={context.Uuid}&Token={context.Token}&Random={context.Random}";
            var res = await dataSource.GetAsync(url);
            var comments = JsonConvert.DeserializeObject<Low.ChatList>(res);
            return comments.data.items;
        }
        public static async Task<Low.WebsocketContext2> GetWebsocketContext2(IDataSource dataSource, string movieId, CookieContainer cc)
        {
            var url = $"https://chat.openrec.tv/socket.io/?movieId={movieId}&EIO=3&transport=polling&t={Tools.Yeast()}";
            var bytes = await dataSource.GetByteArrayAsync(url,cc);
            var str = Tools.Bytes2String(bytes);
            var packet = Packet.Parse(str) as PacketOpen;
            return packet.Context;
        }
    }
}
namespace OpenrecSitePlugin.Low
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