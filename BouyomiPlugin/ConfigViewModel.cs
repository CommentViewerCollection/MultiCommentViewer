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
        #endregion //Mirrativ

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
