using System.Linq;
using System.Xml.Serialization;
using System;
using Newtonsoft.Json;
namespace NicoSitePlugin.Test
{
    public static class Tools
    {
        public static RoomInfo[] GetRooms(RoomInfo current, ProviderType providerType, INicoSiteOptions siteOptions)
        {
            RoomResolverBase roomResolver;
            switch (providerType)
            {
                case ProviderType.Channel:
                    roomResolver = new ChannelRoomResolver(current, siteOptions);
                    break;
                case ProviderType.Community:
                    roomResolver = new CommunityRoomResolver(current, siteOptions);
                    break;
                case ProviderType.Official:
                    roomResolver = new OfficialRoomResolver(current, siteOptions);
                    break;
                default:
                    throw new NotSupportedException();
            }
            var rooms = roomResolver.GetRooms();
            return rooms;
        }

        public static ErrorCode ConvertErrorCode(string code)
        {
            if(Enum.TryParse(code, out ErrorCode e))
            {
                return e;
            }
            else
            {
#if DEBUG
                using(var sw = new System.IO.StreamWriter("UnknownErrorCode.txt", true))
                {
                    sw.WriteLine(code);
                }
#endif
                return ErrorCode.unknown;
            }
        }
        public static IPlayerStatus Parse(string playerStatus_xml)
        {
            var serializer = new XmlSerializer(typeof(Low.Getplayerstatus));
            var bytes = System.Text.Encoding.UTF8.GetBytes(playerStatus_xml);
            Low.Getplayerstatus ps;
            using (var ms = new System.IO.MemoryStream(bytes))
            {
                ps = (Low.Getplayerstatus)serializer.Deserialize(ms);
            }

            var msList = ps.Ms_list?.Ms.Select(ms => new MsTest(ms.Addr, ms.Thread, int.Parse(ms.Port))).Cast<IMs>().ToArray();
            return new PlayerStatusTest
            {
                Title = ps.Stream.Title,
                BaseTime = long.Parse(ps.Stream.Base_time),
                Description = ps.Stream.Description,
                EndTime = long.Parse(ps.Stream.End_time),
                IsJoin = ps.User.Is_join == "1",
                Ms = new MsTest(ps.Ms.Addr, ps.Ms.Thread, int.Parse(ps.Ms.Port)),
                Nickname = ps.User.Nickname,
                OpenTime = long.Parse(ps.Stream.Open_time),
                StartTime = long.Parse(ps.Stream.Start_time),
                ProviderType = Convert(ps.Stream.Provider_type),
                UserId = ps.User.User_id,
                RoomSeetNo = int.Parse(ps.User.Room_seetno),
                RoomLabel = ps.User.Room_label,
                MsList = msList,
            };
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

        public static EmbeddedDataLow.RootObject ParseEmbeddedData(string data)
        {
            var t = JsonConvert.DeserializeObject<EmbeddedDataLow.RootObject>(data);
            return t;
        }
    }
}
