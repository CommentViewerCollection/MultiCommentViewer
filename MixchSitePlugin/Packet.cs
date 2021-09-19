using System;
using System.Collections.ObjectModel;

namespace MixchSitePlugin
{
    public class Packet
    {

        const int NORMAL_COMMENT = 0;

        ReadOnlyCollection<int> kindOfChat = Array.AsReadOnly(new int[] {
            NORMAL_COMMENT,
        });

        public int kind { get; set; }
        public int user_id { get; set; }
        public string name { get; set; }
        public int level { get; set; }
        public int created { get; set; }
        public string body { get; set; }

        public bool IsChat()
        {
            return kindOfChat.Contains(kind);
        }

        public string Message()
        {
            switch (kind)
            {
                case NORMAL_COMMENT:
                    return body;
            }
            return "";
        }
    }
}
