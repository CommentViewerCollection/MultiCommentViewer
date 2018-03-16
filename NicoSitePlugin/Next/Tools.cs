using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace NicoSitePlugin.Next
{
    static class Tools
    {
        public static string GetShortRoomName(string roomName)
        {
            if (Regex.IsMatch(roomName, "^ch\\d+$") || Regex.IsMatch(roomName, "^co\\d+$"))
            {
                return "ｱ";
            }
            var match = Regex.Match(roomName, "^立ち見([A-Z])列$");
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

        internal static List<IXmlWsRoomInfo> Distinct(List<IXmlWsRoomInfo> rooms, IEnumerable<IXmlWsRoomInfo> enumerable)
        {
            var newRooms = new List<IXmlWsRoomInfo>();
            foreach(var r in enumerable)
            {
                if (!rooms.Contains(r))
                {
                    newRooms.Add(r);
                }
            }
            return newRooms;
        }
    }
}
