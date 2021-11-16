using System.Text.RegularExpressions;

namespace NicoSitePlugin
{
    static class Tools
    {
        public static IInput ParseInput(string input)
        {
            if (IsLivePageUrl(input))
            {
                var liveId = ExtractLiveIdFromLivePageUrl(input);
                return new LivePageUrl(liveId);
            }
            else if (IsChannelUrl(input))
            {
                var channelScreenName = ExtractChannelScreenNameFromUrl(input);
                return new ChannelUrl(channelScreenName);
            }
            else if (IsCommunityUrl(input))
            {
                var communityId = ExtractCommunityIdFromUrl(input);
                return new CommunityUrl(communityId);
            }
            else if (IsLiveId(input))
            {
                return new LiveId(input);
            }
            else
            {
                return new InvalidInput(input);
            }
        }

        private static string ExtractCommunityIdFromUrl(string input)
        {
            var match = Regex.Match(input, "nicovideo\\.jp/community/(co\\d+)");
            if (!match.Success) return null;
            return match.Groups[1].Value;
        }

        public static bool IsCommunityUrl(string input)
        {
            return Regex.IsMatch(input, "nicovideo\\.jp/community/(co\\d+)");
        }

        private static string ExtractChannelScreenNameFromUrl(string input)
        {
            var match = Regex.Match(input, "ch\\.nicovideo\\.jp/([^/?&]+)");
            if (!match.Success) return null;
            return match.Groups[1].Value;
        }

        public static bool IsChannelUrl(string input)
        {
            return Regex.IsMatch(input, "ch\\.nicovideo\\.jp/([^/?&]+)");
        }

        public static bool IsLivePageUrl(string input)
        {
            return Regex.IsMatch(input, "nicovideo\\.jp/watch/lv\\d+");
        }
        public static string ExtractLiveIdFromLivePageUrl(string input)
        {
            var match = Regex.Match(input, "nicovideo\\.jp/watch/(lv\\d+)");
            if (!match.Success) return null;
            return match.Groups[1].Value;
        }
        public static bool IsLiveId(string input)
        {
            return Regex.IsMatch(input, "lv\\d+");
        }
        internal static string ExtractLiveId(string str)
        {
            var match = Regex.Match(str, "(lv\\d+)");
            if (match.Success)
            {
                return match.Groups[1].Value;
            }
            else
            {
                return null;
            }
        }
    }
}
