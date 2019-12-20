using Common;
using SitePlugin;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Text.RegularExpressions;
using SitePluginCommon;

namespace NicoSitePlugin
{
    class JikkyoCommentProvider : CommentProviderInternalBase
    {
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
            var channelId = ExtractJikkyoId(input);
            if (!channelId.HasValue)
            {
                //TODO:throwしたい
                return;
            }
            var jkInfo = await API.GetJikkyoInfoAsync(_dataSource, channelId.Value);
            _mainRoomThreadId = jkInfo.ThreadId;
            _roomCommentProvider = new Next20181012.XmlSocketRoomCommentProvider(jkInfo.Name, jkInfo.ThreadId, 1000, CreateStreamSoket(jkInfo.XmlSocketAddr, jkInfo.XmlSocketPort));
            _roomCommentProvider.CommentReceived +=async (s, e) =>
            {
                var chat = e;
                Debug.WriteLine(chat.Text);
                try
                {
                    var context = await CreateMessageContextAsync(chat, jkInfo.Name, false);
                    if (context != null)
                    {
                        RaiseMessageReceived(context);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogException(ex);
                }
            };
            _roomCommentProvider.InitialCommentsReceived +=async (s, e) =>
            {
                var chats = e;
                foreach (var chat in chats)
                {
                    Debug.WriteLine(chat.Text);
                    try
                    {
                        var context = await CreateMessageContextAsync(chat, jkInfo.Name, false);
                        if (context != null)
                        {
                            RaiseMessageReceived(context);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogException(ex);
                    }
                }
            };
            await _roomCommentProvider.ReceiveAsync();
            await Task.CompletedTask;
        }

        private NicoComment Convert(Chat chat, string roomName)
        {
            string id;
            if (chat.No.HasValue)
            {
                var shortName = Tools.GetShortRoomName(roomName);
                id = $"{shortName}:{chat.No}";
            }
            else
            {
                id = roomName;
            }
            var message = new NicoComment(chat.Raw, _siteOptions)
            {
                ChatNo = chat.No,
                CommentItems = new List<IMessagePart> { MessagePartFactory.CreateMessageText(chat.Text) },
                Id = id,
                Is184 = Tools.Is184UserId(chat.UserId),
                NameItems = null,
                PostTime = chat.Date.ToString("HH:mm:ss"),
                PostedDate = chat.Date,
                RoomName = roomName,
                UserIcon = null,
                UserId = chat.UserId,
            };
            return message;
        }
        Next20181012.XmlSocketRoomCommentProvider _roomCommentProvider;
        protected virtual IStreamSocket CreateStreamSoket(string host, int port)
        {
            return new StreamSocket(host, port, 4096, new SplitBuffer("\0"));
        }
        public override void Disconnect()
        {
            _roomCommentProvider?.Disconnect();
        }
        public int? ExtractJikkyoId(string input)
        {
            var match = Regex.Match(input, "jk(\\d+)");
            if (match.Success)
            {
                return int.Parse(match.Groups[1].Value);
            }
            else
            {
                return null;
            }
        }
        public override bool IsValidInput(string input)
        {
            var id = ExtractJikkyoId(input);
            return id.HasValue;
        }

        public override Task PostCommentAsync(string comment, string mail)
        {
            throw new NotImplementedException();
        }

        public JikkyoCommentProvider(ICommentOptions options, INicoSiteOptions siteOptions, IUserStoreManager userStoreManager, IDataSource dataSource, ILogger logger, ICommentProvider commentProvider)
            :base(options, siteOptions, userStoreManager, dataSource, logger)
        {
        }
    }
}
