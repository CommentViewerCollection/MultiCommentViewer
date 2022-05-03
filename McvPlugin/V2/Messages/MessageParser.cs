using Newtonsoft.Json;
using System;

namespace Mcv.PluginV2.Messages
{
    public class MessageParser
    {
        private static ISetMessageToCoreV2 ParseSetMessage(dynamic d)
        {
            var set = (string)d.set;
            switch (set)
            {
                case "add_connection":
                    {
                        //{"type":"set", "set":"add_connection"}
                        return new RequestAddConnection();
                    }
                case "remove_connection":
                    {
                        //{"type":"set", "set":"remove_connection", "conn_id":"292255B2-8E9F-4289-A926-F7B302D8A3FC"}
                        var rawConnId = (string)d.conn_id;
                        if (Guid.TryParse(rawConnId, out var guid))
                        {
                            var connId = new ConnectionId(guid);
                            return new RequestRemoveConnection(connId);
                        }
                    }
                    break;
            }
            throw new Exception("unknown message");
        }
        public static IMessage Parse(string raw)
        {
            //{"type":"req","req":"add_connection"}
            //{"type":"notify","notify":"connection_added"}
            //

            dynamic? d = null;
            try
            {
                d = JsonConvert.DeserializeObject(raw);
            }
            catch (Exception)
            {
                return new UnknownMessage(raw);
            }
            if (d is null)
            {
                return new UnknownMessage(raw);
            }
            if (!d.ContainsKey("type"))
            {
                return new UnknownMessage(raw);
            }
            try
            {
                var type = (string)d.type;
                if (type == "set" && d.ContainsKey("set"))
                {
                    return ParseSetMessage(d);
                }
                else if (type == "notify" && d.ContainsKey("notify"))
                {
                    var notify = (string)d.notify;
                }
                else if (type == "get" && d.ContainsKey("get"))
                {
                    var get = (string)d.get;
                    switch (get)
                    {
                        case "appname":
                            return new GetAppName();
                    }
                }
            }
            catch (Exception) { }
            return new UnknownMessage(raw);
        }
    }
}
