using Mcv.PluginV2.Messages;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mcv.PluginV2.Messages
{
    public class UnknownMessage : IMessage
    {
        public UnknownMessage(string raw)
        {
            Raw = raw;
        }

        public string Raw { get; }
    }
    public class UnknownSetMessage : UnknownMessage, ISetMessageToCoreV2
    {
        public UnknownSetMessage(string raw) : base(raw) { }
    }
}
