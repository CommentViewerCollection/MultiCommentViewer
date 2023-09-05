using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WhowatchSitePlugin
{
    internal static class Tools
    {
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
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="json"></param>
        /// <exception cref="ParseException"></exception>
        /// <returns></returns>
        public static T Deserialize<T>(string json)
        {
            T low;
            try
            {
                low = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json);
            }
            catch (Exception ex)
            {
                throw new ParseException(json, ex);
            }
            return low;
        }
        public static Live Parse(Low.LiveData.Live low)
        {
            var live = new Live
            {
                Title = low.Title,
                CommentCount = low.CommentCount,
                Id = low.Id,
                ItemCount = low.ItemCount,
                LiveStatus = low.LiveStatus,
                StartedAt = low.StartedAt,
                Telop = low.Telop,
                TotalViewCount = low.TotalViewCount,
                ViewCount = low.ViewCount,
            };
            return live;
        }
        public static CommentUser Parse(Low.LiveData.CommentUser low)
        {
            var user = new CommentUser
            {
                AccountName = low.AccountName,
                IconUrl = low.IconUrl,
                Id = low.Id,
                Name = low.Name,
                UserPath = low.UserPath,
                UserType = low.UserType,
            };
            return user;
        }
        public static Comment Parse(Low.LiveData.Comment low)
        {
            var comment = new Comment
            {
                Id = low.Id,
                CommentType = low.CommentType,
                ItemCount = low.ItemCount,
                Message = low.Message,
                PlayItemPatternId = low.PlayItemPatternId,
                User = Parse(low.User),
                PostedAt =low.PostedAt,
                NgWordIncluded = low.NgWordIncluded,
            };
            return comment;
        }
        public static LiveData Parse(Low.LiveData.RootObject low)
        {
            var comments = low.Comments.Select(Parse).ToList();
            var data = new LiveData
            {
                Live = Parse(low.Live),
                Comments = comments,
                UpdatedAt = low.UpdatedAt,
                Jwt = low.Jwt,
            };
            return data;
        }
        public static bool IsValidUrl(string input)
        {
            //https://whowatch.tv/profile/w:kagawapro
            //https://whowatch.tv/viewer/7005150
            //https://whowatch.tv/profile/t:asknvstontno
            if (string.IsNullOrEmpty(input)) return false;
            return Regex.IsMatch(input, "whowatch\\.tv/(?:profile|viewer|archives)/[a-z0-9:]+");
        }
        public static long? ExtractLiveIdFromInput(string input)
        {
            var match = Regex.Match(input, "whowatch\\.tv/(?:viewer|archives)/(\\d+)");
            if (match.Success)
            {
                return long.Parse(match.Groups[1].Value);
            }
            else
            {
                return null;
            }
        }
        public static string ExtractUserPathFromInput(string input)
        {
            if (string.IsNullOrEmpty(input)) return null;
            var match = Regex.Match(input, "(?:whowatch\\.tv/profile/)?((t:|w:)[a-zA-Z0-9_]+)");
            if (match.Success)
            {
                return match.Groups[1].Value;
            }
            else
            {
                return null;
            }
        }
    }
}
