using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Common;
using Newtonsoft.Json;
using SitePlugin;

namespace BLiveSitePlugin
{
    static class Tools
    {
        public static List<Cookie> ExtractCookies(CookieContainer container)
        {
            var cookies = new List<Cookie>();

            var table = (Hashtable)container.GetType().InvokeMember("m_domainTable",
                                                                    BindingFlags.NonPublic |
                                                                    BindingFlags.GetField |
                                                                    BindingFlags.Instance,
                                                                    null,
                                                                    container,
                                                                    new object[] { });

            foreach (var key in table.Keys)
            {
                var domain = key as string;

                if (domain == null)
                    continue;

                if (domain.StartsWith("."))
                    domain = domain.Substring(1);

                var address = string.Format("http://{0}/", domain);

                if (Uri.TryCreate(address, UriKind.RelativeOrAbsolute, out Uri uri) == false)
                    continue;

                foreach (Cookie cookie in container.GetCookies(uri))
                {
                    cookies.Add(cookie);
                }
            }

            return cookies;
        }
        public static async Task<string> GetLiveId(IDataSource dataSource, string input)
        {
            //LIVE_ID
            //https://live.carol-i.com/room/LIVE_ID

            string id;
            var match = Regex.Match(input, "carol-i\\.com/room/(?<id>[0-9]+)");
            if (match.Success)
            {
                id = match.Groups[1].Value;
            }
            else
            {
                throw new InvalidInputException();
            }

            return id;
        }
        public static bool IsValidUrl(string input)
        {
            var b = Regex.IsMatch(input, "carol-i\\.com/room/([0-9]+)");
            return b;
        }
        public static string ElapsedToString(TimeSpan elapsed)
        {
            string ret;
            if (elapsed.Hours == 0)
            {
                ret = elapsed.ToString("mm\\:ss");
            }
            else
            {
                ret = elapsed.ToString("h\\:mm\\:ss");
            }
            return ret;
        }
        public interface IComment
        {
            bool IsOfficial { get; }
            bool IsModerating { get; }
            bool IsPremium { get; }
            bool IsFresh { get; }
            string Message { get; }
            string StampUrl { get; }
            long? YellPoints { get; }
            DateTime PostedAt { get; }
            string Nickname { get; }
            string UserId { get; }
            string Id { get; }
            string UserIconUrl { get; }
        }
        internal class Comment : IComment
        {
            public string UserIconUrl { get; set; }
            public bool IsOfficial { get; set; }
            public bool IsModerating { get; set; }
            public bool IsPremium { get; set; }
            public bool IsFresh { get; set; }
            public string Message { get; set; }
            public string StampUrl { get; set; }
            public long? YellPoints { get; set; }
            public DateTime PostedAt { get; set; }
            public string Nickname { get; set; }
            public string UserId { get; set; }
            public string Id { get; set; }
        }
        public static IComment Parse(Low.Chats.RootObject obj)
        {
            var comment = new Comment
            {
                // Id = obj.Id.ToString(),
                // IsFresh = obj.User.IsFresh,
                // IsModerating = obj.IsModerating,
                // IsOfficial = obj.User.IsOfficial,
                // IsPremium = obj.User.IsPremium,
                Message = obj.Message,
                Nickname = obj.User.Nickname,
                PostedAt = obj.PostedAt.DateTime,
                // StampUrl = obj.Stamp?.ImageUrl,
                //2019/07/09 obj.User.Idはnullの場合があったため変更した
                //User.Idはユーザページの「ユーザ情報」で変更できる。
                //「 ユーザーIDを設定すると、チャットやコメントが可能になります。」との記載があるため、
                //コメントを投稿できている時点でnullは無いと思っていた。
                //恐らく設定直後で反映されていないんだろう。
                UserId = obj.User.BLiveUserId.ToString(),
                // YellPoints = obj.Yell?.Points,
                // UserIconUrl = obj.User.IconImageUrl,
            };
            return comment;
        }
        public static IComment Parse(Low.Item item)
        {
            var comment = new Comment
            {
                Id = item.chat_id,
                IsFresh = item.is_fresh != 0,
                IsModerating = item.is_moderator != 0,
                IsOfficial = item.user_type == "1",
                IsPremium = item.is_premium != 0,
                Message = item.message,
                Nickname = item.user_name,
                PostedAt = DateTime.Parse(item.cre_dt),
                UserId = item.user_id,
            };
            return comment;
        }
        public static IBLiveCommentData CreateCommentData(IComment obj, DateTime startAt, BLiveSiteOptions siteOptions)
        {
            var nameIcons = new List<IMessagePart>();
            if (obj.IsOfficial)
            {
                var officialIcon = new MessageImagePortion
                {
                    Alt = "オフィシャルユーザー",
                    Height = 20,
                    Width = 20,
                    Image = Properties.Resources.sprite_v5,
                    SrcX = 14,
                    SrcY = 268,
                    SrcHeight = 30,
                    SrcWidth = 30,
                };
                nameIcons.Add(officialIcon);
            }
            if (obj.IsModerating)
            {
                var premiumIcon = new MessageImagePortion
                {
                    Alt = "スタッフ",
                    Height = 20,
                    Width = 20,
                    Image = Properties.Resources.sprite_v5,
                    SrcX = 524,
                    SrcY = 384,
                    SrcHeight = 30,
                    SrcWidth = 30,
                };
                nameIcons.Add(premiumIcon);
            }
            if (obj.IsPremium)
            {
                var premiumIcon = new MessageImagePortion
                {
                    Alt = "プレミアム会員",
                    Height = 20,
                    Width = 20,
                    Image = Properties.Resources.sprite_v6,
                    SrcX = 96,
                    SrcY = 86,
                    SrcHeight = 30,
                    SrcWidth = 30,
                };
                nameIcons.Add(premiumIcon);
            }
            if (obj.IsFresh)
            {
                var premiumIcon = new MessageImagePortion
                {
                    Alt = "新規ユーザー",
                    Height = 20,
                    Width = 21,
                    Image = Properties.Resources.sprite_v5,
                    SrcX = 62,
                    SrcY = 314,
                    SrcHeight = 30,
                    SrcWidth = 32,
                };
                nameIcons.Add(premiumIcon);
            }

            var text = Tools.DecodeHtmlEntity(obj.Message);

            IMessageImage stamp = null;
            if (!string.IsNullOrEmpty(obj.StampUrl))
            {
                var stampIcon = new MessageImage
                {
                    Alt = "",
                    Url = obj.StampUrl,
                    Height = siteOptions.StampSize,
                    Width = siteOptions.StampSize,
                };
                stamp = stampIcon;
            }

            var yellPoints = obj.YellPoints?.ToString();

            var postTime = obj.PostedAt;
            return new BLiveCommentData
            {
                Name = obj.Nickname,
                NameIcons = nameIcons,
                Message = text,
                Stamp = stamp,
                YellPoints = yellPoints,
                Id = obj.Id,
                UserId = obj.UserId,
                PostTime = postTime,
                Elapsed = postTime - startAt,
                UserIconUrl = obj.UserIconUrl,
            };
        }
        public static string Yeast()
        {
            var unix = FromDateTime(DateTime.UtcNow);
            decimal n = ((decimal)unix) * 1000;
            return Encode(n);
        }
        private static string Encode(decimal num)
        {
            const string alphabet = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz-_";
            const int length = 64;
            var encoded = "";
            do
            {
                var n = num % length;
                var a = alphabet[(int)n];
                encoded = a + encoded;
                num = Math.Floor((decimal)num / length);

            } while (num > 0);

            return encoded;
        }
        private static int FromDateTime(DateTime dateTime)
        {
            var UNIX_EPOCH = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            double nowTicks = (dateTime.ToUniversalTime() - UNIX_EPOCH).TotalSeconds;
            return (int)nowTicks;
        }
        public static string Bytes2String(byte[] bytes)
        {
            var isString = bytes[0] == 0x00;
            var sizeArr = new List<byte>();

            for (int i = 1; bytes[i] != 0xFF; i++)
            {
                if (i >= bytes.Length)
                {
                    throw new IndexOutOfRangeException();
                }
                sizeArr.Add(bytes[i]);
            }
            int size = 0;//文字列のサイズ
            sizeArr.Reverse();
            for (int j = sizeArr.Count - 1; j >= 0; j--)
            {
                size += sizeArr[j] * (int)Math.Pow(10, (int)j);
            }
            var len = 2 + sizeArr.Count;
            var json = Encoding.UTF8.GetString(bytes, len, size);
            return json;
        }
        public static Context GetContext(List<Cookie> cookieList)
        {
            if (cookieList == null) throw new ArgumentNullException(nameof(cookieList));

            var uuid = "";
            var accessToken = "";
            foreach (var cookie in cookieList)
            {
                switch (cookie.Name.ToLower())
                {
                    case "uuid":
                        uuid = cookie.Value;
                        break;
                    case "access_token":
                        accessToken = cookie.Value;
                        break;
                }
            }
            return new Context(uuid, accessToken);
        }
        public static string DecodeHtmlEntity(string str)
        {
            var sb = new StringBuilder(str);
            sb.Replace("&lt;", "<");
            sb.Replace("&gt;", ">");
            sb.Replace("&quot;", "\"");
            sb.Replace("&#39;", "'");
            sb.Replace("&#039;", "'");
            sb.Replace("&#x2F;", "/");
            sb.Replace("&amp;", "&");
            var s = sb.ToString();
            return s;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="str"></param>
        /// <returns></returns>
        /// <exception cref="JsonReaderException"></exception>
        public static T Deserialize<T>(string str)
        {
            var s = JsonConvert.DeserializeObject<T>(str);
            return s;
        }

        public static MovieContext2 ParseLivePageHtml2(string livePageHtml)
        {
            MovieContext2 context = null;
            var match00 = Regex.Match(livePageHtml, "window.stores\\s*=\\s*({.+?});");
            if (match00.Success)
            {
                var json = match00.Groups[1].Value;
                var chats = Tools.Deserialize<Low.LivePage.RootObject>(json);

                context = new MovieContext2()
                {
                    Title = chats.MoviePageStore.MovieStore.Title,
                    OnairStatus = chats.MoviePageStore.MovieStore.OnairStatus.ToString(),
                    MovieId = chats.MoviePageStore.MovieStore.MovieId.ToString(),
                    RecxuserId = chats.MoviePageStore.MovieStore.Channel.RecxuserId.ToString(),
                    Id = chats.MoviePageStore.MovieStore.Id,
                    StartAt = chats.MoviePageStore.MovieStore.StartedAt.DateTime,
                };
            }
            return context;
        }
    }
}
