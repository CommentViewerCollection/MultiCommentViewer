using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace SitePluginCommon
{
    public static class Utils
    {
        public static string ElapsedToString(TimeSpan elapsed)
        {
            string ret;
            if (elapsed.Hours == 0)
            {
                ret = elapsed.ToString("mm\\:ss");
            }
            else
            {
                ret = elapsed.ToString("h\\:mm\\:ss");
            }
            return ret;
        }
        public static DateTime UnixtimeToDateTime(long unixTimeStamp)
        {
            var dt = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            return dt.AddSeconds(unixTimeStamp).ToLocalTime();
        }
        /// <summary>
        /// 文字列から@ニックネームを抽出する
        /// 文字列中に@が複数ある場合は一番最後のものを採用する
        /// 数字だけのニックネームは不可
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string ExtractNickname(string text)
        {
            if (string.IsNullOrEmpty(text))
                return null;
            var matches = Regex.Matches(text, "(?:@|＠)([^@＠\\s]+)", RegexOptions.Singleline);
            if (matches.Count > 0)
            {
                foreach (Match match in matches.Cast<Match>().Reverse())
                {
                    var val = match.Groups[1].Value;
                    if (!Regex.IsMatch(val, "^[0-9０１２３４５６７８９]+$"))
                    {
                        return val;
                    }
                }
            }
            return null;
        }
    }
}
