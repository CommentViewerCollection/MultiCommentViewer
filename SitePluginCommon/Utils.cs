using SitePlugin;
using System;
using System.Linq;
using System.Text;
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
        /// @コテハンがあったら抽出してuser.Nicknameに設定する
        /// </summary>
        /// <param name="message">コメント本文</param>
        /// <param name="user">コメントを投稿したユーザ</param>
        public static void SetNickname(string message, IUser user, string matchStr = "@|＠")
        {
            var nick = ExtractNickname(message, matchStr);
            if (!string.IsNullOrEmpty(nick))
            {
                user.Nickname = nick;
            }
        }
        /// <summary>
        /// @コテハンがあったら抽出してuser.Nicknameに設定する
        /// </summary>
        /// <param name="message">コメント本文</param>
        /// <param name="user">コメントを投稿したユーザ</param>
        public static void SetNickname(string message, IUser2 user)
        {
            var nick = ExtractNickname(message);
            if (!string.IsNullOrEmpty(nick))
            {
                user.Nickname = nick;
            }
        }
        /// <summary>
        /// 文字列から@ニックネームを抽出する
        /// 文字列中に@が複数ある場合は一番最後のものを採用する
        /// 数字だけのニックネームは不可
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string ExtractNickname(string text, string matchStr = "@|＠")
        {
            if (string.IsNullOrEmpty(text))
                return null;
            //2019/08/03 "|"の前後はcharに制限すべき。文字列を指定される可能性を考えたらほとんど使われないくせに複雑すぎる
            //やっぱりComboBoxで選んでもらう形式にしたい。面倒くさい。候補はenumで用意する。
            //
            //var sb = new StringBuilder();
            //sb.Replace("\\", "\\\\");
            //sb.Replace("?", "\\?");
            //sb.Replace("$", "\\$");
            //sb.Replace("(", "\\(");
            //sb.Replace(")", "\\)");
            //sb.Replace("[", "\\[");
            //sb.Replace("]", "\\]");
            var splitted = matchStr.Split('|').Where(k=>!string.IsNullOrWhiteSpace(k)).Select(k=>Regex.Escape(k)).ToList();
            var matchStrEscaped = splitted.Count == 0 ? Regex.Escape(matchStr) : string.Join("|", splitted);// sb.ToString();
            var a = splitted.Count == 0 ? Regex.Escape(matchStr) : string.Join("", splitted);// sb.ToString();

            var s = "(?:" + matchStrEscaped + ")([^" + a + "\\s]+)";
            var matches = Regex.Matches(text, s, RegexOptions.Singleline);
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
