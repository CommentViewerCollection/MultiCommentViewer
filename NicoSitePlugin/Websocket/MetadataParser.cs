using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Codeplex.Data;
namespace NicoSitePlugin.Websocket
{
    public interface IMessageParser
    {
        IInternalMessage Parse(string s);
    }
    public class MessageParser : IMessageParser
    {
        //{"type":"watch","body":{"command":"disconnect","params":["4840229962359","NO_PERMISSION"]}}
        public IInternalMessage Parse(string s)
        {
            IInternalMessage message = null;
            var d = DynamicJson.Parse(s);
            if (!d.IsDefined("type"))
            {
                throw new ParseException(s);
            }
            var type = d.type;
            if (type == "watch")
            {
                if (!d.IsDefined("body"))
                    throw new ParseException(s);
                var body = d.body;
                if (!body.IsDefined("command"))
                    throw new ParseException(s);
                var command = body.command;
                switch (command)
                {
                    case "servertime":
                        message = new Servertime { Time = body.@params[0] };
                        break;
                    case "permit":
                        message = new Permit { Value = body.@params[0] };
                        break;
                    case "schedule":
                        var update = body.update;
                        message = new Schedule { BeginTime = (long)update.begintime, EndTime = (long)update.endtime };
                        break;
                    case "statistics":
                        message = new Statistics { ViewerCount = body.@params[0], CommentCount = body.@params[1] };
                        break;
                    case "currentroom":
                        var room = body.room;
                        message = new CurrentRoom { MessageServerUri = room.messageServerUri, MessageServerType = room.messageServerType, RoomName = room.roomName, ThreadId = room.threadId };
                        break;
                    default:
                        //{"type":"watch","body":{"currentStream":{"uri":"https://pa1212365f5.dmc.nico/hlslive/ht2_nicolive/nicolive-production-pg14650921976390_e3839d5914a76beceda85885e6733dd52055d5429d597d174ef71ec5207f0423/master.m3u8?ht2_nicolive=2297426.8qhddq_psdgya_1q1h8zfle71tz","name":null,"quality":"abr","qualityTypes":["abr","super_high","high","normal","low","super_low"],"mediaServerType":"dmc","mediaServerAuth":null,"streamingProtocol":"hls"},"command":"currentstream"}}
                        //{"type":"watch","body":{"command":"watchinginterval","params":["30"]}}
                        //{"type":"watch","body":{"command":"disconnect","params":["18401151943248","END_PROGRAM"]}}
                        throw new ParseException(s);
                }
            }
            else if (type == "ping")
            {
                message = new Ping();
            }
            else
            {
                throw new ParseException(s);
            }
            return message;
        }
    }
    public interface IInternalMessage
    {
        string Raw { get; }
    }
    public interface IServertime : IInternalMessage
    {
        string Time { get; }
    }
    public interface ICurrentRoom : IInternalMessage
    {
        string MessageServerUri { get; }
        string MessageServerType { get; }
        string RoomName { get; }
        string ThreadId { get; }
    }
    public class CurrentRoom : ICurrentRoom
    {
        public string MessageServerUri { get; set; }
        public string MessageServerType { get; set; }
        public string RoomName { get; set; }
        public string ThreadId { get; set; }
        public string Raw
        {
            get
            {
                return "{\"type\":\"watch\",\"body\":{\"room\":{\"messageServerUri\":\"" + MessageServerUri + "\",\"messageServerType\":\"" + MessageServerType + "\",\"roomName\":\"" + RoomName + "\",\"threadId\":\"" + ThreadId + "\",\"forks\":[0],\"importedForks\":[]},\"command\":\"currentroom\"}}";
            }
        }
    }
    public interface IPermit : IInternalMessage
    {
        string Value { get; }
    }
    public interface IPing : IInternalMessage
    {
    }
    public interface IStatistics : IInternalMessage
    {
        string ViewerCount { get; }
        string CommentCount { get; }
    }
    public class Statistics : IStatistics
    {
        public string ViewerCount { get; set; }
        public string CommentCount { get; set; }
        public string Unknown1 { get; set; } = "0";
        public string Unknown2 { get; set; } = "0";
        public string Raw
        {
            get
            {
                return "{\"type\":\"watch\",\"body\":{\"command\":\"statistics\",\"params\":[\"" + ViewerCount + "\",\"" + CommentCount + "\",\"" + Unknown1 + "\",\"" + Unknown2 + "\"]}}";
            }
        }
    }
    public class Servertime : IServertime
    {
        public string Time { get; set; }
        public string Raw
        {
            get
            {
                return "{\"type\":\"watch\",\"body\":{\"command\":\"servertime\",\"params\":[\"" + Time + "\"]}}";
            }
        }
    }
    public class Permit : IPermit
    {
        public string Value { get; set; }
        public string Raw
        {
            get
            {
                return "{\"type\":\"watch\",\"body\":{\"command\":\"permit\",\"params\":[\"" + Value + "\"]}}";
            }
        }
    }
    public class Ping : IPing
    {
        public string Raw
        {
            get
            {
                return "{\"type\":\"ping\",\"body\":{}}";
            }
        }
    }
    public interface ISchedule : IInternalMessage
    {
        long BeginTime { get; }
        long EndTime { get; }
    }
    public class Schedule : ISchedule
    {
        public long BeginTime { get; set; }
        public long EndTime { get; set; }
        public string Raw
        {
            get
            {
                return "{\"type\":\"watch\",\"body\":{\"update\":{\"begintime\":" + BeginTime + ",\"endtime\":" + EndTime + "},\"command\":\"schedule\"}}";
            }
        }
    }
}
