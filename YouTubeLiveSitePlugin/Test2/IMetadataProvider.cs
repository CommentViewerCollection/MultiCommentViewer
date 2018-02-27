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
        protected EventHandler<string> noticed;
        public event EventHandler<string> Noticed
        {
            add { noticed += value; }
            remove { noticed -= value; }
        }
        protected CancellationTokenSource _cts;
        protected readonly ILogger _logger;
        public void Disconnect()
        {
            if (_cts != null)
            {
                _cts.Cancel();
            }
        }
        protected void SendNotice(string message)
        {
            noticed?.Invoke(this, message);
        }
        protected Metadata ActionsToMetadata(dynamic actions)
        {
            var metadata = new Metadata();
            foreach (var action in actions)
            {
                if (action.IsDefined("updateViewershipAction"))
                {
                    var re = action.updateViewershipAction.viewership.videoViewCountRenderer;
                    if (re.IsDefined("isLive"))
                    {
                        var isLive = re.isLive;
                        metadata.IsLive = isLive;
                    }
                    if (re.IsDefined("viewCount"))//viewCountが存在しない場合があった
                    {
                        var lowViewCount = (string)re.viewCount.runs[0].text;
                        metadata.CurrentViewers = new string(lowViewCount.Where(char.IsDigit).ToArray());
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