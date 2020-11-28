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
    public class MessageParser2 : IMessageParser
    {
        public IInternalMessage Parse(string s)
        {
            IInternalMessage message = null;
            dynamic d = Newtonsoft.Json.JsonConvert.DeserializeObject(s);
            switch ((string)d.type)
            {
                case "room":
                    message = new RoomInternalMessage
                    {
                        IsFirst = (bool)d.data.isFirst,
                        MessageServerUrl = (string)d.data.messageServer.uri,
                        MessageServerType = (string)d.data.messageServer.type,
                        RoomName = (string)d.data.name,
                        ThreadId = (string)d.data.threadId,
                        WaybackKey = (string)d.data.waybackkey,
                        Raw = s,
                    };
                    break;
                case "ping":
                    message = new PingInternalMessage
                    {
                        Raw = s,
                    };
                    break;
                case "seat":
                    message = new SeatInternalMessage
                    {
                        KeepIntervalSec = (int)d.data.keepIntervalSec,
                        Raw = s,
                    };
                    break;
                case "statistics":
                    message = new StatisticsInternalMessage
                    {
                        AdPoint = (int)d.data.adPoints,
                        Comments = (int)d.data.comments,
                        GiftPoints = (int)d.data.giftPoints,
                        Viewers = (int)d.data.viewers,
                        Raw = s,
                    };
                    break;
                case "schedule":
                    message = new ScheduleInternalMessage
                    {
                        Begin = (string)d.data.begin,
                        End = (string)d.data.end,
                        Raw = s,
                    };
                    break;
                case "disconnect":
                    message = new DisconnectInternalMessage
                    {
                        Reason = (string)d.data.reason,
                        Raw = s,
                    };
                    break;
                default:
                    throw new ParseException(s);
            }
            return message;
        }
    }
    public interface IInternalMessage
    {
        string Raw { get; }
    }
    class RoomInternalMessage : IInternalMessage
    {
        public bool IsFirst { get; set; }
        public string MessageServerUrl { get; set; }
        public string MessageServerType { get; set; }
        public string RoomName { get; set; }
        public string ThreadId { get; set; }
        public string WaybackKey { get; set; }
        public string Raw { get; set; }
    }
    public class PingInternalMessage : IInternalMessage
    {
        public string Raw { get; set; }
    }
    class SeatInternalMessage : IInternalMessage
    {
        public int KeepIntervalSec { get; set; }
        public string Raw { get; set; }
    }
    class StatisticsInternalMessage : IInternalMessage
    {
        public int AdPoint { get; set; }
        public int Comments { get; set; }
        public int GiftPoints { get; set; }
        public int Viewers { get; set; }
        public string Raw { get; set; }
    }
    class DisconnectInternalMessage : IInternalMessage
    {
        public string Reason { get; set; }
        public string Raw { get; set; }
    }
    class ScheduleInternalMessage : IInternalMessage
    {
        public string Begin { get; set; }
        public string End { get; set; }
        public string Raw { get; set; }
    }
}
