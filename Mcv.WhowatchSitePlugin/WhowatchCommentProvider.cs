﻿using Mcv.PluginV2;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace WhowatchSitePlugin
{
    internal class WhowatchCommentProvider : ICommentProvider
    {
        #region ICommentProvider
        #region CanConnect
        private bool _canConnect;
        public bool CanConnect
        {
            get { return _canConnect; }
            set
            {
                if (_canConnect == value)
                    return;
                _canConnect = value;
                CanConnectChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        #endregion//CanConnect

        #region CanDisconnect
        private bool _canDisconnect;
        private readonly IDataServer _server;
        private readonly IWhowatchSiteOptions _siteOptions;
        private readonly ILogger _logger;

        public bool IsConnected => CanConnect;
        public bool CanDisconnect
        {
            get { return _canDisconnect; }
            set
            {
                if (_canDisconnect == value)
                    return;
                _canDisconnect = value;
                CanDisconnectChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        #endregion//CanDisconnect

        public event EventHandler<ConnectedEventArgs> Connected;
        public event EventHandler<IMetadata> MetadataUpdated;
        public event EventHandler CanConnectChanged;
        public event EventHandler CanDisconnectChanged;
        public event EventHandler<IMessageContext> MessageReceived;

        public async Task<ICurrentUserInfo> GetCurrentUserInfo(List<Cookie> cookies)
        {
            var cc = CreateCookieContainer(cookies);
            var me = await Api.GetMeAsync(_server, cc);

            return new CurrentUserInfo
            {
                Username = me.AccountName,
                UserId = me.UserPath,
                IsLoggedIn = !string.IsNullOrEmpty(me.UserPath),
            };
        }

        protected virtual CookieContainer CreateCookieContainer(List<Cookie> cookies)
        {
            var cc = new CookieContainer();
            try
            {
                foreach (var cookie in cookies)
                {
                    cc.Add(cookie);
                }
            }
            catch { }
            return cc;
        }
        private void SendSystemInfo(string message, InfoType type)
        {
            var context = InfoMessageContext.Create(new InfoMessage
            {
                Text = message,
                SiteType = SiteType.Whowatch,
                Type = type,
            });
            MessageReceived?.Invoke(this, context);
        }
        public bool IsLoggedIn => _me != null && !string.IsNullOrEmpty(_me.UserPath);
        public string LoggedInUsername => _me?.Name;
        long _live_id;
        long _lastUpdatedAt;
        DateTime? _startedAt;
        CookieContainer _cc;
        const string PUBLISHING = "PUBLISHING";
        protected virtual void BeforeConnect()
        {
            CanConnect = false;
            CanDisconnect = true;
            _cts = new CancellationTokenSource();
            _first.Reset();
            SendConnectedMessage();
            _startedAt = null;
            _elapsedTimer.Enabled = true;
        }
        System.Timers.Timer _elapsedTimer = new System.Timers.Timer();
        private void AfterDisconnected()
        {
            CanConnect = true;
            CanDisconnect = false;
            _me = null;
            SendDisconnectedMessage();
            _elapsedTimer.Enabled = false;
        }
        private void SendConnectedMessage()
        {

        }
        private void SendDisconnectedMessage()
        {

        }
        FirstCommentDetector _first = new FirstCommentDetector();
        public virtual async Task ConnectAsync(string input, List<Cookie> cookies)
        {
            //lastUpdatedAt==0でLiveDataを取る
            //配信中だったらそこに入っているInitialCommentsを送る
            //配信してなかったら始まるまでループ
            //websocketでコメントを取り始める

            BeforeConnect();
            LiveData initialLiveData = null;
            try
            {
                _cc = CreateCookieContainer(cookies);
                var itemDict = await GetPlayItemsAsync();
                MessageParser.Resolver = new ItemNameResolver(itemDict);

                _me = await Api.GetMeAsync(_server, _cc);

                long live_id = -1;
                var liveIdTest = Tools.ExtractLiveIdFromInput(input);
                if (liveIdTest.HasValue)
                {
                    //inputにLiveIdが含まれていた
                    live_id = liveIdTest.Value;
                }
                else
                {
                    //inputにuserPathが含まれているか調べる
                    var userPath = Tools.ExtractUserPathFromInput(input);
                    if (string.IsNullOrEmpty(userPath))
                    {
                        //LiveIdもuserPathも含まれていなかった
                        throw new InvalidInputException(input);
                    }
                    else
                    {
                        //userPathからLiveIdを取得する
                        live_id = await GetLiveIdFromUserPath(userPath, _cc, _cts.Token);
                    }
                }
                System.Diagnostics.Debug.Assert(live_id != -1);
                _live_id = live_id;

                _lastUpdatedAt = 0;
                initialLiveData = await Api.GetLiveDataAsync(_server, live_id, _lastUpdatedAt, _cc);
                if (initialLiveData.Live.LiveStatus != PUBLISHING)
                {
                    SendSystemInfo("LiveStatus: " + initialLiveData.Live.LiveStatus, InfoType.Debug);
                    SendSystemInfo("配信が終了しました", InfoType.Notice);
                    AfterDisconnected();
                    return;
                }
                RaiseMetadataUpdated(initialLiveData);
                _startedAt = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(initialLiveData.Live.StartedAt);
                _lastUpdatedAt = initialLiveData.UpdatedAt;
                foreach (var initialComment in Enumerable.Reverse(initialLiveData.Comments))
                {
                    Debug.WriteLine(initialComment.Message);

                    var message = MessageParser.ParseMessage(initialComment, "");
                    var context = CreateMessageContext(message, true);
                    if (context != null)
                    {
                        MessageReceived?.Invoke(this, context);
                    }
                }
            }
            catch (OperationCanceledException)//TaskCancelledもここに来る
            {
                AfterDisconnected();
                return;
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                SendSystemInfo(ex.Message, InfoType.Error);
                AfterDisconnected();
                return;
            }

            var internalCommentProvider = new InternalCommentProvider(_logger);
            _internalCommentProvider = internalCommentProvider;
            internalCommentProvider.MessageReceived += InternalCommentProvider_MessageReceived;
            //var d = internal

            var retryCount = 0;
Retry:
            var commentProviderTask = internalCommentProvider.ReceiveAsync(_live_id, initialLiveData.Jwt);


            var metadataProvider = new MetadataProvider(_server, _siteOptions);
            metadataProvider.MetadataUpdated += MetadataProvider_MetadataUpdated;
            var metaProviderTask = metadataProvider.ReceiveAsync(_live_id, initialLiveData.UpdatedAt, _cc);

            var tasks = new List<Task>();
            tasks.Add(commentProviderTask);
            tasks.Add(metaProviderTask);

            while (tasks.Count > 0)
            {
                var t = await Task.WhenAny(tasks);
                if (t == commentProviderTask)
                {
                    metadataProvider.Disconnect();
                    try
                    {
                        await metaProviderTask;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogException(ex);
                    }
                    tasks.Remove(metaProviderTask);
                    try
                    {
                        await commentProviderTask;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogException(ex);
                    }
                    tasks.Remove(commentProviderTask);
                }
                else if (t == metaProviderTask)
                {
                    try
                    {
                        await metaProviderTask;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogException(ex);
                    }
                    tasks.Remove(metaProviderTask);
                }
            }
            AfterDisconnected();
        }
        private void RaiseMetadataUpdated(LiveData liveData)
        {
            MetadataUpdated?.Invoke(this, new Metadata
            {
                Title = liveData.Live.Title,
                CurrentViewers = liveData.Live.ViewCount.ToString(),
                TotalViewers = liveData.Live.TotalViewCount.ToString(),
            });
        }
        private void MetadataProvider_MetadataUpdated(object sender, LiveData e)
        {
            _lastUpdatedAt = e.UpdatedAt;
            RaiseMetadataUpdated(e);
        }

        private WhowatchMessageContext CreateMessageContext(IWhowatchMessage message, bool isInitialComment)
        {
            string? userId = null;
            string? newNickname = null;
            if (message is IWhowatchComment comment)
            {
                userId = comment.UserId;
            }
            else if (message is IWhowatchItem item)
            {
                userId = item.UserId.ToString();
            }
            return new WhowatchMessageContext(message, userId, newNickname, isInitialComment);
        }

        InternalCommentProvider _internalCommentProvider;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="userPath"></param>
        /// <param name="cc"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        /// <exception cref="TaskCanceledException"></exception>
        /// <exception cref="OperationCanceledException"></exception>
        protected virtual async Task<long> GetLiveIdFromUserPath(string userPath, CookieContainer cc, CancellationToken ct)
        {
            while (true)
            {
                ct.ThrowIfCancellationRequested();
                var profile = await Api.GetProfileAsync(_server, userPath, cc);
                if (profile.Live == null)
                {
                    await Task.Delay(_siteOptions.LiveCheckIntervalSec * 1000, ct);
                }
                else
                {
                    return profile.Live.Id;
                }
            }
        }
        private void InternalCommentProvider_MessageReceived(object sender, IWhowatchMessage e)
        {
            var whowatchMessage = e;
            try
            {
                var context = CreateMessageContext(whowatchMessage, false);
                if (context != null)
                {
                    MessageReceived?.Invoke(this, context);
                }
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
            }
        }
        /// <summary>
        /// 文字列から@ニックネームを抽出する
        /// 文字列中に@が複数ある場合は一番最後のものを採用する
        /// 数字だけのニックネームは不可
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        protected string ExtractNickname(string text)
        {
            if (string.IsNullOrEmpty(text))
                return null;
            var matches = Regex.Matches(text, "(?:@|＠)(\\S+)", RegexOptions.Singleline);
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
        Dictionary<long, PlayItem> _itemDict;
        IMe _me;
        protected virtual Task<Dictionary<long, PlayItem>> GetPlayItemsAsync()
        {
            return Api.GetPlayItemsAsync(_server);
        }
        CancellationTokenSource _cts;
        public void Disconnect()
        {
            _cts?.Cancel();
            _internalCommentProvider?.Disconnect();
        }
        public async Task PostCommentAsync(string text)
        {
            var res = await Api.PostCommentAsync(_server, _live_id, _lastUpdatedAt, text, _cc);
        }
        public Guid SiteContextGuid { get; set; }

        #endregion //ICommentProvider
        public WhowatchCommentProvider(IDataServer server, IWhowatchSiteOptions siteOptions, ILogger logger)
        {
            _server = server;
            _siteOptions = siteOptions;
            _logger = logger;
            CanConnect = true;
            CanDisconnect = false;
            _elapsedTimer.Interval = 500;
            _elapsedTimer.Elapsed += ElapsedTimer_Elapsed;
        }
        protected virtual DateTime GetCurrentDateTime()
        {
            return DateTime.Now;
        }
        private void ElapsedTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (_startedAt.HasValue)
            {
                var elapsed = (GetCurrentDateTime().ToUniversalTime() - _startedAt.Value);
                MetadataUpdated?.Invoke(this, new Metadata
                {
                    Elapsed = Tools.ElapsedToString(elapsed),
                });
            }
        }

        public void SetMessage(string raw)
        {
            throw new NotImplementedException();
        }
    }
}
