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
using Common;
using TwitchSitePlugin.Low.ChannelProduct;

namespace TwitchSitePlugin
{
    internal static class Tools
    {
        public static ICommentData ParsePrivMsg(Result result)
        {
            if (result.Command != "PRIVMSG") throw new InvalidOperationException();

            var commentData = new CommentData();
            if (result == null)
                throw new ArgumentNullException(nameof(result));

            if (!result.Tags.TryGetValue("username", out string name))
            {
                if (!result.Tags.TryGetValue("login", out name))
                {
                    name = result.Prefix.Split('!')[0];
                }
            }
            commentData.Username = name;
            if (result.Tags.TryGetValue("id", out string id))
            {
                commentData.Id = id;
            }
            if (result.Tags.TryGetValue("user-id", out string userId))
            {
                commentData.UserId = userId;
            }
            if (result.Tags.TryGetValue("display-name", out string displayName))
            {
                commentData.DisplayName = displayName;
            }
            if (result.Tags.TryGetValue("tmi-sent-ts", out string ts))
            {
                var unix = new DateTime(1970, 1, 1).AddMilliseconds(long.Parse(ts));
                commentData.SentAt = unix.ToLocalTime();
            }
            commentData.Message = result.Params[1];
            return commentData;
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
            string emotes;
            if (result.Tags.ContainsKey("emotes"))
            {
                emotes = result.Tags["emotes"];
            }
            else
            {
                emotes = null;
            }
            var message = result.Params[1];

            message = RemoveActionFormat(message);
            return GetMessageItems(message, emotes);
        }
        /// <summary>
        /// "/me abc"コマンド使用時に"\u0001ACTION abc\u0001"という形式になっているから"abc"だけにする
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static string RemoveActionFormat(string message)
        {
            var s = message;
            var match = Regex.Match(s, "^\u0001ACTION ([^\u0001]+)\u0001$");
            if (match.Success)
            {
                s = match.Groups[1].Value;
            }
            return s;
        }
        public static List<IMessagePart> GetMessageItems(string message, string emotes)
        {
            if (string.IsNullOrEmpty(emotes))
            {
                return new List<IMessagePart> { MessagePartFactory.CreateMessageText(message) };
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
                    actual.Add(MessagePartFactory.CreateMessageText(m.Message));
                }
                else
                {
                    actual.Add(new MessageImage { Url = $"https://static-cdn.jtvnw.net/emoticons/v1/{m.Emot.Id}/1.0", Alt = m.Message, Height = 28, Width = 28 });
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

            if (Regex.IsMatch(s, "^[^/:?]+$"))
            {
                return s;
            }
            var match = Regex.Match(s, "twitch.tv/([^/?]+)");
            if (match.Success)
            {
                return match.Groups[1].Value;
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

                if (!(key is string domain))
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

        public static Product[] CreateProducts(RootObject obj)
        {
            if (obj.Plans == null)
            {
                var p = new Product(obj);
                return new[] { p };
            }
            else
            {
                return obj.Plans.Select(o => new Product(o)).ToArray();
            }
        }
        public static string CreatePrivMsg(UserState userState, string name, string channelName, string text, DateTime time)
        {
            var kvList = new List<KeyValuePair<string, string>>();
            kvList.AddRange(userState.Tags);
            var millis = (long)(time.ToUniversalTime() - UnixTime).TotalMilliseconds;
            kvList.Add(new KeyValuePair<string, string>("tmi-sent-ts", millis.ToString()));
            var tagsStr = string.Join(";", kvList.Select(kv => kv.Key + '=' + kv.Value));
            string message;
            if (userState.Tags.ContainsKey("id"))
            {
                message = $"@{tagsStr} :{name}!{name}@{name}.tmi.twitch.tv PRIVMSG #{channelName} :{text}";
            }
            else
            {
                var id = Guid.NewGuid();
                message = $"@{tagsStr};id={id.ToString()} :{name}!{name}@{name}.tmi.twitch.tv PRIVMSG #{channelName} :{text}";
            }
            return message;
        }
        private static DateTime UnixTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

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

            var result = new Result() { Raw = s };
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
        public string Raw { get; set; }
        public Dictionary<string, string> Tags { get; set; } = new Dictionary<string, string>();
        public string Prefix { get; set; }
        public string Command { get; set; }
        public List<string> Params { get; set; } = new List<string>();
    }
}
