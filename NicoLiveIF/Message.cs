using SitePlugin;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Media;

namespace NicoSitePlugin
{
    public enum NicoMessageType
    {
        Unknown,
        Comment,
        Connected,
        Disconnected,
        Ad,
        Item,
        Spi,
        Emotion,
        Kick,
        Info,
        Ignored,
    }

    public interface INicoMessage : ISiteMessage
    {
        NicoMessageType NicoMessageType { get; }
    }
    public interface INicoConnected : INicoMessage
    {
        string Text { get; }
    }
    public interface INicoDisconnected : INicoMessage
    {
        string Text { get; }
    }
    public interface INicoComment : INicoMessage
    {
        string UserName { get; }
        string Text { get; }
        string Id { get; }
        bool Is184 { get; }
        [Obsolete]
        string RoomName { get; }
        int? ChatNo { get; }
        DateTime PostedAt { get; }
        string UserId { get; }
        string ThumbnailUrl { get; set; }
        void SetUserName(string userName);
    }
    public interface INicoAd : INicoMessage
    {
        string Text { get; }
        DateTime PostedAt { get; }
        string UserId { get; }
        string RoomName { get; }
    }
    public interface INicoGift : INicoMessage
    {
        string UserId { get; }
        DateTime PostedAt { get; }
        string Text { get; }
        [Obsolete]
        string RoomName { get; }
        string ItemName { get; }
        int ItemCount { get; }
    }
    public interface INicoSpi : INicoMessage
    {
        string Text { get; }
        DateTime PostedAt { get; }
        string UserId { get; }
    }
    public interface INicoEmotion : INicoMessage
    {
        string Content { get; }
        DateTime PostedAt { get; }
        int? ChatNo { get; }
        int Vpos { get; }
        string UserId { get; }
        int Premium { get; }
        int Anonymity { get; }
    }
    public interface INicoKickCommand : INicoMessage
    {

    }
    public interface INicoInfo : INicoMessage
    {
        string Text { get; }
        DateTime PostedAt { get; }
        string UserId { get; }
        string RoomName { get; }
        int No { get; }
    }
    public interface INicoSiteOptions : ISiteOptions, INotifyPropertyChanged
    {
        //公式放送では自分の部屋を入れて何部屋取得するか
        //"アリーナ:123456"等のコメントIDの表示形式。カスタマイズできるようにしたい
        //放送者コメント等の文字色、背景色
        int OfficialRoomsRetrieveCount { get; set; }
        /// <summary>
        /// 接続時に取得する直近の過去コメントの数
        /// </summary>
        int ResNum { get; set; }
        Color OperatorBackColor { get; set; }
        Color OperatorForeColor { get; set; }
        Color AdBackColor { get; set; }
        Color AdForeColor { get; set; }
        Color ItemBackColor { get; set; }
        Color ItemForeColor { get; set; }
        Color SpiBackColor { get; set; }
        Color SpiForeColor { get; set; }
        bool IsShowEmotion { get; }
        Color EmotionBackColor { get; set; }
        Color EmotionForeColor { get; set; }

        /// <summary>
        /// 184コメントを表示するか
        /// </summary>
        bool IsShow184 { get; set; }

        bool IsAutoSetNickname { get; set; }

        bool IsShow184Id { get; set; }
        bool IsAutoGetUsername { get; set; }
    }
}