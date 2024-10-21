using System;
using System.ComponentModel;
using Common;
namespace BouyomiPlugin
{
    class Options : DynamicOptionsBase
    {
        public bool IsEnabled { get { return GetValue(); } set { SetValue(value); } }
        public string BouyomiChanPath { get { return GetValue(); } set { SetValue(value); } }
        public bool IsReadHandleName { get { return GetValue(); } set { SetValue(value); } }
        public bool IsReadComment { get { return GetValue(); } set { SetValue(value); } }
        public bool IsAppendNickTitle { get { return GetValue(); } set { SetValue(value); } }
        public string NickTitle { get { return GetValue(); } set { SetValue(value); } }
        public bool Want184Read { get { return GetValue(); } set { SetValue(value); } }
        public bool IsExecBouyomiChanAtBoot { get { return GetValue(); } set { SetValue(value); } }
        public bool IsKillBouyomiChan { get { return GetValue(); } set { SetValue(value); } }
        public bool IsVoiceTypeSpecfied { get { return GetValue(); } set { SetValue(value); } }
        public int VoiceTypeIndex { get { return GetValue(); } set { SetValue(value); } }
        public int VoiceVolume { get { return GetValue(); } set { SetValue(value); } }
        public int VoiceSpeed { get { return GetValue(); } set { SetValue(value); } }
        public int VoiceTone { get { return GetValue(); } set { SetValue(value); } }

        //YouTubeLive
        public bool IsYouTubeLiveConnect { get { return GetValue(); } set { SetValue(value); } }
        public bool IsYouTubeLiveDisconnect { get { return GetValue(); } set { SetValue(value); } }
        public bool IsYouTubeLiveComment { get { return GetValue(); } set { SetValue(value); } }
        public bool IsYouTubeLiveCommentNickname { get { return GetValue(); } set { SetValue(value); } }
        /// <summary>
        /// YouTubeLiveのスタンプのaltを読み上げるか
        /// </summary>
        public bool IsYouTubeLiveCommentStamp { get { return GetValue(); } set { SetValue(value); } }
        public bool IsYouTubeLiveSuperchat { get { return GetValue(); } set { SetValue(value); } }
        public bool IsYouTubeLiveSuperchatNickname { get { return GetValue(); } set { SetValue(value); } }
        public bool IsYouTubeLiveMembership { get { return GetValue(); } set { SetValue(value); } }
        public bool IsYouTubeLiveMembershipNickname { get { return GetValue(); } set { SetValue(value); } }

        //OPENREC
        public bool IsOpenrecConnect { get { return GetValue(); } set { SetValue(value); } }
        public bool IsOpenrecDisconnect { get { return GetValue(); } set { SetValue(value); } }
        public bool IsOpenrecComment { get { return GetValue(); } set { SetValue(value); } }
        public bool IsOpenrecCommentNickname { get { return GetValue(); } set { SetValue(value); } }

        //B-LIVE
        public bool IsBLiveConnect { get { return GetValue(); } set { SetValue(value); } }
        public bool IsBLiveDisconnect { get { return GetValue(); } set { SetValue(value); } }
        public bool IsBLiveComment { get { return GetValue(); } set { SetValue(value); } }
        public bool IsBLiveCommentNickname { get { return GetValue(); } set { SetValue(value); } }

        //ミクチャ
        public bool IsMixchComment { get { return GetValue(); } set { SetValue(value); } }
        public bool IsMixchCommentNickname { get { return GetValue(); } set { SetValue(value); } }
        public bool IsMixchCommentOnlyFirst { get { return GetValue(); } set { SetValue(value); } }
        public bool IsMixchItem { get { return GetValue(); } set { SetValue(value); } }
        public bool IsMixchItemNickname { get { return GetValue(); } set { SetValue(value); } }
        public bool IsMixchSystem { get { return GetValue(); } set { SetValue(value); } }

