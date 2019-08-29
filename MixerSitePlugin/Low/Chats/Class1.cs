using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MixerSitePlugin.Low.Chats
{
    public class RootObject
    {
        [JsonProperty("roles")]
        public string[] Roles { get; set; }

        [JsonProperty("authkey")]
        public string Authkey { get; set; }

        [JsonProperty("permissions")]
        public string[] Permissions { get; set; }

        [JsonProperty("endpoints")]
        public string[] Endpoints { get; set; }

        [JsonProperty("isLoadShed")]
        public bool IsLoadShed { get; set; }
    }
}
