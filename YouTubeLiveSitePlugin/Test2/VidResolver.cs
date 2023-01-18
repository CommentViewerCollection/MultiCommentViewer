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
        static readonly Regex _regexHandleChannel = new Regex("youtube\\.com/@(" + ChannelIdPattern + ")");
        static readonly Regex _regexWatch = new Regex("(?:youtube\\.com/watch\\?v=|youtu\\.be/|dashboard\\?v=)(" + VID_PATTERN + ")");
        const string VID_PATTERN = "[^?#:/&]+";
        const string USERID_PATTERN = VID_PATTERN;
        const string ChannelIdPattern = VID_PATTERN;
        private static readonly Regex _regexUser = new Regex("youtube\\.com/user/(" + USERID_PATTERN + ")");
        private static readonly Regex _regexCustomChannel = new Regex("/c/(" + ChannelIdPattern + ")");
        private static readonly Regex _regexStudio = new Regex("studio\\.youtube\\.com/[a-z]+/(" + VID_PATTERN + ")");

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
        public static bool IsNormalChannel(string input)
        {
            if (string.IsNullOrEmpty(input)) return false;
            return _regexChannel.IsMatch(input);
        }
        public static bool IsHandleChannel(string input)
        {
            if (string.IsNullOrEmpty(input)) return false;
            return _regexHandleChannel.IsMatch(input);
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
            return IsWatch(input) || IsUser(input) || IsNormalChannel(input) || IsCustomChannel(input) || IsHandleChannel(input) || IsStudio(input);
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
        internal static bool TryStudio(string input, out string vid)
        {
            if (string.IsNullOrEmpty(input))
            {
                vid = null;
                return false;
            }
            var match = _regexStudio.Match(input);
            if (match.Success)
            {
                vid = match.Groups[1].Value;
                return true;
            }
            vid = null;
            return false;
        }
        public async Task<IVidResult> GetVid(IYouTubeLiveServer server, Input.IInput input)
        {
            if (input is Input.Vid vid)
            {
                return new VidResult { Vid = vid.Raw };
            }
            else if (input is WatchUrl watchUrl)
            {
                return new VidResult { Vid = watchUrl.Vid };
            }
            else if (input is IChannelUrl channelUrl)
            {
                var vids = await ChannelLiveFinder.FindLiveVidsAsync(server, channelUrl);
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
            else if (input is StudioUrl studioUrl)
            {
                return new VidResult { Vid = studioUrl.Vid };
            }
            return new NoVidResult();
        }
    }
}
