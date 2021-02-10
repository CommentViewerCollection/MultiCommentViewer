using System.Threading.Tasks;
using System.Net;
using System.Text.RegularExpressions;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Diagnostics;
using YouTubeLiveSitePlugin.Input;

namespace YouTubeLiveSitePlugin.Test2
{
    class LiveStatus
    {
        public string Title { get; set; }
        public string Vid { get; set; }
        public string State { get; set; }
    }
    static class ChannelLiveResearcher
    {
        enum ListType
        {
            AllVideos,
            Uploads,
            LiveNow,
            UpcomingLiveStreams,
            PastLiveStreams,
            Unknown,
        }
        private static dynamic GetVideosTab(dynamic ytInitialData)
        {
            var tabs = ytInitialData.contents.twoColumnBrowseResultsRenderer.tabs;
            foreach (var tab in tabs)
            {
                string title;
                if (tab.ContainsKey("tabRenderer"))
                {
                    title = (string)tab.tabRenderer.title;
                }
                else
                {
                    throw new Exception();
                }
                if (title == "Videos")
                {
                    return tab;
                }
            }
            throw new Exception();
        }
        private static ListType GetType(string ytInitialData)
        {
            dynamic d = JsonConvert.DeserializeObject(ytInitialData);
            var videoTab = GetVideosTab(d);
            var arr = videoTab.tabRenderer.content.sectionListRenderer.subMenu.channelSubMenuRenderer.contentTypeSubMenuItems;
            foreach (var item in arr)
            {
                var title = (string)item.title;
                var selected = (bool)item.selected;
                if (selected)
                {
                    var type = GetTypeByName(title);
                    return type;
                }
            }
            return ListType.Unknown;
        }
        private static ListType GetTypeByName(string listTypeName)
        {
            ListType type;
            switch (listTypeName)
            {
                case "All videos":
                    type = ListType.AllVideos;
                    break;
                case "Uploads":
                    type = ListType.Uploads;
                    break;
                case "Live now":
                    type = ListType.LiveNow;
                    break;
                case "Upcoming live streams":
                    type = ListType.UpcomingLiveStreams;
                    break;
                case "Past live streams":
                    type = ListType.PastLiveStreams;
                    break;
                default:
                    type = ListType.Unknown;
                    break;
            }
            return type;
        }
        private static string GetChannelLiveListUrl(string channelId, ListType type)
        {
            string url;
            switch (type)
            {
                case ListType.LiveNow:
                    url = $"https://www.youtube.com/channel/{channelId}/videos?view=2&live_view=501";
                    break;
                case ListType.UpcomingLiveStreams:
                    url = $"https://www.youtube.com/channel/{channelId}/videos?view=2&live_view=502";
                    break;
                default:
                    throw new NotImplementedException();
            }
            return url;
        }
        private static async Task<(string ytInitialData, ListType)> GetYtinitialData(IYouTubeLibeServer server, string url)
        {
            var html = await server.GetEnAsync(url);
            var ytInitialData = Tools.ExtractYtInitialDataFromChannelHtml(html);
            var type = GetType(ytInitialData);
            return (ytInitialData, type);
        }
        private static List<string> GetVidsFromYtInitialData(string ytInitialData)
        {
            dynamic d = JsonConvert.DeserializeObject(ytInitialData);
            var videoTab = GetVideosTab(d);
            var items = videoTab.tabRenderer.content.sectionListRenderer.contents[0].itemSectionRenderer.contents[0].gridRenderer.items;
            var list = new List<string>();
            foreach (var item in items)
            {
                var title = (string)item.gridVideoRenderer.title.runs[0].text;
                var videoId = (string)item.gridVideoRenderer.videoId;
                list.Add(videoId);
            }
            return list;
        }
        public static async Task<List<string>> GetVidsAsync(IYouTubeLibeServer server, string channelId)
        {
            //まずは配信中という前提でデータを取得する
            {
                var url = GetChannelLiveListUrl(channelId, ListType.LiveNow);
                var (ytInitialData, type) = await GetYtinitialData(server, url);
                if (type == ListType.LiveNow)
                {
                    return GetVidsFromYtInitialData(ytInitialData);
                }
            }
            //配信の予約が入っているのかもしれない
            {
                var url = GetChannelLiveListUrl(channelId, ListType.UpcomingLiveStreams);
                var (ytInitialData, type) = await GetYtinitialData(server, url);
                if (type == ListType.UpcomingLiveStreams)
                {
                    return GetVidsFromYtInitialData(ytInitialData);
                }
            }
            //これ以外にチャットを取得できる場面は無いから諦める
            return new List<string>();
        }
    }
    interface IVidResult
    {

    }
    class MultiVidsResult : IVidResult
    {
        public List<string> Vids { get; set; }
    }
    class NoVidResult : IVidResult
    {

    }
    class VidResult : IVidResult
    {
        public string Vid { get; set; }
    }
    internal class VidResolver
    {
        static readonly Regex _regexVid = new Regex("^" + VID_PATTERN + "$");
        static readonly Regex _regexChannel = new Regex("youtube\\.com/channel/(" + ChannelIdPattern + ")");
        static readonly Regex _regexWatch = new Regex("(?:youtube\\.com/watch\\?v=|youtu\\.be/|dashboard\\?v=)(" + VID_PATTERN + ")");
        const string VID_PATTERN = "[^?#:/&]+";
        const string USERID_PATTERN = VID_PATTERN;
        const string ChannelIdPattern = VID_PATTERN;
        private static readonly Regex _regexUser = new Regex("youtube\\.com/user/(" + USERID_PATTERN + ")");
        private static readonly Regex _regexCustomChannel = new Regex("/c/(" + ChannelIdPattern + ")");
        private static readonly Regex _regexStudio = new Regex("studio\\.youtube\\.com/video/(" + VID_PATTERN + ")");
        enum InputType
        {
            Unknown,
            Vid,
            User,
            Watch,
            Channel,
            CustomChannel,
        }
        internal static bool IsVid(string input)
        {
            if (string.IsNullOrEmpty(input)) return false;
            return _regexVid.IsMatch(input);
        }
        internal static bool IsWatch(string input)
        {
            if (string.IsNullOrEmpty(input)) return false;
            return _regexWatch.IsMatch(input);
        }
        public static bool IsUser(string input)
        {
            if (string.IsNullOrEmpty(input)) return false;
            return _regexUser.IsMatch(input);
        }
        public static bool IsChannel(string input)
        {
            if (string.IsNullOrEmpty(input)) return false;
            return _regexChannel.IsMatch(input);
        }
        public static bool IsCustomChannel(string input)
        {
            if (string.IsNullOrEmpty(input)) return false;
            return _regexCustomChannel.IsMatch(input);
        }
        public static bool IsStudio(string input)
        {
            if (string.IsNullOrEmpty(input)) return false;
            return _regexStudio.IsMatch(input);
        }

