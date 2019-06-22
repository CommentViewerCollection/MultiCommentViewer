using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Diagnostics;
using System.Windows.Input;
namespace BouyomiPlugin
{
    class ConfigViewModel : ViewModelBase
    {
        private readonly Options _options;
        public bool IsEnabled
        {
            get { return _options.IsEnabled; }
            set { _options.IsEnabled = value; }
        }
        public string ExeLocation
        {
            get { return _options.BouyomiChanPath; }
            set { _options.BouyomiChanPath = value; }
        }
        public bool IsReadHandleName
        {
            get { return _options.IsReadHandleName; }
            set { _options.IsReadHandleName = value; }
        }
        public bool IsReadComment
        {
            get { return _options.IsReadComment; }
            set {  _options.IsReadComment = value; }
        }
        public bool IsAppendNickTitle
        {
            get { return _options.IsAppendNickTitle; }
            set { _options.IsAppendNickTitle = value; }
        }
        public string NickTitle
        {
            get { return _options.NickTitle; }
            set { _options.NickTitle = value; }
        }
        public bool Want184Read
        {
            get { return _options.Want184Read; }
            set { _options.Want184Read = value; }
        }
        public bool IsKillBouyomiChan
        {
            get { return _options.IsKillBouyomiChan; }
            set { _options.IsKillBouyomiChan = value; }
        }

        #region ふわっち
        /// <summary>
        /// ふわっちの接続メッセージを読み上げるか
        /// </summary>
        public bool IsWhowatchConnect
        {
            get => _options.IsWhowatchConnect;
            set => _options.IsWhowatchConnect = value;
        }
        /// <summary>
        /// ふわっちの切断メッセージを読み上げるか
        /// </summary>
        public bool IsWhowatchDisconnect
        {
            get => _options.IsWhowatchDisconnect;
            set => _options.IsWhowatchDisconnect = value;
        }
        /// <summary>
        /// ふわっちのコメントを読み上げるか
        /// </summary>
        public bool IsWhowatchComment
        {
            get => _options.IsWhowatchComment;
            set => _options.IsWhowatchComment = value;
        }
        /// <summary>
        /// ふわっちのコメントのコテハンを読み上げるか
        /// </summary>
        public bool IsWhowatchCommentNickname
        {
            get => _options.IsWhowatchCommentNickname;
            set => _options.IsWhowatchCommentNickname = value;
        }
        /// <summary>
        /// ふわっちのアイテムを読み上げるか
        /// </summary>
        public bool IsWhowatchItem
        {
            get => _options.IsWhowatchItem;
            set => _options.IsWhowatchItem = value;
        }
        /// <summary>
        /// ふわっちのアイテムのコテハンを読み上げるか
        /// </summary>
        public bool IsWhowatchItemNickname
        {
            get => _options.IsWhowatchItemNickname;
            set => _options.IsWhowatchItemNickname = value;
        }
        #endregion //ふわっち

        #region YouTubeLive
        /// <summary>
        /// YouTubeLiveの接続メッセージを読み上げるか
        /// </summary>
        public bool IsYouTubeLiveConnect
        {
            get => _options.IsYouTubeLiveConnect;
            set => _options.IsYouTubeLiveConnect = value;
        }
        /// <summary>
        /// YouTubeLiveの切断メッセージを読み上げるか
        /// </summary>
        public bool IsYouTubeLiveDisconnect
        {
            get => _options.IsYouTubeLiveDisconnect;
            set => _options.IsYouTubeLiveDisconnect = value;
        }
        /// <summary>
        /// YouTubeLiveのコメントを読み上げるか
        /// </summary>
        public bool IsYouTubeLiveComment
        {
            get => _options.IsYouTubeLiveComment;
            set => _options.IsYouTubeLiveComment = value;
        }
        /// <summary>
        /// YouTubeLiveのコメント中のスタンプを読み上げるか
        /// </summary>
        public bool IsYouTubeLiveCommentStamp
        {
            get => _options.IsYouTubeLiveCommentStamp;
            set => _options.IsYouTubeLiveCommentStamp = value;
        }
        /// <summary>
        /// YouTubeLiveのコメントのコテハンを読み上げるか
        /// </summary>
        public bool IsYouTubeLiveCommentNickname
        {
            get => _options.IsYouTubeLiveCommentNickname;
            set => _options.IsYouTubeLiveCommentNickname = value;
        }
        /// <summary>
        /// YouTubeLiveのsuper chat（投げ銭）を読み上げるか
        /// </summary>
        public bool IsYouTubeLiveSuperchat
        {
            get => _options.IsYouTubeLiveSuperchat;
            set => _options.IsYouTubeLiveSuperchat = value;
        }
        /// <summary>
        /// YouTubeLiveのsuper chat（投げ銭）のコテハンを読み上げるか
        /// </summary>
        public bool IsYouTubeLiveSuperchatNickname
        {
            get => _options.IsYouTubeLiveSuperchatNickname;
            set => _options.IsYouTubeLiveSuperchatNickname = value;
        }
        #endregion //YouTubeLive

