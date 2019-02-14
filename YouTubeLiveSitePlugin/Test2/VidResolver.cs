using System.Threading.Tasks;
using System.Net;
using System.Text.RegularExpressions;
using System;
using System.Collections.Generic;
using Codeplex.Data;
using Newtonsoft.Json;
using System.Diagnostics;

namespace YouTubeLiveSitePlugin.Test2
{
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
        static readonly Regex _regexWatch = new Regex("youtube\\.com/watch\\?v=(" + VID_PATTERN + ")");
        const string VID_PATTERN = "[^?#:/&]+";
        const string USERID_PATTERN = VID_PATTERN;
        const string ChannelIdPattern = VID_PATTERN;
        private readonly Regex _regexUser = new Regex("youtube\\.com/user/(" + USERID_PATTERN + ")");
        private readonly Regex _regexCustomChannel = new Regex("/c/(" + ChannelIdPattern + ")");
        enum InputType
        {
            Unknown,
            Vid,
            User,
            Watch,
            Channel,
            CustomChannel,
        }
        internal bool IsVid(string input)
        {
            if (string.IsNullOrEmpty(input)) return false;
            return _regexVid.IsMatch(input);
        }
        internal bool IsWatch(string input)
        {
            if (string.IsNullOrEmpty(input)) return false;
            return _regexWatch.IsMatch(input);
        }
        public bool IsUser(string input)
        {
            if (string.IsNullOrEmpty(input)) return false;
            return _regexUser.IsMatch(input);
        }
        public bool IsChannel(string input)
        {
            if (string.IsNullOrEmpty(input)) return false;
            return _regexChannel.IsMatch(input);
        }
        public bool IsCustomChannel(string input)
        {
            if (string.IsNullOrEmpty(input)) return false;
            return _regexCustomChannel.IsMatch(input);
        }
        
        public bool IsValidInput(string input)
        {
            return IsWatch(input) || IsUser(input) || IsChannel(input) || IsCustomChannel(input);
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
        internal bool TryWatch(string input, out string vid)
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
            var match1 =_regexCustomChannel.Match(input);
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
        internal string ExtractChannelId(string input)
        {
            var match = _regexChannel.Match(input);
            return match.Groups[1].Value;
        }
        string ExtractUserId(string input)
        {
            var match = _regexUser.Match(input);
            return match.Groups[1].Value;
        }
        public async Task<IVidResult> GetVid(IYouTubeLibeServer server, string input)
        {
            if (string.IsNullOrEmpty(input)) throw new ArgumentNullException(nameof(input));
            if (TryVid(input, out string vid))
            {
                return new VidResult { Vid = vid };
            }
            else if (TryWatch(input, out vid))
            {
                return new VidResult { Vid = vid };
            }
            else if (IsUser(input))
            {
                var userId = ExtractUserId(input);
                var channelId = await GetChannelIdFromUserId(server, userId);
                var vids = await GetVidsFromChannelId(server, channelId);
                if (vids.Count == 0)
                {
                    return new NoVidResult();
                }
                else if (vids.Count == 1)
                {
                    return new VidResult { Vid = vids[0] };
                }
                else
                {
                    return new MultiVidsResult { Vids = vids };
                }
            }
            else if (IsCustomChannel(input))
            {
                var (channelId, reason) = await TryGetChannelIdFromCustomChannel(server, input);
                return await GetResultFromChannelId(server, channelId);
            }
            else if (IsChannel(input))
            {
                var channelId = ExtractChannelId(input);
                return await GetResultFromChannelId(server, channelId);
            }
            throw new ParseException(input);
        }
        internal async Task<IVidResult> GetResultFromChannelId(IYouTubeLibeServer server, string channelId)
        {
            if (string.IsNullOrEmpty(channelId))
                throw new ArgumentNullException(nameof(channelId));

            var vids = await GetVidsFromChannelId(server, channelId);
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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="server"></param>
        /// <param name="channelId"></param>
        /// <returns></returns>
        /// <exception cref="YtInitialDataNotFoundException"></exception>
        /// <exception cref="SpecChangedException"></exception>
        internal async Task<List<string>> GetVidsFromChannelId(IYouTubeLibeServer server, string channelId)
        {
            var url = $"https://www.youtube.com/channel/{channelId}/videos?flow=list&view=2";
            var html = await server.GetEnAsync(url);
            var match = Regex.Match(html, "window\\[\"ytInitialData\"\\]\\s*=\\s*({.+?});\\s+", RegexOptions.Singleline);
            if (!match.Success)
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
                var ytInitialData = match.Groups[1].Value;
                var json = JsonConvert.DeserializeObject<Low.ChannelYtInitialData.RootObject>(ytInitialData);
                var tabs = json.contents.twoColumnBrowseResultsRenderer.tabs;
                Low.ChannelYtInitialData.Tab videosTab = null;
                foreach(var tab in tabs)
                {
                    if (tab.tabRenderer == null)
                    {
                        continue;
                    }
                    if(tab.tabRenderer.title == "Videos")
                    {
                        videosTab = tab;
                        break;
                    }
                }
                if(videosTab == null)
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
            foreach(var badge in badges)
            {
                var renderer = badge.metadataBadgeRenderer;
                if (renderer == null) continue;
                //labelには他にも"CC"等がある。ちゃんと値を見ないとダメ。
                if(renderer.label == "LIVE NOW" || renderer.style == "BADGE_STYLE_TYPE_LIVE_NOW")
                {
                    return true;
                }
            }
            return false;
        }
    }
}
