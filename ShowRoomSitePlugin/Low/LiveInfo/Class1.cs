using Newtonsoft.Json;

namespace ShowRoomSitePlugin.Low.LiveInfo
{
    public class RootObject
    {
        [JsonProperty("age_verification_status")]
        public long AgeVerificationStatus { get; set; }

        [JsonProperty("video_type")]
        public long VideoType { get; set; }

        [JsonProperty("enquete_gift_num")]
        public long EnqueteGiftNum { get; set; }

        [JsonProperty("is_enquete")]
        public bool IsEnquete { get; set; }

        [JsonProperty("bcsvr_port")]
        public long BcsvrPort { get; set; }

        [JsonProperty("live_type")]
        public long LiveType { get; set; }

        [JsonProperty("is_free_gift_only")]
        public bool IsFreeGiftOnly { get; set; }

        [JsonProperty("bcsvr_host")]
        public string BcsvrHost { get; set; }

        [JsonProperty("live_id")]
        public long LiveId { get; set; }

        [JsonProperty("is_enquete_result")]
        public bool IsEnqueteResult { get; set; }

        [JsonProperty("live_status")]
        public long LiveStatus { get; set; }

        [JsonProperty("room_name")]
        public string RoomName { get; set; }

        [JsonProperty("room_id")]
        public long RoomId { get; set; }

        [JsonProperty("bcsvr_key")]
        public string BcsvrKey { get; set; }

        [JsonProperty("background_image_url")]
        public string BackgroundImageUrl { get; set; }
    }
}
