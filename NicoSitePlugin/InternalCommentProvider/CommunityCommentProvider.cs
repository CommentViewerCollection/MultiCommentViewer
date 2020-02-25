using Common;
using SitePlugin;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Linq;
using System.Text.RegularExpressions;
using SitePluginCommon;
using System.Diagnostics;

namespace NicoSitePlugin
{
    class CommunityCommentProvider : CommentProviderInternalBase
    {
        public override void BeforeConnect()
        {
            _rooms = new List<IXmlWsRoomInfo>();
            _mainRoomThreadId = int.MaxValue.ToString();
        }
        public override void AfterDisconnected()
        {

        }
        public override void Disconnect()
        {
            _chatProvider?.Disconnect();
            _programInfoProvider?.Disconnect();
        }
        public string ExtractLiveId(string input)
        {
            var match = Regex.Match(input, "(lv\\d+)");
            if (match.Success)
            {
                return match.Groups[1].Value;
            }
            else
            {
                return null;
            }
        }
        public string ExtractId(string prefix, string input)
        {
            var match = Regex.Match(input, "(" + prefix + "\\d+)");
            if (match.Success)
            {
                return match.Groups[1].Value;
            }
            else
            {
                return null;
            }
        }
        public bool ContainsLiveId(string input)
        {
            return !string.IsNullOrEmpty(ExtractId("lv", input));
        }
        public bool ContainsCommunityId(string input)
        {
            return !string.IsNullOrEmpty(ExtractId("co", input));
        }
        public bool ContainsChannelId(string input)
        {
            return !string.IsNullOrEmpty(ExtractId("ch", input));
        }
        bool IsValidChannelUrl(string input)
        {
            return Regex.IsMatch(input, "http://ch.nicovideo.jp/\\S+");
        }
        public override bool IsValidInput(string input)
        {
            return ContainsLiveId(input) || ContainsCommunityId(input) || ContainsChannelId(input) || IsValidChannelUrl(input);
        }

        TimeSpan _serverTimeDiff;


        protected virtual ChatProvider CreateChatProvider(int res_from)
        {
            return new ChatProvider(res_from);
        }
        protected virtual ProgramInfoProvider CreateProgramInfoProvider(IDataSource dataSource, string liveId, CookieContainer cc)
        {
            return new ProgramInfoProvider(dataSource, liveId, cc);
        }
        static readonly DateTime _baseTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private DateTime ConvertUnixMilliSec2DateTime(long unixtimeMilliSec)
        {
            return _baseTime.AddMilliseconds(unixtimeMilliSec);
        }
        CookieContainer _cc;
        public override async Task ConnectAsync(string input, CookieContainer cc)
        {
            _cc = cc;
            string liveId;
            if (ContainsLiveId(input))
            {
                liveId = ExtractLiveId(input);
                if (string.IsNullOrEmpty(liveId))
                {
                    SendSystemInfo("", InfoType.Error);
                    return;
                }
            }
            else if (ContainsCommunityId(input))
            {
                var communityId = ExtractId("co", input);
                liveId = await API.GetCurrentCommunityLiveId(_dataSource, communityId, cc);
            }
            else if (ContainsChannelId(input))
            {
                var channelId = ExtractId("ch", input);
                liveId = await API.GetCurrentChannelLiveId(_dataSource, channelId);
            }
            else if (IsValidChannelUrl(input))
            {
                var id = Tools.ExtractChannelScreenName(input);
                liveId = await API.GetCurrentChannelLiveId(_dataSource, id);
            }
            else
            {
                throw new ArgumentException("", nameof(input));
            }

            RaiseConnected(new ConnectedEventArgs
            {
                IsInputStoringNeeded = false,
                UrlToRestore = null,
            });
            //TODO:現在の部屋情報を取得する。これはコメント投稿にのみ必要
            var dataProps = await API.GetWatchDataProps(_dataSource, liveId, cc);
            _userId = dataProps.UserId;
            _serverTimeDiff = ConvertUnixMilliSec2DateTime(dataProps.ServerTime) - GetCurrentDateTime();


            var ps = await API.GetPlayerStatusAsync(_dataSource, liveId, cc);
            if (ps.Success)
            {
                _currentRoomThreadId = ps.PlayerStatus.Ms.Thread;
            }

            var chatProvider = CreateChatProvider(_siteOptions.ResNum);
            _chatProvider = chatProvider;
            chatProvider.TicketReceived += ChatProvider_TicketReceived;
            chatProvider.InitialCommentsReceived += ChatProvider_InitialCommentsReceived;
            chatProvider.CommentReceived += ChatProvider_CommentReceived;

            var chatTask = chatProvider.ReceiveAsync();
            var programInfoProvider = CreateProgramInfoProvider(_dataSource, liveId, cc);
            _programInfoProvider = programInfoProvider;
            programInfoProvider.ProgramInfoReceived += ProgramInfoProvider_ProgramInfoReceived;
            var piTask = programInfoProvider.ReceiveAsync();

            var tasks = new List<Task>
            {
                chatTask,
                piTask,
            };

            while (tasks.Count > 0)
            {
                var t = await Task.WhenAny(tasks);
                if (t == piTask)
                {
                    try
                    {
                        await piTask;
                    }
                    catch (Exception ex)
                    {
                        SendSystemInfo($"{ex.Message}", InfoType.Notice);
                    }
                    tasks.Remove(piTask);
                }
                else
                {
                    programInfoProvider.Disconnect();
                    chatProvider.Disconnect();
                    try
                    {
                        await piTask;
                    }
                    catch (Exception ex)
                    {
                        SendSystemInfo($"{ex.Message}", InfoType.Notice);
                    }
                    tasks.Remove(piTask);
                    try
                    {
                        await chatTask;
                    }
                    catch (Exception ex)
                    {
                        SendSystemInfo($"{ex.Message}", InfoType.Notice);
                    }
                    tasks.Remove(chatTask);
                    break;
                }
            }
        }
        ChatProvider _chatProvider;
        ProgramInfoProvider _programInfoProvider;
        List<IXmlWsRoomInfo> _rooms;
        long _vposBaseAt;
        string _userId;
        string _currentRoomThreadId;
        string _ticket;
        private DateTime GetCurrentServerTime()
        {
            return GetCurrentDateTime() + _serverTimeDiff;
        }
        protected virtual DateTime GetCurrentDateTime()
        {
            return DateTime.Now;
        }
        private void ProgramInfoProvider_ProgramInfoReceived(object sender, IProgramInfo e)
        {
            var metadata = new Metadata
            {
                Title = e.Title,
            };
            RaiseMetadataUpdated(metadata);

            _vposBaseAt = e.VposBaseAt;

            var newRooms = Tools.Distinct(_rooms, e.Rooms.Cast<IXmlWsRoomInfo>().ToList());
            if (newRooms.Count > 0)
            {
                _mainRoomThreadId = Math.Min(int.Parse(_mainRoomThreadId), newRooms.Select(r => int.Parse(r.ThreadId)).Min()).ToString();
                _rooms.AddRange(newRooms);
                _chatProvider.Add(newRooms);
            }
        }
        private void ChatProvider_TicketReceived(object sender, TicketReceivedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"{e.RoomInfo.Name}, {e.Ticket}");
            if (e.RoomInfo.ThreadId == _currentRoomThreadId)
            {
                _ticket = e.Ticket;
            }
        }

