using System;
using System.Collections.Generic;

namespace SitePlugin
{
    public abstract class MessageBase2 : ISiteMessage, IValueChanged
    {
        public string Raw { get; }
        public abstract SiteType SiteType { get; }
        public event EventHandler<ValueChangedEventArgs> ValueChanged;
        public void OnValueChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = "")
        {
            ValueChanged?.Invoke(this, new ValueChangedEventArgs { PropertyName = propertyName });
        }
        public MessageBase2(string raw)
        {
            Raw = raw;
        }
    }
    //public abstract class MessageBase : IMessage2, IValueChanged
    //{
    //    private IEnumerable<IMessagePart> _nameItems;
    //    private IEnumerable<IMessagePart> _commentItems;

    //    public string Raw { get; }
    //    public abstract SiteType SiteType { get; }
    //    public IEnumerable<IMessagePart> NameItems
    //    {
    //        get
    //        {
    //            return _nameItems;
    //        }
    //        set
    //        {
    //            if (_nameItems == value)
    //            {
    //                return;
    //            }
    //            _nameItems = value;
    //            OnValueChanged();
    //        }
    //    }
    //    public IEnumerable<IMessagePart> CommentItems
    //    {
    //        get
    //        {
    //            return _commentItems;
    //        }
    //        set
    //        {
    //            if (_commentItems == value)
    //            {
    //                return;
    //            }
    //            _commentItems = value;
    //            OnValueChanged();
    //        }
    //    }
    //    public event EventHandler<ValueChangedEventArgs> ValueChanged;
    //    public void OnValueChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = "")
    //    {
    //        ValueChanged?.Invoke(this, new ValueChangedEventArgs { PropertyName = propertyName });
    //    }
    //    public MessageBase(string raw)
    //    {
    //        Raw = raw;
    //    }
    //}
}
