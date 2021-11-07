using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using SitePlugin;
using Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Diagnostics;
using System.Text;
using Xceed.Wpf.Toolkit;

namespace YouTubeLiveSitePlugin.Test2
{
    class ChatContinuation
    {
        public string AllChatContinuation { get; set; }
        public string JouiChatContinuation { get; set; }
    }
    static class Tools
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="channelHtml"></param>
        /// <returns>成功したらYtInitialData,失敗したらnull</returns>
        private static string ExtractYtInitialDataFromChannelHtmlInternal1(string channelHtml)
        {
            //window["ytInitialData"] = JSON.parse("{\"responseContext\":{\"se
            var match = Regex.Match(channelHtml, "window\\[\"ytInitialData\"\\]\\s*=\\s*(.+?})(?:\"\\))?;", RegexOptions.Singleline);
            if (!match.Success)
            {
                return null;
            }
            var preYtInitialData = match.Groups[1].Value;
            string ytInitialData;
            if (preYtInitialData.StartsWith("JSON"))
            {
                var preJson = preYtInitialData.Substring(12);//先頭の"JSON.parse(\""を消す
                //preJsonは\や"がエスケープされた状態になっているため外す
                var sb = new StringBuilder(preJson);
                sb.Replace("\\\\", "\\");
                sb.Replace("\\\"", "\"");
                ytInitialData = sb.ToString();
            }
            else
            {
                ytInitialData = preYtInitialData;
            }
            return ytInitialData;
        }
        private static string ExtractYtInitialDataFromChannelHtmlInternal2(string channelHtml)
        {
            //2020/11/09 ttps://www.youtube.com/channel/CHANNEL_ID?view_as=subscriber のHTMLの仕様が通常のものと違った
            //<script nonce=\"orPyHr12z1j4Y/4tOnK12A\">var ytInitialData = {\"responseContext\":
            var match = Regex.Match(channelHtml, "<script nonce=\"[^\"]+\">var ytInitialData\\s*=\\s*(.+?});</script>", RegexOptions.Singleline);
            if (!match.Success)
            {
                return null;
            }
            var ytInitialData = match.Groups[1].Value;
            return ytInitialData;
        }
        public static string ExtractYtInitialDataFromChannelHtml(string channelHtml)
        {
            var ytInitialData = ExtractYtInitialDataFromChannelHtmlInternal1(channelHtml);
            if (!string.IsNullOrEmpty(ytInitialData)) return ytInitialData;

            ytInitialData = ExtractYtInitialDataFromChannelHtmlInternal2(channelHtml);
            if (!string.IsNullOrEmpty(ytInitialData)) return ytInitialData;

            throw new ParseException(channelHtml);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ytPlayerConfig">ExtractYtPlayerConfig(string)の戻り値</param>
        /// <returns>{"isLiveNow":true,"startTimestamp":"2020-11-12T12:16:53+00:00"}</returns>
        public static string ExtractLiveBroadcastDetails(string ytPlayerConfig)
        {
            if (string.IsNullOrEmpty(ytPlayerConfig)) return null;
            var match = Regex.Match(ytPlayerConfig, "\\\\\"liveBroadcastDetails\\\\\":({.+?})");
            if (!match.Success) return null;
            return match.Groups[1].Value.Replace("\\\"", "\"");
        }

        public static string ExtractYtPlayerConfig(string html)
        {
            var match = Regex.Match(html, "ytplayer\\.config\\s*=\\s*({.+?\"}});");
            if (!match.Success) return null;
            return match.Groups[1].Value;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="html"></param>
        /// <returns>{"isLiveNow":true,"startTimestamp":"2020-11-12T14:02:34+00:00"}</returns>
        public static string ExtractLiveBroadcastDetailsFromLivePage(string html)
        {
            //"liveBroadcastDetails":{"isLiveNow":true,"startTimestamp":"2020-11-12T14:02:34+00:00"}
            //liveBroadcastDetailsはページによってytPlayerConfigに入っている場合とytInitialPlayerResponseの場合を確認。
            //あんまり検証していないから詳細は不明。
            //ytplayerconfigの場合、liveBroadcastDetailsを要素に持つJSON自体が文字列としてJSONの値に格納されている関係で\"liveBroadcastDetails\"のようにエスケープされている。            

            //ちなみにHTMLのmetaタグでも配信開始日時が格納されていた。
            ////<meta itemprop="startDate" content="2020-11-12T14:02:34+00:00">

            var match = Regex.Match(html, "(?:\\\\)?\"liveBroadcastDetails(?:\\\\)?\":({.+?})");
            if (!match.Success) return null;
            return match.Groups[1].Value.Replace("\\\"", "\"");
        }
    }
    static class Parser
    {
        public static CommentData ParseLiveChatPaidMessageRenderer(string renderer)
        {
            dynamic d = JsonConvert.DeserializeObject(renderer);
            return ParseLiveChatPaidMessageRenderer(d);
        }
        public static CommentData ParseLiveChatPaidMessageRenderer(dynamic liveChatPaidMessageRenderer)
        {
            var commentData = Parser.ParseLiveChatTextMessageRenderer(liveChatPaidMessageRenderer);
            var purchaseAmount = (string)liveChatPaidMessageRenderer.purchaseAmountText.simpleText;
            commentData.PurchaseAmount = purchaseAmount;
            return commentData;
        }
        public static CommentData ParseLiveChatTextMessageRenderer(string renderer)
        {
            var d = JsonConvert.DeserializeObject(renderer);
            return ParseLiveChatTextMessageRenderer(d);
        }
        public static CommentData ParseLiveChatTextMessageRenderer(dynamic ren)
        {
            var commentData = new CommentData
            {
                UserId = ren.authorExternalChannelId,
                TimestampUsec = long.Parse((string)ren.timestampUsec),
                Id = ren.id
            };
            //authorPhoto
            {
                var thumbnail = ren.authorPhoto.thumbnails[0];
                var url = thumbnail.url;
                var width = (int)thumbnail.width;
                var height = (int)thumbnail.height;
                var authorPhoto = new MessageImage { Url = url, Width = width, Height = height };
                commentData.Thumbnail = authorPhoto;
            }
            //message
            {
                var messageItems = new List<IMessagePart>();
                commentData.MessageItems = messageItems;
                if (ren.ContainsKey("message"))//PaidMessageではコメント無しも可能
                {
                    if (ren.message.ContainsKey("runs"))
                    {
                        foreach (var r in ren.message.runs)
                        {
                            if (r.ContainsKey("text"))
                            {
                                var text = (string)r.text;
                                messageItems.Add(MessagePartFactory.CreateMessageText(text));
                            }
                            else if (r.ContainsKey("emoji"))
                            {
                                var emoji = r.emoji;
                                var thumbnail = emoji.image.thumbnails[0];
                                var emojiUrl = thumbnail.url;
                                var emojiWidth = (int)thumbnail.width;
                                var emojiHeight = (int)thumbnail.height;
                                var emojiAlt = emoji.image.accessibility.accessibilityData.label;
                                messageItems.Add(new MessageImage { Url = emojiUrl, Alt = emojiAlt, Height = emojiHeight, Width = emojiWidth });
                            }
                            else
                            {
                                throw new ParseException();
                            }
                        }
                    }
                    else if (ren.message.ContainsKey("simpleText"))
                    {
                        var text = (string)ren.message.simpleText;
                        messageItems.Add(MessagePartFactory.CreateMessageText(text));
                    }
                    else
                    {
                        throw new ParseException();
                    }
                }
            }
            //name
            {
                var nameItems = new List<IMessagePart>();
                commentData.NameItems = nameItems;
                nameItems.Add(MessagePartFactory.CreateMessageText((string)ren.authorName.simpleText));
                if (ren.ContainsKey("authorBadges"))
                {
                    foreach (var badge in ren.authorBadges)
                    {
                        if (badge.liveChatAuthorBadgeRenderer.ContainsKey("customThumbnail"))
                        {
                            var url = badge.liveChatAuthorBadgeRenderer.customThumbnail.thumbnails[0].url;
                            var alt = badge.liveChatAuthorBadgeRenderer.tooltip;
                            nameItems.Add(new MessageImage { Url = url, Alt = alt, Width = 16, Height = 16 });
                        }
                        else if (badge.liveChatAuthorBadgeRenderer.ContainsKey("icon"))
                        {
                            var iconType = badge.liveChatAuthorBadgeRenderer.icon.iconType;
                            var alt = badge.liveChatAuthorBadgeRenderer.tooltip;
                        }
                        else
                        {
                            throw new ParseException();
                        }
                    }
                }
            }
            return commentData;
        }
    }
}
