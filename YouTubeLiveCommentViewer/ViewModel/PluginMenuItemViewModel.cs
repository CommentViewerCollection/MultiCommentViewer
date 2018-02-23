using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System.Windows.Input;
using Plugin;
using System.Collections.ObjectModel;
using System;
using System.Diagnostics;

namespace YouTubeLiveCommentViewer.ViewModel
{
    public class PluginMenuItemViewModel : ViewModelBase
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
        private readonly IPlugin _plugin;
        public PluginMenuItemViewModel(IPlugin plugin)// PluginContext plugin, string name, ICommand command)
        {
            Name = plugin.Name;
            _plugin = plugin;
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
