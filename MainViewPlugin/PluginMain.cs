using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mcv.PluginV2;
using Mcv.PluginV2.Messages.ToPlugin;
using Messages = Mcv.PluginV2.Messages;
namespace Mcv.MainViewPlugin
{
    [Export(typeof(IPlugin))]
    public class MainViewPlugin : IPlugin
    {
        public PluginId Id { get; } = new PluginId(new Guid("A53CCC44-6A14-4533-99CA-184D5584E257"));
        public string Name => "MainViewPlugin";
        public IPluginHost Host { get; set; }
        private IAdapter _adapter;
        private MainViewModel _vm;
        private readonly DynamicOptionsTest _options;
        private IMainViewPluginOptions LoadOptions()
        {
            var loadedOptions = Host.RequestMessage(new Messages.RequestLoadPluginOptions(Name)) as Messages.ReplyPluginOptions;

            var options = new DynamicOptionsTest();
            options.Deserialize(loadedOptions.RawOptions);
            return options;
        }
        public void OnLoading()
        {
            _options.Set(LoadOptions());
            _adapter = new IAdapter(Host, _options);
            _vm = new MainViewModel(_adapter);//_adapterのイベントを購読する処理がctorにある。SiteAddedがOnLoaded()の前に来るからこのタイミングで初期化しないと間に合わない。
        }
        public void OnLoaded()
        {
            _v = new MainWindow();
            _v.DataContext = _vm;
            _v.Show();
            //タイトルを設定する
            //ソフト名を取得する
            //バージョン番号を取得する
            var appName = _adapter.GetAppName();
        }
        private MainWindow _v;

        public void OnClosing()
        {
            Host.SetMessage(new Messages.RequestSavePluginOptions(Name, _options.Serialize()));
        }

        public void SetMessage(Messages.INotifyMessageV2 message)
        {
            if (_adapter == null)
            {
                return;
            }
            switch (message)
            {
                case Messages.NotifyConnectionAdded connAdded:
                    _adapter.OnConnectionAdded(connAdded.ConnSt);
                    break;
                case Messages.NotifyConnectionRemoved connRemoved:
                    _adapter.OnConnectionRemoved(connRemoved.ConnId);
                    break;
                case Messages.NotifyPluginInfoList pluginInfoList:
                    {
                        foreach (var pluginInfo in pluginInfoList.Plugins)
                        {
                            _adapter.OnPluginAdded(pluginInfo);
                        }
                    }
                    break;
                case Messages.NotifyConnectionStatusChanged connStChanged:
                    _adapter.OnConnectionStatusChanged(connStChanged.ConnStDiff);
                    break;
                case Messages.NotifySiteAdded siteAdded:
                    _adapter.OnSiteAdded(siteAdded.SiteId, siteAdded.SiteDisplayName);
                    break;
                case Messages.NotifyBrowserAdded browserAdded:
                    _adapter.OnBrowserAdded(browserAdded.BrowserProfileId, browserAdded.BrowserDisplayName, browserAdded.ProfileDisplayName);
                    break;
                case Messages.NotifyMessageReceived messageReceived:
                    _adapter.OnMessageReceived(messageReceived);
                    break;
                case Messages.NotifyMetadataUpdated metadataUpdated:
                    _adapter.OnMetadataUpdated(metadataUpdated);
                    break;
            }
        }

        public void SetMessage(Messages.ISetMessageToPluginV2 message)
        {
            switch (message)
            {
                case Messages.RequestShowSettingsPanelToPlugin showSettingsPanel:
                    break;
                case Messages.RequestCloseToPlugin close:
                    _vm.IsClose = true;

                    _v.Visibility = System.Windows.Visibility.Hidden;

                    break;
            }
        }

        public MainViewPlugin()
        {
            _options = new DynamicOptionsTest();
        }
    }
}