        //Twitch
        public bool IsTwitchConnect { get { return GetValue(); } set { SetValue(value); } }
        public bool IsTwitchDisconnect { get { return GetValue(); } set { SetValue(value); } }
        public bool IsTwitchComment { get { return GetValue(); } set { SetValue(value); } }
        public bool IsTwitchCommentNickname { get { return GetValue(); } set { SetValue(value); } }

        //ニコ生
        public bool IsNicoConnect { get { return GetValue(); } set { SetValue(value); } }
        public bool IsNicoDisconnect { get { return GetValue(); } set { SetValue(value); } }
        public bool IsNicoComment { get { return GetValue(); } set { SetValue(value); } }
        public bool IsNicoCommentNickname { get { return GetValue(); } set { SetValue(value); } }
        public bool IsNicoItem { get { return GetValue(); } set { SetValue(value); } }
        public bool IsNicoItemNickname { get { return GetValue(); } set { SetValue(value); } }
        public bool IsNicoAd { get { return GetValue(); } set { SetValue(value); } }
        public bool IsNicoSpi { get { return GetValue(); } set { SetValue(value); } }
        public bool IsNicoEmotion { get { return GetValue(); } set { SetValue(value); } }

        //Twicas
        public bool IsTwicasConnect { get { return GetValue(); } set { SetValue(value); } }
        public bool IsTwicasDisconnect { get { return GetValue(); } set { SetValue(value); } }
        public bool IsTwicasComment { get { return GetValue(); } set { SetValue(value); } }
        public bool IsTwicasCommentNickname { get { return GetValue(); } set { SetValue(value); } }
        public bool IsTwicasItem { get { return GetValue(); } set { SetValue(value); } }
        public bool IsTwicasItemNickname { get { return GetValue(); } set { SetValue(value); } }

        //LINELIVE
        public bool IsLineLiveConnect { get { return GetValue(); } set { SetValue(value); } }
        public bool IsLineLiveDisconnect { get { return GetValue(); } set { SetValue(value); } }
        public bool IsLineLiveComment { get { return GetValue(); } set { SetValue(value); } }
        public bool IsLineLiveCommentNickname { get { return GetValue(); } set { SetValue(value); } }

        //ふわっち
        public bool IsWhowatchConnect { get { return GetValue(); } set { SetValue(value); } }
        public bool IsWhowatchDisconnect { get { return GetValue(); } set { SetValue(value); } }
        public bool IsWhowatchComment { get { return GetValue(); } set { SetValue(value); } }
        public bool IsWhowatchCommentNickname { get { return GetValue(); } set { SetValue(value); } }
        public bool IsWhowatchItem { get { return GetValue(); } set { SetValue(value); } }
        public bool IsWhowatchItemNickname { get { return GetValue(); } set { SetValue(value); } }

        //Mirrativ
        public bool IsMirrativConnect { get { return GetValue(); } set { SetValue(value); } }
        public bool IsMirrativDisconnect { get { return GetValue(); } set { SetValue(value); } }
        public bool IsMirrativComment { get { return GetValue(); } set { SetValue(value); } }
        public bool IsMirrativCommentNickname { get { return GetValue(); } set { SetValue(value); } }
        public bool IsMirrativJoinRoom { get { return GetValue(); } set { SetValue(value); } }
        public bool IsMirrativItem { get { return GetValue(); } set { SetValue(value); } }

        //Periscope
        public bool IsPeriscopeConnect { get { return GetValue(); } set { SetValue(value); } }
        public bool IsPeriscopeDisconnect { get { return GetValue(); } set { SetValue(value); } }
        public bool IsPeriscopeComment { get { return GetValue(); } set { SetValue(value); } }
        public bool IsPeriscopeCommentNickname { get { return GetValue(); } set { SetValue(value); } }
        public bool IsPeriscopeJoin { get { return GetValue(); } set { SetValue(value); } }
        public bool IsPeriscopeLeave { get { return GetValue(); } set { SetValue(value); } }

