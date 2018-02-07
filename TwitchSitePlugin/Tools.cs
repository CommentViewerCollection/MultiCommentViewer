using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchSitePlugin
{
    public static class Tools
    {
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
