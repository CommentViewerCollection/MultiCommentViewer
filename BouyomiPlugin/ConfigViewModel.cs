using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Diagnostics;
using System.Windows.Input;
namespace BouyomiPlugin
{
    class ConfigViewModel : ViewModelBase
    {
        private const int VoiceTypeSapi5Offset = 10001;
        private readonly int VoiceTypeLength = Enum.GetNames(typeof(FNF.Utility.VoiceType)).Length;

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
            set { _options.IsReadComment = value; }
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
        public bool IsExecBouyomiChanAtBoot
        {
            get { return _options.IsExecBouyomiChanAtBoot; }
            set { _options.IsExecBouyomiChanAtBoot = value; }
        }
        public bool IsKillBouyomiChan
        {
            get { return _options.IsKillBouyomiChan; }
            set { _options.IsKillBouyomiChan = value; }
        }

        public bool IsVoiceTypeSpecfied
        {
            get { return _options.IsVoiceTypeSpecfied; }
            set { _options.IsVoiceTypeSpecfied = value; }
        }

        public int VoiceTypeSelectedIndex
        {
            get { var index = _options.VoiceTypeIndex; return index >= VoiceTypeSapi5Offset ? (index - VoiceTypeSapi5Offset) + VoiceTypeLength : index; }
            set { _options.VoiceTypeIndex = value < 0 ? 0 : value >= VoiceTypeLength ? VoiceTypeSapi5Offset + value - VoiceTypeLength : value; }
        }

        public int VoiceVolume
        {
            get { return _options.VoiceVolume; }
            set { _options.VoiceVolume = value; }
        }

        public int VoiceSpeed
        {
            get { return _options.VoiceSpeed; }
            set { _options.VoiceSpeed = value; }
        }

        public int VoiceTone
        {
            get { return _options.VoiceTone; }
            set { _options.VoiceTone = value; }
        }

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
        /// <summary>
        /// YouTubeLiveのメンバー登録を読み上げるか
        /// </summary>
        public bool IsYouTubeLiveMembership
        {
            get => _options.IsYouTubeLiveMembership;
            set => _options.IsYouTubeLiveMembership = value;
        }
        /// <summary>
        /// YouTubeLiveのメンバー登録のコテハンを読み上げるか
        /// </summary>
        public bool IsYouTubeLiveMembershipNickname
        {
            get => _options.IsYouTubeLiveMembershipNickname;
            set => _options.IsYouTubeLiveMembershipNickname = value;
        }
        #endregion //YouTubeLive

        #region OPENREC
        /// <summary>
        /// OPENRECの接続メッセージを読み上げるか
        /// </summary>
        public bool IsOpenrecConnect
        {
            get => _options.IsOpenrecConnect;
            set => _options.IsOpenrecConnect = value;
        }
        /// <summary>
        /// OPENRECの切断メッセージを読み上げるか
        /// </summary>
        public bool IsOpenrecDisconnect
        {
            get => _options.IsOpenrecDisconnect;
            set => _options.IsOpenrecDisconnect = value;
        }
        /// <summary>
        /// OPENRECのコメントを読み上げるか
        /// </summary>
        public bool IsOpenrecComment
        {
            get => _options.IsOpenrecComment;
            set => _options.IsOpenrecComment = value;
        }
        /// <summary>
        /// OPENRECのコメントのコテハンを読み上げるか
        /// </summary>
        public bool IsOpenrecCommentNickname
        {
            get => _options.IsOpenrecCommentNickname;
            set => _options.IsOpenrecCommentNickname = value;
        }
        ///// <summary>
        ///// OPENRECのアイテムを読み上げるか
        ///// </summary>
        //public bool IsOpenrecItem
        //{
        //    get => _options.IsOpenrecItem;
        //    set => _options.IsOpenrecItem = value;
        //}
        ///// <summary>
        ///// OPENRECのアイテムのコテハンを読み上げるか
        ///// </summary>
        //public bool IsOpenrecItemNickname
        //{
        //    get => _options.IsOpenrecItemNickname;
        //    set => _options.IsOpenrecItemNickname = value;
        //}
        #endregion //OPENREC

        #region B-LIVE
        /// <summary>
        /// B-LIVEの接続メッセージを読み上げるか
        /// </summary>
        public bool IsBLiveConnect
        {
            get => _options.IsBLiveConnect;
            set => _options.IsBLiveConnect = value;
        }
        /// <summary>
        /// B-LIVEの切断メッセージを読み上げるか
        /// </summary>
        public bool IsBLiveDisconnect
        {
            get => _options.IsBLiveDisconnect;
            set => _options.IsBLiveDisconnect = value;
        }
        /// <summary>
        /// B-LIVEのコメントを読み上げるか
        /// </summary>
        public bool IsBLiveComment
        {
            get => _options.IsBLiveComment;
            set => _options.IsBLiveComment = value;
        }
        /// <summary>
        /// B-LIVEのコメントのコテハンを読み上げるか
        /// </summary>
        public bool IsBLiveCommentNickname
        {
            get => _options.IsBLiveCommentNickname;
            set => _options.IsBLiveCommentNickname = value;
        }
        #endregion //B-LIVE

