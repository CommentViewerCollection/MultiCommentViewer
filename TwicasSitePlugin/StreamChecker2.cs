using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace TwicasSitePlugin.LowObject
{
    public class StreamChecker2 : IStreamChecker
    {
        public long? LiveId { get; set; }
        public bool IsWatchable { get; set; }
        public bool IsNeverShowState { get; set; }
        public bool IsPrivate { get; set; }
        public string Telop { get; set; }
        public int CurrentViewers { get; set; }
        public int TotalViewers { get; set; }
        public int ContinueCount { get; set; }
        public object TimeUpTimer { get; set; }
        public string GroupId { get; set; }
        public long StartedAt { get; set; }
        public List<Item> Items { get; set; }

        private static string UrlDecode(string encoded)
        {
            return System.Web.HttpUtility.UrlDecode(encoded);
        }
        public static IStreamChecker ParseStreamChcker(string s)
        {
            var r = new StreamChecker2();
            var arr = s.Split('\t');
            if (arr.Length != 20) throw new ArgumentException("");

            var f = int.Parse(arr[1]);

            if(long.TryParse(arr[0], out long n))
            {
                r.LiveId = n;
            }
            else
            {
                r.LiveId = null;
            }
            r.IsWatchable = f == (int)StreamType.Watchable;
            r.IsNeverShowState = (int.Parse(arr[19]) & 1) > 0;
            r.IsPrivate= (int.Parse(arr[19]) & 2) > 0;
            r.Telop = f == (int)StreamType.Private ? "" : UrlDecode(arr[7]);
            r.CurrentViewers = int.Parse(arr[3]);
            r.TotalViewers = int.Parse(arr[5]);
            r.ContinueCount = int.Parse(arr[10]);
            r.TimeUpTimer = int.Parse(arr[12]);
            var m = arr.Length >= 18 ? arr[18].Replace("\n", "") : "0";
            r.GroupId = m == "0" ? "" : m;
            var offset = 1000 * int.Parse(arr[6]);
            r.StartedAt = offset;

            r.Items = ParseItemData(arr[16]);
            var nextPollingDuration = int.Parse(arr[4]);
            //arr[11]はSNSのID？Twitterの場合は数字の羅列、Twicasアカウントの場合はUserIdと同値(c:～)
            return r;
        }
        private static List<Item> ParseItemData(string data)
        {
            //一度に複数のアイテムが含まれている場合があるっぽい。アイテム\nアイテムのように\nで区切られている。
            //投げられた瞬間だけではなく、表示されている間中はずっとStreamChecker中に含まれるっぽい？
            //投げられたアイテムはそれぞれがIDを持っているため区別できる。同じ人が同じアイテムを複数投げてもどれがどれなのかちゃんと識別できると思う。
            var k = UrlDecode(data).Split(new[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
            return k.Select(n => Parse(n)).Where(o => o != null).ToList();
        }
        private static Item Parse(string e)
        {
            var t = e.Split('\t');
            if (int.Parse(t[9]) != 1)
            {
                return null;
            }
            var n = t[0].Trim();
            var r = t[1].Trim();
            var i = Tools.DecodeBase64(t[2]);
            var a = t[3].Trim();
            var o = Tools.DecodeBase64(t[4]);
            //t[8]はBase64にエンコードされた値。
            //flowitem("https://twitcasting.tv/img/anim/anim_tea_10", 3000, 1, 1, 5)
            var c = Tools.DecodeBase64(t[8]);

            var t12 = Tools.DecodeBase64(t[12]);
            Debug.WriteLine($"t12={t12}");//アイテムを投げた人のScreenNameっぽい

            var t13 = Tools.DecodeBase64(t[13]);
            Debug.WriteLine($"t13={t13}");//


            return new Item
            {
                Raw = e,
                Id = n,
                SenderName = o,
                SenderImage = a,
                ItemImage = r,
                Message = i,
                Effect = c,
                t5 = t[5],
                t6 = t[6],
                t7 = t[7],
                t9 = t[9],
                t10 = t[10],
                t11 = t[11],
                t12 = Tools.DecodeBase64(t[12]),
                t13 = Tools.DecodeBase64(t[13]),
                t14 = t[14],
                t15 = t[15],

            };
        }

        enum StreamType:int
        {
            Watchable=0,
            Private=5,
            Offline=7,
        }
    }
}
