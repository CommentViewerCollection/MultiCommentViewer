using Common;
using SitePlugin;
using System;
using System.Net;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using SitePluginCommon;

namespace NicoSitePlugin
{
    class NicoCasCommentProvider : CommentProviderInternalBase
    {
        public event EventHandler<IMessageContext> MessageReceived;
        public event EventHandler<IMetadata> MetadataUpdated;
        public event EventHandler<ConnectedEventArgs> Connected;
        public override void AfterDisconnected()
        {
        }

        public override void BeforeConnect()
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="liveId">jk\d+</param>
        /// <param name="cc"></param>
        /// <returns></returns>
        public override async Task ConnectAsync(string input, CookieContainer cc)
        {
            //https://cas.nicovideo.jp/user/38655/lv316253164
            var (_, userId, liveId) = IsValidInputWithUserId(input);
            var userInfo = API.GetNicoCasUserInfo(_dataSource, userId);
            await Task.CompletedTask;
        }
        public override void Disconnect()
        {
        }
        public override bool IsValidInput(string input)
        {
            var (isValid, b, c) = IsValidInputWithUserId(input);
            return isValid;
        }
        public static (bool isValid, string userId, string liveId) IsValidInputWithUserId(string input)
        {
            var match = Regex.Match(input, "cas\\.nicovideo\\.jp/user/(?<userid>\\d+)/(?<liveid>lv\\d+)");
            if (match.Success)
            {
                
                return (true, match.Groups["userid"].Value, match.Groups["liveid"].Value);
            }
            else
            {
                return (false, null, null);
            }
        }
        public NicoCasCommentProvider(ICommentOptions options, INicoSiteOptions siteOptions, IUserStoreManager userStoreManager, IDataSource dataSource, ILogger logger, ICommentProvider commentProvider)
            :base(options,siteOptions,userStoreManager,dataSource, logger)
        {
        }
    }
}
