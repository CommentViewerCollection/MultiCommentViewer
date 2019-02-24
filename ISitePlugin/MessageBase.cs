using System;
using System.Collections.Generic;

namespace SitePlugin
{
    public abstract class MessageBase : IMessage, IValueChanged
    {
        public string Raw { get; }
        public abstract SiteType SiteType { get; }
        public IEnumerable<IMessagePart> NameItems { get; set; }
        public IEnumerable<IMessagePart> CommentItems { get; set; }
        public event EventHandler<ValueChangedEventArgs> ValueChanged;
        public void OnValueChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = "")
        {
            ValueChanged?.Invoke(this, new ValueChangedEventArgs { PropertyName = propertyName });
        }
        public MessageBase(string raw)
        {
            Raw = raw;
        }
    }
}
