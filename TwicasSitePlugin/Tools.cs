using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SitePlugin;
using System.Text.RegularExpressions;
using System.Diagnostics;
namespace TwicasSitePlugin
{

    class MessageLink : IMessageText
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
    class MessageText : IMessageText
    {
        public string Text { get; }
        public MessageText(string text)
        {
            Text = text;
        }
        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (obj is MessageText text)
            {
                return this.Text.Equals(text.Text);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Text.GetHashCode();
        }
    }
    public class MessageImage : IMessageImage
    {
        public int? Width { get; set; }

        public int? Height { get; set; }

        public string Url { get; set; }

        public string Alt { get; set; }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (obj is MessageImage image)
            {
                return this.Url.Equals(image.Url) && this.Alt.Equals(image.Alt);
            }
            return false;
        }
        public override int GetHashCode()
        {
            return Url.GetHashCode() ^ Alt.GetHashCode();
        }
    }
    static class Tools
    {
        public static ICommentData Parse(LowObject.Comment low)
        {
            var (name, thumbnailUrl, message) = SplitHtml(low.html);
            var data = new CommentData
            {
                Id = low.id,
                UserId = low.uid,
                Name = name,
                Message = ParseMessage(message),
                ThumbnailUrl = "https:" + thumbnailUrl,
            };
            return data;
        }


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
            var match1 = Regex.Match(html, "<span class=\"user\"><a .+?>(?<name>.+?)</a>");
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
                    list.Add(new MessageText(decoded));
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
                    list.Add(new MessageText(Environment.NewLine));
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

        internal static string ExtractBroadcasterId(string input)
        {
            if (string.IsNullOrEmpty(input))
                throw new ArgumentNullException(nameof(input));
            var match0 = Regex.Match(input, "twitcasting\\.tv/([a-zA-Z0-9:_]+)");
            if (match0.Success)
            {
                return match0.Groups[1].Value;
            }
            throw new ArgumentException("invalid input");
        }
    }
}
