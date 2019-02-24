using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net;
using System.Text.RegularExpressions;
using System;

namespace TwicasSitePlugin
{
    public interface IStreamChecker
    {
        long? LiveId { get; }
        bool IsWatchable { get; }
        bool IsNeverShowState { get; }
        bool IsPrivate { get; }
        string Telop { get; }
        int CurrentViewers { get; }
        int TotalViewers { get; }
        int ContinueCount { get; }
        object TimeUpTimer { get; }
        string GroupId { get; }
        long StartedAt { get; }
        List<Item> Items { get; }
    }
    public class Item
    {
        public string Id { get; set; }
        public string SenderName { get; set; }
        public string SenderImage { get; set; }
        public string ItemImage { get; set; }
        public string Message { get; set; }
        public string Effect { get; set; }
    }
    static class API
    {
        /// <summary>
        /// コメント一覧を取得する（非公開API）
        /// </summary>
        /// <param name="live_id"></param>
        /// <param name="lastCommentId"></param>
        /// <param name="f"></param>
        /// <param name="count"></param>
        public static async Task<(LowObject.Comment[], string raw)> GetListAll(IDataServer dataSource, string broadcasterName, long live_id, long lastCommentId, int from, int count, CookieContainer cc)
        {
            var url = $"http://twitcasting.tv/{broadcasterName}/userajax.php?c=listall&m={live_id}&k={lastCommentId}&f={from}&n={count}";
            var str = await dataSource.GetAsync(url, cc);
            var obj = Tools.Deserialize<LowObject.Comment[]>(str);
            return (obj, str);
        }
        public static async Task<(IStreamChecker, string raw)> GetUtreamChecker(IDataServer dataServer, string broadcasterId, string lastItemId)
        {
            if (string.IsNullOrEmpty(lastItemId))
            {
                lastItemId = "-1";
            }
            var url = $"https://twitcasting.tv/streamchecker.php?u={broadcasterId}&v=999&islive=1&lastitemid={lastItemId}";
            var str = await dataServer.GetAsync(url);
            IStreamChecker s;
            try
            {
                s = LowObject.StreamChecker2.ParseStreamChcker(str);
            }
            catch (Exception ex)
            {
                throw new ParseException(str, ex);
            }
            return (s, str);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataSource"></param>
        /// <param name="broadcaster"></param>
        /// <returns></returns>
        /// <exception cref="System.Net.Http.HttpRequestException"></exception>
        /// <exception cref="InvalidBroadcasterIdException"></exception>
        public static async Task<(LowObject.LiveContext,string raw)> GetLiveContext(IDataServer dataSource, string broadcaster, CookieContainer cc)
        {
            var url = "http://twitcasting.tv/" + broadcaster;
            var userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/65.0.3325.146 Safari/537.36";
            var str = await dataSource.GetAsync(url, userAgent, cc);
            var context = new LowObject.LiveContext();
            {
                var match0 = Regex.Match(str, "var movie_cnum = (?<cnum>[\\d]+);");
                if (match0.Success)
                {
                    context.MovieCnum = int.Parse(match0.Groups["cnum"].Value);
                }
                else
                {
                    throw new InvalidBroadcasterIdException(broadcaster);
                }
            }
            {
                var match1 = Regex.Match(str, "var movieid = \"(\\d+)\"");
                if (match1.Success)
                {
                    context.MovieId = long.Parse(match1.Groups[1].Value);
                }
                else
                {
                    throw new InvalidBroadcasterIdException(broadcaster);
                }
            }
            {
                var match = Regex.Match(str, "a href=\"/([^\"/]+)/notification");
                if (match.Success)
                {
                    var audienceId = match.Groups[1].Value;
                    context.AudienceId = audienceId;
                }
                else
                {
                    context.AudienceId = null;
                }
            }
            {
                var match = Regex.Match(str, "\"cs_session_id\":\"([^\"]+)\"");
                if (match.Success)
                {
                    var csSessionId = match.Groups[1].Value;
                    context.CsSessionId = csSessionId;
                }
            }
            return (context, str);
        }
        /// <summary>
        /// （非公開API）
        /// </summary>
        /// <param name="live_id"></param>
        /// <param name="n"></param>
        /// <param name="lastCommentId"></param>
        public static async Task<(List<LowObject.Comment> LowComments, int Cnum, string raw)> GetListUpdate(IDataServer dataSource, string broadcaster, long live_id, int cnum, long lastCommentId, CookieContainer cc)
        {
            var url = $"http://twitcasting.tv/{broadcaster}/userajax.php?c=listupdate&m={live_id}&n={cnum}&k={lastCommentId}";

            var str = await dataSource.GetAsync(url, cc);

            if (str == "[]")
            {
                return (new List<LowObject.Comment>(), cnum, str);
            }
            var obj = Tools.Deserialize<LowObject.ListUpdate>(str);

            var comments = obj.comment;
            var newCnum = obj.cnum;
            return (comments, newCnum, str);
        }

        internal static async Task<(LowObject.Comment[],string raw)> PostCommentAsync(
            IDataServer dataSource, string broadcasterId,long liveId, long lastCommentId, string comment, string csSessionId, CookieContainer cc)
        {
            var url = $"https://twitcasting.tv/{broadcasterId}/userajax.php?c=post";
            var data = new Dictionary<string, string>
            {
                {"m", liveId.ToString() },
                {"s", comment },
                {"o", broadcasterId },
                {"k", lastCommentId.ToString() },
                {"cs_session_id", csSessionId },
                {"nt", "2" },
            };

            var str = await dataSource.PostAsync(url, data, cc);
            var obj = Tools.Deserialize<LowObject.Comment[]>(str);
            return (obj, str);
        }
    }
}
