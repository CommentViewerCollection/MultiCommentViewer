using System.Collections.Generic;
using SitePlugin;

namespace CommentViewer.Plugin
{
    static class MessageItemsExtensions
    {
        public static string ToText(this IEnumerable<IMessagePart> parts)
        {
            string s = "";
            if (parts != null)
            {
                foreach (var part in parts)
                {
                    if (part is IMessageText text)
                    {
                        s += text;
                    }
                }
            }
            return s;
        }
    }
}