        public static bool IsValidInput(string input)
        {
            return IsWatch(input) || IsUser(input) || IsChannel(input) || IsCustomChannel(input) || IsStudio(input);
        }

        internal bool TryVid(string input, out string vid)
        {
            if (string.IsNullOrEmpty(input))
            {
                vid = null;
                return false;
            }
            var match = _regexVid.Match(input);
            if (match.Success)
            {
                vid = input;
                return true;
            }
            vid = null;
            return false;
        }
        internal async Task<string> GetChannelIdFromUserId(IYouTubeLibeServer server, string userId)
        {
            var url = "https://www.youtube.com/user/" + userId;
            var html = await server.GetAsync(url);
            var match = Regex.Match(html, "<meta property=\"og:url\" content=\"https://www.youtube.com/channel/([^\"]+)\">");
            if (match.Success)
            {
                var channelId = match.Groups[1].Value;
                return channelId;
            }
            throw new ParseException(html);
        }
        internal static bool TryWatch(string input, out string vid)
        {
            if (string.IsNullOrEmpty(input))
            {
                vid = null;
                return false;
            }
            var match = _regexWatch.Match(input);
            if (match.Success)
            {
                vid = match.Groups[1].Value;
                return true;
            }
            vid = null;
            return false;
        }
        internal async Task<(string channelId, string reason)> TryGetChannelIdFromCustomChannel(IYouTubeLibeServer server, string input)
        {
            var match1 = _regexCustomChannel.Match(input);
            if (match1.Success)
            {
                var userId = match1.Groups[1].Value;
                var html = await server.GetAsync($"https://www.youtube.com/c/{userId}");
                var match2 = Regex.Match(html, "property=\"og:url\" content=\"https://www\\.youtube\\.com/channel/(?<channelid>[^/\"?]+)\">");
                if (match2.Success)
                {
                    var channelId = match2.Groups["channelid"].Value;
                    return (channelId, null);
                }
            }
            return (null, "");
        }
        internal static string ExtractChannelId(string input)
        {
            var match = _regexChannel.Match(input);
            return match.Groups[1].Value;
        }
        public static string ExtractCustomChannelId(string input)
        {
            var match = _regexCustomChannel.Match(input);
            return match.Groups[1].Value;
        }
        internal static string ExtractVidFromStudioUrl(string input)
        {
            var match = _regexStudio.Match(input);
            if (!match.Success)
            {
                return null;
            }
            return match.Groups[1].Value;
        }
        public static string ExtractUserId(string input)
        {
            var match = _regexUser.Match(input);
            return match.Groups[1].Value;
        }
        public async Task<IVidResult> GetVid(IYouTubeLibeServer server, Input.IInput input)
        {
            if (input is Input.Vid vid)
            {
                return new VidResult { Vid = vid.Raw };
            }
            else if (input is WatchUrl watchUrl)
            {
                return new VidResult { Vid = watchUrl.Vid };
            }
            else if (input is Input.ChannelUrl channelUrl)
            {
                var channelId = channelUrl.ChannelId;
                return await GetResultFromChannelId(server, channelId);
            }
            else if (input is Input.UserUrl userUrl)
            {
                var userId = userUrl.UserId;
                var channelId = await GetChannelIdFromUserId(server, userId);
                return await GetResultFromChannelId(server, channelId);
            }
            else if (input is Input.CustomChannelUrl customChannelUrl)
            {
                var (channelId, _) = await TryGetChannelIdFromCustomChannel(server, customChannelUrl.Raw);
                return await GetResultFromChannelId(server, channelId);
            }
            else if (input is Input.StudioUrl studioUrl)
            {
                return new VidResult { Vid = studioUrl.Vid };
            }
            return new NoVidResult();
        }
        internal async Task<IVidResult> GetResultFromChannelId(IYouTubeLibeServer server, string channelId)
        {
            if (string.IsNullOrEmpty(channelId))
                throw new ArgumentNullException(nameof(channelId));

            var vids = await GetVidsFromChannelId3(server, channelId);
            if (vids.Count == 1)
            {
                return new VidResult { Vid = vids[0] };
            }
            else if (vids.Count == 0)
            {
                return new NoVidResult();
            }
            else
            {
                return new MultiVidsResult { Vids = vids };
            }
        }
        internal Task<List<string>> GetVidsFromChannelId3(IYouTubeLibeServer server, string channelId)
        {
            return ChannelLiveResearcher.GetVidsAsync(server, channelId);
        }
        internal async Task<List<string>> GetVidsFromChannelId2(IYouTubeLibeServer server, string channelId)
        {
            //2021/01/10 生放送履歴が無い場合は投稿された動画の一覧になってしまうっぽい。
            var url = $"https://www.youtube.com/channel/{channelId}/videos?view=2&live_view=501";
            var html = await server.GetEnAsync(url);
            var matches = Regex.Matches(html, "\"url\":\"/watch\\?v=([^\"]+)\"");
            var vids = new List<string>();
            foreach (Match match in matches)
            {
                if (match == null) continue;
                vids.Add(match.Groups[1].Value);
            }
            return vids;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="server"></param>
        /// <param name="channelId"></param>
        /// <returns></returns>
        /// <exception cref="YtInitialDataNotFoundException"></exception>
        /// <exception cref="SpecChangedException"></exception>
        [Obsolete("2019/07/18放送開始から５分程度経過しないとvidを取得できなくなっている")]
        internal async Task<List<string>> GetVidsFromChannelId(IYouTubeLibeServer server, string channelId)
        {
            var url = $"https://www.youtube.com/channel/{channelId}/videos?flow=list&view=0";
            var html = await server.GetEnAsync(url);
            string ytInitialData;
            try
            {
                ytInitialData = Tools.ExtractYtInitialDataFromChannelHtml(html);
            }
            catch (ParseException)
            {
                if (!html.Contains("ytInitialData"))
                {
                    //条件がわからないけど結構よくある。
                    throw new YtInitialDataNotFoundException(url: url, html: html);
                }
                else
                {
                    //空白が無くなったりだとかそういう系だろうか
                    throw new SpecChangedException(html);
                }
            }

            var list = new List<string>();
            try
            {
                var json = JsonConvert.DeserializeObject<Low.ChannelYtInitialData.RootObject>(ytInitialData);
                var tabs = json.contents.twoColumnBrowseResultsRenderer.tabs;
                Low.ChannelYtInitialData.Tab videosTab = null;
                foreach (var tab in tabs)
                {
                    if (tab.tabRenderer == null)
                    {
                        continue;
                    }
                    if (tab.tabRenderer.title == "Videos")
                    {
                        videosTab = tab;
                        break;
                    }
                }
                if (videosTab == null)
                {
                    return list;
                }
                var contents = videosTab.tabRenderer.content.sectionListRenderer.contents;

                foreach (var content in contents)
                {
                    var videoRenderer = content.itemSectionRenderer.contents[0].videoRenderer;
                    //"このチャンネルには動画がありません"のとき、videoRendererがnull
                    if (videoRenderer == null)
                    {
                        continue;
                    }
                    var videoId = videoRenderer.videoId;

                    var isLive = IsLive(videoRenderer.badges);
                    if (isLive)
                    {
                        list.Add(videoId);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new SpecChangedException(html, ex);
            }
            return list;
        }
        private bool IsLive(List<Low.ChannelYtInitialData.Badge> badges)
        {
            if (badges == null) return false;
            foreach (var badge in badges)
            {
                var renderer = badge.metadataBadgeRenderer;
                if (renderer == null) continue;
                //labelには他にも"CC"等がある。ちゃんと値を見ないとダメ。
                if (renderer.label == "LIVE NOW" || renderer.style == "BADGE_STYLE_TYPE_LIVE_NOW")
                {
                    return true;
                }
            }
            return false;
        }
    }
}
