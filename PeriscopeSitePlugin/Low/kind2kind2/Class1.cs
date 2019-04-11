using Newtonsoft.Json;

namespace PeriscopeSitePlugin.Low.kind2kind2
{
    public class RootObject
    {
        [JsonProperty("room")]
        public string Room { get; set; }

        [JsonProperty("following")]
        public bool Following { get; set; }

        [JsonProperty("unlimited")]
        public bool Unlimited { get; set; }
    }
}
