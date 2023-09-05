using Newtonsoft.Json;
using LineLiveSitePlugin.ParseMessage;
namespace LineLiveSitePlugin.Low.FollowStart
{
    public partial class RootObject
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public partial class Data: IFollowStartData
    {
        [JsonProperty("user")]
        public User User { get; set; }

        [JsonProperty("roomId")]
        public string RoomId { get; set; }

        [JsonProperty("followedAt")]
        public long FollowedAt { get; set; }
    }

    public partial class User:IUser
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("hashedId")]
        public string HashedId { get; set; }

        [JsonProperty("displayName")]
        public string DisplayName { get; set; }

        [JsonProperty("iconUrl")]
        public string IconUrl { get; set; }

        [JsonProperty("hashedIconId")]
        public string HashedIconId { get; set; }

        [JsonProperty("isGuest")]
        public bool IsGuest { get; set; }

        [JsonProperty("isBlocked")]
        public bool IsBlocked { get; set; }
    }
}
