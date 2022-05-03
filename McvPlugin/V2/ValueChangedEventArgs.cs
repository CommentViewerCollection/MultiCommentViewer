using System;

namespace Mcv.PluginV2;

public class ValueChangedEventArgs : EventArgs
{
    public string PropertyName { get; set; }
}
