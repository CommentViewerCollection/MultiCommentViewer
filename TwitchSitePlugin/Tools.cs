using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Collections;
using System.Reflection;
using SitePlugin;
namespace TwitchSitePlugin
{
    internal static class Tools
    {
        class EmotContext
        {
            public string Id { get; set; }
            public int StartAt { get; set; }
            public int EndAt { get; set; }
            public string Alt { get; set; }
        }
        class MessageContext
        {
            public string Message { get; set; }
            public EmotContext Emot { get; set; }
        }
        public static List<IMessagePart> GetMessageItems(Result result)
        {
            var emotes = result.Tags["emotes"];
            var message = result.Params[1];

            if (string.IsNullOrEmpty(emotes))
            {
                return new List<IMessagePart> { new MessageText(message) };
            }
            var emote = emotes.Split('/');
            var emoteList = new List<EmotContext>();         
            foreach (var emo in emote)
            {
                var r = emo.IndexOf(':');
                var id = emo.Substring(0, r);

                var sub = emo.Substring(r + 1);
                var poses = sub.Split(',');
                foreach (var pos in poses)
                {
                    var po_s = pos.Split('-');

                    var startAt = int.Parse(po_s[0]);
                    var endAt = int.Parse(po_s[1]);

                    var alt = message.Substring(startAt, endAt - startAt + 1);

                    var context = new EmotContext
                    {
                        Id = id,
                        StartAt = startAt,
                        EndAt = endAt,
                        Alt = alt
                    };
                    emoteList.Add(context);
                }
            }

            //emotesに該当しない部分の抜き出し？
            //EmoteContextのStartAt-EndAtどうしに重複は無いという前提で。
            var starts = emoteList.Select(e => e.StartAt).ToList();
            starts.Sort();
            var ends = emoteList.Select(e => e.EndAt).ToList();
            ends.Sort();

            var messageContexts = new List<MessageContext>();
            int n = 0;
            for (; starts.Count > 0 && ends.Count > 0;)
            {
                var lowestStart = starts[0];
                var s1 = message.Substring(n, lowestStart - n);
                if (!string.IsNullOrEmpty(s1))
                {
                    messageContexts.Add(new MessageContext { Message = s1, Emot = null });
                }
                n += s1.Length;
                var lowestEnd = ends[0];
                var s2 = message.Substring(n, lowestEnd - n + 1);

                var emot = emoteList.Where(e => e.StartAt == lowestStart).First();
                if (!string.IsNullOrEmpty(s2))//ここが""は無いだろう。
                {
                    messageContexts.Add(new MessageContext { Message = s2, Emot = emot });
                }
                n += s2.Length;
                starts.Remove(lowestStart);
                ends.Remove(lowestEnd);
            }
            var s3 = message.Substring(n);
            if (!string.IsNullOrEmpty(s3))
            {
                messageContexts.Add(new MessageContext { Message = s3 });
            }

            var actual = new List<IMessagePart>();
            foreach (var m in messageContexts)
            {
                if (m.Emot == null)
                {
                    actual.Add(new MessageText(m.Message));
                }
                else
                {
                    actual.Add(new MessageImage { Url = $"https://static-cdn.jtvnw.net/emoticons/v1/{m.Emot.Id}/1.0", Alt = m.Message, Height=28,Width=28 });
                }
            }
            return actual;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <exception cref="ArgumentException"></exception>
        /// <returns></returns>
        public static string GetChannelName(string input)
        {
            var s = input;
            //https://www.twitch.tv/stylishnoob4
            
            if(Regex.IsMatch(s, "^[^/:?]+$"))
            {
                return "#" + s;
            }
            var match = Regex.Match(s, "twitch.tv/([^/?]+)");
            if (match.Success)
            {
                return "#" + match.Groups[1].Value;
            }
        
            throw new ArgumentException();
        }
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
        public static Result Parse(string s)
        {
            //https://static.twitchcdn.net/assets/vendor-128c346a9442245620332c7c735c08c2.js を移植
            //            e.prototype.msg = function(e) {
            //                var t = {
            //                    raw: e,
            //                    tags: {},
            //                    prefix: null,
            //                    command: null,
            //                    params: []
            //                }
            //                  , n = 0
            //                  , r = 0;
            //                if (64 === e.charCodeAt(0)) {
            //                    if (-1 === (r = e.indexOf(" ")))
            //                        return null;
            //                    for (var o = 0, i = e.slice(1, r).split(";"); o < i.length; o++) {
            //                        var s = i[o].split("=")
            //                          , a = s[0]
            //                          , u = void 0;
            //                        u = 2 === s.length ? s[1] : "true",
            //                        t.tags[a] = u
            //                    }
            //                    n = r + 1
            //                }
            //                for (; 32 === e.charCodeAt(n); )
            //                    n++;
            //                if (58 === e.charCodeAt(n)) {
            //                    if (-1 === (r = e.indexOf(" ", n)))
            //                        return null;
            //                    for (t.prefix = e.slice(n + 1, r),
            //                    n = r + 1; 32 === e.charCodeAt(n); )
            //                        n++
            //                }
            //                if (-1 === (r = e.indexOf(" ", n)))
            //                    return e.length > n ? (t.command = e.slice(n),
            //                    t) : null;
            //                for (t.command = e.slice(n, r),
            //                n = r + 1; 32 === e.charCodeAt(n); )
            //                    n++;
            //                for (; n < e.length; ) {
            //                    if (r = e.indexOf(" ", n),
            //                    58 === e.charCodeAt(n)) {
            //                        t.params.push(e.slice(n + 1));
            //                        break
            //                    }
            //                    if (-1 === r) {
            //                        if (-1 === r) {
            //                            t.params.push(e.slice(n));
            //                            break
            //                        }
            //                    } else
            //                        for (t.params.push(e.slice(n, r)),
            //                        n = r + 1; 32 === e.charCodeAt(n); )
            //                            n++
            //                }
            //                return t
            //            }

            var result = new Result();
            var n = 0;
            var r = 0;
            if (s[0] == '@')
            {
                var sPos = s.IndexOf(' ');
                var sub = s.Substring(1, sPos - 1).Split(';');
                foreach (var kv in sub)
                {
                    var arr = kv.Split('=');
                    if (arr.Length == 2)
                    {
                        result.Tags.Add(arr[0], arr[1]);
                    }
                    else
                    {
                        result.Tags.Add(arr[0], "");
                    }

                }
                n += sPos + 1;
            }
            while (s[n] == ' ')
                n++;
            if (s[n] == ':')
            {
                r = s.IndexOf(' ', n);
                result.Prefix = s.Substring(n + 1, r - (n + 1));
                n = r + 1;
                while (s[n] == ' ')
                    n++;
            }
            r = s.IndexOf(' ', n);
            if (r == -1)
            {
                result.Command = s.Length > n ? s.Substring(n) : null;
                n = r + 1;
                return result;
            }
            result.Command = s.Substring(n, r - n);
            n = r + 1;
            while (s[n] == ' ')
                n++;


            for (; n < s.Length;)
            {
                r = s.IndexOf(' ', n);
                if (s[n] == ':')
                {
                    result.Params.Add(s.Substring(n + 1));
                    break;
                }
                if (r == -1)
                {
                    result.Params.Add(s.Substring(n));
                    break;
                }
                else
                {
                    var len = s.Length;
                    result.Params.Add(s.Substring(n, r - n));
                    n = r + 1;
                    while (s[n] == ' ')
                        n++;
                }
            }
            return result;
        }

        //public static string GetRandomGuestUsername()
        //{
        //    //return "justinfan" + Math.floor(8e4 * Math.random() + 1e3)
        //    //var random = new Random();
        //    //random.Next()
        //    return "justinfan12345";

        //}

        public static string GetRandomGuestUsername()
        {
            var random = new Random();
            var n = random.Next(10000, 99999);
            return "justinfan" + n;
        }
    }
    public class Result
    {
        public Dictionary<string, string> Tags { get; } = new Dictionary<string, string>();
        public string Prefix { get; set; }
        public string Command { get; set; }
        public List<string> Params { get; } = new List<string>();
    }
}
