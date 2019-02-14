using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SitePlugin;
using System.Text.RegularExpressions;
using System.Diagnostics;
using Common;
using System.Windows.Media;
using System.Net;
using System.Reflection;
using System.Collections;

namespace TwicasSitePlugin
{
    class MessageLink : IMessageLink
    {
        public string Text { get; set; }
        public string Url { get; set; }
        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (obj is MessageLink text)
            {
                return this.Text.Equals(text.Text) && this.Url.Equals(text.Url);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Text.GetHashCode() ^ Url.GetHashCode();
        }
    }
    //class MessageText : IMessageText
    //{
    //    public string Text { get; }
    //    public MessageText(string text)
    //    {
    //        Text = text;
    //    }
    //    public override bool Equals(object obj)
    //    {
    //        if (obj == null)
    //        {
    //            return false;
    //        }
    //        if (obj is MessageText text)
    //        {
    //            return this.Text.Equals(text.Text);
    //        }
    //        return false;
    //    }

    //    public override int GetHashCode()
    //    {
    //        return Text.GetHashCode();
    //    }
    //}
    //public class MessageImage : IMessageImage
    //{
    //    public int? Width { get; set; }

    //    public int? Height { get; set; }

    //    public string Url { get; set; }

    //    public string Alt { get; set; }