        private async void ChatProvider_InitialCommentsReceived(object sender, InitialChatsReceivedEventArgs e)
        {
            try
            {
                foreach (var chat in e.Chat)
                {
                    var messageContext = await CreateMessageContextAsync(chat, e.RoomInfo.Name, true);
                    if (messageContext == null) continue;
                    RaiseMessageReceived(messageContext);
                }
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                SendSystemInfo(ex.Message, InfoType.Debug);
            }
        }
        private void ChatProvider_CommentReceived(object sender, ChatReceivedEventArgs e)
        {
            SetMessage(e.ChatStr, e.RoomInfo.Name);
        }
        private async void SetMessage(string raw, string roomName)
        {
            //2020/02/17、"<chat "はXmlSocketの場合。Websocketのメッセージには未対応
            var chatStr = raw;
            IChat chat = null;
            if (chatStr.StartsWith("<chat "))
            {
                chat = new Chat(chatStr);
            }
            else if (chatStr.StartsWith("<chat_result "))
            {
                Debug.WriteLine(chatStr);
                //<chat_result thread="1622396675" status="0" no="4"/>
                //status=0で成功、失敗の場合4を確認済み
                //status=1は連投規制？
            }
            else
            {
                //<leave_thread thread="1622163911" reason="2"/>
#if DEBUG
                using (var sw = new System.IO.StreamWriter("nico_unknownData.txt", true))
                {
                    sw.WriteLine(chatStr);
                }
#endif
            }
            if (chat == null) return;
            try
            {
                var messageContext = await CreateMessageContextAsync(chat, roomName, false);
                if (messageContext == null) return;
                RaiseMessageReceived(messageContext);
                if (messageContext.Message is INicoComment comment)
                {
                    _latestCommentNo = comment.ChatNo;
                }
                //var cvm = CreateCommentViewModel(e.Chat, e.RoomInfo);
                //if (cvm == null) return;
                //CommentReceived?.Invoke(this, cvm);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                SendSystemInfo(ex.Message, InfoType.Debug);
            }
        }
        public override void SetMessage(string raw)
        {
            var testRoomName = "test";
            SetMessage(raw, testRoomName);
        }

        public CommunityCommentProvider(ICommentOptions options, INicoSiteOptions siteOptions, IUserStoreManager userStoreManager, IDataSource dataSource, ILogger logger, ICommentProvider commentProvider)
            : base(options, siteOptions, userStoreManager, dataSource, logger)
        {
        }
        private IUser GetUser(string userId)
        {
            return _userStoreManager.GetUser(SiteType.NicoLive, userId);
        }
        int? _latestCommentNo;
        public override async Task PostCommentAsync(string comment, string mail)
        {
            var cc = _cc;
            var threadId = _currentRoomThreadId;
            var latestCommentNo = _latestCommentNo;
            var blockNo = GetBlockNo(latestCommentNo);
            var postKey = await API.GetPostKey(_dataSource, threadId, blockNo, cc);
            var nowUnix = Common.UnixTimeConverter.ToUnixTime(GetCurrentServerTime());
            var baseUnix = _vposBaseAt;
            int vpos = (int)(nowUnix - baseUnix) * 100;
            var user_id = _userId;
            var premium = "1";
            var locale = "ja-jp";
            var ticket = _ticket;

            var encodedText = System.Web.HttpUtility.HtmlEncode(comment);
            var xml = $"<chat thread=\"{threadId}\" ticket=\"{ticket}\" vpos=\"{vpos}\" postkey=\"{postKey}\" mail=\"{mail}\" user_id=\"{user_id}\" premium=\"{premium}\" locale=\"{locale}\">{encodedText}</chat>\0";

            await _chatProvider.SendAsync(_rooms.First(r => r.ThreadId == _currentRoomThreadId), xml);
        }
        private int GetBlockNo(int? latestCommentNo)
        {
            if (latestCommentNo.HasValue)
            {
                var blockNo = (latestCommentNo.Value + 1) / 100;
                return blockNo;
            }
            else
            {
                return 0;
            }
        }
    }
}
