using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwicasSitePlugin.LowObject
{
    public class LiveContext
    {
        public long MovieId { get; set; }
        public int MovieCnum { get; set; }
    }
    public class StreamChecker
    {
        /// <summary>
        /// 放送ID。放送してない時はnull
        /// </summary>
        public long? LiveId { get; }
        public int CurrentViewers { get; }
        public int TotalViewers { get; }
        public int CommentCount { get; }
        public StreamChecker(string tabSplitted)
        {
            var arr = tabSplitted.Split('\t');
            if (arr.Length < 5) throw new ArgumentException("");
            var liveId = arr[0];
            LiveId = string.IsNullOrEmpty(liveId) ? (long?)null : long.Parse(liveId);
            CommentCount = int.Parse(arr[2]);
            CurrentViewers = int.Parse(arr[3]);
            TotalViewers = int.Parse(arr[5]);
        }
    }
    public class ListUpdate
    {
        public List<Comment> comment { get; set; }
        public int cnum { get; set; }
        public string edit { get; set; }
    }
    public class Comment
    {
        public long id { get; set; }
        public string @class { get; set; }
        public string html { get; set; }
        public string date { get; set; }
        public string dur { get; set; }
        public string uid { get; set; }
        public string screen { get; set; }
        public string statusid { get; set; }
        public int lat { get; set; }
        public int lng { get; set; }
        public bool show { get; set; }
        public string yomi { get; set; }
    }
}
