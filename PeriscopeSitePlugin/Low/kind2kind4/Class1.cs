using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeriscopeSitePlugin.Low.kind2kind4
{
    public class RootObject
    {
        [JsonProperty("room")]
        public string Room { get; set; }

        [JsonProperty("occupancy")]
        public long Occupancy { get; set; }

        [JsonProperty("total_participants")]
        public long TotalParticipants { get; set; }
    }
}
