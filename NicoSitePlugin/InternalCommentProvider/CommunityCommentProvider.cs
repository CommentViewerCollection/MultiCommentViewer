using Common;
using SitePlugin;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Linq;
using System.Text.RegularExpressions;
using SitePluginCommon;

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

        //TimeSpan _serverTimeDiff;


        protected virtual ChatProvider CreateChatProvider(int res_from)
        {
            return new ChatProvider(res_from);
        }
        protected virtual ProgramInfoProvider CreateProgramInfoProvider(IDataSource dataSource, string liveId, CookieContainer cc)
        {
            return new ProgramInfoProvider(dataSource, liveId, cc);
        }
        public override async Task ConnectAsync(string input, CookieContainer cc)
        {
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
                liveId = await API.GetCurrentCommunityLiveId(_dataSource, communityId,cc);
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
                    try
                    {
                        await chatTask;
                    }
                    catch (Exception ex)
                    {
                        SendSystemInfo($"{ex.Message}", InfoType.Notice);
                    }
                    break;
                }
            }
        }
        ChatProvider _chatProvider;
        ProgramInfoProvider _programInfoProvider;
        List<IXmlWsRoomInfo> _rooms;

        private void ProgramInfoProvider_ProgramInfoReceived(object sender, IProgramInfo e)
        {
            var metadata = new Metadata
            {
                Title = e.Title,
            };
            RaiseMetadataUpdated(metadata);

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
        }

        private async void ChatProvider_InitialCommentsReceived(object sender, InitialChatsReceivedEventArgs e)
        {
            try
            {                
                foreach(var chat in e.Chat)
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

        private async void ChatProvider_CommentReceived(object sender, ChatReceivedEventArgs e)
        {
            try
            {
                var messageContext = await CreateMessageContextAsync(e.Chat, e.RoomInfo.Name, false);
                if (messageContext == null) return;
                RaiseMessageReceived(messageContext);

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

        public CommunityCommentProvider(ICommentOptions options, INicoSiteOptions siteOptions, IUserStoreManager userStoreManager, IDataSource dataSource, ILogger logger,ICommentProvider commentProvider)
            :base(options, siteOptions, userStoreManager, dataSource,logger)
        {
        }
        private IUser GetUser(string userId)
        {
            return _userStoreManager.GetUser(SiteType.NicoLive, userId);
        }
    }
}
