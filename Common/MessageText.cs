using System;
using System.Collections.Generic;
using SitePlugin;
namespace Common
{
    public static class MessagePartFactory
    {
        public static IMessageText CreateMessageText(string text)
        {
            return MessageText.New(text);
        }
    }
    public static class MessagePartsTools
    {
        public static string ToText(this IEnumerable<IMessagePart> items)
        {
            if(items == null)
            {
                return null;
            }
            var list = new List<string>();
            foreach(var item in items)
            {
                if(item is IMessageText text)
                {
                    list.Add(text.Text);
                }else if(item is IMessageLink link)
                {
                    list.Add(link.Text);
                }
            }
            return string.Join("", list);
        }
    }
    internal class MessageText : IMessageText
    {
        public string Text { get; }
        public static MessageText New(string text)
        {
#pragma warning disable CS0618 // Type or member is obsolete
            return new MessageText(text);
#pragma warning restore CS0618 // Type or member is obsolete
        }
        public MessageText(string text)
        {
            Text = text;
        }
        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (obj is MessageText text)
            {
                return this.Text.Equals(text.Text);
            }
            return false;
        }
        public override string ToString() => Text;
        public override int GetHashCode()
        {
            return Text.GetHashCode();
        }
    }
}
