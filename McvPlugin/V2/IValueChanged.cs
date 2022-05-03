using System;

namespace Mcv.PluginV2;

public interface IValueChanged
{
    event EventHandler<ValueChangedEventArgs> ValueChanged;
}
