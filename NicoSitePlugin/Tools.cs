using Codeplex.Data;
using Common;
using SitePlugin;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NicoSitePlugin
{
    static class Tools
    {
        public static bool IsKickCommand(IChat chat)
        {
            return chat.Text.StartsWith("/hb ifseetno ");
        }
        public static NicoMessageType GetMessageType(IChat chat, string mainRoomThreadId)
        {
            NicoMessageType type;
            if (IsKickCommand(chat))
            {
                type = NicoMessageType.Kick;
            }
            //アリーナ以外の運営コメントは除外
            else if ((chat.Premium == 2 || chat.Premium == 3 || chat.Premium == 7) && chat.Thread != mainRoomThreadId)
            {
                type = NicoMessageType.Ignored;
            }
            else if ((chat.Premium == 2 || chat.Premium == 3 || chat.Premium == 7) && chat.Text.StartsWith("/uadpoint"))
            {
                type = NicoMessageType.Ignored;
            }
            //アリーナ以外のBSPコメントは除外
            else if (chat.IsBsp && chat.Thread != mainRoomThreadId)
            {
                type = NicoMessageType.Ignored;
            }
            else if (IsAdRanking(chat))
            {
                type = NicoMessageType.Ignored;
            }
            else if (IsAd(chat))
            {
                type = NicoMessageType.Ad;
            }
            else if (IsInfo(chat))
            {
                type = NicoMessageType.Info;
            }
            else
            {
                type = NicoMessageType.Comment;
            }
            return type;
        }
        public static async Task<INicoComment> CreateNicoComment(IChat chat, IUser user, INicoSiteOptions _siteOptions, string roomName,Func<string,Task<IUserInfo>> f, ILogger logger)
        {
            var userId = chat.UserId;
            var is184 = Tools.Is184UserId(userId);
            if (_siteOptions.IsAutoSetNickname)
            {
                var messageText = chat.Text;
                var nick = SitePluginCommon.Utils.ExtractNickname(messageText);
                if (!string.IsNullOrEmpty(nick))
                {
                    user.Nickname = nick;
                }
            }

            string thumbnailUrl = null;
            List<IMessagePart> nameItems = null;
            try
            {

                if (!is184 && userId != "900000000")
                {
                    var userInfo = await f(userId);//API.GetUserInfo(_dataSource, userId);
                    thumbnailUrl = userInfo.ThumbnailUrl;
                    nameItems = new List<IMessagePart> { MessagePartFactory.CreateMessageText(userInfo.Name) };
                    user.Name = nameItems;
                }
            }
            catch (Exception ex)
            {
                logger.LogException(ex);
            }

            string id;
            if (chat.No.HasValue)
            {
                var shortName = Tools.GetShortRoomName(roomName);
                id = $"{shortName}:{chat.No}";
            }
            else
            {
                id = roomName;
            }
            var comment = chat.Text;
            return new NicoComment(chat.Raw, _siteOptions)
            {
                CommentItems = new List<IMessagePart> { MessagePartFactory.CreateMessageText(comment) },
                Id = id,
                NameItems = nameItems,
                PostTime = chat.Date.ToString("HH:mm:ss"),
                UserIcon = thumbnailUrl != null ? new MessageImage
                {
                    Url = thumbnailUrl,
                    Alt = null,
                    Height = 40,
                    Width = 40,
                } : null,
                UserId = userId,
                ChatNo = chat.No,
                RoomName = roomName,
                Is184 = is184,
            };
        }

        private static bool IsInfo(IChat chat)
        {
            return chat.Text.StartsWith("/info ");
        }

        private static bool IsAd(IChat chat)
        {
            return chat.Text.StartsWith("/nicoad ") && chat.Text.Contains("message");
        }

        private static bool IsAdRanking(IChat chat)
        {
            return chat.Text.StartsWith("/nicoad ") && chat.Text.Contains("contributionRanking");
        }

        public static INicoAd CreateNicoAd(IChat chat, string roomName, INicoSiteOptions siteOptions)
        {
            var jsonStr = chat.Text.Substring(8);
            var d = Codeplex.Data.DynamicJson.Parse(jsonStr);
            if (d.version == "1")
            {
                var content = (string)d.message;
                return new NicoAd(chat.Raw, siteOptions)
                {
                    CommentItems = new List<IMessagePart> { MessagePartFactory.CreateMessageText(content) },
                    NameItems = null,
                    PostTime = chat.Date.ToString("HH:mm:ss"),
                    UserId = chat.UserId,
                    RoomName = roomName,
                };
            }
            else
            {
                throw new ParseException(chat.Raw);
            }
        }

        public static INicoInfo CreateNicoInfo(IChat chat, string roomName, INicoSiteOptions siteOptions)
        {
            var match = Regex.Match(chat.Text, "^/info (?<no>\\d+) (?<content>.+)$", RegexOptions.Singleline);
            if (match.Success)
            {
                var no = int.Parse(match.Groups["no"].Value);
                var content = match.Groups["content"].Value;
                return new NicoInfo(chat.Raw, siteOptions)
                {
                    CommentItems = new List<IMessagePart> { MessagePartFactory.CreateMessageText(content) },
                    NameItems = null,
                    PostTime = chat.Date.ToString("HH:mm:ss"),
                    UserId = chat.UserId,
                    RoomName = roomName,
                    No = no,
                };
            }
            else
            {
                throw new ParseException(chat.Raw);
            }
        }
        public static string GetAdComment(string nicoad)
        {
            string text = null;
            var match = Regex.Match(nicoad, "\"latestNicoad\":{\"advertiser\":\"([^\"]+)\",\"point\":(\\d+)(?:,\"message\":\"([^\"]+)\")?}");
            if (match.Success)
            {
                var name = match.Groups[1].Value;
                var points = int.Parse(match.Groups[2].Value);
                string message = match.Groups[3].Value;
                if (!string.IsNullOrEmpty(message))
                {
                    text = $"提供：{name}「{message}」（{points}pt）";
                }
                else
                {
                    text = $"提供：{name}（{points}pt）";
                }
            }
            return text;
        }
        /// <summary>
        /// チャンネルのURLからChannel IDもしくはScreenNameを取得する
        /// </summary>
        /// <param name="url"></param>
        /// <returns>channelId/screenName</returns>
        /// <remarks>チャンネルにはIDの他に一意のScreenNameが存在する。例えばch2603560の場合はner-ch。基本的にURLにはIDではなくScreenNameが使われている。</remarks>
        public static string ExtractChannelScreenName(string url)
        {
            var match = Regex.Match(url, "ch.nicovideo.jp/([^/?&]+)");
            if (!match.Success) return null;
            return match.Groups[1].Value;
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
        public static bool Is184UserId(string userid)
        {
            return !int.TryParse(userid, out _);
        }
        public static string GetShortRoomName(string roomName)
        {
            if (Regex.IsMatch(roomName, "^ch\\d+$") || Regex.IsMatch(roomName, "^co\\d+$"))
            {
                return "ｱ";
            }
            var match0 = Regex.Match(roomName, "^立ち見(\\d+)$");
            if (match0.Success)
            {
                var num = match0.Groups[1].Value;
                return "立" + num;
            }
            //2018/07/06ニコ生コミュニティの立ち見？列：A～Z→[→\→]→^→_→`→a～z
            var match = Regex.Match(roomName, "^立ち見(.+)列$");
            if (match.Success)
            {
                var letter = match.Groups[1].Value;
                return letter;
            }
            if (roomName == "立ち見席")
            {
                return "立";
            }

            //ここに来るのはofficialのみ。
            //officialはコメ番が無いから短縮する必要は無い。そのまま帰す。
            //ただし全角スペースは半角にする。後々何かに使うときのことを考えて。
            return roomName.Replace("　", " ");
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
        public static string ExtractLiveId(string input)
        {
            var match = Regex.Match(input, "(lv\\d+)");
            if (match.Success)
            {
                return match.Groups[1].Value;
            }
            else
            {
                return "";
            }
        }
        public static ErrorCode ConvertErrorCode(string code)
        {
            if (Enum.TryParse(code, out ErrorCode e))
            {
                return e;
            }
            throw new ParseException(code);
        }
        public static ProviderType Convert(string providerType)
        {
            ProviderType type;
            switch (providerType)
            {
                case "channel":
                    type = ProviderType.Channel;
                    break;
                case "community":
                    type = ProviderType.Community;
                    break;
                case "official":
                    type = ProviderType.Official;
                    break;
                default:
                    type = ProviderType.Unknown;
                    break;
            }
            return type;
        }
        public static List<T> Distinct<T>(List<T> main, List<T> newList)
        {
            var ret = new List<T>();
            foreach (var item in newList)
            {
                if (!main.Contains(item))
                {
                    ret.Add(item);
                }
            }
            return ret;
        }
    }
}
