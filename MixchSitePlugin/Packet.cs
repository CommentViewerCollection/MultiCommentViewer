using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Codeplex.Data;
namespace MixchSitePlugin
{
    public class AudienceCount
    {
        public string movie_id { get; set; }
        public string viewers { get; set; }
        public long live_viewers { get; set; }
    }
    public class Packet
    {
        const int MIXCH_SOCKET_TYPE_CHAT = 0;
        const int MIXCH_SOCKET_TYPE_AUDIENCE_COUNT = 1;
        const int MIXCH_SOCKET_TYPE_SYSTEM_MESSAGE = 2;
        const int MIXCH_SOCKET_TYPE_LIVE_END = 3;
        const int MIXCH_SOCKET_TYPE_STATUS = 4;
        const int MIXCH_SOCKET_TYPE_LIVE_START = 5;
        const int MIXCH_SOCKET_TYPE_BLACKLIST_ADD = 6;
        const int MIXCH_SOCKET_TYPE_BLACKLIST_DELETE = 7;
        const int MIXCH_SOCKET_TYPE_MODERATOR_ADD = 8;
        const int MIXCH_SOCKET_TYPE_MODERATOR_DELETE = 9;
        public static IPacket Parse(string str)
        {

            IPacket ret = null;
            try
            {
                var context = JsonConvert.DeserializeObject<Low.WebsocketContext2>(str);
                switch (context.kind)
                {
                    case 0:
                    {
                        ret = new PacketBase(context);
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new ParseException(str, ex);
            }
            if (ret == null)
            {
                throw new ParseException(str);
            }
            return ret;
        }
    }
    public interface IPacket { }
    public class PacketOpen : IPacket
    {
        public Low.WebsocketContext2 Context { get; }
        public PacketOpen(Low.WebsocketContext2 context)
        {
            Context = context;
        }
    }
    public class PacketBase : IPacket
    {
        public Low.WebsocketContext2 Context { get; }
        public PacketBase(Low.WebsocketContext2 context)
        {
            Context = context;
        }
    }
    public class PacketClose : IPacket { }
    public class PacketPing : IPacket
    {
        public string Raw => "2";
    }
    public class PacketPong : IPacket
    {
        public string Content { get; } = "";
        public PacketPong() { }
        public PacketPong(string content)
        {
            Content = content;
        }
    }
    public abstract class PacketMessageBase : IPacket { }
    public class PacketMessageConnect : PacketMessageBase { }
    public class PacketMessageDisconnect : PacketMessageBase { }
    public abstract class PacketMessageEventBase : PacketMessageBase { }
    public class PacketMessageEventOpen : PacketMessageEventBase { }
    public class PacketMessageEventPong : PacketMessageEventBase { }
    public class PacketMessageEventError : PacketMessageEventBase { }
    public class PacketMessageEventMessageBase : PacketMessageEventBase { }
    public class PacketMessageEventMessageChat : PacketMessageEventMessageBase
    {
        public Low.Item Comment { get; }
        public PacketMessageEventMessageChat(Low.Item comment)
        {
            Comment = comment;
        }
    }
    public class PacketMessageEventMessageAudienceCount : PacketMessageEventMessageBase
    {
        public AudienceCount AudienceCount { get; }
        public PacketMessageEventMessageAudienceCount(AudienceCount audienceCount)
        {
            AudienceCount = audienceCount;
        }
    }
    public class PacketMessageEventMessageSystemMessage : PacketMessageEventMessageBase { }
    public class PacketMessageEventMessageLiveEnd : PacketMessageEventMessageBase { }
    public class PacketMessageEventMessageStatus : PacketMessageEventMessageBase { }
    public class PacketMessageEventMessageLiveStart : PacketMessageEventMessageBase { }
    public class PacketMessageEventMessageModeratorAdd : PacketMessageEventMessageBase { }
    public class PacketMessageEventMessageModeratorDelete : PacketMessageEventMessageBase { }
    public class PacketMessageAck : PacketMessageBase { }
    public class PacketMessageError : PacketMessageBase { }
    public class PacketMessageBinaryEvent : PacketMessageBase { }
    public class PacketMessageBinaryAck : PacketMessageBase { }
    public class PacketUpgrade : IPacket { }
    public class PacketNoop : IPacket { }
}