        #region ミクチャ
        /// <summary>
        /// MIXCHのコメントを読み上げるか
        /// </summary>
        public bool IsMixchComment
        {
            get => _options.IsMixchComment;
            set => _options.IsMixchComment = value;
        }
        /// <summary>
        /// MIXCHのコメントのコテハンを読み上げるか
        /// </summary>
        public bool IsMixchCommentNickname
        {
            get => _options.IsMixchCommentNickname;
            set => _options.IsMixchCommentNickname = value;
        }
        /// <summary>
        /// MIXCHのコメントで最初だけ読むか
        /// </summary>
        public bool IsMixchCommentOnlyFirst
        {
            get => _options.IsMixchCommentOnlyFirst;
            set => _options.IsMixchCommentOnlyFirst = value;
        }
        /// <summary>
        /// MIXCHのアイテムを読み上げるか
        /// </summary>
        public bool IsMixchItem
        {
            get => _options.IsMixchItem;
            set => _options.IsMixchItem = value;
        }
        /// <summary>
        /// MIXCHのアイテムのコテハンを読み上げるか
        /// </summary>
        public bool IsMixchItemNickname
        {
            get => _options.IsMixchItemNickname;
            set => _options.IsMixchItemNickname = value;
        }
        /// <summary>
        /// MIXCHのシステムメッセージを読み上げるか
        /// </summary>
        public bool IsMixchSystem
        {
            get => _options.IsMixchSystem;
            set => _options.IsMixchSystem = value;
        }
        #endregion //MIXCH

        #region Twitch
        /// <summary>
        /// Twitchの接続メッセージを読み上げるか
        /// </summary>
        public bool IsTwitchConnect
        {
            get => _options.IsTwitchConnect;
            set => _options.IsTwitchConnect = value;
        }
        /// <summary>
        /// Twitchの切断メッセージを読み上げるか
        /// </summary>
        public bool IsTwitchDisconnect
        {
            get => _options.IsTwitchDisconnect;
            set => _options.IsTwitchDisconnect = value;
        }
        /// <summary>
        /// Twitchのコメントを読み上げるか
        /// </summary>
        public bool IsTwitchComment
        {
            get => _options.IsTwitchComment;
            set => _options.IsTwitchComment = value;
        }
        /// <summary>
        /// Twitchのコメントのコテハンを読み上げるか
        /// </summary>
        public bool IsTwitchCommentNickname
        {
            get => _options.IsTwitchCommentNickname;
            set => _options.IsTwitchCommentNickname = value;
        }
        ///// <summary>
        ///// Twitchのアイテムを読み上げるか
        ///// </summary>
        //public bool IsTwitchItem
        //{
        //    get => _options.IsTwitchItem;
        //    set => _options.IsTwitchItem = value;
        //}
        ///// <summary>
        ///// Twitchのアイテムのコテハンを読み上げるか
        ///// </summary>
        //public bool IsTwitchItemNickname
        //{
        //    get => _options.IsTwitchItemNickname;
        //    set => _options.IsTwitchItemNickname = value;
        //}
        #endregion //Twitch

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
        /// <summary>
        /// ニコ生のリクエストを読み上げるか
        /// </summary>
        public bool IsNicoSpi
        {
            get => _options.IsNicoSpi;
            set => _options.IsNicoSpi = value;
        }
        /// <summary>
        /// ニコ生のエモーションを読み上げるか
        /// </summary>
        public bool IsNicoEmotion
        {
            get => _options.IsNicoEmotion;
            set => _options.IsNicoEmotion = value;
        }
        #endregion //ニコ生

        #region Twicas
        /// <summary>
        /// Twicasの接続メッセージを読み上げるか
        /// </summary>
        public bool IsTwicasConnect
        {
            get => _options.IsTwicasConnect;
            set => _options.IsTwicasConnect = value;
        }
        /// <summary>
        /// Twicasの切断メッセージを読み上げるか
        /// </summary>
        public bool IsTwicasDisconnect
        {
            get => _options.IsTwicasDisconnect;
            set => _options.IsTwicasDisconnect = value;
        }
        /// <summary>
        /// Twicasのコメントを読み上げるか
        /// </summary>
        public bool IsTwicasComment
        {
            get => _options.IsTwicasComment;
            set => _options.IsTwicasComment = value;
        }
        /// <summary>
        /// Twicasのコメントのコテハンを読み上げるか
        /// </summary>
        public bool IsTwicasCommentNickname
        {
            get => _options.IsTwicasCommentNickname;
            set => _options.IsTwicasCommentNickname = value;
        }
        /// <summary>
        /// Twicasのアイテムを読み上げるか
        /// </summary>
        public bool IsTwicasItem
        {
            get => _options.IsTwicasItem;
            set => _options.IsTwicasItem = value;
        }
        /// <summary>
        /// Twicasのアイテムのコテハンを読み上げるか
        /// </summary>
        public bool IsTwicasItemNickname
        {
            get => _options.IsTwicasItemNickname;
            set => _options.IsTwicasItemNickname = value;
        }
        #endregion //Twicas

