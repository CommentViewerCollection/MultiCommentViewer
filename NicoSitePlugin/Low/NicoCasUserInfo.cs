using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NicoSitePlugin.Low.NicoCasUserInfo
{
    public partial class RootObject
    {
        [JsonProperty("meta")]
        public Meta Meta { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public partial class Data
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("contentGroups")]
        public List<ContentGroup> ContentGroups { get; set; }

        [JsonProperty("messageServer")]
        public MessageServer MessageServer { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("owner")]
        public Owner Owner { get; set; }

        [JsonProperty("updateAt")]
        public string UpdateAt { get; set; }

        [JsonProperty("follow")]
        public Follow Follow { get; set; }

        [JsonProperty("notification")]
        public Follow Notification { get; set; }

        [JsonProperty("annotation")]
        public DataAnnotation Annotation { get; set; }

        [JsonProperty("icon")]
        public Uri Icon { get; set; }

        [JsonProperty("priority")]
        public long Priority { get; set; }
    }

    public partial class DataAnnotation
    {
        [JsonProperty("player")]
        public Player Player { get; set; }
    }

    public partial class Player
    {
        [JsonProperty("expandDetail")]
        public bool ExpandDetail { get; set; }
    }

    public partial class ContentGroup
    {
        [JsonProperty("groupId")]
        public string GroupId { get; set; }

        [JsonProperty("totalCount")]
        public long TotalCount { get; set; }

        [JsonProperty("items")]
        public List<ContentGroupItem> Items { get; set; }

        [JsonProperty("continuousPlayable")]
        public bool ContinuousPlayable { get; set; }

        [JsonProperty("options")]
        public List<Option> Options { get; set; }

        [JsonProperty("defaultOption")]
        public List<DefaultOption> DefaultOption { get; set; }

        [JsonProperty("label")]
        public string Label { get; set; }

        [JsonProperty("annotation")]
        public ContentGroupAnnotation Annotation { get; set; }
    }

    public partial class ContentGroupAnnotation
    {
        [JsonProperty("preferredDisplayContents")]
        public long PreferredDisplayContents { get; set; }
    }

    public partial class DefaultOption
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }
    }

    public partial class ContentGroupItem
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("thumbnailUrl")]
        public Uri ThumbnailUrl { get; set; }

        [JsonProperty("onAirTime")]
        public Time OnAirTime { get; set; }

        [JsonProperty("showTime")]
        public Time ShowTime { get; set; }

        [JsonProperty("viewers")]
        public long Viewers { get; set; }

        [JsonProperty("comments")]
        public long Comments { get; set; }

        [JsonProperty("programType")]
        public string ProgramType { get; set; }

        [JsonProperty("contentOwner")]
        public ContentOwner ContentOwner { get; set; }

        [JsonProperty("hasArchive")]
        public bool HasArchive { get; set; }

        [JsonProperty("deviceFilter")]
        public DeviceFilter DeviceFilter { get; set; }
    }

    public partial class ContentOwner
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }
    }

    public partial class DeviceFilter
    {
        [JsonProperty("isArchivePlayable")]
        public bool IsArchivePlayable { get; set; }

        [JsonProperty("isPlayable")]
        public bool IsPlayable { get; set; }

        [JsonProperty("isListing")]
        public bool IsListing { get; set; }
    }

    public partial class Time
    {
        [JsonProperty("beginAt")]
        public DateTimeOffset BeginAt { get; set; }

        [JsonProperty("endAt")]
        public DateTimeOffset EndAt { get; set; }
    }

    public partial class Option
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("label")]
        public string Label { get; set; }

        [JsonProperty("items")]
        public List<OptionItem> Items { get; set; }
    }

    public partial class OptionItem
    {
        [JsonProperty("label")]
        public string Label { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }
    }

    public partial class Follow
    {
        [JsonProperty("type")]
        public string Type { get; set; }
    }

    public partial class MessageServer
    {
        [JsonProperty("wss")]
        public string Wss { get; set; }

        [JsonProperty("ws")]
        public string Ws { get; set; }

        [JsonProperty("https")]
        public Uri Https { get; set; }

        [JsonProperty("version")]
        public long Version { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("service")]
        public string Service { get; set; }
    }

    public partial class Owner
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("icon")]
        public Uri Icon { get; set; }
    }

    public partial class Meta
    {
        [JsonProperty("status")]
        public long Status { get; set; }
    }
}
