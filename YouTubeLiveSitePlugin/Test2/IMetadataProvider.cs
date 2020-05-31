using System;
using System.Linq;
using System.Threading.Tasks;
using SitePlugin;
using System.Net;
using Common;
using System.Threading;
using System.Diagnostics;

namespace YouTubeLiveSitePlugin.Test2
{
    abstract class IMetadataProvider
    {
        protected EventHandler<IMetadata> metaReceived;
        public event EventHandler<IMetadata> MetadataReceived
        {
            add { metaReceived += value; }
            remove { metaReceived -= value; }
        }
        public event EventHandler<InfoData> InfoReceived;
        protected CancellationTokenSource _cts;
        protected readonly ILogger _logger;
        public void Disconnect()
        {
            if (_cts != null)
            {
                _cts.Cancel();
            }
        }
        protected void SendInfo(string message, InfoType type)
        {
            InfoReceived?.Invoke(this, new InfoData { Comment = message, Type = type });
        }
        protected Metadata ActionsToMetadata(dynamic actions)
        {
            var metadata = new Metadata();
            string like = null;
            string dislike = null;
            foreach (var action in actions)
            {
                if (action.IsDefined("updateViewershipAction"))
                {
                    dynamic re;
                    if (action.updateViewershipAction.IsDefined("viewCount"))//2018/06/22 仕様変更だろうか。こっちに移行したっぽい
                    {
                        re = action.updateViewershipAction.viewCount.videoViewCountRenderer;
                    }
                    else
                    {
                        re = action.updateViewershipAction.viewership.videoViewCountRenderer;
                    }
                    if (re.IsDefined("isLive"))
                    {
                        var isLive = re.isLive;
                        metadata.IsLive = isLive;
                    }
                    if (re.IsDefined("viewCount"))//viewCountが存在しない場合があった
                    {
                        if (re.viewCount.IsDefined("runs"))
                        {
                            var lowViewCount = (string)re.viewCount.runs[0].text;
                            metadata.CurrentViewers = new string(lowViewCount.Where(char.IsDigit).ToArray());
                        }
                        else if (re.viewCount.IsDefined("simpleText"))
                        {
                            var lowViewCount = (string)re.viewCount.simpleText;
                            metadata.CurrentViewers = new string(lowViewCount.Where(char.IsDigit).ToArray());
                        }
                    }
                    else
                    {
                        metadata.CurrentViewers = "0";
                    }
                }
                else if (action.IsDefined("updateTitleAction"))
                {
                    string title;
                    if (action.updateTitleAction.title.IsDefined("runs"))
                    {
                        title = action.updateTitleAction.title.runs[0].text;
                    }
                    else if (action.updateTitleAction.title.IsDefined("simpleText"))
                    {
                        title = action.updateTitleAction.title.simpleText;
                    }
                    else
                    {
                        title = null;
                    }
                    metadata.Title = title;
                }
                else if (action.IsDefined("updateDescriptionAction"))
                {

                }
                else if (action.IsDefined("updateToggleButtonTextAction"))
                {
                    //"toggledText"は自分が押した場合の数値
                    //{{"updateToggleButtonTextAction":{"defaultText":{"simpleText":"1227"},"toggledText":{"simpleText":"1228"},"buttonId":"TOGGLE_BUTTON_ID_TYPE_LIKE"}}}
                    //{{"updateToggleButtonTextAction":{"defaultText":{"simpleText":"42"},"toggledText":{"simpleText":"43"},"buttonId":"TOGGLE_BUTTON_ID_TYPE_DISLIKE"}}}
                    if (action.updateToggleButtonTextAction.buttonId == "TOGGLE_BUTTON_ID_TYPE_LIKE")
                    {
                        like = (string)action.updateToggleButtonTextAction.defaultText.simpleText;
                    }
                    else if (action.updateToggleButtonTextAction.buttonId == "TOGGLE_BUTTON_ID_TYPE_DISLIKE")
                    {
                        dislike = (string)action.updateToggleButtonTextAction.defaultText.simpleText;
                    }
                }
                else if (action.IsDefined("updateDateTextAction"))
                {
                    //{"updateDateTextAction":{"dateText":{"simpleText":"41 分前にライブ配信開始"}}}
                    var input = (string)action.updateDateTextAction.dateText.simpleText;
                    var match = System.Text.RegularExpressions.Regex.Match(input, "(\\d+)");
                    if (match.Success)
                    {
                        var min = int.Parse(match.Groups[1].Value);
                        metadata.Elapsed = $"{min}分";
                    }
                }
                else
                {
                    Debug.WriteLine((string)action.ToString());
                }
            }
            string others = "";
            if (!string.IsNullOrEmpty(like))
            {
                others += $"高く評価:{like} ";
            }
            if (!string.IsNullOrEmpty(dislike))
            {
                others += $"低く評価:{dislike} ";
            }
            metadata.Others = others;
            return metadata;
        }
        public abstract Task ReceiveAsync(string ytCfg, string vid, CookieContainer cc);
        public IMetadataProvider(ILogger logger)
        {
            _logger = logger;
        }
    }
}