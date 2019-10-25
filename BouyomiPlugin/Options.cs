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

        //OPENREC
        public bool IsOpenrecConnect { get { return GetValue(); } set { SetValue(value); } }
        public bool IsOpenrecDisconnect { get { return GetValue(); } set { SetValue(value); } }
        public bool IsOpenrecComment { get { return GetValue(); } set { SetValue(value); } }
        public bool IsOpenrecCommentNickname { get { return GetValue(); } set { SetValue(value); } }

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

        //Twicas
        public bool IsTwicasConnect { get { return GetValue(); } set { SetValue(value); } }
        public bool IsTwicasDisconnect { get { return GetValue(); } set { SetValue(value); } }
        public bool IsTwicasComment { get { return GetValue(); } set { SetValue(value); } }
        public bool IsTwicasCommentNickname { get { return GetValue(); } set { SetValue(value); } }

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
        protected override void Init()
        {
            Dict.Add(nameof(IsEnabled), new Item { DefaultValue = false, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });
            Dict.Add(nameof(BouyomiChanPath), new Item { DefaultValue = "", Predicate = s => true, Serializer = s => s, Deserializer = s => s });
            Dict.Add(nameof(IsReadHandleName), new Item { DefaultValue = true, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });
            Dict.Add(nameof(IsReadComment), new Item { DefaultValue = true, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });
            Dict.Add(nameof(IsAppendNickTitle), new Item { DefaultValue = true, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });
            Dict.Add(nameof(NickTitle), new Item { DefaultValue = "さん", Predicate = s => true, Serializer = s => s, Deserializer = s => s });
            Dict.Add(nameof(Want184Read), new Item { DefaultValue = true, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });
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

            //OPENREC
            Dict.Add(nameof(IsOpenrecConnect), new Item { DefaultValue = false, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });
            Dict.Add(nameof(IsOpenrecDisconnect), new Item { DefaultValue = false, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });
            Dict.Add(nameof(IsOpenrecComment), new Item { DefaultValue = true, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });
            Dict.Add(nameof(IsOpenrecCommentNickname), new Item { DefaultValue = true, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });

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

            //Twicas
            Dict.Add(nameof(IsTwicasConnect), new Item { DefaultValue = false, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });
            Dict.Add(nameof(IsTwicasDisconnect), new Item { DefaultValue = false, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });
            Dict.Add(nameof(IsTwicasComment), new Item { DefaultValue = true, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });
            Dict.Add(nameof(IsTwicasCommentNickname), new Item { DefaultValue = true, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });

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
        }
    }
}
