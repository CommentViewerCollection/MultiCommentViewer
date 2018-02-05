using System;

namespace NicoSitePlugin.Test
{
    public static class LiveServerInfo
    {
        /// <summary>
        /// 自分の部屋のアドレスとポートの組み合わせに対応する変換テーブルの行を探す。
        /// </summary>
        /// <param name="addrPortArr">変換テーブル</param>
        /// <param name="provider_type"></param>
        /// <param name="addr"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        public static int GetAddrPortArrPos(AddrPort[] addrPortArr, string addr, int port)
        {
            int myPos = -1;
            for (int i = 0; i < addrPortArr.Length; i++)
            {
                var pair = addrPortArr[i];
                if (addr.StartsWith(pair.Addr) && port == pair.Port)
                {
                    myPos = i;
                    break;
                }
            }
            if (myPos < 0)
            {
                throw new Exception("アドレスポート変換テーブルにない組み合わせ Addr:" + addr + " port:" + port);
            }
            return myPos;
        }
        [Serializable]
        public class AddrPort
        {
            public string Addr { get; private set; }
            public int Port { get; private set; }
            public AddrPort(string addr, int port)
            {
                Addr = addr;
                Port = port;
            }
            public override string ToString()
            {
                return $"{Addr}, {Port}";
            }
            public override bool Equals(object obj)
            {
                if (obj == null)
                    return false;
                AddrPort p = obj as AddrPort;
                if ((object)p == null)
                    return false;
                return this.Equals(p);
            }
            public bool Equals(AddrPort a)
            {
                if ((object)a == null)
                {
                    return false;
                }
                return this.Addr == a.Addr && this.Port == a.Port;
            }
            public override int GetHashCode()
            {
                return base.GetHashCode();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public static AddrPort[] ChannelAddrPortArr = new[]
        {
            new AddrPort("omsg101", 2815),
            new AddrPort("omsg102", 2828),
            new AddrPort("omsg103", 2841),
            new AddrPort("omsg104", 2854),
            new AddrPort("omsg105", 2867),
            new AddrPort("omsg106", 2880),
            new AddrPort("omsg101", 2816),
            new AddrPort("omsg102", 2829),
            new AddrPort("omsg103", 2842),
            new AddrPort("omsg104", 2855),
            new AddrPort("omsg105", 2868),
            new AddrPort("omsg106", 2881),
            new AddrPort("omsg101", 2817),
            new AddrPort("omsg102", 2830),
            new AddrPort("omsg103", 2843),
            new AddrPort("omsg104", 2856),
            new AddrPort("omsg105", 2869),
            new AddrPort("omsg106", 2882),
        };
        /// <summary>
        /// 
        /// </summary>
        public static AddrPort[] OfficialAddrPortArr = new[]
        {
            new AddrPort("omsg101" ,2805),
            new AddrPort("omsg102" ,2818),
            new AddrPort("omsg103" ,2831),
            new AddrPort("omsg104" ,2844),
            new AddrPort("omsg105" ,2857),
            new AddrPort("omsg106" ,2870),
            new AddrPort("omsg101" ,2806),
            new AddrPort("omsg102" ,2819),
            new AddrPort("omsg103" ,2832),
            new AddrPort("omsg104" ,2845),
            new AddrPort("omsg105" ,2858),
            new AddrPort("omsg106" ,2871),
            new AddrPort("omsg101" ,2807),
            new AddrPort("omsg102" ,2820),
            new AddrPort("omsg103" ,2833),
            new AddrPort("omsg104" ,2846),
            new AddrPort("omsg105" ,2859),
            new AddrPort("omsg106" ,2872),
            new AddrPort("omsg101" ,2808),
            new AddrPort("omsg102" ,2821),
            new AddrPort("omsg103" ,2834),
            new AddrPort("omsg104" ,2847),
            new AddrPort("omsg105" ,2860),
            new AddrPort("omsg106" ,2873),
            new AddrPort("omsg101" ,2809),
            new AddrPort("omsg102" ,2822),
            new AddrPort("omsg103" ,2835),
            new AddrPort("omsg104" ,2848),
            new AddrPort("omsg105" ,2861),
            new AddrPort("omsg106" ,2874),
            new AddrPort("omsg101" ,2810),
            new AddrPort("omsg102" ,2823),
            new AddrPort("omsg103" ,2836),
            new AddrPort("omsg104" ,2849),
            new AddrPort("omsg105" ,2862),
            new AddrPort("omsg106" ,2875),
            new AddrPort("omsg101" ,2811),
            new AddrPort("omsg102" ,2824),
            new AddrPort("omsg103" ,2837),
            new AddrPort("omsg104" ,2850),
            new AddrPort("omsg105" ,2863),
            new AddrPort("omsg106" ,2876),
            new AddrPort("omsg101" ,2812),
            new AddrPort("omsg102" ,2825),
            new AddrPort("omsg103" ,2838),
            new AddrPort("omsg104" ,2851),
            new AddrPort("omsg105" ,2864),
            new AddrPort("omsg106" ,2877),
            new AddrPort("omsg101" ,2813),
            new AddrPort("omsg102" ,2826),
            new AddrPort("omsg103" ,2839),
            new AddrPort("omsg104" ,2852),
            new AddrPort("omsg105" ,2865),
            new AddrPort("omsg106" ,2878),
            new AddrPort("omsg101" ,2814),
            new AddrPort("omsg102" ,2827),
            new AddrPort("omsg103" ,2840),
            new AddrPort("omsg104" ,2853),
            new AddrPort("omsg105" ,2866),
            new AddrPort("omsg106" ,2879),
        };
        /// <summary>
        /// 
        /// </summary>
        public static AddrPort[] CommunityAddrPortArr = new[]
        {
            new AddrPort("msg101", 2805),
            new AddrPort("msg102", 2815),
            new AddrPort("msg103", 2825),
            new AddrPort("msg104", 2835),
            new AddrPort("msg105", 2845),
            new AddrPort("msg101", 2806),
            new AddrPort("msg102", 2816),
            new AddrPort("msg103", 2826),
            new AddrPort("msg104", 2836),
            new AddrPort("msg105", 2846),
            new AddrPort("msg101", 2807),
            new AddrPort("msg102", 2817),
            new AddrPort("msg103", 2827),
            new AddrPort("msg104", 2837),
            new AddrPort("msg105", 2847),
            new AddrPort("msg101", 2808),
            new AddrPort("msg102", 2818),
            new AddrPort("msg103", 2828),
            new AddrPort("msg104", 2838),
            new AddrPort("msg105", 2848),
            new AddrPort("msg101", 2809),
            new AddrPort("msg102", 2819),
            new AddrPort("msg103", 2829),
            new AddrPort("msg104", 2839),
            new AddrPort("msg105", 2849),
            new AddrPort("msg101", 2810),
            new AddrPort("msg102", 2820),
            new AddrPort("msg103", 2830),
            new AddrPort("msg104", 2840),
            new AddrPort("msg105", 2850),
            new AddrPort("msg101", 2811),
            new AddrPort("msg102", 2821),
            new AddrPort("msg103", 2831),
            new AddrPort("msg104", 2841),
            new AddrPort("msg105", 2851),
            new AddrPort("msg101", 2812),
            new AddrPort("msg102", 2822),
            new AddrPort("msg103", 2832),
            new AddrPort("msg104", 2842),
            new AddrPort("msg105", 2852),
            new AddrPort("msg101", 2813),
            new AddrPort("msg102", 2823),
            new AddrPort("msg103", 2833),
            new AddrPort("msg104", 2843),
            new AddrPort("msg105", 2853),
            new AddrPort("msg101", 2814),
            new AddrPort("msg102", 2824),
            new AddrPort("msg103", 2834),
            new AddrPort("msg104", 2844),
            new AddrPort("msg105", 2854),
        };
    }
}