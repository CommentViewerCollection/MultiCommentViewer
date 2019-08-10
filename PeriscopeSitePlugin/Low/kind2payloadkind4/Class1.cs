using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeriscopeSitePlugin.Low.kind2payloadkind4
{
    public partial class RootObject
    {
        [JsonProperty("kind")]
        public long Kind { get; set; }

        [JsonProperty("sender")]
        public Sender Sender { get; set; }

        [JsonProperty("body")]
        public string Body { get; set; }
    }

    public partial class Sender
    {
        [JsonProperty("user_id")]
        public string UserId { get; set; }
    }
}
