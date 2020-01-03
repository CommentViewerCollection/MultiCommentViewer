using SitePlugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using Codeplex.Data;
using SitePluginCommon.AutoReconnection;

namespace TwicasSitePlugin
{
    interface IInternalMessage
    {
        string Raw { get; }
    }
    class InternalComment : IInternalMessage
    {
        public string Raw { get; set; }
        public long Id { get; set; }
        public string Message { get; set; }
        public string RawMessage { get; set; }
        public bool HasMention { get; set; }
        public DateTime CreatedAt { get; set; }
        public object SpecialImage { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string ScreenName { get; set; }
        public string ProfileImageUrl { get; set; }
        public long Grade { get; set; }
    }
    class UnknownMessage : IInternalMessage
    {
        public string Raw { get; set; }
    }
    class WebsocketMessageProvider : IProvider
    {
        private readonly IWebsocket _websocket;
        private readonly IDataServer _server;

        public Guid SiteContextGuid { get; set; }
        public IProvider Master { get; set; }
        public bool IsFinished { get; }
        public Task Work { get; set; }
        public ProviderFinishReason FinishReason { get; }

        public event EventHandler<IInternalMessage> MessageReceived;
        public System.Net.CookieContainer Cc { get; set; }
        public string BroadcasterId { get; set; }
        private async Task ConnectAsync()
        {
            var (context, contextRaw) = await API.GetLiveContext(_server, BroadcasterId, Cc);
            var liveId = context.MovieId;
            var wsUrl = await API.GetWebsocketUrl(_server, liveId);
            await _websocket.ReceiveAsync(wsUrl);
        }
        public WebsocketMessageProvider(IWebsocket websocket, IDataServer server)
        {
            _websocket = websocket;
            _server = server;
            websocket.Opened += Websocket_Opened;
            websocket.Received += Websocket_Received;
        }

        private void Websocket_Received(object sender, string e)
        {
            var raw = e;
            var messages = MessageParser.Parse(raw);
            foreach (var internalMessage in messages)
            {
                MessageReceived?.Invoke(this, internalMessage);
            }
        }

        private void Websocket_Opened(object sender, EventArgs e)
        {
        }

        public void Start()
        {
            Work = ConnectAsync();
        }

        public void Stop()
        {
            _websocket.Disconnect();
        }
    }
    static class MessageParser
    {
        private static IInternalMessage ParseEachItem(dynamic item)
        {
            var raw = (string)item.ToString();
            if (item.type == "comment")
            {
                var comment = Tools.Deserialize<Low.Comment.RootObject>(raw);
                string profileImageUrl;
                if (comment.Author.ProfileImage.StartsWith("//"))
                {
                    profileImageUrl = "https:" + comment.Author.ProfileImage;
                }
                else
                {
                    profileImageUrl = comment.Author.ProfileImage;
                }
                var inter = new InternalComment
                {
                    Id = comment.Id,
                    Message = comment.Message,
                    RawMessage = comment.RawMessage,
                    HasMention = comment.HasMention,
                    CreatedAt = UnixTime2DateTime(comment.CreatedAt / 1000),
                    SpecialImage = comment.SpecialImage,
                    UserId = comment.Author.Id,
                    UserName = comment.Author.ScreenName,//2020/01/04 Twicasのバグだろうか。NameとScreenNameが逆な気がする
                    ScreenName = comment.Author.Name,
                    ProfileImageUrl = profileImageUrl,
                    Grade = comment.Author.Grade,
                    Raw = raw,
                };
                return inter;
            }
            else
            {
                return new UnknownMessage { Raw = raw };
            }
        }
        public static List<IInternalMessage> Parse(string raw)
        {
            var d = DynamicJson.Parse(raw);
            var list = new List<IInternalMessage>();
            foreach (var item in d)
            {
                var imsg = ParseEachItem(item);
                list.Add(imsg);
            }
            return list;
        }
        private static DateTime UnixTime2DateTime(long unixTime)
        {
            return DateTimeOffset.FromUnixTimeSeconds(unixTime).LocalDateTime;
        }
    }
}
