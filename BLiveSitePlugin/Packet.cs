using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Codeplex.Data;
namespace BLiveSitePlugin
{
    public class AudienceCount
    {
        //"{\"movie_id\":\"139217\",\"viewers\":\"5999\",\"live_viewers\":1406}"

        public string movie_id { get; set; }
        public string viewers { get; set; }
        public long live_viewers { get; set; }
    }
    public class Packet
    {
        const int BLIVE_SOCKET_TYPE_CHAT = 0;
        const int BLIVE_SOCKET_TYPE_AUDIENCE_COUNT = 1;
        const int BLIVE_SOCKET_TYPE_SYSTEM_MESSAGE = 2;
        const int BLIVE_SOCKET_TYPE_LIVE_END = 3;
        const int BLIVE_SOCKET_TYPE_STATUS = 4;
        const int BLIVE_SOCKET_TYPE_LIVE_START = 5;
        const int BLIVE_SOCKET_TYPE_BLACKLIST_ADD = 6;
        const int BLIVE_SOCKET_TYPE_BLACKLIST_DELETE = 7;
        const int BLIVE_SOCKET_TYPE_MODERATOR_ADD = 8;
        const int BLIVE_SOCKET_TYPE_MODERATOR_DELETE = 9;
        public static IPacket Parse(string str)
        {

            IPacket ret = null;
            try
            {
                var c = str[0];
                switch (c)
                {
                    case '0':
                        {
                            var content = str.Substring(1);
                            var context = JsonConvert.DeserializeObject<Low.WebsocketContext2>(content);
                            ret = new PacketOpen(context);
                        }
                        break;
                    case '1':
                        break;
                    case '2':
                        break;
                    case '3':
                        {
                            var content = str.Substring(1);
                            if (string.IsNullOrEmpty(content))
                            {
                                ret = new PacketPong();
                            }
                            else
                            {
                                ret = new PacketPong(content);
                            }
                        }
                        break;
                    case '4':
                        var c1 = str[1];
                        switch (c1)
                        {
                            case '0':
                                ret = new PacketMessageConnect();
                                break;
                            case '1':
                                break;
                            case '2':
                                // strの値
                                // 42["App\\Events\\MessageReceived","message.received.4873",{"message":{"user_id":2,"room_id":4873,"content":"コメントテスト","updated_at":"2024-10-11T14:47:44.000000Z","created_at":"2024-10-11T14:47:44.000000Z","id":63635},"socket":null}]
                                var jsonArray = JsonConvert.DeserializeObject<object[]>(str.Substring(2));
                                string eventType = jsonArray[0].ToString();
                                var messageData = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonArray[2].ToString());
                                var message = JsonConvert.DeserializeObject<Dictionary<string, object>>(messageData["message"].ToString());
                                if (eventType == "App\\Events\\MessageReceived")
                                {
                                    int type;
                                    /*
                                    if (t.type.GetType() == typeof(string))
                                    {
                                        type = int.Parse(t.type);
                                    }
                                    else
                                    {
                                        type = (int)t.type;
                                    }
                                    */
                                    type = BLIVE_SOCKET_TYPE_CHAT;
                                    if (type == BLIVE_SOCKET_TYPE_CHAT)
                                    {
                                        int userId = Convert.ToInt32(message["user_id"]);
                                        var comment = new Low.Item
                                        {
                                            chat_id = Convert.ToString(message["id"]),
                                            user_id = Convert.ToString(message["user_id"]),
                                            user_name = Convert.ToString(message["user_name"]),
                                            room_id = Convert.ToString(message["room_id"]),
                                            message = Convert.ToString(message["content"]),
                                            cre_dt = Convert.ToString(message["created_at"]),
                                        };
                                        ret = new PacketMessageEventMessageChat(comment);
                                    }
                                    else if (type == BLIVE_SOCKET_TYPE_AUDIENCE_COUNT)
                                    {
                                        var audi = JsonConvert.DeserializeObject<AudienceCount>(message.ToString());
                                        ret = new PacketMessageEventMessageAudienceCount(audi);
                                    }
                                    else if (type == BLIVE_SOCKET_TYPE_LIVE_END)
                                    {
                                        ret = new PacketMessageEventMessageLiveEnd();
                                    }
                                    else if (type == BLIVE_SOCKET_TYPE_LIVE_START)
                                    {
                                        //"42[\"message\",\"{\\\"type\\\":5,\\\"data\\\":{\\\"movie_id\\\":\\\"427901\\\"}}\"]"
                                        ret = new PacketMessageEventMessageLiveStart();
                                    }
                                    else if (type == BLIVE_SOCKET_TYPE_MODERATOR_ADD)
                                    {
                                        //なぜかtypeが文字列！！
                                        //"42[\"message\",\"{\\\"type\\\":\\\"8\\\",\\\"data\\\":{\\\"owner_to_moderator_user_id\\\":\\\"42978100\\\"}}\"]"
                                        ret = new PacketMessageEventMessageModeratorAdd();
                                    }
                                    else if (type == BLIVE_SOCKET_TYPE_MODERATOR_DELETE)
                                    {
                                        //なぜかtypeが文字列！！
                                        //"42[\"message\",\"{\\\"type\\\":\\\"9\\\",\\\"data\\\":{\\\"owner_to_moderator_user_id\\\":\\\"42978100\\\"}}\"]"
                                        ret = new PacketMessageEventMessageModeratorDelete();
                                    }
                                }
                                break;
                            case '3':
                                break;
                            case '4':
                                break;
                            case '5':
                                break;
                        }
                        break;
                    case '5':
                        ret = new PacketUpgrade();
                        break;
                    case '6':
                        ret = new PacketNoop();
                        break;
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
