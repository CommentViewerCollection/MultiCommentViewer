using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchSitePlugin.Low.ChannelProduct
{
    public partial class RootObject
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("custom_name")]
        public string CustomName { get; set; }

        [JsonProperty("product_url")]
        public Uri ProductUrl { get; set; }

        [JsonProperty("price")]
        public string Price { get; set; }

        [JsonProperty("hide_ads")]
        public bool HideAds { get; set; }

        [JsonProperty("subonly_archives")]
        public bool SubonlyArchives { get; set; }

        [JsonProperty("subsonly")]
        public bool Subsonly { get; set; }

        [JsonProperty("fastsubs")]
        public bool Fastsubs { get; set; }

        [JsonProperty("bitrate_access")]
        public object[] BitrateAccess { get; set; }

        [JsonProperty("sub_interval")]
        public string SubInterval { get; set; }

        [JsonProperty("duration")]
        public long Duration { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("html", NullValueHandling = NullValueHandling.Ignore)]
        public string Html { get; set; }

        [JsonProperty("css", NullValueHandling = NullValueHandling.Ignore)]
        public string Css { get; set; }

        [JsonProperty("payment_amendment_unsigned", NullValueHandling = NullValueHandling.Ignore)]
        public bool? PaymentAmendmentUnsigned { get; set; }

        [JsonProperty("plans", NullValueHandling = NullValueHandling.Ignore)]
        public RootObject[] Plans { get; set; }

        [JsonProperty("emoticons")]
        public Emoticon[] Emoticons { get; set; }

        [JsonProperty("plan", NullValueHandling = NullValueHandling.Ignore)]
        public string Plan { get; set; }

        [JsonProperty("emoticon_set_ids", NullValueHandling = NullValueHandling.Ignore)]
        public long[] EmoticonSetIds { get; set; }
    }

    public partial class Emoticon
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("width")]
        public long Width { get; set; }

        [JsonProperty("height")]
        public long Height { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }

        [JsonProperty("regex")]
        public string Regex { get; set; }

        [JsonProperty("emoticon_set")]
        public long EmoticonSet { get; set; }

        [JsonProperty("url")]
        public Uri Url { get; set; }

        [JsonProperty("subscriber_only")]
        public bool SubscriberOnly { get; set; }
    }
}
