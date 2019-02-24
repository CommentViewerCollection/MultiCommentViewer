using System;

namespace SitePlugin
{
    public class ValueChangedEventArgs : EventArgs
    {
        public string PropertyName { get; set; }
    }
}
