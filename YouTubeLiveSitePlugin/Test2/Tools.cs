using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using SitePlugin;
using Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Diagnostics;
using System.Text;
using Xceed.Wpf.Toolkit;

namespace YouTubeLiveSitePlugin.Test2
{
    static class Tools
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="channelHtml"></param>
        /// <returns>成功したらYtInitialData,失敗したらnull</returns>
        private static string ExtractYtInitialDataFromChannelHtmlInternal1(string channelHtml)
        {
            //window["ytInitialData"] = JSON.parse("{\"responseContext\":{\"se
            var match = Regex.Match(channelHtml, "window\\[\"ytInitialData\"\\]\\s*=\\s*(.+?})(?:\"\\))?;", RegexOptions.Singleline);
            if (!match.Success)
            {
                return null;
            }
            var preYtInitialData = match.Groups[1].Value;
            string ytInitialData;
            if (preYtInitialData.StartsWith("JSON"))
            {
                var preJson = preYtInitialData.Substring(12);//先頭の"JSON.parse(\""を消す
                //preJsonは\や"がエスケープされた状態になっているため外す
                var sb = new StringBuilder(preJson);
                sb.Replace("\\\\", "\\");
                sb.Replace("\\\"", "\"");
                ytInitialData = sb.ToString();
            }
            else
            {
                ytInitialData = preYtInitialData;
            }
            return ytInitialData;
        }
        private static string ExtractYtInitialDataFromChannelHtmlInternal2(string channelHtml)
        {
            //2020/11/09 ttps://www.youtube.com/channel/CHANNEL_ID?view_as=subscriber のHTMLの仕様が通常のものと違った
            //<script nonce=\"orPyHr12z1j4Y/4tOnK12A\">var ytInitialData = {\"responseContext\":
            var match = Regex.Match(channelHtml, "<script nonce=\"[^\"]+\">var ytInitialData\\s*=\\s*(.+?});</script>", RegexOptions.Singleline);
            if (!match.Success)
            {
                return null;
            }
            var ytInitialData = match.Groups[1].Value;
            return ytInitialData;
        }
        public static string ExtractYtInitialDataFromChannelHtml(string channelHtml)
        {
            var ytInitialData = ExtractYtInitialDataFromChannelHtmlInternal1(channelHtml);
            if (!string.IsNullOrEmpty(ytInitialData)) return ytInitialData;

            ytInitialData = ExtractYtInitialDataFromChannelHtmlInternal2(channelHtml);
            if (!string.IsNullOrEmpty(ytInitialData)) return ytInitialData;

            throw new ParseException(channelHtml);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="html"></param>
        /// <returns>{"isLiveNow":true,"startTimestamp":"2020-11-12T14:02:34+00:00"}</returns>
        public static string ExtractLiveBroadcastDetailsFromLivePage(string html)
        {
            //"liveBroadcastDetails":{"isLiveNow":true,"startTimestamp":"2020-11-12T14:02:34+00:00"}
            //liveBroadcastDetailsはページによってytPlayerConfigに入っている場合とytInitialPlayerResponseの場合を確認。
            //あんまり検証していないから詳細は不明。
            //ytplayerconfigの場合、liveBroadcastDetailsを要素に持つJSON自体が文字列としてJSONの値に格納されている関係で\"liveBroadcastDetails\"のようにエスケープされている。            

            //ちなみにHTMLのmetaタグでも配信開始日時が格納されていた。
            ////<meta itemprop="startDate" content="2020-11-12T14:02:34+00:00">

            var match = Regex.Match(html, "(?:\\\\)?\"liveBroadcastDetails(?:\\\\)?\":({.+?})");
            if (!match.Success) return null;
            return match.Groups[1].Value.Replace("\\\"", "\"");
        }
    }
}
