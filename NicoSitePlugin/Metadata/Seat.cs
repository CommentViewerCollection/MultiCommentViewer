using Newtonsoft.Json;

namespace NicoSitePlugin.Metadata
{
    class Seat : IMetaMessage
    {
        public int KeepIntervalSec { get; }
        public Seat(string raw)
        {
            dynamic d = JsonConvert.DeserializeObject(raw);
            KeepIntervalSec = (int)d.data.keepIntervalSec;
        }
        public string Raw => $"{{\"type\":\"seat\",\"data\":{{\"keepIntervalSec\":{KeepIntervalSec}}}}}";
    }
}