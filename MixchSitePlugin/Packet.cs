using System;
using System.Collections.ObjectModel;
using Newtonsoft.Json;

namespace MixchSitePlugin
{
    public class Packet
    {
        [JsonProperty("kind")]
        public int Kind { get; set; }

        [JsonProperty("user_id")]
        public int UserId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("created")]
        public int Created { get; set; }

        [JsonProperty("body")]
        public string Body { get; set; }

        [JsonProperty("resource_id")]
        public int ResourceId { get; set; }

        [JsonProperty("count")]
        public int Count { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("elapsed")]
        public int Elapsed { get; set; }

        [JsonProperty("display_point")]
        public int DisplayPoint { get; set; }

        [JsonProperty("status")]
        public int Status { get; set; }

        public bool IsStatus()
        {
            return (MixchMessageType)Kind == MixchMessageType.Status;
        }

        public bool HasMessage()
        {
            return !string.IsNullOrEmpty(Message());
        }

        public string Message()
        {
            switch ((MixchMessageType)Kind)
            {
                case MixchMessageType.Comment:
                case MixchMessageType.Share:
                case MixchMessageType.EnterNewbie:
                case MixchMessageType.EnterLevel:
                case MixchMessageType.Follow:
                case MixchMessageType.EnterFanclub:
                    return Body;
                case MixchMessageType.SuperComment:
                    return $"【スパコメ】{Body}";
                case MixchMessageType.Stamp:
                    return $"【スタンプ】{ItemName()}で応援しました";
                case MixchMessageType.PoiPoi:
                    return $"【アイテム】{Count}個のアイテムで応援しました";
                case MixchMessageType.Item:
                case MixchMessageType.CoinBox:
                    return $"【アイテム】{ItemName()}で応援しました";
            }
            return "";
        }

        public string DisplayPointString()
        {
            return String.Format("盛り上がり度: {0:#,0}", DisplayPoint);
        }

        private string ItemName()
        {
            var name = Item.NameByResourceId(ResourceId);
            return !string.IsNullOrEmpty(name) ? $"「{name}」" : $"名称不明(id={ResourceId})";
        }
    }
}