        #region ニコ生
        /// <summary>
        /// ニコ生の接続メッセージを読み上げるか
        /// </summary>
        public bool IsNicoConnect
        {
            get => _options.IsNicoConnect;
            set => _options.IsNicoConnect = value;
        }
        /// <summary>
        /// ニコ生の切断メッセージを読み上げるか
        /// </summary>
        public bool IsNicoDisconnect
        {
            get => _options.IsNicoDisconnect;
            set => _options.IsNicoDisconnect = value;
        }
        /// <summary>
        /// ニコ生のコメントを読み上げるか
        /// </summary>
        public bool IsNicoComment
        {
            get => _options.IsNicoComment;
            set => _options.IsNicoComment = value;
        }
        /// <summary>
        /// ニコ生のコメントのコテハンを読み上げるか
        /// </summary>
        public bool IsNicoCommentNickname
        {
            get => _options.IsNicoCommentNickname;
            set => _options.IsNicoCommentNickname = value;
        }
        /// <summary>
        /// ニコ生のアイテムを読み上げるか
        /// </summary>
        public bool IsNicoItem
        {
            get => _options.IsNicoItem;
            set => _options.IsNicoItem = value;
        }
        /// <summary>
        /// ニコ生のアイテムのコテハンを読み上げるか
        /// </summary>
        public bool IsNicoItemNickname
        {
            get => _options.IsNicoItemNickname;
            set => _options.IsNicoItemNickname = value;
        }
        /// <summary>
        /// ニコ生の広告を読み上げるか
        /// </summary>
        public bool IsNicoAd
        {
            get => _options.IsNicoAd;
            set => _options.IsNicoAd = value;
        }
        #endregion //ニコ生

        #region Mirrativ
        /// <summary>
        /// Mirrativの接続メッセージを読み上げるか
        /// </summary>
        public bool IsMirrativConnect
        {
            get => _options.IsMirrativConnect;
            set => _options.IsMirrativConnect = value;
        }
        /// <summary>
        /// Mirrativの切断メッセージを読み上げるか
        /// </summary>
        public bool IsMirrativDisconnect
        {
            get => _options.IsMirrativDisconnect;
            set => _options.IsMirrativDisconnect = value;
        }
        /// <summary>
        /// Mirrativのコメントを読み上げるか
        /// </summary>
        public bool IsMirrativComment
        {
            get => _options.IsMirrativComment;
            set => _options.IsMirrativComment = value;
        }
        /// <summary>
        /// Mirrativのコメントのコテハンを読み上げるか
        /// </summary>
        public bool IsMirrativCommentNickname
        {
            get => _options.IsMirrativCommentNickname;
            set => _options.IsMirrativCommentNickname = value;
        }
        /// <summary>
        /// Mirrativの入室メッセージを読み上げるか
        /// </summary>
        public bool IsMirrativJoinRoom
        {
            get => _options.IsMirrativJoinRoom;
            set => _options.IsMirrativJoinRoom = value;
        }
        /// <summary>
        /// Mirrativのアイテムを読み上げるか
        /// </summary>
        public bool IsMirrativItem
        {
            get => _options.IsMirrativItem;
            set => _options.IsMirrativItem = value;
        }
        #endregion //Mirrativ

        #region Periscope
        /// <summary>
        /// Periscopeの接続メッセージを読み上げるか
        /// </summary>
        public bool IsPeriscopeConnect
        {
            get => _options.IsPeriscopeConnect;
            set => _options.IsPeriscopeConnect = value;
        }
        /// <summary>
        /// Periscopeの切断メッセージを読み上げるか
        /// </summary>
        public bool IsPeriscopeDisconnect
        {
            get => _options.IsPeriscopeDisconnect;
            set => _options.IsPeriscopeDisconnect = value;
        }
        /// <summary>
        /// Periscopeのコメントを読み上げるか
        /// </summary>
        public bool IsPeriscopeComment
        {
            get => _options.IsPeriscopeComment;
            set => _options.IsPeriscopeComment = value;
        }
        /// <summary>
        /// Periscopeのコメントのコテハンを読み上げるか
        /// </summary>
        public bool IsPeriscopeCommentNickname
        {
            get => _options.IsPeriscopeCommentNickname;
            set => _options.IsPeriscopeCommentNickname = value;
        }
        /// <summary>
        /// Periscopeの入室メッセージを読み上げるか
        /// </summary>
        public bool IsPeriscopeJoin
        {
            get => _options.IsPeriscopeJoin;
            set => _options.IsPeriscopeJoin = value;
        }
        /// <summary>
        /// Periscopeのアイテムを読み上げるか
        /// </summary>
        public bool IsPeriscopeLeave
        {
            get => _options.IsPeriscopeLeave;
            set => _options.IsPeriscopeLeave = value;
        }
        #endregion //Periscope

        public ICommand ShowFilePickerCommand { get; }
        private void ShowFilePicker()
        {
            try
            {
                var fileDialog = new System.Windows.Forms.OpenFileDialog
                {
                    Title = "棒読みちゃんの実行ファイル（BouyomiChan.exe）を選択してください",
                    Filter = "棒読みちゃん | BouyomiChan.exe"
                };
                var result = fileDialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    this.ExeLocation = fileDialog.FileName;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }
        public ConfigViewModel()
        {
            //if (IsInDesignMode)
            //{

            //}else
            //{
            //    throw new NotSupportedException();
            //}
        }
        [GalaSoft.MvvmLight.Ioc.PreferredConstructor]
        public ConfigViewModel(Options options)
        {
            _options = options;
            _options.PropertyChanged += (s, e) =>
            {
                switch (e.PropertyName)
                {
                    case nameof(_options.IsEnabled):
                        RaisePropertyChanged(nameof(IsEnabled));
                        break;
                    case nameof(_options.BouyomiChanPath):
                        RaisePropertyChanged(nameof(ExeLocation));
                        break;
                    case nameof(_options.IsReadHandleName):
                        RaisePropertyChanged(nameof(IsReadHandleName));
                        break;
                    case nameof(_options.IsReadComment):
                        RaisePropertyChanged(nameof(IsReadComment));
                        break;
                }
            };
            ShowFilePickerCommand = new RelayCommand(ShowFilePicker);
        }
    }
}
