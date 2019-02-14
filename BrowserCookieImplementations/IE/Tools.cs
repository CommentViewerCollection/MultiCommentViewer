using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static BrowserCookieImplementations.IE.NativeMethods;
namespace BrowserCookieImplementations.IE
{
    static class Tools
    {
        public static Dictionary<string, string> GetCookieDict(string cookieData)
        {
            var dict = new Dictionary<string, string>();
            var matches = Regex.Matches(cookieData, "(?<key>[^=]+)=(?<val>[^;]+?)(?:(?:;\\s*)|$)");
            foreach (Match match in matches)
            {
                var key = match.Groups["key"].Value;
                var val = match.Groups["val"].Value;
                dict.Add(key, val);
            }
            return dict;
        }
        public static string GetCookieData(string url)
        {
            uint length = 1024;
retry:
            var cookieData = new StringBuilder((int)length);
            var b = InternetGetCookieEx(url, null, cookieData, ref length, INTERNET_COOKIE_HTTPONLY, IntPtr.Zero);
            if (b)
            {
                var a = cookieData.ToString();
                return a;
            }
            else
            {
                var errorCode = Marshal.GetLastWin32Error();
                if (errorCode == ERROR_INSUFFICIENT_BUFFER)
                {
                    //lengthには適切な値が設定されているから何もせずにもう一回実行すればいい。
                    goto retry;
                }
                else
                {
                    throw new IECookieRetrieveFailedException("クッキーの取得に失敗しました", $"url={url}, errorCode={errorCode}");
                }
            }
        }
        public static string GetProtectedModeCookieData(string url)
        {
            uint length = 4096;
retry:
            var cookieData = new StringBuilder((int)length);
            var hResult = IEGetProtectedModeCookie(url, null, cookieData, ref length, INTERNET_COOKIE_HTTPONLY);
            if (hResult == S_OK)
            {
                var s = cookieData.ToString();
                return s;
            }
            else
            {
                var errorCode = hResult & 0x0000FFFF;
                if (errorCode == ERROR_INSUFFICIENT_BUFFER)
                {
                    //lengthが自動更新されない。
                    length *= 2;
                    goto retry;
                }
                else
                {
                    throw new IECookieRetrieveFailedException("クッキーの取得に失敗しました", $"url={url}, HRESULT={hResult}");
                }
            }
        }
    }
}
