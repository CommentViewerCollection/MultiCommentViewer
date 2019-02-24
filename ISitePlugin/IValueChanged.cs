using System;

namespace SitePlugin
{
    public interface IValueChanged
    {
        event EventHandler<ValueChangedEventArgs> ValueChanged;
    }
}
