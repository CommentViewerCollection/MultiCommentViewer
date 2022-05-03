using Mcv.PluginV2;
using System.ComponentModel;

namespace Mcv.MainViewPlugin
{
    class SiteViewModel : ViewModelBase, INotifyPropertyChanged
    {
        public SiteViewModel(SiteId id, string name)
        {
            Id = id;
            DisplayName = name;
        }

        public SiteId Id { get; }
        public string DisplayName { get; }
    }
}
