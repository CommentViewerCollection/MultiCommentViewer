using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LineLiveSitePlugin.Low.Loves
{
    public partial class RootObject
    {
        [JsonProperty("coin")]
        public object Coin { get; set; }

        [JsonProperty("lineCoin")]
        public object LineCoin { get; set; }

        [JsonProperty("paidCoin")]
        public object PaidCoin { get; set; }

        [JsonProperty("freeCoin")]
        public object FreeCoin { get; set; }

        [JsonProperty("freeExpireAt")]
        public object FreeExpireAt { get; set; }

        [JsonProperty("items")]
        public List<Item> Items { get; set; }

        [JsonProperty("apistatusCode")]
        public long ApistatusCode { get; set; }

        [JsonProperty("status")]
        public long Status { get; set; }
    }

    public partial class Item
    {
        [JsonProperty("itemId")]
        public string ItemId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("nameJa")]
        public string NameJa { get; set; }

        [JsonProperty("price")]
        public long Price { get; set; }

        [JsonProperty("gift")]
        public long Gift { get; set; }

        [JsonProperty("assets")]
        public Assets Assets { get; set; }

        [JsonProperty("commentable")]
        public bool Commentable { get; set; }

        [JsonProperty("animates")]
        public bool Animates { get; set; }

        [JsonProperty("repeatable")]
        public bool Repeatable { get; set; }

        [JsonProperty("displayDuration")]
        public long DisplayDuration { get; set; }

        [JsonProperty("createdAt")]
        public long CreatedAt { get; set; }

        [JsonProperty("updatedAt")]
        public long UpdatedAt { get; set; }

        [JsonProperty("apistatusCode")]
        public long ApistatusCode { get; set; }
    }

    public partial class Assets
    {
        [JsonProperty("animationUrl")]
        public string AnimationUrl { get; set; }

        [JsonProperty("thumbnailUrl")]
        public string ThumbnailUrl { get; set; }
    }
}
