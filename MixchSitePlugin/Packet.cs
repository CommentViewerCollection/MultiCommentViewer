using System;
using System.Collections.ObjectModel;

namespace MixchSitePlugin
{
    public class Packet
    {
        public int kind { get; set; }
        public int user_id { get; set; }
        public string name { get; set; }
        public int level { get; set; }
        public int created { get; set; }
        public string body { get; set; }
        public int resource_id { get; set; }
        public int count { get; set; }
        public string title { get; set; }
        public int elapsed { get; set; }
        public int display_point { get; set; }
        public int status { get; set; }

        public bool IsStatus()
        {
            return (MixchMessageType)kind == MixchMessageType.Status;
        }

        public bool HasMessage()
        {
            return !string.IsNullOrEmpty(Message());
        }

        public string Message()
        {
            switch ((MixchMessageType)kind)
            {
                case MixchMessageType.Comment:
                case MixchMessageType.Share:
                case MixchMessageType.EnterNewbie:
                case MixchMessageType.EnterLevel:
                case MixchMessageType.Follow:
                case MixchMessageType.EnterFanclub:
                    return body;
                case MixchMessageType.SuperComment:
                    return $"【スパコメ】{body}";
                case MixchMessageType.Stamp:
                    return $"【スタンプ】{ItemName()}で応援しました";
                case MixchMessageType.PoiPoi:
                    return $"【アイテム】{count}個のアイテムで応援しました";
                case MixchMessageType.Item:
                    return $"【アイテム】{ItemName()}で応援しました";
            }
            return "";
        }

        public string DisplayPoint()
        {
            return String.Format("盛り上がり度: {0:#,0}", display_point);
        }

        private string ItemName()
        {
            var name = Item.NameByResourceId(resource_id);
            return !string.IsNullOrEmpty(name) ? $"「{name}」" : $"名称不明(id={resource_id})";
        }
    }
}
