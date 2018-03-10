using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace BrowserCookieImplementations.IE
{
    [Serializable]
    public class IECookieRetrieveFailedException : Exception
    {
        public string Detail { get; }
        public IECookieRetrieveFailedException(string message, string detail) : base(message)
        {
            Detail = detail;
        }
    }
    static class NativeMethods
    {
        public const int S_OK = 0;
        public const int ERROR_INVALID_PARAMETER = 87;
        public const int ERROR_INSUFFICIENT_BUFFER = 122;
        public const int ERROR_NO_MORE_ITEMS = 259;
        public const int ERROR_INTERNET_UNRECOGNIZED_SCHEME = 12006;

        public const int INTERNET_COOKIE_HTTPONLY = 0x2000;
        [DllImport("wininet.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern bool InternetGetCookieEx(string pchURL, string pchCookieName, StringBuilder pchCookieData, ref uint pcchCookieData, int dwFlags, IntPtr lpReserved);
        [DllImport("Ieframe.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public extern static long IEIsProtectedModeProcess(ref bool pbResult);
        [DllImport("Ieframe.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public extern static long IEGetProtectedModeCookie(string lpszURL, string lpszCookieName, StringBuilder pszCookieData, ref uint pcchCookieData, uint dwFlags);

    }
}
