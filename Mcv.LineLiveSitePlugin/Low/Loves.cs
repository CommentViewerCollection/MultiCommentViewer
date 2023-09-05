using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LineLive.Api
{
    class Loves
    {
        public long Status { get; private set; }
        public List<Item> Items { get; private set; }
        public static Loves Parse(string json)
        {
            dynamic d = JsonConvert.DeserializeObject(json);
            var status = (long)d.status;
            var items = new List<Item>();
            foreach (var tab in d.tabs)
            {
                foreach (var item in tab.items)
                {
                    items.Add(Item.Parse(item));
                }
            }

            return new Loves
            {
                Status = status,
                Items = items,
            };

        }
    }
    class Item
    {
        public string Name { get; private set; }
        public string NameJs { get; private set; }
        public string ItemId { get; private set; }
        public string ThumbnailUrl { get; private set; }
        internal static Item Parse(dynamic d)
        {
            var itemId = (string)d.itemId;
            var name = (string)d.name;
            var nameJa = (string)d.nameJa;
            var thumbnailUrl = (string)d.assets.thumbnailUrl;
            return new Item
            {
                ItemId = itemId,
                Name = name,
                NameJs = nameJa,
                ThumbnailUrl = thumbnailUrl,
            };
        }
    }
}
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
