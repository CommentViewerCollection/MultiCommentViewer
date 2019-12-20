using Codeplex.Data;
using SitePlugin;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MixerSitePlugin
{
    class ChatMessageData : IInternalMessage
    {
        public long Channel { get; set; }
        public string Id { get; set; }
        public string UserName { get; set; }
        public long UserId { get; set; }
        public string[] UserRoles { get; set; }
        public int? UserLevel { get; set; }
        public IEnumerable<IMessagePart> MessageItems { get; set; }
        public bool ShouldDrop { get; set; }
        public string Raw { get; set; }
    }
    public interface IInternalMessage
    {
        string Raw { get; }
    }
    class AuthMethodArgs
    {
        //bundle.jsの11509行目
        //o && c ? u.auth(s, o, c, a) : u.auth(s, null, null, a)
        //未ログイン時はUserIdとAuthKeyがnullということ？
        public long ChannelId { get; set; }
        public long UserId { get; set; }
        public string AuthKey { get; set; }
        public string AccessKey { get; set; }
    }
    class InternalMessageParser
    {
        /// <summary>
        /// サーバからwebsocketで送られてきた生のデータをInternalMessageに変換する
        /// </summary>
        /// <param name="raw">サーバから送られてきた生データ</param>
        /// <param name="methodReplyDict">Replyがどのmethodに対するものかを記録して適切な形に変換するためのdictionary。Reply以外を変換する場合はnull可</param>
        /// <returns></returns>
        public static IInternalMessage Parse(string raw, IDictionary<long, MethodBase> methodReplyDict)
        {
            IInternalMessage ret;
            dynamic d;
            try
            {
                d = DynamicJson.Parse(raw);
            }
            catch (Exception)
            {
                return new UnknownMessage(raw);
            }
            if (!d.IsDefined("type"))
            {
                ret = new UnknownMessage(raw);
            }
            else
            {
                switch (d.type)
                {
                    case "event":
                        ret = ParseEvent(raw, d);
                        break;
                    case "method":
                        ret = ParseMethod(raw, d);
                        break;
                    case "reply":
                        ret = ParseReply(raw, methodReplyDict, d);
                        break;
                    default:
                        ret = new UnknownMessage(raw);
                        break;
                }
            }
            return ret;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="raw"></param>
        /// <param name="methodReplyDict"></param>
        /// <param name="d"></param>
        /// <returns></returns>
        private static IInternalMessage ParseReply(string raw, IDictionary<long, MethodBase> methodReplyDict, dynamic d)
        {
            IInternalMessage ret;
            //{"type":"reply","error":"Already authenticated, please reconnect.","id":1}

            if (methodReplyDict == null)
            {
                //replyを変換する場合はmethodReplyDictが必須
                throw new ArgumentNullException(nameof(methodReplyDict));
            }
            var id = (long)d.id;
            if (!methodReplyDict.TryGetValue(id, out var method))
            {
                throw new ArgumentException($"methodReplyDictにid:{id}が登録されていないため、どのmehodに対するreplyなのか判定できない");
            }
            switch (method)
            {
                case OptOutEventsMethod _:
                    ret = new OptOutEventsReply(id);
                    break;
                case AuthMethod _:
                    ret = new AuthReply(id, d.data.authenticated, (string[])d.data.roles);
                    break;
                case PingMethod _:
                    ret = new PingReply(id);
                    break;
                default:
                    ret = new UnknownMessage(raw);
                    break;
            }

            return ret;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="raw"></param>
        /// <param name="d"></param>
        /// <returns></returns>
        private static IInternalMessage ParseMethod(string raw, dynamic d)
        {
            IInternalMessage ret;
            switch (d.method)
            {
                case "optOutEvents":
                    {
                        var id = (long)d.id;
                        var args = (string[])d.arguments;
                        ret = new OptOutEventsMethod(id, args);
                    }
                    break;
                case "auth":
                    {
                        var id = (long)d.id;
                        var channelId = (long)d.arguments[0];
                        var myId = (long?)d.arguments[1];
                        var token = (string)d.arguments[2];
                        //2019/10/29 d.arguments[3]が何なのか不明。

                        if (myId.HasValue)
                        {
                            ret = new AuthMethod(id, channelId, myId.Value, token);
                        }
                        else
                        {
                            ret = new AuthMethod(id, channelId);
                        }
                    }
                    break;
                default:
                    ret = new UnknownMessage(raw);
                    break;
            }

            return ret;
        }
        /// <summary>
        /// websocketで送られてきたEventメッセージの生データをIInternalMessageに変換する
        /// </summary>
        /// <param name="raw"></param>
        /// <param name="d"></param>
        /// <returns></returns>
        private static IInternalMessage ParseEvent(string raw, dynamic d)
        {
            IInternalMessage ret;
            switch (d.@event)
            {
                case "WelcomeEvent":
                    {
                        ret = new WelcomeEvent(d.data.server);
                    }
                    break;
                case "ChatMessage":
                    {
                        try
                        {
                            var data = d.data;
                            var messageItems = new List<IMessagePart>();
                            foreach (var item in d.data.message.message)
                            {
                                switch (item.type)
                                {
                                    case "text":
                                        messageItems.Add(Common.MessagePartFactory.CreateMessageText(item.text));
                                        break;
                                    case "emoticon":
                                        if (item.source == "external")
                                        {
                                            //{{"type":"emoticon","source":"external","pack":"https:\/\/uploads.mixer.com\/emoticons\/nmxv2s5c-276399.png","coords":{"x":72,"y":24,"width":24,"height":24},"text":":covent"}}
                                            messageItems.Add(new Common.MessageImage
                                            {
                                                Alt = item.text,
                                                Url = item.pack,
                                                X = (int)item.coords.x,
                                                Y = (int)item.coords.y,
                                                Width = (int)item.coords.width,
                                                Height = (int)item.coords.height,
                                            });
                                        }
                                        break;
                                    case "link":
                                        //{"type":"event","event":"ChatMessage","data":{"channel":523004,"id":"bac9e0a0-f9b5-11e9-8cfb-998f98ddb9b5","user_name":"reinyanBOT","user_id":12006809,"user_roles":["ChannelEditor","Mod","Subscriber","User"],"user_level":85,"user_avatar":"https://uploads.mixer.com/avatar/rnjnv7aa-12006809.jpg","message":{"message":[{"type":"text","data":"。私のYouTubeでは過去の放送がアップロードされています。チャンネル登録してくれるとテンション上がります>>>","text":"。私のYouTubeでは過去の放送がアップロードされています。チャンネル登録してくれるとテンション上がります>>>"},{"type":"link","url":"https://www.youtube.com/c/ReinyaNchannel","text":"https://www.youtube.com/c/ReinyaNchannel"}],"meta":{}},"user_ascension_level":99}}
                                        //{"type":"link","url":"https://www.youtube.com/c/ReinyaNchannel","text":"https://www.youtube.com/c/ReinyaNchannel"}
                                        messageItems.Add(new MessageLink { Text = item.text, Url = item.url });
                                        break;
                                    case "tag":
                                        //{"type":"event","event":"ChatMessage","data":{"channel":103551540,"id":"708b9560-f9ba-11e9-8041-79222f5b1a18","user_name":"IIwhite_LordII","user_id":7646222,"user_roles":["Subscriber","User"],"user_level":56,"user_avatar":"https://uploads.beam.pro/avatar/7945o1k4-7646222.jpg","message":{"message":[{"type":"text","data":"Twitch ","text":"Twitch "},{"text":"@ReaperDB","type":"tag","username":"reaperdb","id":30380194},{"type":"text","data":" ","text":" "}],"meta":{"shouldDrop":false}},"user_ascension_level":21}}
                                        messageItems.Add(Common.MessagePartFactory.CreateMessageText(item.text));
                                        break;
                                    case "image":
                                        //{"type":"event","event":"ChatMessage","data":{"channel":103551540,"id":"bef052d7-fc75-11e9-a950-000d3a0360b5","user_id":44745820,"user_name":"MembreSolide162","user_roles":["Subscriber","User"],"user_level":68,"user_ascension_level":34,"user_avatar":null,"message":{"message":[{"type":"image","data":"Shroud W","text":"Shroud W","url":"https://xforgeassets002.xboxlive.com/serviceid-562a2165-bb5d-46f6-828c-2acdc8b22eb9/90d5e009-9c1f-4a5b-80c3-7757724e27c9/114211413_63707467539294.5.png"},{"type":"text","data":"Escape from tarkov back to back in arrow again try it😁👍","text":"Escape from tarkov back to back in arrow again try it😁👍"}],"meta":{"images":{},"is_skill":true,"skill":{"skill_id":"ed062a17-3d4e-4501-ac0c-b2b63f73d670","skill_name":"Shroud W","execution_id":"bef052d7-fc75-11e9-a950-000d3a0360b5","icon_url":"https://xforgeassets002.xboxlive.com/serviceid-562a2165-bb5d-46f6-828c-2acdc8b22eb9/90d5e009-9c1f-4a5b-80c3-7757724e27c9/114211413_63707467539294.5.png","cost":50,"currency":"Embers"}}}}}
                                        messageItems.Add(new Common.MessageImage
                                        {
                                            Alt = item.text,
                                            Url = item.url,
                                            X = null,
                                            Y = null,
                                            Width = 60,
                                            Height = 60,
                                        });
                                        break;
                                    case "inaspacesuit":
                                        //{"type":"event","event":"ChatMessage","data":{"channel":90571077,"id":"2180c9b0-fcfb-11e9-83c2-9197da9e9973","user_name":"TDOT_DEEJAY","user_id":46376566,"user_roles":["Subscriber","User"],"user_level":108,"user_avatar":"https://uploads.mixer.com/avatar/avb6tagt-46376566.jpg","message":{"message":[{"type":"text","data":" ","text":" "},{"type":"inaspacesuit","username":"ninja","userId":101052282,"text":":ninjainaspacesuit"}],"meta":{"shouldDrop":false}},"user_ascension_level":34}}
                                        break;
                                    default:
                                        throw new ParseException(raw);
                                }
                            }
                            var roles = new List<string>();
                            foreach (var item in data.user_roles)
                            {
                                roles.Add(item);
                            }
                            bool shouldDrop;
                            if (data.message.meta.IsDefined("shouldDrop"))
                            {
                                shouldDrop = data.message.meta.shouldDrop;
                            }
                            else
                            {
                                shouldDrop = false;
                            }
                            //2019/11/03 botにはuser_levelが無いっぽい
                            //{"type":"event","event":"ChatMessage","data":{"channel":276399,"id":"de9b5c10-fe16-11e9-81be-47a1ca288e23","user_name":"HypeBot","user_roles":["Mod","User"],"user_id":696,"message":{"message":[{"data":"","text":"","type":"text"},{"type":"tag","username":"americanskull22","text":"@americanskull22","id":129123117},{"data":" is now hosting you!","text":" is now hosting you!","type":"text"}],"meta":{}}}}
                            var userLevel = data.IsDefined("user_level") ? (int)data.user_level : (int?)null;
                            var a = new ChatMessageData
                            {
                                Raw = raw,
                                Channel = (long)data.channel,
                                Id = data.id,
                                UserName = data.user_name,
                                UserId = (long)data.user_id,
                                MessageItems = messageItems,
                                UserLevel = userLevel,
                                UserRoles = roles.ToArray(),
                                ShouldDrop = shouldDrop,
                            };
                            ret = a;
                        }
                        catch (Exception)
                        {
                            ret = new UnknownMessage(raw);
                        }
                    }
                    break;
                case "UserUpdate":
                    {
                        //{"type":"event","event":"UserUpdate","data":{"roles":["Subscriber","User"],"user":121885488}}
                        var userId = (long)d.data.user;
                        var roles = (string[])d.data.roles;
                        ret = new UserUpdateEvent(userId, roles);
                    }
                    break;
                case "DeleteMessage":
                    {
                        //    //{"type":"event","event":"DeleteMessage","data":{"moderator":{"user_name":"Nostie","user_id":58111179,"user_roles":["Mod","Subscriber","User"],"user_level":51},"id":"ed0e6e50-f9ba-11e9-a062-d152ba44aeae"}}
                        var id = (string)d.data.id;
                        var name = (string)d.data.moderator.user_name;
                        var userid = (long)d.data.moderator.user_id;
                        var roles = (string[])d.data.moderator.user_roles;
                        var level = (long)d.data.moderator.user_level;
                        ret = new DeleteMessageEvent(id, new User(name, userid, roles, level));
                    }
                    break;
                case "PurgeMessage":
                    //    //{"type":"event","event":"PurgeMessage","data":{"moderator":{"user_name":"MCS_Kilo","user_id":31621807,"user_roles":["GlobalMod","User"],"user_level":97},"user_id":18740075}}
                    //    //{"type":"event","event":"PurgeMessage","data":{"user_id":18740075}}
                    ret = new UnknownMessage(raw);
                    break;
                case "UserJoin":
                    {
                        //{"type":"event","event":"UserJoin","data":{"originatingChannel":276399,"username":"EqualPhoenix23","roles":["User"],"id":131608671}}

                        var channel = (long)d.data.originatingChannel;
                        var username = (string)d.data.username;
                        var roles = (string[])d.data.roles;
                        var id = (long)d.data.id;
                        ret = new UserJoinEvent(channel, username, roles, id);
                    }
                    break;
                case "UserLeave":
                    {
                        //{"type":"event","event":"UserLeave","data":{"originatingChannel":276399,"username":"imkkrakenup","roles":["User"],"id":125523573}}
                        var channel = (long)d.data.originatingChannel;
                        var username = (string)d.data.username;
                        var roles = (string[])d.data.roles;
                        var id = (long)d.data.id;
                        ret = new UserLeaveEvent(channel, username, roles, id);
                    }
                    break;
                default:
                    ret = new UnknownMessage(raw);
                    break;
            }

            return ret;
        }
    }
    class UserJoinEvent : EventBase
    {
        public long OriginatingChannel { get; }
        public string UserName { get; }
        public string[] Roles { get; }
        public long Id { get; }
        public UserJoinEvent(long originatingChannel, string userName, string[] roles, long id)
        {
            OriginatingChannel = originatingChannel;
            UserName = userName;
            Roles = roles;
            Id = id;
        }
        public override string Raw
        {
            get
            {
                var rolesStr = "\"" + string.Join("\",\"", Roles) + "\"";
                var s = "{\"type\":\"event\",\"event\":\"UserLeave\",\"data\":{\"originatingChannel\":" + OriginatingChannel + ",\"username\":\"" + UserName + "\",\"roles\":[\"" + rolesStr + "\"],\"id\":" + Id + "}}";
                return s;
            }
        }
    }
    class UserLeaveEvent : EventBase
    {
        public long OriginatingChannel { get; }
        public string UserName { get; }
        public string[] Roles { get; }
        public long Id { get; }
        public UserLeaveEvent(long originatingChannel, string userName, string[] roles, long id)
        {
            OriginatingChannel = originatingChannel;
            UserName = userName;
            Roles = roles;
            Id = id;
        }
        public override string Raw
        {
            get
            {
                var rolesStr = "\"" + string.Join("\",\"", Roles) + "\"";
                var s = "{\"type\":\"event\",\"event\":\"UserJoin\",\"data\":{\"originatingChannel\":" + OriginatingChannel + ",\"username\":\"" + UserName + "\",\"roles\":[\"" + rolesStr + "\"],\"id\":" + Id + "}}";
                return s;
            }
        }
    }
    /// <summary>
    /// 未知のメッセージ。形式が分からないから扱いようが無い。
    /// </summary>
    class UnknownMessage : IInternalMessage
    {
        public UnknownMessage(string raw)
        {
            Raw = raw;
        }
        public string Raw { get; }
    }
    abstract class EventBase : IInternalMessage
    {
        public abstract string Raw { get; }
    }
    class WelcomeEvent : EventBase
    {
        //{"type":"event","event":"WelcomeEvent","data":{"server":"ecd05842-85c3-42b3-ac45-442a0da19b80"}}
        public string Server { get; }
        public WelcomeEvent(string server)
        {
            Server = server;
        }
        public override string Raw
        {
            get
            {
                var s = "{\"type\":\"event\",\"event\":\"WelcomeEvent\",\"data\":{\"server\":\"" + Server + "}}";
                return s;
            }
        }
    }
    class UserUpdateEvent : EventBase
    {
        //{"type":"event","event":"UserUpdate","data":{"roles":["Subscriber","User"],"user":121885488}}
        public long UserId { get; }
        public string[] Roles { get; }
        public UserUpdateEvent(long userId, string[] roles)
        {
            UserId = userId;
            Roles = roles;
        }
        public override string Raw
        {
            get
            {
                var rolesStr = "\"" + string.Join("\",\"", Roles) + "\"";
                var s = "{\"type\":\"event\",\"event\":\"UserUpdate\",\"data\":{\"roles\":[" + rolesStr + "],\"user\":" + UserId + "}}";
                return s;
            }
        }
    }
    class User
    {
        public string UserName { get; }
        public long UserId { get; }
        public string[] UserRoles { get; }
        public long UserLevel { get; }
        public User(string userName, long userId, string[] userRoles, long userLevel)
        {
            UserName = userName;
            UserId = userId;
            UserRoles = userRoles;
            UserLevel = userLevel;
        }
        public string Raw
        {
            get
            {
                var rolesStr = "\"" + string.Join("\",\"", UserRoles) + "\"";
                var s = "{\"user_name\":\"" + UserName + "\",\"user_id\":" + UserId + ",\"user_roles\":[" + rolesStr + "],\"user_level\":" + UserLevel + "}";
                return s;
            }
        }
    }
    class DeleteMessageEvent : EventBase
    {
        public string Id { get; }
        public User Moderator { get; }
        public DeleteMessageEvent(string id, User moderator)
        {
            Id = id;
            Moderator = moderator;
        }
        public override string Raw
        {
            get
            {
                //"{\"type\":\"event\",\"event\":\"DeleteMessage\",\"data\":{\"moderator\":{\"user_name\":\"iKronic_As\",\"user_id\":77724798,\"user_roles\":[\"Subscriber\",\"User\"],\"user_level\":35},\"id\":\"d95b0560-fa66-11e9-82db-c3c12e5c8ef6\"}}"
                var s = "{\"type\":\"event\",\"event\":\"DeleteMessage\",\"data\":{\"moderator\":" + Moderator.Raw + ",\"id\":\"" + Id + "\"}}";
                return s;
            }
        }
    }
    abstract class ReplyBase : IInternalMessage
    {
        public long Id { get; }
        public ReplyBase(long id)
        {
            Id = id;
        }
        public /*TODO:abstract*/ string Raw { get; }
    }
    class PingReply : ReplyBase
    {
        public PingReply(long id) : base(id)
        {
        }
    }
    class OptOutEventsReply : ReplyBase
    {
        public OptOutEventsReply(long id) : base(id)
        {
        }
    }
    class AuthReply : ReplyBase
    {
        public bool Authenticated { get; }
        public string[] Roles { get; }
        public AuthReply(long id, bool authenticated, string[] roles) : base(id)
        {
            Authenticated = authenticated;
            Roles = roles;
        }
    }
    abstract class MethodBase : IInternalMessage
    {
        /// <summary>
        /// methodとreplyを対応付けるためのID
        /// </summary>
        public long Id { get; }
        public MethodBase(long id)
        {
            Id = id;
        }
        public abstract string Raw { get; }
    }
    class OptOutEventsMethod : MethodBase
    {
        public string[] Arguments { get; }
        //{"type":"method","method":"optOutEvents","arguments":["UserJoin","UserLeave"],"id":0}
        public OptOutEventsMethod(long id, string[] arguments) : base(id)
        {
            Arguments = arguments;
        }
        public override string Raw
        {
            get
            {
                var argsStr = "\"" + string.Join("\",\"", Arguments) + "\"";
                var s = "{\"type\":\"method\",\"method\":\"optOutEvents\",\"arguments\":[" + argsStr + "],\"id\":" + Id + "}";
                return s;
            }
        }
        public override bool Equals(object obj)
        {
            if (obj is OptOutEventsMethod m)
            {
                return Id == m.Id && Arguments.SequenceEqual(m.Arguments);
            }
            else
            {
                return false;
            }
        }
        public override int GetHashCode()
        {
            return Id.GetHashCode() ^ Arguments.GetHashCode();
        }
        public override string ToString() => Raw;
    }
    class AuthMethod : MethodBase
    {
        //{"type":"method","method":"auth","arguments":[523004,714570,"01oX7WRBhN1av5Qz",null],"id":1}
        //{"type":"method","method":"auth","arguments":[523004,null,null,null],"id":1}
        public long ChannelId { get; }
        public long? MyId { get; }
        public string Token { get; }
        public AuthMethod(long id, long channelId) : base(id)
        {
            ChannelId = channelId;
        }
        public AuthMethod(long id, long channelId, long myId, string token) : base(id)
        {
            ChannelId = channelId;
            MyId = myId;
            Token = token;
        }
        public override string Raw
        {
            get
            {
                var myIdStr = MyId.HasValue ? MyId.Value.ToString() : "null";
                var tokenStr = !string.IsNullOrEmpty(Token) ? $"\"{Token}\"" : "null";
                var s = $"{{\"type\":\"method\",\"method\":\"auth\",\"arguments\":[{ChannelId},{myIdStr},{tokenStr},null],\"id\":{Id}}}";
                return s;
            }
        }
        public override string ToString() => Raw;
    }
    class PingMethod : MethodBase
    {
        //{"type":"method","method":"ping","arguments":[],"id":2}
        public PingMethod(long id) : base(id)
        {

        }
        public override string Raw
        {
            get
            {
                var s = $"{{\"type\":\"method\",\"method\":\"ping\",\"arguments\":[],\"id\":{Id}}}";
                return s;
            }
        }
        public override string ToString() => Raw;
    }
}
