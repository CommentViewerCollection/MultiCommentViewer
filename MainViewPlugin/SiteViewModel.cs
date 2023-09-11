using Mcv.PluginV2;
using System.ComponentModel;

namespace Mcv.MainViewPlugin;

class SiteViewModel : ViewModelBase, INotifyPropertyChanged
{
    public SiteViewModel(PluginId id, string name)
    {
        Id = id;
        DisplayName = name;
    }

    public PluginId Id { get; }
    public string DisplayName { get; }
}
