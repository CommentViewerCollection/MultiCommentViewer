using System;
using Common;
using SitePlugin;
using System.Threading.Tasks;
using System.Net;
using System.Threading;
using System.Linq;
using System.Net.Http;
using System.Diagnostics;
using Codeplex.Data;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace ShowRoomSitePlugin
{
    abstract class InternalMessageBase : IInternalMessage
    {
        public InternalMessageBase(string raw)
        {
            Raw = raw;
        }
        public abstract InternalMessageType MessageType { get; }
        public string Raw { get; }
    }
    abstract class MsgInternalMessageBase : InternalMessageBase
    {
        protected string GetRawData()
        {
            var pos = Raw.IndexOf('{');
            return Raw.Substring(pos);
        }
        public MsgInternalMessageBase(string raw) : base(raw)
        {

        }
    }
    class Ping : InternalMessageBase
    {
        public override InternalMessageType MessageType => InternalMessageType.Ping;
        public Ping(string raw) : base(raw)
        {

        }
    }
    class Pong : InternalMessageBase
    {
        public override InternalMessageType MessageType => InternalMessageType.Pong;
        public Pong(string raw) : base(raw)
        {

        }
    }
    class T1 : MsgInternalMessageBase
    {
        /// <summary>
        /// comment
        /// </summary>
        public string Comment { get; internal set; }
        /// <summary>
        /// userName
        /// </summary>
        public string UserName { get; internal set; }
        /// <summary>
        /// UserId
        /// </summary>
        public long UserId { get; internal set; }
        /// <summary>
        /// 投稿日時(UTC)
        /// </summary>
        public long CreatedAt { get; internal set; }
        public T1() : base("") { }
        public T1(string raw) : base(raw)
        {

        }
        public static T1 Parse(string raw)
        {
            var json = DynamicJson.Parse(raw);

            var comment = (string)json.cm;
            var userName = (string)json.ac;
            var userId = (long)json.u;
            var createdAt = (long)json.created_at;
            return new T1(raw)
            {
                Comment = comment,
                CreatedAt = createdAt,
                UserId = userId,
                UserName = userName,
            };
        }
        public override InternalMessageType MessageType { get; } = InternalMessageType.t1;
    }
    class T2 : MsgInternalMessageBase
    {
        public long N { get; }
        public long Av { get; }
        public long D { get; }
        public string Ac { get; }
        public long CreatedAt { get; }
        public long U { get; }
        public long H { get; }
        public long G { get; }
        public long Gt { get; }
        public long At { get; }
        public T2(string raw) : base(raw)
        {
            var data = GetRawData();
            var json = DynamicJson.Parse(data);
            N = (long)json.n;
            Av = (long)json.av;
            D = (long)json.d;
            Ac = json.ac;
            CreatedAt = (long)json.created_at;
            U = (long)json.u;
            H = (long)json.h;
            G = (long)json.g;
            Gt = (long)json.gt;
            At = (long)json.at;
        }
        public override InternalMessageType MessageType { get; } = InternalMessageType.t2;
    }
    class T6 : MsgInternalMessageBase
    {
        public long CreatedAt { get; }
        public long U { get; }
        public long At { get; }
        public T6(string raw) : base(raw)
        {
            var data = GetRawData();
            var json = DynamicJson.Parse(data);
            CreatedAt = (long)json.created_at;
            U = (long)json.u;
            At = (long)json.at;
        }
        public override InternalMessageType MessageType { get; } = InternalMessageType.t2;
    }
    /// <summary>
    /// 未知のメッセージ
    /// </summary>
    class UnknownMessage : InternalMessageBase
    {
        public override InternalMessageType MessageType => InternalMessageType.Unknown;
        public UnknownMessage(string raw) : base(raw) { }
    }
    /// <summary>
    /// 不要なメッセージ
    /// </summary>
    class IgnoredMessage : InternalMessageBase
    {
        public override InternalMessageType MessageType => InternalMessageType.Ignored;
        public IgnoredMessage(string raw) : base(raw) { }
    }
    static class MessageParser
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="raw">json</param>
        /// <returns></returns>
        /// <exception cref="ParseException"></exception>
        public static IInternalMessage ParseMsg(string raw)
        {
            IInternalMessage internalMessage;
            var match = Regex.Match(raw, "\"t\":\"?(\\d+)");
            if (!match.Success)
            {
                //MSG	70724a:VGEKqmGT	{"av":1010794,"d":8,"ac":"@LTFismar_","cm":"頑張って...
                //MSG	70724a:VGEKqmGT	{"av":1018790,"d":0,"ac":"やいず216","cm":"みさきちゃん頑張ろうね！
                throw new ParseException(raw);
            }
            var type = match.Groups[1].Value;
            switch (type)
            {
                case "1":
                    internalMessage = new T1(raw);
                    break;
                case "2"://throwGifts
                    internalMessage = new T2(raw);
                    break;
                //case "3"://startVote
                //    internalMessage = null;
                //    break;
                //case "4"://endVote
                //    internalMessage = null;
                //    break;
                case "5":
                    //"MSG\t73d189:UPpINjdV\t{\"created_at\":1567284520,\"c\":0,\"p\":69484,\"t\":5}"
                    internalMessage = new IgnoredMessage(raw);
                    break;
                case "6":
                    internalMessage = new T6(raw);
                    break;
                case "8"://setTelop
                         //"MSG\t73d189:UPpINjdV\t{\"telops\":[{\"color\":{\"r\":255,\"b\":255,\"g\":255},\"text\":\"更新あり６：３０迄.:＊・゜次枠→８時\",\"type\":\"user\"}],\"telop\":\"更新あり６：３０迄.:＊・゜次枠→８時\",\"interval\":6000,\"t\":8,\"api\":\"https://www.showroom-live.com/live/telop?live_id=7590281\"}"
                    internalMessage = new IgnoredMessage(raw);
                    break;
                //case "9"://hideTelop
                //    internalMessage = null;
                //    break;
                case "11"://addGiftLog
                          //"MSG\t73d1db:FspoWUsJ\t{\"n\":100,\"av\":1012144,\"d\":0,\"ac\":\"PHYSALIS RX78GP02A strike back\",\"u\":2755708,\"created_at\":1567285801,\"g\":4,\"t\":11}"
                    internalMessage = new IgnoredMessage(raw);
                    break;
                case "100"://fetchAvatar
                           //"MSG\t73d189:UPpINjdV\t{\"created_at\":1567284607,\"t\":100}"
                    internalMessage = new IgnoredMessage(raw);
                    break;
                case "101"://endLive
                           //"MSG\t73d1db:FspoWUsJ\t{\"created_at\":1567287018,\"n\":1567288800,\"a\":0,\"t\":101}"
                    internalMessage = new IgnoredMessage(raw);
                    break;
                case "104"://startLive
                           //"MSG\tff123f20bfa089e3ea99adad05ce4d66760491380eec06f704be0e69a6cd1faf\t{\"created_at\":1567292533,\"t\":104}"
                    internalMessage = new IgnoredMessage(raw);
                    break;
                //case "302"://enterOwner
                //    internalMessage = null;
                //    break;
                //case "303"://leaveOwner
                //    internalMessage = null;
                //    break;
                default:
                    //"MSG\t6ce05f:8Jvx9D6M\t{\"created_at\":1561899486,\"t\":100}"
                    //"MSG\t6ce05f:8Jvx9D6M\t{\"telops\":[{\"color\":{\"r\":255,\"b\":255,\"g\":255},\"text\":\"最高です！ありがとう！ファミマ行けー！\",\"type\":\"user\"}],\"telop\":\"最高です！ありがとう！ファミマ行けー！\",\"interval\":6000,\"t\":8,\"api\":\"https://www.showroom-live.com/live/telop?live_id=7135327\"}"
                    //"MSG\tef9dc3:3M8XqUaq\t{\"created_at\":1655446677,\"c\":\"FF6C1A\",\"u\":2789719,\"me\":\"たいき leveled up to fan level 9!\",\"m\":\"たいきのファンレベルが9にあがりました！\",\"tt\":0,\"t\":18}"
                    //"MSG\tef9dc3:3M8XqUaq\t{\"created_at\":1655446700,\"c\":\"FF6C1A\",\"u\":6392080,\"me\":\"さんしろう is here again😊\",\"m\":\"さんしろうさんが2度目の訪問✨\",\"tt\":1,\"t\":18}"
                    internalMessage = new UnknownMessage(raw);
                    break;
            }
            return internalMessage;
        }
        public static IInternalMessage Parse(string raw)
        {
            var arr = raw.Split('\t');
            if (arr.Length == 0)
            {
                throw new ParseException(raw);
            }
            var command = arr[0];
            IInternalMessage internalMessage;
            switch (command)
            {
                case "MSG":
                    internalMessage = ParseMsg(arr[2]);
                    break;
                case "PING":
                    internalMessage = new Ping(raw);
                    break;
                case "PONG":
                    internalMessage = new Pong(raw);
                    break;
                case "ACK":
                    //"ACK\tshowroom"
                    internalMessage = null;
                    break;
                default:
                    throw new ParseException(raw);
            }
            if (internalMessage == null)
            {
                throw new ParseException(raw);
            }
            return internalMessage;
        }
    }
    class MessageProvider : IMessageProvider
    {
        public event EventHandler<IInternalMessage> Received;
        private readonly IWebsocket _ws;
        private readonly ILogger _logger;
        string _bcsvr_key;
        public async Task ReceiveAsync(string bcsvr_host, string bcsvr_key)
        {
            if (string.IsNullOrEmpty(bcsvr_key))
            {
                throw new ArgumentNullException(nameof(bcsvr_key));
            }
            _bcsvr_key = bcsvr_key;

            await _ws.ReceiveAsync("wss://" + bcsvr_host + "/");
        }
        public MessageProvider(IWebsocket websocket, ILogger logger)
        {
            _ws = websocket;
            _logger = logger;
            websocket.Opened += Websocket_Opened;
            websocket.Received += Websocket_Received;
        }
        public void Disconnect()
        {
            _ws.Disconnect();
        }

        private void Websocket_Received(object sender, string e)
        {
            try
            {
                var internalMessage = MessageParser.Parse(e);
                if (internalMessage != null)
                {
                    Received?.Invoke(this, internalMessage);
                }
            }
            catch (ParseException ex)
            {
                _logger.LogException(ex);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
            }
        }
        public async Task SendAsync(string message)
        {
            await _ws.SendAsync(message);
        }
        private async void Websocket_Opened(object sender, EventArgs e)
        {
            await SendAsync($"SUB\t{_bcsvr_key}");
        }
    }
}
