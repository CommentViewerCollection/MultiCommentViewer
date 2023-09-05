using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MildomSitePlugin.InternalMessage
{
    public static class InternalMessageParser
    {
        static byte[] IntToBytesBE(int n)
        {
            var bytes = BitConverter.GetBytes(n);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }
            return bytes;
        }
        static byte[] hexstringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }
        static byte[] create_first_data()
        {
            var str0 = "{\"cmd\":\"enterRoom\",\"msg_data\":{\"user_id\":0,\"loginname\":\"guest429839\",\"noble_id\":0,\"noble_level\":0,\"avatar_decortation_id\":0,\"re_connect\":0,\"nonopara\":\"fr=web`sfr=pc`devi=Windows 10 64-bit`la=ja`gid=pc-gp-c5f6f156-12ab-4f7a-9f7a-13f103f2799a`na=Japan`loc=Japan|Kanagawa`clu=aws_japan`wh=1920*1080`rtm=2022-01-30T11:25:27.890Z`ua=Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/97.0.4692.99 Safari/537.36`click_time=`pcv=v3.8.5\"},\"req_id\":1}";
            var str1 = "12jj34ii56ee@#$bb&*!ii^%$nn";
            var actual = encryptToString(str0, str1);
            var r = new byte[8 + actual.Length];
            var a = new byte[] { 0, 3 };
            var o = new byte[] { 1, 1 };
            var e = IntToBytesBE(actual.Length);
            var i = Encoding.UTF8.GetBytes(actual);//Unicodeの良かったりするかも？

            a.CopyTo(r, 0);
            o.CopyTo(r, a.Length);
            e.CopyTo(r, a.Length + o.Length);
            i.CopyTo(r, a.Length + o.Length + e.Length);
            return r;
        }
        static void show_byte_array(byte[] a)
        {
            var c = string.Join(",", a.Select(b => String.Format("{0}", b)));
            Console.WriteLine(c);
        }
        static bool u_test()
        {
            var t = "{\"cmd\":\"enterRoom\",\"msg_data\":{\"user_id\":0,\"loginname\":\"guest429839\",\"noble_id\":0,\"noble_level\":0,\"avatar_decortation_id\":0,\"re_connect\":0,\"nonopara\":\"fr=web`sfr=pc`devi=Windows 10 64-bit`la=ja`gid=pc-gp-c5f6f156-12ab-4f7a-9f7a-13f103f2799a`na=Japan`loc=Japan|Kanagawa`clu=aws_japan`wh=1920*1080`rtm=2022-01-30T11:25:27.890Z`ua=Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/97.0.4692.99 Safari/537.36`click_time=`pcv=v3.8.5\"},\"req_id\":1}";
            var e = "12jj34ii56ee@#$bb&*!ii^%$nn";
            var actual = u(t, e);
            var expected = new byte[] { 197, 49, 124, 88, 79, 228, 204, 192, 108, 238, 215, 44, 145, 19, 131, 150, 250, 80, 97, 227, 191, 4, 116, 237, 27, 222, 132, 143, 182, 224, 38, 61, 175, 152, 11, 211, 47, 224, 136, 92, 193, 25, 16, 185, 247, 180, 60, 54, 170, 105, 170, 24, 251, 95, 63, 121, 213, 118, 153, 38, 5, 66, 235, 191, 11, 105, 228, 193, 252, 132, 66, 167, 134, 226, 177, 87, 178, 60, 66, 250, 16, 174, 149, 4, 190, 27, 10, 13, 28, 63, 188, 31, 245, 217, 57, 128, 66, 133, 87, 165, 142, 128, 217, 192, 68, 161, 222, 17, 206, 74, 170, 168, 247, 194, 50, 33, 174, 252, 97, 171, 223, 38, 204, 100, 153, 230, 163, 171, 152, 219, 76, 64, 254, 4, 137, 80, 4, 188, 35, 85, 22, 13, 121, 221, 143, 77, 194, 139, 181, 85, 135, 137, 203, 114, 68, 106, 161, 187, 231, 42, 231, 85, 45, 56, 234, 253, 70, 126, 133, 124, 156, 78, 80, 196, 75, 159, 190, 59, 157, 213, 109, 111, 175, 187, 34, 91, 142, 14, 237, 216, 191, 5, 215, 254, 61, 21, 15, 115, 145, 93, 157, 1, 175, 222, 35, 10, 64, 78, 177, 120, 231, 183, 93, 48, 107, 179, 72, 67, 132, 167, 160, 203, 22, 2, 253, 232, 204, 38, 217, 157, 149, 62, 24, 53, 32, 185, 85, 40, 159, 2, 101, 131, 218, 130, 244, 223, 90, 56, 253, 194, 219, 123, 142, 231, 247, 119, 56, 228, 212, 39, 0, 134, 120, 190, 124, 118, 14, 90, 140, 172, 2, 13, 215, 96, 245, 68, 160, 189, 18, 227, 86, 85, 122, 114, 100, 241, 214, 126, 195, 217, 193, 242, 91, 211, 31, 136, 62, 230, 1, 42, 78, 6, 130, 69, 176, 182, 37, 113, 135, 221, 109, 80, 147, 147, 82, 114, 2, 103, 104, 17, 96, 52, 18, 229, 55, 90, 95, 229, 99, 191, 139, 179, 243, 245, 198, 120, 162, 97, 198, 46, 219, 147, 100, 149, 90, 253, 111, 253, 197, 48, 142, 116, 65, 240, 191, 243, 193, 234, 34, 22, 18, 149, 221, 255, 175, 196, 160, 41, 254, 185, 69, 33, 143, 37, 68, 67, 129, 217, 47, 216, 75, 52, 22, 110, 253, 94, 200, 148, 124, 28, 245, 104, 117, 85, 48, 91, 171, 76, 163, 65, 67, 33, 191, 46, 165, 24, 92, 66, 13, 165, 55, 202, 112, 42, 150, 221, 198, 92, 109, 230, 181, 115, 205, 43, 27, 11, 200, 216, 136, 212, 173, 187, 108, 169, 157, 92, 246, 4, 152, 195, 44, 173, 155, 144, 62, 1, 159, 246, 139, 66, 105, 138, 76, 51, 164, 179, 80, 116, 124, 18, 158, 61, 107, 17, 50, 212, 139, 112, 128, 85, 74, 62, 143, 84, 130, 222, 62, 148, 42, 33, 215, 202, 17, 160 };
            var ddcc = Convert.ToBase64String(expected);
            return bytearray_equals(actual, expected);
        }
        internal static string encryptToString(string t, string e)
        {
            var a = u(t, e);
            var base64ed = Convert.ToBase64String(a);
            return base64ed;
        }
        static byte[] u(string t, string e)
        {
            var t1 = a(t);
            var b = a(e);
            var c = sub_u(i(t1, true), i(o(b), false));
            var ret = r(c, false);
            return ret;
        }
        /// <summary>
        /// Mildomサーバから送られてきたバイナリデータを復号する
        /// </summary>
        /// <param name="raw"></param>
        /// <returns></returns>
        public static string DecryptMessageWithBase64(byte[] raw)
        {
            var a = raw.Skip(8).ToArray();
            var b = Encoding.ASCII.GetString(a);
            return decryptToString(b, IV_Ke);
        }
        /// <summary>
        /// Mildomサーバから送られてきたバイナリデータを復号する
        /// </summary>
        /// <param name="raw"></param>
        /// <returns></returns>
        public static string DecryptMessage(byte[] raw)
        {
            var a = raw.Skip(8).ToArray();
            var b = btoa(a);
            return decryptToString(b, IV_L);
        }
        /// <summary>
        /// コメント等を暗号化・復号する際に必要なやつ。名称不明。変数名はL。
        /// </summary>
        static readonly string IV_L = "32l*!i1^l56e%$xnm1j9i@#$cr&";
        /// <summary>
        /// メタデータを暗号化・復号する際に必要なやつ。名称不明。変数名はyとKe。
        /// </summary>
        static readonly string IV_Ke = "12jj34ii56ee@#$bb&*!ii^%$nn";
        /// <summary>
        /// 暗号化してBase64してバイト配列化するやつ。function Ye(e)
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static byte[] EncryptMessageWithBase64(string str)
        {
            var encrypted = encryptToString(str, IV_Ke);
            var r = new byte[8 + encrypted.Length];
            var a = new byte[] { 0, 3 };
            var o = new byte[] { 1, 1 };
            var e = IntToBytesBE(encrypted.Length);
            var i = Encoding.ASCII.GetBytes(encrypted);//base64はasciiしか使わない

            a.CopyTo(r, 0);
            o.CopyTo(r, a.Length);
            e.CopyTo(r, a.Length + o.Length);
            i.CopyTo(r, a.Length + o.Length + e.Length);
            return r;
        }
        /// <summary>
        /// 暗号化してBase64してバイト配列化するやつ。function messageEncrypt(e)
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static byte[] EnctyptMessage(string str)
        {
            var t = u(str, IV_L);
            var r = new byte[8 + t.Length];
            var a = new byte[] { 0, 4 };
            var o = new byte[] { 1, 1 };
            var e = IntToBytesBE(t.Length);

            a.CopyTo(r, 0);
            o.CopyTo(r, a.Length);
            e.CopyTo(r, a.Length + o.Length);
            t.CopyTo(r, a.Length + o.Length + e.Length);
            return r;
        }
        static bool r_test()
        {
            var t = new uint[] { 1835213435, 574235236, 1801680236, 1718175609, 2020557428, 1635021626, 1433630068, 1952539760, 573317733, 1635017060, 578501154, 1735357040, 1936942450, 875706914, 1948396595, 1701278305, 825893492, 741355568, 577005858, 2105356602, 80 };

            var kk = s(r(t, true));
            var kddi = s(r(t, false));
            var e = true;
            var ret = r(t, e);
            var str = Encoding.UTF8.GetString(ret);
            return str == "{\"cmd\":\"luckyGiftBox:statusUpdate\",\"data\":{\"progress\":243,\"target\":1000,\"id\":1}}";
        }
        static bool btoa_test()
        {
            var t = new byte[] { 181, 54, 57, 184, 219, 30, 87, 142, 125, 202, 41, 54, 108, 53, 198, 205, 9, 215, 163, 4, 68, 49, 233, 67, 119, 119, 121, 238, 77, 79, 58, 182, 161, 102, 11, 79, 168, 253, 132, 226, 145, 193, 8, 77, 241, 184, 173, 228, 175, 237, 226, 220, 10, 144, 162, 62, 45, 157, 227, 104, 118, 253, 106, 40, 180, 224, 208, 58, 85, 235, 107, 71, 2, 172, 90, 107, 99, 39, 130, 171, 94, 127, 114, 64, 195, 239, 39, 171, 253, 16, 214, 234, 169, 243, 159, 176, 143, 111, 190, 164, 155, 97, 211, 163, 253, 46, 255, 116, 51, 208, 122, 133, 40, 230, 32, 19, 231, 1, 2, 235, 55, 225, 142, 76, 165, 212, 95, 32, 169, 108, 171, 48, 18, 40, 153, 69, 55, 138, 138, 136, 22, 200, 69, 50, 24, 159, 75, 86, 166, 243, 202, 214, 197, 173, 249, 16, 4, 187, 156, 47, 63, 156, 176, 123, 81, 136, 60, 237, 232, 208, 57, 87, 229, 12, 155, 43, 137, 35, 150, 86, 87, 89, 248, 222, 38, 197, 213, 86, 152, 238, 251, 110, 60, 83, 23, 85, 199, 134, 38, 111, 28, 75, 240, 177, 201, 34, 216, 110, 186, 60, 236, 164, 47, 67, 249, 61, 124, 69, 33, 94, 39, 116, 167, 206, 147, 254, 47, 39, 197, 131, 170, 235, 194, 226, 11, 138, 0, 146, 158, 249, 33, 184, 69, 43, 61, 72, 146, 15, 214, 85, 248, 48, 44, 102, 225, 168, 220, 247, 139, 9, 154, 202, 69, 93, 8, 60, 165, 40, 52, 54, 112, 237, 128, 45, 201, 246, 90, 97, 84, 64, 132, 79, 96, 42, 26, 240, 113, 115, 215, 225, 183, 127, 161, 106, 249, 184 };
            var ret = btoa(t);
            return ret == "tTY5uNseV459yik2bDXGzQnXowREMelDd3d57k1POrahZgtPqP2E4pHBCE3xuK3kr+3i3AqQoj4tneNodv1qKLTg0DpV62tHAqxaa2Mngqtef3JAw+8nq/0Q1uqp85+wj2++pJth06P9Lv90M9B6hSjmIBPnAQLrN+GOTKXUXyCpbKswEiiZRTeKiogWyEUyGJ9LVqbzytbFrfkQBLucLz+csHtRiDzt6NA5V+UMmyuJI5ZWV1n43ibF1VaY7vtuPFMXVceGJm8cS/CxySLYbro87KQvQ/k9fEUhXid0p86T/i8nxYOq68LiC4oAkp75IbhFKz1Ikg/WVfgwLGbhqNz3iwmaykVdCDylKDQ2cO2ALcn2WmFUQIRPYCoa8HFz1+G3f6Fq+bg=";
        }
        static int[] l(string t)
        {
            var e = t.Length;
            if (0 < e % 4)
                throw new Exception("Invalid string. Length must be a multiple of 4");
            var a = t.IndexOf("=");
            if (a == -1)
            {
                a = e;
            }
            var b = a == e ? 0 : 4 - a % 4;
            return new int[] { a, b };
        }
        static bool bytearray_equals(byte[] a, byte[] b)
        {
            if (a.Length != b.Length)
            {
                return false;
            }
            for (int i = 0; i < a.Length; i++)
            {
                if (a[i] != b[i])
                {
                    return false;
                }
            }
            return true;
        }
        static bool uintarray_equals(uint[] a, uint[] b)
        {
            if (a.Length != b.Length)
            {
                return false;
            }
            for (int i = 0; i < a.Length; i++)
            {
                if (a[i] != b[i])
                {
                    return false;
                }
            }
            return true;
        }
        static bool decryptToString_test()
        {
            var t = "GnxqSVLFMbhO9yJhsvzcG41N+7EweyjrUfQr3a2A43JqwwoNUz8JGWp6YOYaJJWXIL5yOPpvT/1D9vjTNHOGZFCc3YA6q3NjBWryA6VInVWLblwA";
            var e = "12jj34ii56ee@#$bb&*!ii^%$nn";//chunk_entry_1b9a08866bab2c45190f.jsに直書きされている
            var actual = decryptToString(t, e);
            return actual == "{\"cmd\":\"luckyGiftBox:statusUpdate\",\"data\":{\"progress\":243,\"target\":1000,\"id\":1}}";
        }
        internal static string decryptToString(string t, string e)
        {
            return s(c(t, e));
        }
        static bool s_test()
        {
            var t = new byte[] { 123, 34, 99, 109, 100, 34, 58, 34, 108, 117, 99, 107, 121, 71, 105, 102, 116, 66, 111, 120, 58, 115, 116, 97, 116, 117, 115, 85, 112, 100, 97, 116, 101, 34, 44, 34, 100, 97, 116, 97, 34, 58, 123, 34, 112, 114, 111, 103, 114, 101, 115, 115, 34, 58, 50, 52, 51, 44, 34, 116, 97, 114, 103, 101, 116, 34, 58, 49, 48, 48, 48, 44, 34, 105, 100, 34, 58, 49, 125, 125 };
            var actual = s(t);
            var expected = "{\"cmd\":\"luckyGiftBox:statusUpdate\",\"data\":{\"progress\":243,\"target\":1000,\"id\":1}}";
            return actual == expected;
        }
        static string s(byte[] t)
        {
            var a = Encoding.UTF8.GetString(t);
            return a;
        }
        static uint[] i(byte[] t, bool e)
        {
            var r = t.Length;
            var i = r >> 2;
            if ((3 & r) != 0)
            {
                i++;
            }
            uint[] n;
            if (e)
            {
                n = new uint[i + 1];
                n[i] = (uint)r;
            }
            else
            {
                n = new uint[i];
            }
            for (var o = 0; o < r; ++o)
            {
                var o1 = t[o] << ((3 & o) << 3);
                n[(uint)o >> 2] |= (uint)o1;
            }
            return n;
        }
        static byte[] o(byte[] t)
        {
            if (t.Length < 16)
            {
                var e = new byte[16];
                t.CopyTo(e, 0);
                return e;
            }
            else
            {
                return t;
            }
        }
        internal static byte[] a(string t)
        {
            return Encoding.UTF8.GetBytes(t);
            //int e = t.Length;
            //byte[] n = new byte[3 * e];
            //var r = 0;
            //var i = 0;
            //for (; i < e; i++)
            //{
            //    var o = t[i];
            //    if (o < 128)
            //    {
            //        n[r++] = (byte)o;
            //    }
            //    else if (o < 2048)
            //    {
            //        n[r++] = (byte)(192 | o >> 6);
            //        n[r++] = (byte)(128 | 63 & o);
            //    }
            //    else
            //    {
            //        if (!(o < 55296 || 57343 < o))
            //        {
            //            if (i + 1 < e)
            //            {
            //                var a = t[i + 1];
            //                if (o < 56320 && 56320 <= a && a <= 57343)
            //                {
            //                    var a1 = (65536 + ((1023 & o) << 10 | 1023 & a));
            //                    n[r++] = (byte)(240 | (a1 >> 18));
            //                    n[r++] = (byte)(128 | a1 >> 12 & 63);
            //                    n[r++] = (byte)(128 | a1 >> 6 & 63);
            //                    n[r++] = (byte)(128 | 63 & a1);
            //                    i++;
            //                    continue;
            //                }
            //            }
            //            throw new Exception("Malformed string");
            //        }
            //        n[r++] = (byte)(224 | o >> 12);
            //        n[r++] = (byte)(128 | o >> 6 & 63);
            //        n[r++] = (byte)(128 | 63 & o);
            //    }
            //}
            //return subarray(n, 0, r);
        }
        //static byte[] subarray(byte[] a, int start, int length)
        //{
        //    return a.Skip(start).Take(length).ToArray();
        //}
        public static byte[] c(string t, string e)
        {
            var t1 = Convert.FromBase64String(t);
            var b = a(e);
            var c = sub_c(i(t1, false), i(o(b), false));
            var ret = r(c, true);
            return ret;
        }
        static string btoa(byte[] t)
        {
            return Convert.ToBase64String(t);
        }
        static uint f(ulong t, uint e, uint n, int r, int i, uint[] o)
        {
            int a = (int)((n >> 5) ^ (e << 2));
            int b = (int)(e >> 3 ^ n << 4);
            int c = (int)(t ^ e);
            int d = (int)(o[3 & r ^ i] ^ n);
            uint ret = (uint)(a + (int)b ^ (int)c + d);
            return ret;
        }
        static bool sub_c_test()
        {
            var t = new uint[] { 1231715354, 3090269522, 1629681486, 467467442, 2986036621, 3945298736, 3710645329, 1927512237, 218809194, 420036435, 3865082474, 2543133722, 947043872, 4249841658, 3556308547, 1686532916, 2162007120, 1668524858, 66218501, 1436371109, 6057611 };
            var e = new uint[] { 1785344561, 1768502323, 1701131829, 1646535488, 556410466, 626944361, 7237156 };
            var actual = sub_c(t, e);
            var expected = new uint[] { 1835213435, 574235236, 1801680236, 1718175609, 2020557428, 1635021626, 1433630068, 1952539760, 573317733, 1635017060, 578501154, 1735357040, 1936942450, 875706914, 1948396595, 1701278305, 825893492, 741355568, 577005858, 2105356602, 80 };
            return uintarray_equals(actual, expected);
        }
        static readonly uint _l = 2654435769;
        public static uint[] sub_u(uint[] t, uint[] e)
        {
            var o = t.Length;
            var a = o - 1;
            var s = t[a];
            ulong u = 0;
            uint c = 0 | (uint)(Math.Floor(6.0 + 52 / o));
            uint n;
            for (; 0 < c; c--)
            {
                u += _l;
                var r = (int)(u >> 2 & 3);
                var i = 0;
                for (; i < a; i++)
                {
                    n = t[i + 1];
                    s = t[i] += f(u, n, s, i, r, e);
                }
                n = t[0];
                s = t[a] += f(u, n, s, i, r, e);
            }
            return t;
        }
        public static uint[] sub_c(uint[] t, uint[] e)
        {
            var o = t.Length;
            var a = o - 1;
            var s = t[0];
            uint u = (uint)(Math.Floor(6.0 + 52 / o) * _l);
            uint n;
            for (; 0 != u; u -= _l)
            {
                var r = (int)(u >> 2 & 3);
                var i = a;
                for (; 0 < i; i--)
                {
                    n = t[i - 1];
                    s = t[i] -= f(u, s, n, i, r, e);
                }
                n = t[a];
                s = t[0] -= f(u, s, n, i, r, e);
            }
            return t;
        }
        static byte[] r(uint[] t, bool e)
        {
            var n = (uint)t.Length;
            var r = n << 2;
            if (e)
            {
                n = t[n - 1];
                if (n < (r -= 4) - 3 || r < n)
                    return new byte[0];
                r = n;
            }
            var i = new byte[r];
            for (var o = 0; o < r; ++o)
                i[o] = (byte)(t[o >> 2] >> ((3 & o) << 3));
            return i;
        }
    }
}
