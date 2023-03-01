using System;
using System.Windows.Input;
using System.Collections.ObjectModel;
using Plugin;
using System.Diagnostics;
using System.Windows.Media;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace MultiCommentViewer
{
    public class PluginMenuItemViewModel : ObservableObject, INotifyPropertyChanged
    {
        public string Name { get; set; }
        public ObservableCollection<PluginMenuItemViewModel> Children { get; } = new ObservableCollection<PluginMenuItemViewModel>();
        private RelayCommand _show;
        public ICommand ShowSettingViewCommand
        {
            //以前はコンストラクタ中でICommandに代入していたが、項目をクリックしてもTest()が呼ばれないことがあった。今の状態に書き換えたら問題なくなった。何故だ？IPluginを保持するようにしたから？GCで無くなっちゃってたとか？
            get
            {
                if (_show == null)
                {
                    _show = new RelayCommand(() => Test(_plugin));
                }
                return _show;
            }
        }
        public Brush MenuBackground
        {
            get => new SolidColorBrush(_options.MenuBackColor);
        }
        public Brush MenuForeground
        {
            get => new SolidColorBrush(_options.MenuForeColor);
        }
        private readonly IPlugin _plugin;
        private readonly IOptions _options;

        public PluginMenuItemViewModel(IPlugin plugin, IOptions options)// PluginContext plugin, string name, ICommand command)
        {
            Name = plugin.Name;
            _plugin = plugin;
            _options = options;
            options.PropertyChanged += Options_PropertyChanged;
        }

        private void Options_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(_options.MenuBackColor):
                    OnPropertyChanged(nameof(MenuBackground));
                    break;
                case nameof(_options.MenuForeColor):
                    OnPropertyChanged(nameof(MenuForeground));
                    break;
            }
        }

        private void Test(IPlugin plugin)
        {
            try
            {
                plugin.ShowSettingView();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }
    }
}

