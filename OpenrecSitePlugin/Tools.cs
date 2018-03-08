using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Common;
using Newtonsoft.Json;
using SitePlugin;

namespace OpenrecSitePlugin
{
    static class Tools
    {
        public static bool IsValidUrl(string input)
        {
            var b = Regex.IsMatch(input, "openrec\\.tv/((?:live)|(?:movie))/(?<programid>[^?/]+)");
            return b;
        }
        public static bool IsValidMovieId(string input)
        {
            return Regex.IsMatch(input, "^[^/:?]+$");
        }
        public static string ExtractLiveId(string input)
        {
            var ret = "";
            const string pattern = "openrec\\.tv/((?:live)|(?:movie))/(?<programid>[^?/]+)";
            var match = Regex.Match(input, pattern);
            if (match.Success)
            {
                ret = match.Groups["programid"].Value;
            }
            return ret;
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
        public static IOpenrecCommentData CreateCommentData(Low.Item item, DateTime startAt,OpenrecSiteOptions siteOptions)
        {
            var nameIcons = new List<IMessagePart>();
            if (item.user_type == "1")
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
            if (item.is_moderator != 0)
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
            if (item.is_premium != 0)
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
            if (item.is_fresh != 0)
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

            var text = new MessageText(Tools.DecodeHtmlEntity(item.message));

            IMessageImage stamp = null;
            if (item.stamp != null)
            {
                var stampIcon = new MessageImage
                {
                    Alt = "",
                    Url = item.stamp.image_url,
                    Height = siteOptions.StampSize,
                    Width = siteOptions.StampSize,
                };
                stamp = stampIcon;
            }

            var yellPoints = item.yell?.points;

            var postTime = DateTime.Parse(item.cre_dt);
            return new OpenrecCommentData
            {
                Name = item.user_name,
                NameIcons = nameIcons,
                Message = text,
                Stamp = stamp,
                YellPoints = yellPoints,
                Id = item.chat_id,
                UserId = item.user_id,
                PostTime = postTime,
                Elapsed = postTime - startAt,
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
        public static Context GetContext(System.Net.CookieContainer cc)
        {
            if (cc == null)
            {
                throw new ArgumentNullException(nameof(cc));
            }
            var cookieList = cc.GetCookies(new Uri("https://www.openrec.tv/live"));
            var context = new Context();
            foreach (System.Net.Cookie cookie in cookieList)
            {
                switch (cookie.Name.ToLower())
                {
                    case "uuid":
                        context.Uuid = cookie.Value;
                        break;
                    case "token":
                        context.Token = cookie.Value;
                        break;
                    case "random":
                        context.Random = cookie.Value;
                        break;
                }
            }
            return context;
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
        public static MovieContext ParseLivePageHtml(string livePageHtml)
        {
            var context = new MovieContext();
            var match4 = Regex.Match(livePageHtml, "gbl_uri\\s*=\\s*\"(?<gbl_uri>[^\\\"]+)\"");
            if (match4.Success)
            {
                context.Uri = match4.Groups["gbl_uri"].Value;
            }
            var match5 = Regex.Match(livePageHtml, "gbl_default_icon_url\\s*=\\s*\"(?<gbl_default_icon_url>[^\\\"]+)\"");
            if (match5.Success)
            {
                context.DefaultIconUrl = match5.Groups["gbl_default_icon_url"].Value;
            }
            var match0 = Regex.Match(livePageHtml, "gbl_movie_id\\s*=\\s*(?<movie_id>\\d+)");
            if (match0.Success)
            {
                context.MovieId = match0.Groups["movie_id"].Value;
            }
            var match1 = Regex.Match(livePageHtml, "gbl_onair_status\\s*=\\s*(?<onair_status>\\d+)");
            if (match1.Success)
            {
                context.OnairStatus = match1.Groups["onair_status"].Value;
            }
            var match2 = Regex.Match(livePageHtml, "gbl_user_id\\s*=\\s*(?<user_id>\\d+)");
            if (match2.Success)
            {
                context.UserId = match2.Groups["user_id"].Value;
            }
            var match3 = Regex.Match(livePageHtml, "gbl_openrec_user_id\\s*=\\s*(?<openrec_user_id>\\d+)");
            if (match3.Success)
            {
                context.OpenrecUserId = match3.Groups["openrec_user_id"].Value;
            }
            var match6 = Regex.Match(livePageHtml, "gbl_movie_user_id\\s*=\\s*(?<movie_user_id>\\d+)");
            if (match6.Success)
            {
                context.MovieUserId = match6.Groups["movie_user_id"].Value;
            }
            var match7 = Regex.Match(livePageHtml, "gbl_movie_openrec_user_id\\s*=\\s*(?<movie_openrec_user_id>\\d+)");
            if (match7.Success)
            {
                context.MovieOpenrecUserId = match7.Groups["movie_openrec_user_id"].Value;
            }
            var match8 = Regex.Match(livePageHtml, "gbl_is_live_archive\\s*=\\s*(?<is_live_archive>\\d+)");
            if (match8.Success)
            {
                context.IsLiveArchive = match8.Groups["is_live_archive"].Value;
            }
            var match9 = Regex.Match(livePageHtml, "gbl_movie_user_info\\s*=\\s*(?<movie_user_info>{[^{}]+})");
            if (match9.Success)
            {
                var json = match9.Groups["movie_user_info"].Value;
                context.MovieUserInfo = JsonConvert.DeserializeObject<MovieUserInfo>(json);
            }
            var match10 = Regex.Match(livePageHtml, "\\<div class=\"p-playbox__content__info__title\"\\>(?:\r\n|\n|\r)\\s+(?<title>[^<]+?)  \\</div\\>", RegexOptions.Singleline);
            //var match10 = Regex.Match(livePageHtml, "\\<div class=\"p-playbox__content__info__title\"\\>\n\\s+(?<title>[^<]+?)  \\</div\\>", RegexOptions.Singleline);
            //var match10 = Regex.Match(livePageHtml, "a\\\n\\s+(?<title>[^<]+?)  \\</div\\>");
            if (match10.Success)
            {
                context.Title = Tools.DecodeHtmlEntity(match10.Groups["title"].Value);
            }
            var match11 = Regex.Match(livePageHtml, "gbl_onair_start_dt = \"([^\"]+?)\"");
            if (match11.Success)
            {
                context.StartAt = match11.Groups[1].Value;
            }
            return context;
        }
    }
}