    //    public override bool Equals(object obj)
    //    {
    //        if (obj == null)
    //        {
    //            return false;
    //        }
    //        if (obj is MessageImage image)
    //        {
    //            return this.Url.Equals(image.Url) && this.Alt.Equals(image.Alt);
    //        }
    //        return false;
    //    }
    //    public override int GetHashCode()
    //    {
    //        return Url.GetHashCode() ^ Alt.GetHashCode();
    //    }
    //}
    static class Tools
    {
        public static string DecodeBase64(string encoded)
        {
            if (string.IsNullOrEmpty(encoded)) return encoded;
            var bytes = Convert.FromBase64String(encoded);
            var s = Encoding.UTF8.GetString(bytes);
            return s;
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
        public static Color ColorFromArgb(string argb)
        {
            if (argb == null)
                throw new ArgumentNullException("argb");
            var pattern = "#(?<a>[0-9a-fA-F]{2})(?<r>[0-9a-fA-F]{2})(?<g>[0-9a-fA-F]{2})(?<b>[0-9a-fA-F]{2})";
            var match = System.Text.RegularExpressions.Regex.Match(argb, pattern, System.Text.RegularExpressions.RegexOptions.Compiled);

            if (!match.Success)
            {
                throw new ArgumentException("形式が不正");
            }
            else
            {
                var a = byte.Parse(match.Groups["a"].Value, System.Globalization.NumberStyles.HexNumber);
                var r = byte.Parse(match.Groups["r"].Value, System.Globalization.NumberStyles.HexNumber);
                var g = byte.Parse(match.Groups["g"].Value, System.Globalization.NumberStyles.HexNumber);
                var b = byte.Parse(match.Groups["b"].Value, System.Globalization.NumberStyles.HexNumber);
                return Color.FromArgb(a, r, g, b);
            }
        }
        public static string ToText(this IEnumerable<IMessagePart> messageParts)
        {
            var s = "";
            foreach (var part in messageParts)
            {
                if (part is IMessageText text)
                {
                    s += text.Text;
                }
                else if (part is IMessageLink link)
                {
                    s += link.Url;
                }
            }
            return s;
        }
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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="low"></param>
        /// <exception cref="ParseException"></exception>
        /// <returns></returns>
        public static ICommentData Parse(LowObject.Comment low)
        {
            var (name, preThumbnailUrl, message) = SplitHtml(low.html);
            string thumbnailUrl;
            if (preThumbnailUrl.StartsWith("https://"))
            {
                thumbnailUrl = preThumbnailUrl;
            }
            else if (preThumbnailUrl.StartsWith("//"))
            {
                thumbnailUrl = "http:" + preThumbnailUrl;
            }
            else
            {
                throw new ParseException(low.html);
            }
            var data = new CommentData
            {
                Id = low.id,
                UserId = low.uid,
                Name = ReplaceHtmlEntities(name),
                Message = ParseMessage(message),
                ThumbnailUrl = thumbnailUrl,
                ThumbnailHeight =50,
                ThumbnailWidth =50,
                Date=DateTime.Parse(low.date),//"Sat, 28 Apr 2018 02:21:28 +0900"
            };
            return data;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="html"></param>
        /// <exception cref="SpecChangedException"></exception>
        /// <returns></returns>
        public static (string name, string thumbnailUrl, string message) SplitHtml(string html)
        {
            string thumbnailUrl;
            string name;
            string message;

            var match = Regex.Match(html, "<img src=\"(?<thumbnail>[^\"]+)\" width=\"\\d+\" height=\"\\d+\"");
            if (match.Success)
            {
                thumbnailUrl = match.Groups["thumbnail"].Value;
            }
            else
            {
                throw new SpecChangedException("仕様変更があったかも",html);
            }

            //名前に"<"とか">"が含まれることがある。
            //2018/05/11 名前に改行が含まれている場合があったためRegexOptions.Singlelineを追加
            var match1 = Regex.Match(html, "<span class=\"user\"><a .+?>(?<name>.*?)</a>", RegexOptions.Singleline);
            if (match1.Success)
            {
                name = match1.Groups["name"].Value;
            }
            else
            {
                throw new SpecChangedException("仕様変更があったかも",html);
            }
            var match2 = Regex.Match(html, "<span class=\"comment-text\">(?<message>.+?)</span>");
            if (match2.Success)
            {
                message = match2.Groups["message"].Value;
            }
            else
            {
                throw new SpecChangedException("仕様変更があったかも",html);
            }
            return (name, thumbnailUrl, message);
        }
        public static string ReplaceLink(string str)
        {
            return Regex.Replace(str, "<a href=\"(?<url>[^\"]+)\" .+?>.+?</a>", m =>
            {
                return "<a href=\"" + m.Groups["url"].Value + "\" />";
            });
        }
        public static string ReplaceHtmlEntities(string html)
        {
            var sb = new StringBuilder(html);
            sb.Replace("&#039;", "'");
            sb.Replace("&quot;", "\"");
            sb.Replace("&nbsp;", " ");
            sb.Replace("<wbr>", "");
            sb.Replace("&lt;", "<");
            sb.Replace("&gt;", ">");
            sb.Replace("&amp;", "&");
#if DEBUG
            var matches = Regex.Matches(sb.ToString(), "(?<entity>&[^;]+;)");
            foreach (Match match in matches)
            {
                using (var sw = new System.IO.StreamWriter("entity.txt", true))
                {
                    sw.WriteLine(match.Groups["entity"].Value);
                }
            }
#endif
            return sb.ToString();
        }
        public static List<IMessagePart> ParseMessage(string message)
        {
            var b = ReplaceLink(message);
            var arr = Regex.Split(b, "(\\<[^\\>]+?\\>)");
            var list = new List<IMessagePart>();
            foreach (var s in arr)
            {
                if (!s.StartsWith("<"))
                {
                    var decoded = ReplaceHtmlEntities(s);
                    list.Add(MessagePartFactory.CreateMessageText(decoded));
                }
                else if (s.StartsWith("<a href"))
                {
                    var match = Regex.Match(s, "^<a href=\"(?<url>[^\"]+)\"");
                    if (match.Success)
                    {
                        var url = match.Groups["url"].Value;
                        list.Add(new MessageLink { Text = url, Url = url });
                    }
                }
                else if (s.StartsWith("<br"))
                {
                    list.Add(MessagePartFactory.CreateMessageText(Environment.NewLine));
                }
                else if (s == "<wbr>")
                {
                    //do nothing
                }
                else if (s.StartsWith("<img"))
                {
                    var match = Regex.Match(s, "(\\<img class=\"emoji\" src=\"(?<url>[^\"]+)\" width=\"(?<width>\\d+)\" height=\"(?<height>\\d+)\" /\\>)");
                    if (match.Success)
                    {
                        var url = "https://twitcasting.tv" + match.Groups["url"].Value;//domainを追加しないと
                        var width = int.Parse(match.Groups["width"].Value);
                        var height = int.Parse(match.Groups["height"].Value);
                        list.Add(new MessageImage { Url = url, Alt = "", Height = height, Width = width });
                    }
                }
                else
                {
#if DEBUG
                    using (var sw = new System.IO.StreamWriter("tag.txt", true))
                    {
                        sw.WriteLine(s);
                    }
#endif
                }
            }
            return list;
        }
        public static bool IsValidUserId(string input)
        {
            return Regex.IsMatch(input, "^[a-zA-Z0-9:_]+$");
        }
        internal static string ExtractBroadcasterId(string input)
        {
            if (string.IsNullOrEmpty(input))
                throw new ArgumentNullException(nameof(input));
            if (IsValidUserId(input))
            {
                return input;
            }
            var match0 = Regex.Match(input, "twitcasting\\.tv/([a-zA-Z0-9:_]+)");
            if (match0.Success)
            {
                return match0.Groups[1].Value;
            }
            throw new ArgumentException("invalid input");
        }
        public static bool IsValidUrl(string input)
        {
            return Regex.IsMatch(input, "twitcasting\\.tv/([a-zA-Z0-9:_]+)");
        }
    }
}
