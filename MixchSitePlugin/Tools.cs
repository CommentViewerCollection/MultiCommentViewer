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

namespace MixchSitePlugin
{
    static class Tools
    {
        public static async Task<string> GetLiveId(IDataSource dataSource, string input)
        {
            //LIVE_ID
            //https://mixch.tv/u/LIVE_ID/live

            string id;
            var match = Regex.Match(input, "mixch\\.tv/u/(?<id>[0-9]+)/live");
            if (match.Success)
            {
                id = match.Groups[1].Value;
            }
            else
            {
                throw new InvalidInputException();
            }

            // TODO: 配信中かどうかチェックが必要かも
            return id;
        }
        public static bool IsValidUrl(string input)
        {
            var b = Regex.IsMatch(input, "mixch\\.tv/u/([0-9]+)/live");
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
        public static IComment Parse(Packet p)
        {
            var comment = new Comment
            {
                // Id = obj.Id.ToString(),
                // IsFresh = obj.User.IsFresh,
                // IsModerating = obj.IsModerating,
                // IsOfficial = obj.User.IsOfficial,
                // IsPremium = obj.User.IsPremium,
                Message = p.Message(),
                Nickname = p.name,
                PostedAt = DateTimeOffset.FromUnixTimeSeconds(p.created).LocalDateTime,
                // StampUrl = obj.Stamp?.ImageUrl,
                //2019/07/09 obj.User.Idはnullの場合があったため変更した
                //User.Idはユーザページの「ユーザ情報」で変更できる。
                //「 ユーザーIDを設定すると、チャットやコメントが可能になります。」との記載があるため、
                //コメントを投稿できている時点でnullは無いと思っていた。
                //恐らく設定直後で反映されていないんだろう。
                UserId = p.user_id.ToString(),
                // YellPoints = obj.Yell?.Points,
                // UserIconUrl = obj.User.IconImageUrl,
            };
            return comment;
        }
        public static IMixchCommentData CreateCommentData(IComment obj, DateTime startAt, MixchSiteOptions siteOptions)
        {

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
            return new MixchCommentData
            {
                Name = obj.Nickname,
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
    }
}
