using CommunityToolkit.Mvvm.Input;
using Mcv.PluginV2;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows.Media;

namespace Mcv.MainViewPlugin;

class PluginMenuItemViewModel : ViewModelBase, INotifyPropertyChanged
{
    private readonly IAdapter _adapter;

    public PluginMenuItemViewModel(string displayName, PluginId pluginId, IAdapter adapter)
    {
        DisplayName = displayName;
        PluginId = pluginId;
        _adapter = adapter;
        ShowSettingViewCommand = new RelayCommand(ShowSettingView);
    }

    public string DisplayName { get; }
    public PluginId PluginId { get; }
    public ICommand ShowSettingViewCommand { get; }
    public ObservableCollection<PluginMenuItemViewModel> Children { get; } = new ObservableCollection<PluginMenuItemViewModel>();
    public Brush MenuBackground => new SolidColorBrush(new Color { A = 0xFF, R = 0xF0, G = 0xF0, B = 0xF0 });
    public Brush MenuForeground => new SolidColorBrush(new Color { A = 0xFF, R = 0x00, G = 0x00, B = 0x00 });

    private void ShowSettingView()
    {
        _adapter.RequestShowSettingsPanel(PluginId);
    }
}
