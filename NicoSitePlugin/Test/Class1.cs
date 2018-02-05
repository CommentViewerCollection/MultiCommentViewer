using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
namespace NicoSitePlugin.Test
{
    public class ResolverOptions
    {
        public ProviderType ProviderType { get; }
        public string DefaultCommunity { get; }
        public string RoomLabel { get; }
        public int RoomSeetno { get; }
        public IMs Ms { get; }
        public ResolverOptions(IPlayerStatus status)
        {
            this.ProviderType = status.ProviderType;
            this.DefaultCommunity = status.DefaultCommunity;
            this.RoomLabel = status.RoomLabel;
            this.Ms = status.Ms;
            this.RoomSeetno = status.RoomSeetNo;
        }
        public ResolverOptions(ProviderType providerType, string defaultCommunity, string roomLabel, int roomSeetno, IMs ms)
        {
            this.ProviderType = providerType;
            this.DefaultCommunity = defaultCommunity;
            this.RoomLabel = roomLabel;
            this.RoomSeetno = roomSeetno;
            this.Ms = ms;
        }
    }
    public static class NewRoomResolver
    {
        private static string[] OfficialRoomLabelArr = new[]
        {
            "^アリーナ\\s最前列$",
            "^アリーナ$",
            "^裏アリーナ$",
            "^1F中央\\s最前列$",
            "^1F中央\\s前方$",
            "^1F中央\\s後方$",
            "^1F右\\s前方$",
            "^1F右\\s後方$",
            "^1F左\\s前方$",
            "^1F左\\s後方$",
            "^2F中央\\s最前列$",
            "^2F中央\\s前方$",
            "^2F右\\sAブロック$",
            "^2F右\\sBブロック$",
            "^2F右\\sCブロック$",
            "^2F右\\sDブロック$",
            "^2F左\\sAブロック$",
            "^2F左\\sBブロック$",
            "^2F左\\sCブロック$",
            "^2F左\\sDブロック$",
        };
        private static string[] NicofarreRoomLabelArr = new[]
        {
            "アリーナAブロック",//（ここが先頭。これより前は無い。）

            "アリーナCブロック",
        };

