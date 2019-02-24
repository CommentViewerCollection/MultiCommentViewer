using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhowatchSitePlugin.Low.PlayItems
{
    public partial class RootObject
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("mobile_banner_url", NullValueHandling = NullValueHandling.Ignore)]
        public string MobileBannerUrl { get; set; }

        [JsonProperty("item_value")]
        public double ItemValue { get; set; }

        [JsonProperty("play_item_payment_product")]
        public List<PlayItemPaymentProduct> PlayItemPaymentProduct { get; set; }

        [JsonProperty("play_item_pattern")]
        public List<PlayItemPattern> PlayItemPattern { get; set; }

        [JsonProperty("balloon")]
        public bool Balloon { get; set; }

        [JsonProperty("image_url")]
        public string ImageUrl { get; set; }

        [JsonProperty("small_image_url")]
        public string SmallImageUrl { get; set; }

        [JsonProperty("simple_description", NullValueHandling = NullValueHandling.Ignore)]
        public string SimpleDescription { get; set; }
    }

    public partial class PlayItemPattern
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("simple_description")]
        public string SimpleDescription { get; set; }

        [JsonProperty("image_url")]
        public string ImageUrl { get; set; }

        [JsonProperty("small_image_url")]
        public string SmallImageUrl { get; set; }

        [JsonProperty("quantity")]
        public long Quantity { get; set; }

        [JsonProperty("pickup_time", NullValueHandling = NullValueHandling.Ignore)]
        public long? PickupTime { get; set; }

        [JsonProperty("send_comment")]
        public bool SendComment { get; set; }

        [JsonProperty("required_comment")]
        public bool RequiredComment { get; set; }

        [JsonProperty("default_comment", NullValueHandling = NullValueHandling.Ignore)]
        public string DefaultComment { get; set; }

        [JsonProperty("pattern_modifiable")]
        public bool PatternModifiable { get; set; }

        [JsonProperty("pattern_limit")]
        public long PatternLimit { get; set; }

        [JsonProperty("birthday_use_only")]
        public bool BirthdayUseOnly { get; set; }

        [JsonProperty("anonymous_post")]
        public bool AnonymousPost { get; set; }

        [JsonProperty("item_order", NullValueHandling = NullValueHandling.Ignore)]
        public long? ItemOrder { get; set; }

        [JsonProperty("animation_url", NullValueHandling = NullValueHandling.Ignore)]
        public string AnimationUrl { get; set; }

        [JsonProperty("animation_time", NullValueHandling = NullValueHandling.Ignore)]
        public long? AnimationTime { get; set; }

        [JsonProperty("animation_duration", NullValueHandling = NullValueHandling.Ignore)]
        public long? AnimationDuration { get; set; }

        [JsonProperty("animation_frames", NullValueHandling = NullValueHandling.Ignore)]
        public long? AnimationFrames { get; set; }

        [JsonProperty("animation_background_color", NullValueHandling = NullValueHandling.Ignore)]
        public string AnimationBackgroundColor { get; set; }
    }

    public partial class PlayItemPaymentProduct
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("product_id")]
        public string ProductId { get; set; }

        [JsonProperty("image_url")]
        public string ImageUrl { get; set; }

        [JsonProperty("confirm_image_url")]
        public string ConfirmImageUrl { get; set; }

        [JsonProperty("quantity")]
        public long Quantity { get; set; }

        [JsonProperty("price")]
        public long Price { get; set; }

        [JsonProperty("decoration")]
        public Decoration Decoration { get; set; }

        [JsonProperty("time_sale_list")]
        public List<object> TimeSaleList { get; set; }

        [JsonProperty("product_name")]
        public string ProductName { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("descriptions")]
        public List<string> Descriptions { get; set; }
    }

    public partial class Decoration
    {
        [JsonProperty("has_animation")]
        public bool HasAnimation { get; set; }

        [JsonProperty("bonus_count")]
        public string BonusCount { get; set; }

        [JsonProperty("ribbon", NullValueHandling = NullValueHandling.Ignore)]
        public string Ribbon { get; set; }

        [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
        public string Description { get; set; }

        [JsonProperty("product_name", NullValueHandling = NullValueHandling.Ignore)]
        public string ProductName { get; set; }

        [JsonProperty("label", NullValueHandling = NullValueHandling.Ignore)]
        public string Label { get; set; }
    }
}
