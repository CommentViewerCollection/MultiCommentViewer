using System.Threading.Tasks;
using System.Net;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace Mcv.YouTubeLiveSitePlugin
{
    internal static class ChannelLiveFinder
    {
        public static async Task<List<string>> FindLiveVidsAsync(IYouTubeLiveServer server, IChannelUrl channelUrl)
        {
            var url = $"{channelUrl.Raw}/streams";
            var html = await server.GetEnAsync(url);//2022/11/30 Cookieを渡してしまうとアカウント設定かなんかのせいで強制的に英語にすることができなかった。
            var ytInitialDataStr = Tools.ExtractYtInitialDataFromChannelHtml(html);
            dynamic ytInitialData = JsonConvert.DeserializeObject(ytInitialDataStr);
            //"Live"タブを探す
            var liveTab = GetLiveTab(ytInitialData);

            var list = new List<string>();
            //"LIVE"ラベルが付いている動画を探す
            foreach (var content in liveTab.tabRenderer.content.richGridRenderer.contents)
            {
                if (!content.ContainsKey("richItemRenderer"))
                {
                    //一番最後の項目はcontinuationItemRendererになっていた。更に表示する用かと。
                    continue;
                }
                var videoId = (string)content.richItemRenderer.content.videoRenderer.videoId;
                var thumbnailText = (string)(GetText(content.richItemRenderer.content.videoRenderer.thumbnailOverlays[0].thumbnailOverlayTimeStatusRenderer.text) ?? "");
                if (thumbnailText == "LIVE")
                {
                    list.Add(videoId);
                }
            }
            return list;
        }
        private static string? GetText(dynamic d)
        {
            if (d.ContainsKey("runs"))
            {
                var title = "";
                var runs = d.runs;
                foreach (var run in runs)
                {
                    if (run.ContainsKey("text"))
                    {
                        title += (string)run.text;
                    }
                }
                return title;
            }
            else if (d.ContainsKey("simpleText"))
            {
                return d.simpleText;
            }
            else
            {
                return null;
            }
        }
        private static dynamic GetLiveTab(dynamic ytInitialData)
        {
            var tabs = ytInitialData.contents.twoColumnBrowseResultsRenderer.tabs;
            foreach (var tab in tabs)
            {
                string title;
                bool selected;
                if (tab.ContainsKey("tabRenderer"))
                {
                    title = (string)tab.tabRenderer.title;
                    selected = tab.tabRenderer.selected ?? false;
                }
                else
                {
                    throw new Exception();
                }
                if (title == "Live" && selected)
                {
                    return tab;
                }
            }
            throw new Exception();
        }
    }
}