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
        const string VID_PATTERN = "[^?#:/]+";
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
            return _regexVid.IsMatch(input);
        }
        internal bool IsWatch(string input)
        {
            return _regexWatch.IsMatch(input);
        }
        public bool IsUser(string input)
        {
            return _regexUser.IsMatch(input);
        }
        public bool IsChannel(string input)
        {
            return _regexChannel.IsMatch(input);
        }
        public bool IsCustomChannel(string input)
        {
            return _regexCustomChannel.IsMatch(input);
        }
        
        public bool IsValidInput(string input)
        {
            return IsWatch(input) || IsUser(input) || IsChannel(input) || IsCustomChannel(input);
        }
        
        internal bool TryVid(string input, out string vid)
        {
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
            throw new ParseException();
        }
        internal bool TryWatch(string input, out string vid)
        {
            var match = Regex.Match(input, "youtube\\.com/watch\\?v=([^?#:/]+)");
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
            var match1 = Regex.Match(input, "/c/([^/\"?:#]+)");
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
        internal async Task<(string vid, string reason)> TryChannel(IYouTubeLibeServer server, string input)
        {
            var match = Regex.Match(input, "youtube\\.com/channel/([^?#:/]+)");
            if (!match.Success)
            {
                return (input, null);
            }
            var channelId = match.Groups[1].Value;
            var url = $"https://www.youtube.com/channel/{channelId}/videos?flow=list&view=2";
            string html;
            try
            {
                html = await server.GetAsync(url);
            }
            catch (WebException)
            {
                //throw new YtException("入力されたchannelIdは存在しない");
                return (null, "入力されたchannelIdは存在しない");
            }
            var match1 = Regex.Match(html, "href=\"\\/watch\\?v=(?<vid>[^\"]+)\"");
            if (match.Success)
            {
                var vid = match.Groups["vid"].Value;
                return (vid, null);
            }
            //throw new YtException("放送IDが見つからなかった");//放送中ではないもしくは仕様変更
            return (null, "放送IDが見つからなかった");
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
                return new MultiVidsResult { Vids = vids };
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
            throw new ParseException();
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
        internal async Task<List<string>> GetVidsFromChannelId(IYouTubeLibeServer server, string channelId)
        {
            var url = $"https://www.youtube.com/channel/{channelId}/videos?flow=list&view=2";
            var html = await server.GetEnAsync(url);
            var match = Regex.Match(html, "window\\[\"ytInitialData\"\\]\\s*=\\s*({.+?});\\s+", RegexOptions.Singleline);
            if (!match.Success)
                throw new SpecChangedException(html);

            var list = new List<string>();
            try
            {
                var ytInitialData = match.Groups[1].Value;
                var json = JsonConvert.DeserializeObject<Low.ChannelYtInitialData.RootObject>(ytInitialData);
                var title = json.contents.twoColumnBrowseResultsRenderer.tabs[1].tabRenderer.title;
                Debug.Assert(title == "Videos");
                var contents = json.contents.twoColumnBrowseResultsRenderer.tabs[1].tabRenderer.content.sectionListRenderer.contents;

                foreach (var content in contents)
                {
                    //"このチャンネルには動画がありません"のとき、videoRendererがnull
                    if (content.itemSectionRenderer.contents[0].videoRenderer == null)
                    {
                        continue;
                    }
                    var videoId = content.itemSectionRenderer.contents[0].videoRenderer.videoId;

                    //badgesの中に"ライブ配信中"などと表示するデータが入っているが、アーカイブ等だとnullになる
                    var isLive = content.itemSectionRenderer.contents[0].videoRenderer.badges != null;
                    if (isLive)
                    {
                        list.Add(videoId);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new SpecChangedException("", ex) { Raw = html };
            }
            return list;
        }
    }

    [Serializable]
    public class SpecChangedException : Exception
    {
        public string Raw { get; set; }
        public SpecChangedException() { }
        public SpecChangedException(string message) : base(message) { }
        public SpecChangedException(string message, Exception inner) : base(message, inner) { }
        protected SpecChangedException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
