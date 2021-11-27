using Newtonsoft.Json;

namespace NicoSitePlugin.Metadata
{
    class Statistics : IMetaMessage
    {
        public Statistics(string json)
        {
            dynamic d = JsonConvert.DeserializeObject(json);
            Viewers = (int?)d.data.viewers;
            Comments = (int?)d.data.comments;
            if (d.data.ContainsKey("adPoints"))
            {
                AdPoints = (int)d.data.adPoints;
            }
            if (d.data.ContainsKey("giftPoints"))
            {
                GiftPoints = (int)d.data.giftPoints;
            }
            Raw = json;
        }
        public int? Viewers { get; }
        public int? Comments { get; }
        public int? AdPoints { get; }
        public int? GiftPoints { get; }
        public string Raw { get; }
    }
}