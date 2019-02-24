using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace WhowatchSitePlugin
{
    internal class LiveSentData
    {
        string Name { get; set; }
        string ImageUrl { get; set; }
        int Count { get; set; }
    }
    internal class CommentUser
    {
        public virtual long Id { get; set; }
        public virtual string UserType { get; set; }
        public virtual string IconUrl { get; set; }
        public virtual string AccountName { get; set; }
        public virtual string UserPath { get; set; }
        public virtual string Name { get; set; }
    }
    internal class Comment
    {
        public long Id { get; set; }
        public string CommentType { get; set; }
        public long? ItemCount { get; set; }
        public long? PlayItemPatternId { get; set; }
        public virtual string Message { get; set; }
        public virtual CommentUser User { get; set; }
        public long PostedAt { get; set; }
    }
    internal class Live
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public string Telop { get; set; }
        //PUBLISHING
        //FINISHED
        //DELETED
        public string LiveStatus { get; set; }
        public long StartedAt { get; set; }
        public long TotalViewCount { get; set; }
        public long CommentCount { get; set; }
        public long ItemCount { get; set; }
        public long ViewCount { get; set; }
    }
    internal class LiveData
    {
        public Live Live { get; set; }
        public List<Comment> Comments { get; set; }
        public long UpdatedAt { get; set; }
        public bool IsItemUpdated { get; set; }
        public string Jwt { get; set; }
    }
    internal static class Api
    {
        public static async Task<LiveData> PostCommentAsync(IDataServer server, long live_id, long lastUpdatedAt, string comment, CookieContainer cc)
        {
            var url = $"https://api.whowatch.tv/lives/{live_id}/comments";
            var headers = new Dictionary<string, string>
            {
                {"Referer", "https://whowatch.tv/" },
                { "Accept", "application/json"},
                {"Origin", "https://whowatch.tv" },
            };
            var dict = new Dictionary<string, string>
            {
                { "last_updated_at",lastUpdatedAt.ToString()},
                { "message", comment },
            };
            var res = await server.PostAsync(url, headers, dict, cc);
            //"Origin"を"https://whowatch.tv/"と間違って送信したら以下の文字列が返ってきた。コメント投稿に失敗した場合のレスポンスが来たら例外を投げるようにしたい。
            //また、その例外には問題解決のためにlive_idとかcookieとか動的な情報を出来るだけ網羅的に入れ込みたい
            //"{\"error_code\":\"Z-005\",\"error_message\":\"お使いのブラウザではご利用いただけません。お手数ですが、別のブラウザをお試しください。(Z-005)\"}"
            var obj = Tools.Deserialize<Low.LiveData.RootObject>(res);
            return Tools.Parse(obj);
        }
        public static async Task<LiveData> GetLiveDataAsync(IDataServer server, long live_id, long lastUpdatedAt, CookieContainer cc)
        {
            //https://api.whowatch.tv/lives/7005919?last_updated_at=0
            var url = $"https://api.whowatch.tv/lives/{live_id}?last_updated_at={lastUpdatedAt}&v5_nomask=true";
            var res = await server.GetAsync(url, cc);
            var obj = Tools.Deserialize<Low.LiveData.RootObject>(res);
            return Tools.Parse(obj);
        }
        public static async Task<Low.Profile.RootObject> GetProfileAsync(IDataServer server, string userPath, CookieContainer cc)
        {
            //https://api.whowatch.tv/users/4697334/profile
            var url = "https://api.whowatch.tv/users/" + userPath + "/profile";
            var res = await server.GetAsync(url, cc);
            var obj = Tools.Deserialize<Low.Profile.RootObject>(res);
            return obj;
        }
        public static async Task<IMe> GetMeAsync(IDataServer server, CookieContainer cc)
        {
            //{"id":1072838,"user_type":"VALID","user_code":"1778649641148661","account_register_status":"TWITTER","whowatch_point":0,"icon_url":"","account_name":"@kv510k","user_path":"t:kv510k","name":"Ryu","is_email_registered":false,"is_twitter_connected":true,"is_facebook_connected":false,"is_related_account_auto_blocked":false}
            var url = "https://api.whowatch.tv/users/me";
            var res = await server.GetAsync(url, cc);
            var obj = Tools.Deserialize<Low.Me.RootObject>(res);
            return obj;
        }

        public static async Task<Dictionary<long, PlayItem>> GetPlayItemsAsync(IDataServer server)
        {
            var url = "https://api.whowatch.tv/playitems";
            var res = await server.GetAsync(url);
            var obj = Tools.Deserialize<Low.PlayItems.RootObject[]>(res);
            
            var dict = new Dictionary<long, PlayItem>();
            foreach (var a in obj)
            {
                var item = new PlayItem
                {
                    Id = a.Id,
                    Name = a.Name,
                    ImageUrl = a.ImageUrl,
                    SmallImageUrl = a.SmallImageUrl,
                };
                foreach (var b in a.PlayItemPattern)
                {
                    var subItem = new PlayItem
                    {
                        Id = b.Id,
                        Name = b.Name,
                        ImageUrl = b.ImageUrl,
                        SmallImageUrl = b.SmallImageUrl,
                    };
                    dict.Add(subItem.Id, subItem);
                }
            }
            return dict;
        }
    }
}
