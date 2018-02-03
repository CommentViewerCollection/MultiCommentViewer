using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Threading.Tasks;
using Plugin;
using System.Diagnostics;
using System.ComponentModel.Composition;
namespace BouyomiPlugin
{
    [Export(typeof(IPlugin))]
    public class BouyomiPlugin : IPlugin
    {
        private readonly FNF.Utility.BouyomiChanClient _bouyomiChanClient;
        private Options _options;
        Process _bouyomiChanProcess;
        public string Name => "棒読みちゃん連携";

        public string Description => "棒読みソフトとうまく連携できるか試してみるプラグインです。";

        public void OnLoaded()
        {
            _options = _OptionsLoader.Load(GetSettingsFilePath());
        }
        public void OnClosing()
        {
            _OptionsLoader.Save(_options, GetSettingsFilePath());
        }
        public void OnCommentReceived(ICommentData data)
        {
            if (!_options.IsEnabled || data.IsNgUser)
                return;
            try
            {
                //棒読みちゃんが事前に起動されていたらそれを使いたい。
                //起動していなかったら起動させて、準備ができ次第それ以降のコメントを読んで貰う

                //とりあえず何も確認せずにコメントを送信する。起動していなかったら例外が起きる。
                if (_options.IsReadHandleName && !string.IsNullOrEmpty(data.Nickname))
                {
                    var nick = data.Nickname;

                    if (_options.IsAppendNickTitle)
                        nick += _options.NickTitle;
                    _bouyomiChanClient.AddTalkTask2(nick);
                }
                if (_options.IsReadComment)
                    _bouyomiChanClient.AddTalkTask2(data.Comment);
            }
            catch (System.Runtime.Remoting.RemotingException)
            {
                //多分棒読みちゃんが起動していない。
                if (_bouyomiChanProcess == null && System.IO.File.Exists(_options.BouyomiChanPath))
                {
                    _bouyomiChanProcess = Process.Start(_options.BouyomiChanPath);
                    _bouyomiChanProcess.EnableRaisingEvents = true;
                    _bouyomiChanProcess.Exited += (s, e) =>
                    {
                        _bouyomiChanProcess.Close();
                        _bouyomiChanProcess = null;
                    };
                }
                //起動するまでの間にコメントが投稿されたらここに来てしまうが諦める。
            }
            catch (Exception)
            {

            }
        }
        public IPluginHost Host { get; set; }
        public string GetSettingsFilePath()
        {
            //ここでRemotingExceptionが発生。終了時の処理だが、既にHostがDisposeされてるのかも。
            var dir = Host.SettingsDirPath;
            return System.IO.Path.Combine(dir, $"{Name}.xml");
        }
        public void ShowSettingView()
        {
            var left = Host.MainViewLeft;
            var top = Host.MainViewTop;
            var view = new ConfigView();
            view.Left = left;
            view.Top = top;
            view.DataContext = new ConfigViewModel(_options);
            view.Show();
        }
        private readonly OptionsLoader _OptionsLoader;
        public BouyomiPlugin()
        {
            _bouyomiChanClient = new FNF.Utility.BouyomiChanClient();
            _OptionsLoader = new OptionsLoader();
        }
    }
    class Options:INotifyPropertyChanged
    {
        public bool IsEnabled { get; set; }
        private string _BouyomiChanPath;
        public string BouyomiChanPath
        {
            get { return _BouyomiChanPath; }
            set
            {
                if (_BouyomiChanPath == value)
                    return;
                _BouyomiChanPath = value;
                RaisePropertyChanged();
            }
        }
        public bool IsReadHandleName { get; set; }
        public bool IsReadComment { get; set; }
        public bool IsAppendNickTitle { get; set; }
        public string NickTitle { get; set; } = "さん";

        #region INotifyPropertyChanged
        [NonSerialized]
        private System.ComponentModel.PropertyChangedEventHandler _propertyChanged;
        /// <summary>
        /// 
        /// </summary>
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged
        {
            add { _propertyChanged += value; }
            remove { _propertyChanged -= value; }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="propertyName"></param>
        protected void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = "")
        {
            _propertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
    class OptionsLoader
    {
        public Options Load(string path)
        {
            var options = new Options();
            return options;
        }
        public void Save(Options options, string path)
        {

        }
    }
}