        //Mixer
        public bool IsMixerConnect { get { return GetValue(); } set { SetValue(value); } }
        public bool IsMixerDisconnect { get { return GetValue(); } set { SetValue(value); } }
        public bool IsMixerComment { get { return GetValue(); } set { SetValue(value); } }
        public bool IsMixerCommentNickname { get { return GetValue(); } set { SetValue(value); } }
        public bool IsMixerJoin { get { return GetValue(); } set { SetValue(value); } }
        public bool IsMixerLeave { get { return GetValue(); } set { SetValue(value); } }

        //Mildom
        public bool IsMildomConnect { get { return GetValue(); } set { SetValue(value); } }
        public bool IsMildomDisconnect { get { return GetValue(); } set { SetValue(value); } }
        public bool IsMildomComment { get { return GetValue(); } set { SetValue(value); } }
        public bool IsMildomCommentNickname { get { return GetValue(); } set { SetValue(value); } }
        public bool IsMildomCommentStampId { get { return GetValue(); } set { SetValue(value); } }
        public bool IsMildomJoin { get { return GetValue(); } set { SetValue(value); } }
        public bool IsMildomLeave { get { return GetValue(); } set { SetValue(value); } }
        public bool IsMildomGift { get { return GetValue(); } set { SetValue(value); } }
        public bool IsMildomGiftNickname { get { return GetValue(); } set { SetValue(value); } }

        //ShowRoom
        public bool IsShowRoomComment { get { return GetValue(); } set { SetValue(value); } }
        public bool IsShowRoomCommentNickname { get { return GetValue(); } set { SetValue(value); } }

        //BigoLive
        public bool IsBigoLiveComment { get { return GetValue(); } set { SetValue(value); } }
        public bool IsBigoLiveCommentNickname { get { return GetValue(); } set { SetValue(value); } }