        #region LINELIVE
        /// <summary>
        /// LINELIVEの接続メッセージを読み上げるか
        /// </summary>
        public bool IsLineLiveConnect
        {
            get => _options.IsLineLiveConnect;
            set => _options.IsLineLiveConnect = value;
        }
        /// <summary>
        /// LINELIVEの切断メッセージを読み上げるか
        /// </summary>
        public bool IsLineLiveDisconnect
        {
            get => _options.IsLineLiveDisconnect;
            set => _options.IsLineLiveDisconnect = value;
        }
        /// <summary>
        /// LINELIVEのコメントを読み上げるか
        /// </summary>
        public bool IsLineLiveComment
        {
            get => _options.IsLineLiveComment;
            set => _options.IsLineLiveComment = value;
        }
        /// <summary>
        /// LINELIVEのコメントのコテハンを読み上げるか
        /// </summary>
        public bool IsLineLiveCommentNickname
        {
            get => _options.IsLineLiveCommentNickname;
            set => _options.IsLineLiveCommentNickname = value;
        }
        ///// <summary>
        ///// LINELIVEのアイテムを読み上げるか
        ///// </summary>
        //public bool IsLineLiveItem
        //{
        //    get => _options.IsLineLiveItem;
        //    set => _options.IsLineLiveItem = value;
        //}
        ///// <summary>
        ///// LINELIVEのアイテムのコテハンを読み上げるか
        ///// </summary>
        //public bool IsLineLiveItemNickname
        //{
        //    get => _options.IsLineLiveItemNickname;
        //    set => _options.IsLineLiveItemNickname = value;
        //}
        #endregion //LINELIVE

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

        #region Mixer
        /// <summary>
        /// Mixerの接続メッセージを読み上げるか
        /// </summary>
        public bool IsMixerConnect
        {
            get => _options.IsMixerConnect;
            set => _options.IsMixerConnect = value;
        }
        /// <summary>
        /// Mixerの切断メッセージを読み上げるか
        /// </summary>
        public bool IsMixerDisconnect
        {
            get => _options.IsMixerDisconnect;
            set => _options.IsMixerDisconnect = value;
        }
        /// <summary>
        /// Mixerのコメントを読み上げるか
        /// </summary>
        public bool IsMixerComment
        {
            get => _options.IsMixerComment;
            set => _options.IsMixerComment = value;
        }
        /// <summary>
        /// Mixerのコメントのコテハンを読み上げるか
        /// </summary>
        public bool IsMixerCommentNickname
        {
            get => _options.IsMixerCommentNickname;
            set => _options.IsMixerCommentNickname = value;
        }
        /// <summary>
        /// Mixerの入室メッセージを読み上げるか
        /// </summary>
        public bool IsMixerJoin
        {
            get => _options.IsMixerJoin;
            set => _options.IsMixerJoin = value;
        }
        /// <summary>
        /// Mixerのアイテムを読み上げるか
        /// </summary>
        public bool IsMixerLeave
        {
            get => _options.IsMixerLeave;
            set => _options.IsMixerLeave = value;
        }
        #endregion //Mixer

        #region Mildom
        /// <summary>
        /// Mildomの接続メッセージを読み上げるか
        /// </summary>
        public bool IsMildomConnect
        {
            get => _options.IsMildomConnect;
            set => _options.IsMildomConnect = value;
        }
        /// <summary>
        /// Mildomの切断メッセージを読み上げるか
        /// </summary>
        public bool IsMildomDisconnect
        {
            get => _options.IsMildomDisconnect;
            set => _options.IsMildomDisconnect = value;
        }
        /// <summary>
        /// Mildomのコメントを読み上げるか
        /// </summary>
        public bool IsMildomComment
        {
            get => _options.IsMildomComment;
            set => _options.IsMildomComment = value;
        }
        /// <summary>
        /// Mildomのコメントのコテハンを読み上げるか
        /// </summary>
        public bool IsMildomCommentNickname
        {
            get => _options.IsMildomCommentNickname;
            set => _options.IsMildomCommentNickname = value;
        }
        /// <summary>
        /// MildomのコメントのスタンプIDを読み上げるか
        /// </summary>
        public bool IsMildomCommentStampId
        {
            get => _options.IsMildomCommentStampId;
            set => _options.IsMildomCommentStampId = value;
        }
        /// <summary>
        /// Mildomの入室メッセージを読み上げるか
        /// </summary>
        public bool IsMildomJoin
        {
            get => _options.IsMildomJoin;
            set => _options.IsMildomJoin = value;
        }
        /// <summary>
        /// Mildomのアイテムを読み上げるか
        /// </summary>
        public bool IsMildomLeave
        {
            get => _options.IsMildomLeave;
            set => _options.IsMildomLeave = value;
        }
        #endregion //Mildom

