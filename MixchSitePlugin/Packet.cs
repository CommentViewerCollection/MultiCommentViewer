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
        public static IPacket Parse(string str)
        {

            IPacket ret = null;
            try
            {
                var context = JsonConvert.DeserializeObject<Low.WebsocketContext2>(str);
                ret = new PacketBase(context);
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
    public class PacketBase : IPacket
    {
        public Low.WebsocketContext2 Context { get; }
        public PacketBase(Low.WebsocketContext2 context)
        {
            Context = context;
        }
    }
}
