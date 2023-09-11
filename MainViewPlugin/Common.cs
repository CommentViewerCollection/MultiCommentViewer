using Mcv.PluginV2;
using System.Collections.Generic;

namespace Common;

class MessageTextImpl : IMessageText
{
    public string Text { get; }
    public MessageTextImpl(string text)
    {
        Text = text;
    }
}
public static class MessagePartFactory
{
    public static IMessageText CreateMessageText(string text)
    {
        return new MessageTextImpl(text);
    }
    public static IEnumerable<IMessagePart> CreateMessageItems(string text)
    {
        return new List<IMessagePart>
        {
            CreateMessageText(text)
        };
    }
}
