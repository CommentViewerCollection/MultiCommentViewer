using System.Linq;
using System.Xml.Serialization;
using System;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace NicoSitePlugin.Test
{
    public static class Tools
    {
        /// <summary>
        ///  例えば、4~10の数字があるとして、
        ///  現在は5、2つ先は？となったら、7となる。
        ///  現在、10、2つ先は5。
        ///  そういう関数です。
        /// </summary>
        /// <param name="from">レンジの初め</param>
        /// <param name="until">レンジの終わり</param>
        /// <param name="start">最初の数字</param>
        /// <param name="next">何個先の数字か。プラスでもマイナスでも可。適切な英単語が分からない。</param>
        /// <returns></returns>
        /// <remarks>
        /// 2015/03/24 nextのマイナス値も一応対応。今のところ問題なし。ただしコードは汚い。
        /// 2015/04/09 綺麗で分かりやすいコードにできたと思う
        /// </remarks>
        public static int ResolveLoopNext(int from, int until, int start, int next)
        {
            if (from > until)
                throw new ArgumentException("from <= untilでなければならない");

            var count = until - from + 1;
            var r = (next >= 0) ? from : until;
            var s = start - r;
            var pos = (s + (next)) % count;
            return pos + r;
        }
        /// <summary>
        /// ResolveLoopNextに近い関数。ループの上限を超えて、下限に戻った回数。
        /// next &lt; 0は未対応？
        /// </summary>
        /// <param name="from"></param>
        /// <param name="until"></param>
        /// <param name="start"></param>
        /// <param name="next">マイナスも可</param>
        /// <returns></returns>
        /// <remarks>
        /// 2015/03/24 nextのマイナス値も一応対応。今のところ問題なし。ただしコードは汚い。
        /// 2015/04/09 式を整理。
        /// </remarks>
        public static int ResolveLoopCount(int from, int until, int start, int next)
        {
            if (from > until)
                throw new ArgumentException("from <= untilでなければならない");

            var count = until - from + 1;
            var r = (next >= 0) ? from : until;
            var s = start - r;
            return (s + (next)) / count;
        }
        public static string ErrorToMessage(ErrorCode code)
        {
            string msg = "";
            switch (code)
            {
                case ErrorCode.full:
                    msg = "満員のため席を取れませんでした";
                    break;
            }
            return msg;
        }
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
