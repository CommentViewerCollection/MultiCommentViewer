using Mcv.PluginV2;
using System.ComponentModel;

namespace Mcv.MainViewPlugin;

class BrowserViewModel : ViewModelBase, INotifyPropertyChanged
{
    public BrowserViewModel(BrowserProfileId browserId, string name, string? profileName)
    {
        Id = browserId;
        Name = name;
        ProfileName = profileName;
    }
    public BrowserProfileId Id { get; }
    public string Name { get; }
    public string? ProfileName { get; }
    public string DisplayName
    {
        get
        {
            if (string.IsNullOrEmpty(ProfileName))
            {
                return Name;
            }
            else
            {
                return $"{Name}({ProfileName})";
            }
        }
    }
}
