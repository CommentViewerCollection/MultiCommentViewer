using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Media;

namespace Mcv.PluginV2;

public static class Utils
{
    public static Color ColorFromArgb(string argb)
    {
        if (argb == null)
            throw new ArgumentNullException("argb");
        var pattern = "#(?<a>[0-9a-fA-F]{2})(?<r>[0-9a-fA-F]{2})(?<g>[0-9a-fA-F]{2})(?<b>[0-9a-fA-F]{2})";
        var match = System.Text.RegularExpressions.Regex.Match(argb, pattern, System.Text.RegularExpressions.RegexOptions.Compiled);

        if (!match.Success)
        {
            throw new ArgumentException("形式が不正");
        }
        else
        {
            var a = byte.Parse(match.Groups["a"].Value, System.Globalization.NumberStyles.HexNumber);
            var r = byte.Parse(match.Groups["r"].Value, System.Globalization.NumberStyles.HexNumber);
            var g = byte.Parse(match.Groups["g"].Value, System.Globalization.NumberStyles.HexNumber);
            var b = byte.Parse(match.Groups["b"].Value, System.Globalization.NumberStyles.HexNumber);
            return Color.FromArgb(a, r, g, b);
        }
    }
    public static string ColorToArgb(Color color)
    {
        var argb = color.ToString();
        return argb;
    }
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
    public static string? ExtractNickname(string text, string matchStr = "@|＠")
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
        var splitted = matchStr.Split('|').Where(k => !string.IsNullOrWhiteSpace(k)).Select(k => Regex.Escape(k)).ToList();
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