        private static int GetCommunityRoomCount(IDataSource dataSource, int level)
        {
            int roomCount;
            if (level <= 50 && level >= 69)
            {
                roomCount = 3;//ア,A,B
            }
            else if (level <= 70 && level >= 104)
            {
                roomCount = 4;//ア,A,B,C
            }
            else if (level <= 105 && level >= 149)
            {
                roomCount = 5;//ア,A,B,C,D
            }
            else if (level <= 150 && level >= 189)
            {
                roomCount = 6;//ア,A,B,C,D,E
            }
            else if (level <= 190 && level >= 229)
            {
                roomCount = 7;//ア,A,B,C,D,E,F
            }
            else if (level <= 230 && level >= 255)
            {
                roomCount = 8;//ア,A,B,C,D,E,F,G
            }
            else if (level == 256)
            {
                roomCount = 10;//ア,A,B,C,D,E,F,G,H,I
            }
            else
            {
                roomCount = 2;//ア,A
            }
            return roomCount;
        }
        private static int GetCommunityCurrentOffset(string communityId, string roomLabel)
        {
            var match = Regex.Match(roomLabel, "立ち見(?<alphabet>[A-Z])列");
            if (match.Success)
            {
                //立ち見
                var c = match.Groups["alphabet"].Value[0];
                return c - 'A' + 1;
            }
            else if (roomLabel == communityId)
            {
                //アリーナ
                return 0;
            }
            else
            {
                throw new Exception("この部屋のオフセットは計算できない。room_label：" + roomLabel);
            }
        }
        private static int GetChannelCurrentOffset(string communityId, string room_label, int room_seetno)
        {
            var match = Regex.Match(room_label, "立ち見(?<alphabet>[A-Z])列");
            if (match.Success)
            {
                //立ち見？列
                var c = match.Groups["alphabet"].Value[0];//charを取得
                return c - 'A' + 1;
            }
            else if (room_label == "立ち見席")
            {
                //立ち見席
                //現在の仕様では、立ち見席は2つずつ生成されるが、席番は前の部屋が0-499、後ろの部屋が500-999となる。
                if (room_seetno < 500)
                    return 0;
                else
                    return 1;
            }
            else if (room_label == communityId)
            {
                //アリーナ
                return 0;
            }
            else
            {
                throw new Exception("この部屋のオフセットは計算できない。room_label：" + room_label);
            }
        }
        private static int GetOfficialCurrentOffset(string room_label)
        {
            int pos = -1;
            for (int i = 0; i < OfficialRoomLabelArr.Length; i++)
            {
                if (Regex.IsMatch(room_label, OfficialRoomLabelArr[i]))
                {
                    pos = i;
                    break;
                }
            }
            if (pos < 0)
            {
                throw new Exception($"未知のroom_label: {room_label}");
            }
            return pos;
        }
        private static string[] GetCommunityRoomLabels(string communityId, int roomCount)
        {
            var list = new List<string>();
            list.Add(communityId);
            for (int i = 1; i < roomCount; i++)
            {
                list.Add(string.Format("立ち見{0}列", Char.ConvertFromUtf32('A' + i - 1)));
            }
            return list.ToArray();
        }
        private static string[] GetOfficialRoomLabels(int roomCount)
        {
            var list = new List<string>();
            for (int i = 0; i < roomCount; i++)
            {
                var label = OfficialRoomLabelArr[i].Replace("\\s", " ").Replace("^", "").Replace("$", "");
                list.Add(label);
            }
            return list.ToArray();
        }
        public static async Task<List<RoomInfo>> GetRooms(IDataSource dataSource, ResolverOptions options)
        {
            var providerType = options.ProviderType;
            var defaultCommunity = options.DefaultCommunity;
            var roomLabel = options.RoomLabel;
            var roomSeetno = options.RoomSeetno;
            var ms = options.Ms;
            var rooms = new List<RoomInfo>();
            switch (providerType)
            {
                case ProviderType.Community:

                    break;
                case ProviderType.Channel:
                    {
                        if (roomLabel == defaultCommunity || Regex.IsMatch(roomLabel, "^立ち見[A-Z]列$"))
                        {
                            //アリーナもしくは立ち見A-Z席
                            //var communityInfo = await API.GetCommunityInfo(dataSource, defaultCommunity);
                            var roomCount = 6;//立ち見E席まで
                            var currentOffset = GetChannelCurrentOffset(defaultCommunity, roomLabel, roomSeetno);
                            var roomLabels = GetCommunityRoomLabels(defaultCommunity, roomCount);
                            var arenaThread = (int.Parse(ms.Thread) - currentOffset).ToString();
                            var addrPortArr = LiveServerInfo.ChannelAddrPortArr;
                            var currentPos = LiveServerInfo.GetAddrPortArrPos(addrPortArr, (string)ms.Addr, ms.Port);
                            var arenaPos = Tools.ResolveLoopNext(0, addrPortArr.Length - 1, currentPos, -currentOffset);

                            for (int i = 0; i < roomCount; i++)
                            {
                                var pos = Tools.ResolveLoopNext(0, addrPortArr.Length - 1, arenaPos, i);
                                var addrPort = addrPortArr[pos];
                                rooms.Add(new RoomInfo(new MsTest(addrPort.Addr + ".live.nicovideo.jp", (int.Parse(arenaThread) + i).ToString(), addrPort.Port), roomLabels[i]));
                            }
                        }
                        else
                        {
                            //立ち見席
                            var roomCount = 2;
                            var currentOffset = roomSeetno < 500 ? 0 : 1;
                            var roomLabels = Enumerable.Repeat("立ち見席", roomCount).ToList();
                            var arenaThread = (int.Parse(ms.Thread) - currentOffset).ToString();
                            var addrPortArr = LiveServerInfo.ChannelAddrPortArr;
                            var currentPos = LiveServerInfo.GetAddrPortArrPos(addrPortArr, (string)ms.Addr, ms.Port);
                            var arenaPos = Tools.ResolveLoopNext(0, addrPortArr.Length - 1, currentPos, -currentOffset);

                            for (int i = 0; i < roomCount; i++)
                            {
                                var pos = Tools.ResolveLoopNext(0, addrPortArr.Length - 1, arenaPos, i);
                                var addrPort = addrPortArr[pos];
                                rooms.Add(new RoomInfo(new MsTest(addrPort.Addr + ".live.nicovideo.jp", (int.Parse(arenaThread) + i).ToString(), addrPort.Port), roomLabels[i]));
                            }
                        }
                    }
                    break;
                case ProviderType.Official:
                    if (roomLabel == "デッキ")
                    {
                        //クルーズ
                        rooms.Add(new RoomInfo(ms, roomLabel));
                    }
                    else if (roomLabel == "試写席")
                    {
                        //世界の新着動画
                        rooms.Add(new RoomInfo(ms, roomLabel));
                    }
                    else if (IsNicoFarre(roomLabel))
                    {
                        //ニコファーレ
                        rooms.Add(new RoomInfo(ms, roomLabel));
                    }
                    else
                    {
                        //一般的な公式放送
                        //アリーナ最前列、アリーナ、裏アリーナを取得する。また、自分の部屋がその3つ以外の時は自分の部屋も入れておく
                        var roomCount = 3;
                        var currentOffset = GetOfficialCurrentOffset(roomLabel);
                        var roomLabels = GetOfficialRoomLabels(roomCount);
                        var arenaThread = (int.Parse(ms.Thread) - currentOffset).ToString();
                        var addrPortArr = LiveServerInfo.OfficialAddrPortArr;
                        var currentPos = LiveServerInfo.GetAddrPortArrPos(addrPortArr, (string)ms.Addr, ms.Port);
                        var arenaPos = Tools.ResolveLoopNext(0, addrPortArr.Length - 1, currentPos, -currentOffset);

                        for (int i = 0; i < roomCount; i++)
                        {
                            var pos = Tools.ResolveLoopNext(0, addrPortArr.Length - 1, arenaPos, i);
                            var addrPort = addrPortArr[pos];
                            rooms.Add(new RoomInfo(new MsTest(addrPort.Addr + ".live.nicovideo.jp", (string)(int.Parse((string)arenaThread) + i).ToString(), addrPort.Port), roomLabels[i]));
                        }

                        //自分の部屋がアリーナの3部屋以外だったら自分の部屋も入れる
                        if (currentOffset >= 3)
                        {
                            rooms.Add(new RoomInfo(ms, roomLabel));
                        }
                    }
                    break;
            }
            return rooms;
        }
        private static bool IsNicoFarre(string roomLabel)
        {
            return NicofarreRoomLabelArr.Contains(roomLabel);
        }
    }
}