        protected override void Init()
        {
            Dict.Add(nameof(IsEnabled), new Item { DefaultValue = false, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });
            Dict.Add(nameof(BouyomiChanPath), new Item { DefaultValue = "", Predicate = s => true, Serializer = s => s, Deserializer = s => s });
            Dict.Add(nameof(IsReadHandleName), new Item { DefaultValue = true, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });
            Dict.Add(nameof(IsReadComment), new Item { DefaultValue = true, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });
            Dict.Add(nameof(IsAppendNickTitle), new Item { DefaultValue = true, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });
            Dict.Add(nameof(NickTitle), new Item { DefaultValue = "さん", Predicate = s => true, Serializer = s => s, Deserializer = s => s });
            Dict.Add(nameof(Want184Read), new Item { DefaultValue = true, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });
            Dict.Add(nameof(IsExecBouyomiChanAtBoot), new Item { DefaultValue = false, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });
            Dict.Add(nameof(IsKillBouyomiChan), new Item { DefaultValue = false, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });
            Dict.Add(nameof(IsVoiceTypeSpecfied), new Item { DefaultValue = false, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });
            Dict.Add(nameof(VoiceTypeIndex), new Item { DefaultValue = 0, Predicate = n => true, Serializer = n => n.ToString(), Deserializer = n => int.Parse(n) });
            Dict.Add(nameof(VoiceVolume), new Item { DefaultValue = 100, Predicate = n => true, Serializer = n => n.ToString(), Deserializer = n => int.Parse(n) });
            Dict.Add(nameof(VoiceSpeed), new Item { DefaultValue = 100, Predicate = n => true, Serializer = n => n.ToString(), Deserializer = n => int.Parse(n) });
            Dict.Add(nameof(VoiceTone), new Item { DefaultValue = 100, Predicate = n => true, Serializer = n => n.ToString(), Deserializer = n => int.Parse(n) });

            //YouTubeLive
            Dict.Add(nameof(IsYouTubeLiveConnect), new Item { DefaultValue = false, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });
            Dict.Add(nameof(IsYouTubeLiveDisconnect), new Item { DefaultValue = false, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });
            Dict.Add(nameof(IsYouTubeLiveComment), new Item { DefaultValue = true, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });
            Dict.Add(nameof(IsYouTubeLiveCommentNickname), new Item { DefaultValue = true, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });
            Dict.Add(nameof(IsYouTubeLiveCommentStamp), new Item { DefaultValue = true, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });
            Dict.Add(nameof(IsYouTubeLiveSuperchat), new Item { DefaultValue = true, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });
            Dict.Add(nameof(IsYouTubeLiveSuperchatNickname), new Item { DefaultValue = true, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });
            Dict.Add(nameof(IsYouTubeLiveMembership), new Item { DefaultValue = true, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });
            Dict.Add(nameof(IsYouTubeLiveMembershipNickname), new Item { DefaultValue = true, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });

            //OPENREC
            Dict.Add(nameof(IsOpenrecConnect), new Item { DefaultValue = false, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });
            Dict.Add(nameof(IsOpenrecDisconnect), new Item { DefaultValue = false, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });
            Dict.Add(nameof(IsOpenrecComment), new Item { DefaultValue = true, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });
            Dict.Add(nameof(IsOpenrecCommentNickname), new Item { DefaultValue = true, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });

            //B-LIVE
            Dict.Add(nameof(IsBLiveConnect), new Item { DefaultValue = false, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });
            Dict.Add(nameof(IsBLiveDisconnect), new Item { DefaultValue = false, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });
            Dict.Add(nameof(IsBLiveComment), new Item { DefaultValue = true, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });
            Dict.Add(nameof(IsBLiveCommentNickname), new Item { DefaultValue = true, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });

            //ミクチャ
            Dict.Add(nameof(IsMixchComment), new Item { DefaultValue = true, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });
            Dict.Add(nameof(IsMixchCommentNickname), new Item { DefaultValue = true, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });
            Dict.Add(nameof(IsMixchCommentOnlyFirst), new Item { DefaultValue = false, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });
            Dict.Add(nameof(IsMixchItem), new Item { DefaultValue = true, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });
            Dict.Add(nameof(IsMixchItemNickname), new Item { DefaultValue = true, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });
            Dict.Add(nameof(IsMixchSystem), new Item { DefaultValue = true, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });

            //Twitch
            Dict.Add(nameof(IsTwitchConnect), new Item { DefaultValue = false, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });
            Dict.Add(nameof(IsTwitchDisconnect), new Item { DefaultValue = false, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });
            Dict.Add(nameof(IsTwitchComment), new Item { DefaultValue = true, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });
            Dict.Add(nameof(IsTwitchCommentNickname), new Item { DefaultValue = true, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });

            //ニコ生
            Dict.Add(nameof(IsNicoConnect), new Item { DefaultValue = false, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });
            Dict.Add(nameof(IsNicoDisconnect), new Item { DefaultValue = false, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });
            Dict.Add(nameof(IsNicoComment), new Item { DefaultValue = true, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });
            Dict.Add(nameof(IsNicoCommentNickname), new Item { DefaultValue = true, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });
            Dict.Add(nameof(IsNicoItem), new Item { DefaultValue = true, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });
            Dict.Add(nameof(IsNicoItemNickname), new Item { DefaultValue = true, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });
            Dict.Add(nameof(IsNicoAd), new Item { DefaultValue = true, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });
            Dict.Add(nameof(IsNicoSpi), new Item { DefaultValue = false, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });
            Dict.Add(nameof(IsNicoEmotion), new Item { DefaultValue = false, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });

            //Twicas
            Dict.Add(nameof(IsTwicasConnect), new Item { DefaultValue = false, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });
            Dict.Add(nameof(IsTwicasDisconnect), new Item { DefaultValue = false, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });
            Dict.Add(nameof(IsTwicasComment), new Item { DefaultValue = true, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });
            Dict.Add(nameof(IsTwicasCommentNickname), new Item { DefaultValue = true, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });
            Dict.Add(nameof(IsTwicasItem), new Item { DefaultValue = true, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });
            Dict.Add(nameof(IsTwicasItemNickname), new Item { DefaultValue = true, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });

            //LINELIVE
            Dict.Add(nameof(IsLineLiveConnect), new Item { DefaultValue = false, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });
            Dict.Add(nameof(IsLineLiveDisconnect), new Item { DefaultValue = false, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });
            Dict.Add(nameof(IsLineLiveComment), new Item { DefaultValue = true, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });
            Dict.Add(nameof(IsLineLiveCommentNickname), new Item { DefaultValue = true, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });

            //ふわっち
            Dict.Add(nameof(IsWhowatchConnect), new Item { DefaultValue = false, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });
            Dict.Add(nameof(IsWhowatchDisconnect), new Item { DefaultValue = false, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });
            Dict.Add(nameof(IsWhowatchComment), new Item { DefaultValue = true, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });
            Dict.Add(nameof(IsWhowatchCommentNickname), new Item { DefaultValue = true, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });
            Dict.Add(nameof(IsWhowatchItem), new Item { DefaultValue = true, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });
            Dict.Add(nameof(IsWhowatchItemNickname), new Item { DefaultValue = true, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });

            //Mirrativ
            Dict.Add(nameof(IsMirrativConnect), new Item { DefaultValue = false, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });
            Dict.Add(nameof(IsMirrativDisconnect), new Item { DefaultValue = false, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });
            Dict.Add(nameof(IsMirrativComment), new Item { DefaultValue = true, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });
            Dict.Add(nameof(IsMirrativCommentNickname), new Item { DefaultValue = true, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });
            Dict.Add(nameof(IsMirrativJoinRoom), new Item { DefaultValue = true, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });
            Dict.Add(nameof(IsMirrativItem), new Item { DefaultValue = true, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });

            //Periscope
            Dict.Add(nameof(IsPeriscopeConnect), new Item { DefaultValue = false, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });
            Dict.Add(nameof(IsPeriscopeDisconnect), new Item { DefaultValue = false, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });
            Dict.Add(nameof(IsPeriscopeComment), new Item { DefaultValue = true, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });
            Dict.Add(nameof(IsPeriscopeCommentNickname), new Item { DefaultValue = true, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });
            Dict.Add(nameof(IsPeriscopeJoin), new Item { DefaultValue = false, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });
            Dict.Add(nameof(IsPeriscopeLeave), new Item { DefaultValue = false, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });

            //Mixer
            Dict.Add(nameof(IsMixerConnect), new Item { DefaultValue = false, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });
            Dict.Add(nameof(IsMixerDisconnect), new Item { DefaultValue = false, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });
            Dict.Add(nameof(IsMixerComment), new Item { DefaultValue = true, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });
            Dict.Add(nameof(IsMixerCommentNickname), new Item { DefaultValue = true, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });
            Dict.Add(nameof(IsMixerJoin), new Item { DefaultValue = false, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });
            Dict.Add(nameof(IsMixerLeave), new Item { DefaultValue = false, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });

            //Mildom
            Dict.Add(nameof(IsMildomConnect), new Item { DefaultValue = false, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });
            Dict.Add(nameof(IsMildomDisconnect), new Item { DefaultValue = false, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });
            Dict.Add(nameof(IsMildomComment), new Item { DefaultValue = true, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });
            Dict.Add(nameof(IsMildomCommentNickname), new Item { DefaultValue = true, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });
            Dict.Add(nameof(IsMildomCommentStampId), new Item { DefaultValue = false, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });
            Dict.Add(nameof(IsMildomJoin), new Item { DefaultValue = false, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });
            Dict.Add(nameof(IsMildomLeave), new Item { DefaultValue = false, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });
            Dict.Add(nameof(IsMildomGift), new Item { DefaultValue = true, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });
            Dict.Add(nameof(IsMildomGiftNickname), new Item { DefaultValue = true, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });


            //ShowRoom
            Dict.Add(nameof(IsShowRoomComment), new Item { DefaultValue = true, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });
            Dict.Add(nameof(IsShowRoomCommentNickname), new Item { DefaultValue = true, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });

            //BigoLive
            Dict.Add(nameof(IsBigoLiveComment), new Item { DefaultValue = true, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });
            Dict.Add(nameof(IsBigoLiveCommentNickname), new Item { DefaultValue = true, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });

        }
    }
}
