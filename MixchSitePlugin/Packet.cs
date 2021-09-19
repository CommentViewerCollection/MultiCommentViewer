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
        public int item_id { get; set; }
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
                    return body;
                case MixchMessageType.SuperComment:
                    return $"【スパコメ】{body}";
                case MixchMessageType.Stamp:
                    return $"【スタンプ】{item_id}で応援しました";
            }
            return "";
        }

        public string DisplayPoint()
        {
            return String.Format("盛り上がり度: {0:#,0}", display_point);
        }
    }
}
