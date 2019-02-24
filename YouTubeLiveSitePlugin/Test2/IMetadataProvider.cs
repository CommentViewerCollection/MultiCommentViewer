using System;
using System.Linq;
using System.Threading.Tasks;
using SitePlugin;
using System.Net;
using Common;
using System.Threading;

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
        protected void SendInfo(string message,InfoType type)
        {
            InfoReceived?.Invoke(this, new InfoData { Comment = message, Type = type });
        }
        protected Metadata ActionsToMetadata(dynamic actions)
        {
            var metadata = new Metadata();
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
                }
                else if (action.IsDefined("updateTitleAction"))
                {
                    var title = action.updateTitleAction.title.simpleText;
                    metadata.Title = title;
                }
                else if (action.IsDefined("updateDescriptionAction"))
                {

                }
            }
            return metadata;
        }
        public abstract Task ReceiveAsync(string ytCfg, string vid, CookieContainer cc);
        public IMetadataProvider(ILogger logger)
        {
            _logger = logger;
        }
    }
}