        #region ShowRoom
        /// <summary>
        /// ShowRoomの接続メッセージを読み上げるか
        /// </summary>
        //public bool IsShowRoomConnect
        //{
        //    get => _options.IsShowRoomConnect;
        //    set => _options.IsShowRoomConnect = value;
        //}
        /// <summary>
        /// ShowRoomの切断メッセージを読み上げるか
        /// </summary>
        //public bool IsShowRoomDisconnect
        //{
        //    get => _options.IsShowRoomDisconnect;
        //    set => _options.IsShowRoomDisconnect = value;
        //}
        /// <summary>
        /// ShowRoomのコメントを読み上げるか
        /// </summary>
        public bool IsShowRoomComment
        {
            get => _options.IsShowRoomComment;
            set => _options.IsShowRoomComment = value;
        }
        /// <summary>
        /// ShowRoomのコメントのコテハンを読み上げるか
        /// </summary>
        public bool IsShowRoomCommentNickname
        {
            get => _options.IsShowRoomCommentNickname;
            set => _options.IsShowRoomCommentNickname = value;
        }
        /// <summary>
        /// ShowRoomの入室メッセージを読み上げるか
        /// </summary>
        //public bool IsShowRoomJoin
        //{
        //    get => _options.IsShowRoomJoin;
        //    set => _options.IsShowRoomJoin = value;
        //}
        /// <summary>
        /// ShowRoomのアイテムを読み上げるか
        /// </summary>
        //public bool IsShowRoomLeave
        //{
        //    get => _options.IsShowRoomLeave;
        //    set => _options.IsShowRoomLeave = value;
        //}
        #endregion //ShowRoom

        #region BigoLive
        /// <summary>
        /// BigoLiveの接続メッセージを読み上げるか
        /// </summary>
        //public bool IsBigoLiveConnect
        //{
        //    get => _options.IsBigoLiveConnect;
        //    set => _options.IsBigoLiveConnect = value;
        //}
        /// <summary>
        /// BigoLiveの切断メッセージを読み上げるか
        /// </summary>
        //public bool IsBigoLiveDisconnect
        //{
        //    get => _options.IsBigoLiveDisconnect;
        //    set => _options.IsBigoLiveDisconnect = value;
        //}
        /// <summary>
        /// BigoLiveのコメントを読み上げるか
        /// </summary>
        public bool IsBigoLiveComment
        {
            get => _options.IsBigoLiveComment;
            set => _options.IsBigoLiveComment = value;
        }
        /// <summary>
        /// BigoLiveのコメントのコテハンを読み上げるか
        /// </summary>
        public bool IsBigoLiveCommentNickname
        {
            get => _options.IsBigoLiveCommentNickname;
            set => _options.IsBigoLiveCommentNickname = value;
        }
        /// <summary>
        /// BigoLiveの入室メッセージを読み上げるか
        /// </summary>
        //public bool IsBigoLiveJoin
        //{
        //    get => _options.IsBigoLiveJoin;
        //    set => _options.IsBigoLiveJoin = value;
        //}
        /// <summary>
        /// BigoLiveのアイテムを読み上げるか
        /// </summary>
        //public bool IsBigoLiveLeave
        //{
        //    get => _options.IsBigoLiveLeave;
        //    set => _options.IsBigoLiveLeave = value;
        //}
        #endregion //BigoLive
        public ICommand ShowFilePickerCommand { get; }
        private void ShowFilePicker()
        {
            try
            {
                var fileDialog = new Microsoft.Win32.OpenFileDialog
                {
                    Title = "棒読みちゃんの実行ファイル（BouyomiChan.exe）を選択してください",
                    Filter = "棒読みちゃん | BouyomiChan.exe"
                };
                var result = fileDialog.ShowDialog();
                if (result == true)
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
                    case nameof(_options.IsVoiceTypeSpecfied):
                        RaisePropertyChanged(nameof(IsVoiceTypeSpecfied));
                        break;
                }
            };
            ShowFilePickerCommand = new RelayCommand(ShowFilePicker);
        }
    }
}
