using System;
using System.Linq;
using System.Threading.Tasks;
using SitePlugin;
using System.Net;
using Common;
using System.Threading;
using System.Diagnostics;
using YouTubeLiveSitePlugin.Next;
using ryu_s.YouTubeLive.Message;

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
                if (action.ContainsKey("updateViewershipAction"))
                {
                    dynamic re;
                    if (action.updateViewershipAction.ContainsKey("viewCount"))//2018/06/22 仕様変更だろうか。こっちに移行したっぽい
                    {
                        re = action.updateViewershipAction.viewCount.videoViewCountRenderer;
                    }
                    else
                    {
                        re = action.updateViewershipAction.viewership.videoViewCountRenderer;
                    }
                    if (re.ContainsKey("isLive"))
                    {
                        var isLive = re.isLive;
                        metadata.IsLive = isLive;
                    }
                    if (re.ContainsKey("viewCount"))//viewCountが存在しない場合があった
                    {
                        if (re.viewCount.ContainsKey("runs"))
                        {
                            var lowViewCount = (string)re.viewCount.runs[0].text;
                            metadata.CurrentViewers = new string(lowViewCount.Where(char.IsDigit).ToArray());
                        }
                        else if (re.viewCount.ContainsKey("simpleText"))
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
                else if (action.ContainsKey("updateTitleAction"))
                {
                    string title;
                    if (action.updateTitleAction.title.ContainsKey("runs"))
                    {
                        title = "";
                        var runs = action.updateTitleAction.title.runs;
                        foreach (var run in runs)
                        {
                            if (run.ContainsKey("text"))
                            {
                                title += (string)run.text;
                            }
                        }
                    }
                    else if (action.updateTitleAction.title.ContainsKey("simpleText"))
                    {
                        title = action.updateTitleAction.title.simpleText;
                    }
                    else
                    {
                        title = null;
                    }
                    metadata.Title = title;
                }
                else if (action.ContainsKey("updateDescriptionAction"))
                {

                }
                else if (action.ContainsKey("updateToggleButtonTextAction"))
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
                else if (action.ContainsKey("updateDateTextAction"))
                {
                    //別の方法で経過時間を取得するから無視。
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
        public abstract Task ReceiveAsync(YtCfg ytCfg, string vid, CookieContainer cc);
        public IMetadataProvider(ILogger logger)
        {
            _logger = logger;
        }
    }
}