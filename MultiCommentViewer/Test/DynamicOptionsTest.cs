using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Diagnostics;
using SitePlugin;
using Common;
namespace MultiCommentViewer.Test
{
    class DynamicOptionsTest : DynamicOptionsBase, IOptions
    {
        public string PluginDir => "plugins";

        #region ConnectionsView
        public int ConnectionsViewConnectionNameDisplayIndex { get => GetValue(); set => SetValue(value); }
        public double ConnectionsViewConnectionNameWidth { get => GetValue(); set => SetValue(value); }
        public bool IsShowConnectionsViewConnectionName { get => GetValue(); set => SetValue(value); }

        public int ConnectionsViewSelectionDisplayIndex { get => GetValue(); set => SetValue(value); }
        public double ConnectionsViewSelectionWidth { get => GetValue(); set => SetValue(value); }
        public bool IsShowConnectionsViewSelection { get => GetValue(); set => SetValue(value); }

        public int ConnectionsViewInputDisplayIndex { get => GetValue(); set => SetValue(value); }
        public double ConnectionsViewInputWidth { get => GetValue(); set => SetValue(value); }
        public bool IsShowConnectionsViewInput { get => GetValue(); set => SetValue(value); }

        public int ConnectionsViewSiteDisplayIndex { get => GetValue(); set => SetValue(value); }
        public double ConnectionsViewSiteWidth { get => GetValue(); set => SetValue(value); }
        public bool IsShowConnectionsViewSite { get => GetValue(); set => SetValue(value); }

        public int ConnectionsViewBrowserDisplayIndex { get => GetValue(); set => SetValue(value); }
        public double ConnectionsViewBrowserWidth { get => GetValue(); set => SetValue(value); }
        public bool IsShowConnectionsViewBrowser { get => GetValue(); set => SetValue(value); }

        public int ConnectionsViewConnectionDisplayIndex { get => GetValue(); set => SetValue(value); }
        public double ConnectionsViewConnectionWidth { get => GetValue(); set => SetValue(value); }
        public bool IsShowConnectionsViewConnection { get => GetValue(); set => SetValue(value); }

        public int ConnectionsViewDisconnectionDisplayIndex { get => GetValue(); set => SetValue(value); }
        public double ConnectionsViewDisconnectionWidth { get => GetValue(); set => SetValue(value); }
        public bool IsShowConnectionsViewDisconnection { get => GetValue(); set => SetValue(value); }

        public int ConnectionsViewSaveDisplayIndex { get => GetValue(); set => SetValue(value); }
        public double ConnectionsViewSaveWidth { get => GetValue(); set => SetValue(value); }
        public bool IsShowConnectionsViewSave { get => GetValue(); set => SetValue(value); }

        public int ConnectionsViewLoggedinUsernameDisplayIndex { get => GetValue(); set => SetValue(value); }
        public double ConnectionsViewLoggedinUsernameWidth { get => GetValue(); set => SetValue(value); }
        public bool IsShowConnectionsViewLoggedinUsername { get => GetValue(); set => SetValue(value); }

        public int ConnectionsViewConnectionBackgroundDisplayIndex { get => GetValue(); set => SetValue(value); }
        public double ConnectionsViewConnectionBackgroundWidth { get => GetValue(); set => SetValue(value); }

        public int ConnectionsViewConnectionForegroundDisplayIndex { get => GetValue(); set => SetValue(value); }
        public double ConnectionsViewConnectionForegroundWidth { get => GetValue(); set => SetValue(value); }
        #endregion

        public FontFamily FontFamily { get => GetValue(); set => SetValue(value); }
        public FontStyle FontStyle { get => GetValue(); set => SetValue(value); }
        public FontWeight FontWeight { get => GetValue(); set => SetValue(value); }
        public int FontSize { get => GetValue(); set => SetValue(value); }
        public FontFamily FirstCommentFontFamily { get => GetValue(); set => SetValue(value); }
        public FontStyle FirstCommentFontStyle { get => GetValue(); set => SetValue(value); }
        public FontWeight FirstCommentFontWeight { get => GetValue(); set => SetValue(value); }
        public int FirstCommentFontSize { get => GetValue(); set => SetValue(value); }
        public Color FirstCommentBackColor { get => GetValue(); set => SetValue(value); }
        public Color FirstCommentForeColor { get => GetValue(); set => SetValue(value); }
        public string SettingsDirPath { get => GetValue(); set => SetValue(value); }
        public Color BackColor { get => GetValue(); set => SetValue(value); }
        public Color ForeColor { get => GetValue(); set => SetValue(value); }
        public double MainViewHeight { get => GetValue(); set => SetValue(value); }
        public double MainViewWidth { get => GetValue(); set => SetValue(value); }
        public double MainViewLeft { get => GetValue(); set => SetValue(value); }
        public double MainViewTop { get => GetValue(); set => SetValue(value); }
        public double ConnectionViewHeight { get => GetValue(); set => SetValue(value); }
        public double MetadataViewHeight { get => GetValue(); set => SetValue(value); }
        public Color HorizontalGridLineColor { get => GetValue(); set => SetValue(value); }
        public Color VerticalGridLineColor { get => GetValue(); set => SetValue(value); }
        public Color InfoForeColor { get => GetValue(); set => SetValue(value); }
        public Color InfoBackColor { get => GetValue(); set => SetValue(value); }
        public Color BroadcastInfoForeColor { get => GetValue(); set => SetValue(value); }
        public Color BroadcastInfoBackColor { get => GetValue(); set => SetValue(value); }
        public Color SelectedRowBackColor { get => GetValue(); set => SetValue(value); }
        public Color SelectedRowForeColor { get => GetValue(); set => SetValue(value); }
        public double ConnectionNameWidth { get => GetValue(); set => SetValue(value); }
        public bool IsShowConnectionName { get => GetValue(); set => SetValue(value); }
        public int ConnectionNameDisplayIndex { get => GetValue(); set => SetValue(value); }
        public double ThumbnailWidth { get => GetValue(); set => SetValue(value); }
        public int ThumbnailDisplayIndex { get => GetValue(); set => SetValue(value); }
        public bool IsShowThumbnail { get => GetValue(); set => SetValue(value); }
        public double CommentIdWidth { get => GetValue(); set => SetValue(value); }
        public int CommentIdDisplayIndex { get => GetValue(); set => SetValue(value); }
        public bool IsShowCommentId { get => GetValue(); set => SetValue(value); }
        public double UsernameWidth { get => GetValue(); set => SetValue(value); }
        public bool IsShowUsername { get => GetValue(); set => SetValue(value); }
        public int UsernameDisplayIndex { get => GetValue(); set => SetValue(value); }
        public double MessageWidth { get => GetValue(); set => SetValue(value); }
        public bool IsShowMessage { get => GetValue(); set => SetValue(value); }
        public int MessageDisplayIndex { get => GetValue(); set => SetValue(value); }

        public double PostTimeWidth { get => GetValue(); set => SetValue(value); }
        public bool IsShowPostTime { get => GetValue(); set => SetValue(value); }
        public int PostTimeDisplayIndex { get => GetValue(); set => SetValue(value); }

        public double InfoWidth { get => GetValue(); set => SetValue(value); }
        public bool IsShowInfo { get => GetValue(); set => SetValue(value); }
        public int InfoDisplayIndex { get => GetValue(); set => SetValue(value); }
        public bool IsShowVerticalGridLine { get => GetValue(); set => SetValue(value); }
        public bool IsShowHorizontalGridLine { get => GetValue(); set => SetValue(value); }


        public double MetadataViewConnectionNameColumnWidth { get => GetValue(); set => SetValue(value); }
        public bool IsShowMetaConnectionName { get => GetValue(); set => SetValue(value); }
        public int MetadataViewConnectionNameDisplayIndex { get => GetValue(); set => SetValue(value); }

        public double MetadataViewTitleColumnWidth { get => GetValue(); set => SetValue(value); }
        public bool IsShowMetaTitle { get => GetValue(); set => SetValue(value); }
        public int MetadataViewTitleDisplayIndex { get => GetValue(); set => SetValue(value); }

        public double MetadataViewElapsedColumnWidth { get => GetValue(); set => SetValue(value); }
        public bool IsShowMetaElapse { get => GetValue(); set => SetValue(value); }
        public int MetadataViewElapsedDisplayIndex { get => GetValue(); set => SetValue(value); }

        public double MetadataViewCurrentViewersColumnWidth { get => GetValue(); set => SetValue(value); }
        public bool IsShowMetaCurrentViewers { get => GetValue(); set => SetValue(value); }
        public int MetadataViewCurrentViewersDisplayIndex { get => GetValue(); set => SetValue(value); }

        public double MetadataViewTotalViewersColumnWidth { get => GetValue(); set => SetValue(value); }
        public bool IsShowMetaTotalViewers { get => GetValue(); set => SetValue(value); }
        public int MetadataViewTotalViewersDisplayIndex { get => GetValue(); set => SetValue(value); }

        public double MetadataViewActiveColumnWidth { get => GetValue(); set => SetValue(value); }
        public bool IsShowMetaActive { get => GetValue(); set => SetValue(value); }
        public int MetadataViewActiveDisplayIndex { get => GetValue(); set => SetValue(value); }

        public double MetadataViewOthersColumnWidth { get => GetValue(); set => SetValue(value); }
        public bool IsShowMetaOthers { get => GetValue(); set => SetValue(value); }
        public int MetadataViewOthersDisplayIndex { get => GetValue(); set => SetValue(value); }

        public Color TitleForeColor { get => GetValue(); set => SetValue(value); }
        public Color TitleBackColor { get => GetValue(); set => SetValue(value); }
        public Color ViewBackColor { get => GetValue(); set => SetValue(value); }
        public Color WindowBorderColor { get => GetValue(); set => SetValue(value); }
        public Color SystemButtonBackColor { get => GetValue(); set => SetValue(value); }
        public Color SystemButtonForeColor { get => GetValue(); set => SetValue(value); }
        public Color SystemButtonBorderColor { get => GetValue(); set => SetValue(value); }
        public Color SystemButtonMouseOverBackColor { get => GetValue(); set => SetValue(value); }
        public Color SystemButtonMouseOverForeColor { get => GetValue(); set => SetValue(value); }
        public Color SystemButtonMouseOverBorderColor { get => GetValue(); set => SetValue(value); }
        public Color MenuBackColor { get => GetValue(); set => SetValue(value); }
        public Color MenuForeColor { get => GetValue(); set => SetValue(value); }
        public Color MenuItemCheckMarkColor { get => GetValue(); set => SetValue(value); }
        public Color MenuItemMouseOverBackColor { get => GetValue(); set => SetValue(value); }
        public Color MenuItemMouseOverForeColor { get => GetValue(); set => SetValue(value); }
        public Color MenuItemMouseOverBorderColor { get => GetValue(); set => SetValue(value); }
        public Color MenuItemMouseOverCheckMarkColor { get => GetValue(); set => SetValue(value); }
        public Color MenuSeparatorBackColor { get => GetValue(); set => SetValue(value); }
        public Color MenuPopupBorderColor { get => GetValue(); set => SetValue(value); }
        public Color ButtonBackColor { get => GetValue(); set => SetValue(value); }
        public Color ButtonForeColor { get => GetValue(); set => SetValue(value); }
        public Color ButtonBorderColor { get => GetValue(); set => SetValue(value); }
        public Color CommentListBackColor { get => GetValue(); set => SetValue(value); }
        public Color CommentListHeaderBackColor { get => GetValue(); set => SetValue(value); }
        public Color CommentListHeaderForeColor { get => GetValue(); set => SetValue(value); }
        public Color CommentListHeaderBorderColor { get => GetValue(); set => SetValue(value); }
        public Color CommentListBorderColor { get => GetValue(); set => SetValue(value); }
        public Color CommentListSeparatorColor { get => GetValue(); set => SetValue(value); }
        public Color ConnectionListBackColor { get => GetValue(); set => SetValue(value); }
        public Color ConnectionListHeaderBackColor { get => GetValue(); set => SetValue(value); }
        public Color ConnectionListHeaderForeColor { get => GetValue(); set => SetValue(value); }
        public Color ConnectionListRowBackColor { get => GetValue(); set => SetValue(value); }

        public Color ScrollBarBackColor { get => GetValue(); set => SetValue(value); }
        public Color ScrollBarBorderColor { get => GetValue(); set => SetValue(value); }
        public Color ScrollBarThumbBackColor { get => GetValue(); set => SetValue(value); }
        public Color ScrollBarThumbMouseOverBackColor { get => GetValue(); set => SetValue(value); }
        public Color ScrollBarThumbPressedBackColor { get => GetValue(); set => SetValue(value); }
        public Color ScrollBarButtonBackColor { get => GetValue(); set => SetValue(value); }
        public Color ScrollBarButtonForeColor { get => GetValue(); set => SetValue(value); }
        public Color ScrollBarButtonBorderColor { get => GetValue(); set => SetValue(value); }
        public Color ScrollBarButtonDisabledBackColor { get => GetValue(); set => SetValue(value); }
        public Color ScrollBarButtonDisabledForeColor { get => GetValue(); set => SetValue(value); }
        public Color ScrollBarButtonDisabledBorderColor { get => GetValue(); set => SetValue(value); }
        public Color ScrollBarButtonMouseOverBackColor { get => GetValue(); set => SetValue(value); }
        public Color ScrollBarButtonMouseOverForeColor { get => GetValue(); set => SetValue(value); }
        public Color ScrollBarButtonMouseOverBorderColor { get => GetValue(); set => SetValue(value); }
        public Color ScrollBarButtonPressedBackColor { get => GetValue(); set => SetValue(value); }
        public Color ScrollBarButtonPressedForeColor { get => GetValue(); set => SetValue(value); }
        public Color ScrollBarButtonPressedBorderColor { get => GetValue(); set => SetValue(value); }

        public bool IsAutoCheckIfUpdateExists { get => GetValue(); set => SetValue(value); }
        public bool IsAddingNewCommentTop { get => GetValue(); set => SetValue(value); }
        public bool IsUserNameWrapping { get => GetValue(); set => SetValue(value); }
        public bool IsTopmost { get => GetValue(); set => SetValue(value); }
        public bool IsPixelScrolling { get => GetValue(); set => SetValue(value); }
        public bool IsEnabledSiteConnectionColor { get => GetValue(); set => SetValue(value); }
        public SiteConnectionColorType SiteConnectionColorType { get => GetValue(); set => SetValue(value); }
        public InfoType ShowingInfoLevel { get => GetValue(); set => SetValue(value); }
        public bool IsActiveCountEnabled { get => GetValue(); set => SetValue(value); }
        public int ActiveCountIntervalSec { get => GetValue(); set => SetValue(value); }
        public int ActiveMeasureSpanMin { get => GetValue(); set => SetValue(value); }

        public Color YouTubeLiveBackColor { get => GetValue(); set => SetValue(value); }
        public Color YouTubeLiveForeColor { get => GetValue(); set => SetValue(value); }
        public Color OpenrecBackColor { get => GetValue(); set => SetValue(value); }
        public Color OpenrecForeColor { get => GetValue(); set => SetValue(value); }
        public Color BLiveBackColor { get => GetValue(); set => SetValue(value); }
        public Color BLiveForeColor { get => GetValue(); set => SetValue(value); }
        public Color MixchBackColor { get => GetValue(); set => SetValue(value); }
        public Color MixchForeColor { get => GetValue(); set => SetValue(value); }
        public Color TwitchBackColor { get => GetValue(); set => SetValue(value); }
        public Color TwitchForeColor { get => GetValue(); set => SetValue(value); }
        public Color NicoLiveBackColor { get => GetValue(); set => SetValue(value); }
        public Color NicoLiveForeColor { get => GetValue(); set => SetValue(value); }
        public Color TwicasBackColor { get => GetValue(); set => SetValue(value); }
        public Color TwicasForeColor { get => GetValue(); set => SetValue(value); }
        public Color LineLiveBackColor { get => GetValue(); set => SetValue(value); }
        public Color LineLiveForeColor { get => GetValue(); set => SetValue(value); }
        public Color WhowatchBackColor { get => GetValue(); set => SetValue(value); }
        public Color WhowatchForeColor { get => GetValue(); set => SetValue(value); }
        public Color MirrativBackColor { get => GetValue(); set => SetValue(value); }
        public Color MirrativForeColor { get => GetValue(); set => SetValue(value); }
        public Color PeriscopeBackColor { get => GetValue(); set => SetValue(value); }
        public Color PeriscopeForeColor { get => GetValue(); set => SetValue(value); }
        public Color ShowRoomBackColor { get => GetValue(); set => SetValue(value); }
        public Color ShowRoomForeColor { get => GetValue(); set => SetValue(value); }

        protected override void Init()
        {
            #region ConnectionsView
            Dict.Add(nameof(ConnectionsViewSelectionDisplayIndex), new Item { DefaultValue = 0, Predicate = f => f >= 0, Serializer = f => f.ToString(), Deserializer = s => int.Parse(s) });
            Dict.Add(nameof(ConnectionsViewSelectionWidth), new Item { DefaultValue = 48, Predicate = n => n > 0, Serializer = n => n.ToString(), Deserializer = s => double.Parse(s) });
            Dict.Add(nameof(IsShowConnectionsViewSelection), new Item { DefaultValue = true, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });

            Dict.Add(nameof(ConnectionsViewSiteDisplayIndex), new Item { DefaultValue = 1, Predicate = f => f >= 0, Serializer = f => f.ToString(), Deserializer = s => int.Parse(s) });
            Dict.Add(nameof(ConnectionsViewSiteWidth), new Item { DefaultValue = 127, Predicate = n => n > 0, Serializer = n => n.ToString(), Deserializer = s => double.Parse(s) });
            Dict.Add(nameof(IsShowConnectionsViewSite), new Item { DefaultValue = true, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });

            Dict.Add(nameof(ConnectionsViewConnectionNameDisplayIndex), new Item { DefaultValue = 2, Predicate = f => f >= 0, Serializer = f => f.ToString(), Deserializer = s => int.Parse(s) });
            Dict.Add(nameof(ConnectionsViewConnectionNameWidth), new Item { DefaultValue = 57, Predicate = n => n > 0, Serializer = n => n.ToString(), Deserializer = s => double.Parse(s) });
            Dict.Add(nameof(IsShowConnectionsViewConnectionName), new Item { DefaultValue = true, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });

            Dict.Add(nameof(ConnectionsViewInputDisplayIndex), new Item { DefaultValue = 3, Predicate = f => f >= 0, Serializer = f => f.ToString(), Deserializer = s => int.Parse(s) });
            Dict.Add(nameof(ConnectionsViewInputWidth), new Item { DefaultValue = 222, Predicate = n => n > 0, Serializer = n => n.ToString(), Deserializer = s => double.Parse(s) });
            Dict.Add(nameof(IsShowConnectionsViewInput), new Item { DefaultValue = true, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });

            Dict.Add(nameof(ConnectionsViewBrowserDisplayIndex), new Item { DefaultValue = 4, Predicate = f => f >= 0, Serializer = f => f.ToString(), Deserializer = s => int.Parse(s) });
            Dict.Add(nameof(ConnectionsViewBrowserWidth), new Item { DefaultValue = 128, Predicate = n => n > 0, Serializer = n => n.ToString(), Deserializer = s => double.Parse(s) });
            Dict.Add(nameof(IsShowConnectionsViewBrowser), new Item { DefaultValue = true, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });

            Dict.Add(nameof(ConnectionsViewConnectionDisplayIndex), new Item { DefaultValue = 5, Predicate = f => f >= 0, Serializer = f => f.ToString(), Deserializer = s => int.Parse(s) });
            Dict.Add(nameof(ConnectionsViewConnectionWidth), new Item { DefaultValue = 49, Predicate = n => n > 0, Serializer = n => n.ToString(), Deserializer = s => double.Parse(s) });
            Dict.Add(nameof(IsShowConnectionsViewConnection), new Item { DefaultValue = true, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });

            Dict.Add(nameof(ConnectionsViewDisconnectionDisplayIndex), new Item { DefaultValue = 6, Predicate = f => f >= 0, Serializer = f => f.ToString(), Deserializer = s => int.Parse(s) });
            Dict.Add(nameof(ConnectionsViewDisconnectionWidth), new Item { DefaultValue = 50, Predicate = n => n > 0, Serializer = n => n.ToString(), Deserializer = s => double.Parse(s) });
            Dict.Add(nameof(IsShowConnectionsViewDisconnection), new Item { DefaultValue = true, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });

            Dict.Add(nameof(ConnectionsViewSaveDisplayIndex), new Item { DefaultValue = 7, Predicate = f => f >= 0, Serializer = f => f.ToString(), Deserializer = s => int.Parse(s) });
            Dict.Add(nameof(ConnectionsViewSaveWidth), new Item { DefaultValue = 37, Predicate = n => n > 0, Serializer = n => n.ToString(), Deserializer = s => double.Parse(s) });
            Dict.Add(nameof(IsShowConnectionsViewSave), new Item { DefaultValue = true, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });

            Dict.Add(nameof(ConnectionsViewLoggedinUsernameDisplayIndex), new Item { DefaultValue = 8, Predicate = f => f >= 0, Serializer = f => f.ToString(), Deserializer = s => int.Parse(s) });
            Dict.Add(nameof(ConnectionsViewLoggedinUsernameWidth), new Item { DefaultValue = 86, Predicate = n => n > 0, Serializer = n => n.ToString(), Deserializer = s => double.Parse(s) });
            Dict.Add(nameof(IsShowConnectionsViewLoggedinUsername), new Item { DefaultValue = true, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });

            Dict.Add(nameof(ConnectionsViewConnectionBackgroundDisplayIndex), new Item { DefaultValue = 9, Predicate = f => f > 0, Serializer = f => f.ToString(), Deserializer = s => int.Parse(s) });
            Dict.Add(nameof(ConnectionsViewConnectionBackgroundWidth), new Item { DefaultValue = 100, Predicate = n => n > 0, Serializer = n => n.ToString(), Deserializer = s => double.Parse(s) });

            Dict.Add(nameof(ConnectionsViewConnectionForegroundDisplayIndex), new Item { DefaultValue = 10, Predicate = f => f > 0, Serializer = f => f.ToString(), Deserializer = s => int.Parse(s) });
            Dict.Add(nameof(ConnectionsViewConnectionForegroundWidth), new Item { DefaultValue = 100, Predicate = n => n > 0, Serializer = n => n.ToString(), Deserializer = s => double.Parse(s) });
            #endregion


            Dict.Add(nameof(FontFamily), new Item { DefaultValue = new FontFamily("メイリオ"), Predicate = f => true, Serializer = f => FontFamilyToString(f), Deserializer = s => FontFamilyFromString(s) });
            Dict.Add(nameof(FontStyle), new Item { DefaultValue = FontStyles.Normal, Predicate = f => true, Serializer = f => FontStyleToString(f), Deserializer = s => FontStyleFromString(s) });
            Dict.Add(nameof(FontWeight), new Item { DefaultValue = FontWeights.Normal, Predicate = f => true, Serializer = f => FontWeightToString(f), Deserializer = s => FontWeightFromString(s) });
            Dict.Add(nameof(FontSize), new Item { DefaultValue = 14, Predicate = f => f > 0, Serializer = f => f.ToString(), Deserializer = s => int.Parse(s) });
            Dict.Add(nameof(FirstCommentFontFamily), new Item { DefaultValue = new FontFamily("メイリオ"), Predicate = f => true, Serializer = f => FontFamilyToString(f), Deserializer = s => FontFamilyFromString(s) });
            Dict.Add(nameof(FirstCommentFontStyle), new Item { DefaultValue = FontStyles.Normal, Predicate = f => true, Serializer = f => FontStyleToString(f), Deserializer = s => FontStyleFromString(s) });
            Dict.Add(nameof(FirstCommentFontWeight), new Item { DefaultValue = FontWeights.Bold, Predicate = f => true, Serializer = f => FontWeightToString(f), Deserializer = s => FontWeightFromString(s) });
            Dict.Add(nameof(FirstCommentFontSize), new Item { DefaultValue = 14, Predicate = f => f > 0, Serializer = f => f.ToString(), Deserializer = s => int.Parse(s) });
            Dict.Add(nameof(FirstCommentBackColor), new Item { DefaultValue = ColorFromArgb("#FFEFEFEF"), Predicate = c => true, Serializer = c => ColorToArgb(c), Deserializer = s => ColorFromArgb(s) });
            Dict.Add(nameof(FirstCommentForeColor), new Item { DefaultValue = ColorFromArgb("#FF000000"), Predicate = c => true, Serializer = c => ColorToArgb(c), Deserializer = s => ColorFromArgb(s) });

            Dict.Add(nameof(SettingsDirPath), new Item { DefaultValue = "settings", Predicate = s => !string.IsNullOrEmpty(s), Serializer = s => s, Deserializer = s => s });
            Dict.Add(nameof(BackColor), new Item { DefaultValue = ColorFromArgb("#FFEFEFEF"), Predicate = c => true, Serializer = c => ColorToArgb(c), Deserializer = s => ColorFromArgb(s) });
            Dict.Add(nameof(ForeColor), new Item { DefaultValue = ColorFromArgb("#FF000000"), Predicate = c => true, Serializer = c => ColorToArgb(c), Deserializer = s => ColorFromArgb(s) });
            Dict.Add(nameof(MainViewHeight), new Item { DefaultValue = 550, Predicate = n => n > 0, Serializer = n => n.ToString(), Deserializer = s => double.Parse(s) });
            Dict.Add(nameof(MainViewWidth), new Item { DefaultValue = 750, Predicate = n => n > 0, Serializer = n => n.ToString(), Deserializer = s => double.Parse(s) });
            Dict.Add(nameof(MainViewLeft), new Item { DefaultValue = 0, Predicate = n => true, Serializer = n => n.ToString(), Deserializer = s => double.Parse(s) });
            Dict.Add(nameof(MainViewTop), new Item { DefaultValue = 0, Predicate = n => true, Serializer = n => n.ToString(), Deserializer = s => double.Parse(s) });
            Dict.Add(nameof(ConnectionViewHeight), new Item { DefaultValue = 150, Predicate = n => n >= 0, Serializer = n => n.ToString(), Deserializer = s => double.Parse(s) });
            Dict.Add(nameof(MetadataViewHeight), new Item { DefaultValue = 100, Predicate = n => n >= 0, Serializer = n => n.ToString(), Deserializer = s => double.Parse(s) });
            Dict.Add(nameof(IsShowVerticalGridLine), new Item { DefaultValue = true, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });
            Dict.Add(nameof(IsShowHorizontalGridLine), new Item { DefaultValue = true, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });

            Dict.Add(nameof(HorizontalGridLineColor), new Item { DefaultValue = ColorFromArgb("#FFDCDCDC"), Predicate = c => true, Serializer = c => ColorToArgb(c), Deserializer = s => ColorFromArgb(s) });
            Dict.Add(nameof(VerticalGridLineColor), new Item { DefaultValue = ColorFromArgb("#FFDCDCDC"), Predicate = c => true, Serializer = c => ColorToArgb(c), Deserializer = s => ColorFromArgb(s) });
            Dict.Add(nameof(InfoForeColor), new Item { DefaultValue = ColorFromArgb("#FF000000"), Predicate = c => true, Serializer = c => ColorToArgb(c), Deserializer = s => ColorFromArgb(s) });
            Dict.Add(nameof(InfoBackColor), new Item { DefaultValue = ColorFromArgb("#FFFFFF00"), Predicate = c => true, Serializer = c => ColorToArgb(c), Deserializer = s => ColorFromArgb(s) });
            Dict.Add(nameof(BroadcastInfoForeColor), new Item { DefaultValue = ColorFromArgb("#FFFF0000"), Predicate = c => true, Serializer = c => ColorToArgb(c), Deserializer = s => ColorFromArgb(s) });
            Dict.Add(nameof(BroadcastInfoBackColor), new Item { DefaultValue = ColorFromArgb("#FFEFEFEF"), Predicate = c => true, Serializer = c => ColorToArgb(c), Deserializer = s => ColorFromArgb(s) });
            Dict.Add(nameof(SelectedRowBackColor), new Item { DefaultValue = ColorFromArgb("#FF0078D7"), Predicate = c => true, Serializer = c => ColorToArgb(c), Deserializer = s => ColorFromArgb(s) });
            Dict.Add(nameof(SelectedRowForeColor), new Item { DefaultValue = ColorFromArgb("#FFFFFFFF"), Predicate = c => true, Serializer = c => ColorToArgb(c), Deserializer = s => ColorFromArgb(s) });

            Dict.Add(nameof(ConnectionNameWidth), new Item { DefaultValue = 100, Predicate = n => n > 0, Serializer = n => n.ToString(), Deserializer = s => double.Parse(s) });
            Dict.Add(nameof(ThumbnailWidth), new Item { DefaultValue = 100, Predicate = n => n > 0, Serializer = n => n.ToString(), Deserializer = s => double.Parse(s) });
            Dict.Add(nameof(CommentIdWidth), new Item { DefaultValue = 100, Predicate = n => n > 0, Serializer = n => n.ToString(), Deserializer = s => double.Parse(s) });
            Dict.Add(nameof(UsernameWidth), new Item { DefaultValue = 100, Predicate = n => n > 0, Serializer = n => n.ToString(), Deserializer = s => double.Parse(s) });
            Dict.Add(nameof(MessageWidth), new Item { DefaultValue = 300, Predicate = n => n > 0, Serializer = n => n.ToString(), Deserializer = s => double.Parse(s) });
            Dict.Add(nameof(PostTimeWidth), new Item { DefaultValue = 100, Predicate = n => n > 0, Serializer = n => n.ToString(), Deserializer = s => double.Parse(s) });
            Dict.Add(nameof(InfoWidth), new Item { DefaultValue = 100, Predicate = n => n > 0, Serializer = n => n.ToString(), Deserializer = s => double.Parse(s) });

            Dict.Add(nameof(ConnectionNameDisplayIndex), new Item { DefaultValue = 0, Predicate = n => n >= 0, Serializer = n => n.ToString(), Deserializer = s => int.Parse(s) });
            Dict.Add(nameof(ThumbnailDisplayIndex), new Item { DefaultValue = 1, Predicate = n => n >= 0, Serializer = n => n.ToString(), Deserializer = s => int.Parse(s) });
            Dict.Add(nameof(CommentIdDisplayIndex), new Item { DefaultValue = 2, Predicate = n => n >= 0, Serializer = n => n.ToString(), Deserializer = s => int.Parse(s) });
            Dict.Add(nameof(UsernameDisplayIndex), new Item { DefaultValue = 3, Predicate = n => n >= 0, Serializer = n => n.ToString(), Deserializer = s => int.Parse(s) });
            Dict.Add(nameof(MessageDisplayIndex), new Item { DefaultValue = 4, Predicate = n => n >= 0, Serializer = n => n.ToString(), Deserializer = s => int.Parse(s) });
            Dict.Add(nameof(PostTimeDisplayIndex), new Item { DefaultValue = 5, Predicate = n => n >= 0, Serializer = n => n.ToString(), Deserializer = s => int.Parse(s) });
            Dict.Add(nameof(InfoDisplayIndex), new Item { DefaultValue = 6, Predicate = n => n >= 0, Serializer = n => n.ToString(), Deserializer = s => int.Parse(s) });

            Dict.Add(nameof(IsShowConnectionName), new Item { DefaultValue = true, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });
            Dict.Add(nameof(IsShowThumbnail), new Item { DefaultValue = false, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });
            Dict.Add(nameof(IsShowCommentId), new Item { DefaultValue = true, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });
            Dict.Add(nameof(IsShowUsername), new Item { DefaultValue = true, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });
            Dict.Add(nameof(IsShowMessage), new Item { DefaultValue = true, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });
            Dict.Add(nameof(IsShowPostTime), new Item { DefaultValue = false, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });
            Dict.Add(nameof(IsShowInfo), new Item { DefaultValue = true, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });

            Dict.Add(nameof(MetadataViewConnectionNameDisplayIndex), new Item { DefaultValue = 0, Predicate = n => n >= 0, Serializer = n => n.ToString(), Deserializer = s => int.Parse(s) });
            Dict.Add(nameof(MetadataViewTitleDisplayIndex), new Item { DefaultValue = 1, Predicate = n => n >= 0, Serializer = n => n.ToString(), Deserializer = s => int.Parse(s) });
            Dict.Add(nameof(MetadataViewElapsedDisplayIndex), new Item { DefaultValue = 2, Predicate = n => n >= 0, Serializer = n => n.ToString(), Deserializer = s => int.Parse(s) });
            Dict.Add(nameof(MetadataViewCurrentViewersDisplayIndex), new Item { DefaultValue = 3, Predicate = n => n >= 0, Serializer = n => n.ToString(), Deserializer = s => int.Parse(s) });
            Dict.Add(nameof(MetadataViewTotalViewersDisplayIndex), new Item { DefaultValue = 4, Predicate = n => n >= 0, Serializer = n => n.ToString(), Deserializer = s => int.Parse(s) });
            Dict.Add(nameof(MetadataViewActiveDisplayIndex), new Item { DefaultValue = 5, Predicate = n => n >= 0, Serializer = n => n.ToString(), Deserializer = s => int.Parse(s) });
            Dict.Add(nameof(MetadataViewOthersDisplayIndex), new Item { DefaultValue = 6, Predicate = n => n >= 0, Serializer = n => n.ToString(), Deserializer = s => int.Parse(s) });
            Dict.Add(nameof(IsShowMetaConnectionName), new Item { DefaultValue = true, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });
            Dict.Add(nameof(IsShowMetaTitle), new Item { DefaultValue = true, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });
            Dict.Add(nameof(IsShowMetaElapse), new Item { DefaultValue = true, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });
            Dict.Add(nameof(IsShowMetaCurrentViewers), new Item { DefaultValue = true, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });
            Dict.Add(nameof(IsShowMetaTotalViewers), new Item { DefaultValue = true, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });
            Dict.Add(nameof(IsShowMetaActive), new Item { DefaultValue = true, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });
            Dict.Add(nameof(IsShowMetaOthers), new Item { DefaultValue = true, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });
            Dict.Add(nameof(MetadataViewConnectionNameColumnWidth), new Item { DefaultValue = 100, Predicate = n => n > 0, Serializer = n => n.ToString(), Deserializer = s => double.Parse(s) });
            Dict.Add(nameof(MetadataViewTitleColumnWidth), new Item { DefaultValue = 100, Predicate = n => n > 0, Serializer = n => n.ToString(), Deserializer = s => double.Parse(s) });
            Dict.Add(nameof(MetadataViewElapsedColumnWidth), new Item { DefaultValue = 100, Predicate = n => n > 0, Serializer = n => n.ToString(), Deserializer = s => double.Parse(s) });
            Dict.Add(nameof(MetadataViewCurrentViewersColumnWidth), new Item { DefaultValue = 100, Predicate = n => n > 0, Serializer = n => n.ToString(), Deserializer = s => double.Parse(s) });
            Dict.Add(nameof(MetadataViewTotalViewersColumnWidth), new Item { DefaultValue = 100, Predicate = n => n > 0, Serializer = n => n.ToString(), Deserializer = s => double.Parse(s) });
            Dict.Add(nameof(MetadataViewActiveColumnWidth), new Item { DefaultValue = 100, Predicate = n => n > 0, Serializer = n => n.ToString(), Deserializer = s => double.Parse(s) });
            Dict.Add(nameof(MetadataViewOthersColumnWidth), new Item { DefaultValue = 100, Predicate = n => n > 0, Serializer = n => n.ToString(), Deserializer = s => double.Parse(s) });

            Dict.Add(nameof(TitleForeColor), new Item { DefaultValue = ColorFromArgb("#FF000000"), Predicate = c => true, Serializer = c => ColorToArgb(c), Deserializer = s => ColorFromArgb(s) });
            Dict.Add(nameof(TitleBackColor), new Item { DefaultValue = ColorFromArgb("#FFFFFFFF"), Predicate = c => true, Serializer = c => ColorToArgb(c), Deserializer = s => ColorFromArgb(s) });
            Dict.Add(nameof(ViewBackColor), new Item { DefaultValue = ColorFromArgb("#FFFFFFFF"), Predicate = c => true, Serializer = c => ColorToArgb(c), Deserializer = s => ColorFromArgb(s) });
            Dict.Add(nameof(WindowBorderColor), new Item { DefaultValue = ColorFromArgb("#FF000000"), Predicate = c => true, Serializer = c => ColorToArgb(c), Deserializer = s => ColorFromArgb(s) });
            Dict.Add(nameof(SystemButtonBackColor), new Item { DefaultValue = ColorFromArgb("#FFFFFFFF"), Predicate = c => true, Serializer = c => ColorToArgb(c), Deserializer = s => ColorFromArgb(s) });
            Dict.Add(nameof(SystemButtonForeColor), new Item { DefaultValue = ColorFromArgb("#FF000000"), Predicate = c => true, Serializer = c => ColorToArgb(c), Deserializer = s => ColorFromArgb(s) });
            Dict.Add(nameof(SystemButtonBorderColor), new Item { DefaultValue = ColorFromArgb("#FFFFFFFF"), Predicate = c => true, Serializer = c => ColorToArgb(c), Deserializer = s => ColorFromArgb(s) });
            Dict.Add(nameof(SystemButtonMouseOverBackColor), new Item { DefaultValue = ColorFromArgb("#FFE5E5E5"), Predicate = c => true, Serializer = c => ColorToArgb(c), Deserializer = s => ColorFromArgb(s) });
            Dict.Add(nameof(SystemButtonMouseOverForeColor), new Item { DefaultValue = ColorFromArgb("#FF000000"), Predicate = c => true, Serializer = c => ColorToArgb(c), Deserializer = s => ColorFromArgb(s) });
            Dict.Add(nameof(SystemButtonMouseOverBorderColor), new Item { DefaultValue = ColorFromArgb("#FFE5E5E5"), Predicate = c => true, Serializer = c => ColorToArgb(c), Deserializer = s => ColorFromArgb(s) });
            Dict.Add(nameof(MenuBackColor), new Item { DefaultValue = ColorFromArgb("##FFF0F0F0"), Predicate = c => true, Serializer = c => ColorToArgb(c), Deserializer = s => ColorFromArgb(s) });
            Dict.Add(nameof(MenuForeColor), new Item { DefaultValue = ColorFromArgb("#FF000000"), Predicate = c => true, Serializer = c => ColorToArgb(c), Deserializer = s => ColorFromArgb(s) });
            Dict.Add(nameof(MenuItemCheckMarkColor), new Item { DefaultValue = ColorFromArgb("#FF000000"), Predicate = c => true, Serializer = c => ColorToArgb(c), Deserializer = s => ColorFromArgb(s) });
            Dict.Add(nameof(MenuItemMouseOverBackColor), new Item { DefaultValue = ColorFromArgb("#FFF0F0F0"), Predicate = c => true, Serializer = c => ColorToArgb(c), Deserializer = s => ColorFromArgb(s) });
            Dict.Add(nameof(MenuItemMouseOverForeColor), new Item { DefaultValue = ColorFromArgb("#FF000000"), Predicate = c => true, Serializer = c => ColorToArgb(c), Deserializer = s => ColorFromArgb(s) });
            Dict.Add(nameof(MenuItemMouseOverBorderColor), new Item { DefaultValue = ColorFromArgb("#FF0000FF"), Predicate = c => true, Serializer = c => ColorToArgb(c), Deserializer = s => ColorFromArgb(s) });
            Dict.Add(nameof(MenuItemMouseOverCheckMarkColor), new Item { DefaultValue = ColorFromArgb("#FF000000"), Predicate = c => true, Serializer = c => ColorToArgb(c), Deserializer = s => ColorFromArgb(s) });
            Dict.Add(nameof(MenuSeparatorBackColor), new Item { DefaultValue = ColorFromArgb("#FFD7D7D7"), Predicate = c => true, Serializer = c => ColorToArgb(c), Deserializer = s => ColorFromArgb(s) });
            Dict.Add(nameof(MenuPopupBorderColor), new Item { DefaultValue = ColorFromArgb("#FF000000"), Predicate = c => true, Serializer = c => ColorToArgb(c), Deserializer = s => ColorFromArgb(s) });


            Dict.Add(nameof(ButtonBackColor), new Item { DefaultValue = ColorFromArgb("#FFDDDDDD"), Predicate = c => true, Serializer = c => ColorToArgb(c), Deserializer = s => ColorFromArgb(s) });
            Dict.Add(nameof(ButtonForeColor), new Item { DefaultValue = ColorFromArgb("#FF000000"), Predicate = c => true, Serializer = c => ColorToArgb(c), Deserializer = s => ColorFromArgb(s) });
            Dict.Add(nameof(ButtonBorderColor), new Item { DefaultValue = ColorFromArgb("#FF000000"), Predicate = c => true, Serializer = c => ColorToArgb(c), Deserializer = s => ColorFromArgb(s) });

            Dict.Add(nameof(CommentListBackColor), new Item { DefaultValue = ColorFromArgb("#FFF0F0F0"), Predicate = c => true, Serializer = c => ColorToArgb(c), Deserializer = s => ColorFromArgb(s) });
            Dict.Add(nameof(CommentListBorderColor), new Item { DefaultValue = ColorFromArgb("#FF000000"), Predicate = c => true, Serializer = c => ColorToArgb(c), Deserializer = s => ColorFromArgb(s) });
            Dict.Add(nameof(CommentListHeaderBackColor), new Item { DefaultValue = ColorFromArgb("#FFF4F5F7"), Predicate = c => true, Serializer = c => ColorToArgb(c), Deserializer = s => ColorFromArgb(s) });
            Dict.Add(nameof(CommentListHeaderForeColor), new Item { DefaultValue = ColorFromArgb("#FF000000"), Predicate = c => true, Serializer = c => ColorToArgb(c), Deserializer = s => ColorFromArgb(s) });
            Dict.Add(nameof(CommentListHeaderBorderColor), new Item { DefaultValue = ColorFromArgb("#FFFFFFFF"), Predicate = c => true, Serializer = c => ColorToArgb(c), Deserializer = s => ColorFromArgb(s) });


            Dict.Add(nameof(CommentListSeparatorColor), new Item { DefaultValue = ColorFromArgb("#FFE4E5E7"), Predicate = c => true, Serializer = c => ColorToArgb(c), Deserializer = s => ColorFromArgb(s) });
            Dict.Add(nameof(ConnectionListBackColor), new Item { DefaultValue = ColorFromArgb("#FFFFFFFF"), Predicate = c => true, Serializer = c => ColorToArgb(c), Deserializer = s => ColorFromArgb(s) });
            Dict.Add(nameof(ConnectionListHeaderBackColor), new Item { DefaultValue = ColorFromArgb("#FFF8F9FA"), Predicate = c => true, Serializer = c => ColorToArgb(c), Deserializer = s => ColorFromArgb(s) });
            Dict.Add(nameof(ConnectionListHeaderForeColor), new Item { DefaultValue = ColorFromArgb("#FF000000"), Predicate = c => true, Serializer = c => ColorToArgb(c), Deserializer = s => ColorFromArgb(s) });
            Dict.Add(nameof(ConnectionListRowBackColor), new Item { DefaultValue = ColorFromArgb("#FFFFFFFF"), Predicate = c => true, Serializer = c => ColorToArgb(c), Deserializer = s => ColorFromArgb(s) });

            Dict.Add(nameof(ScrollBarBackColor), new Item { DefaultValue = ColorFromArgb("#FFF0F0F0"), Predicate = c => true, Serializer = c => ColorToArgb(c), Deserializer = s => ColorFromArgb(s) });
            Dict.Add(nameof(ScrollBarBorderColor), new Item { DefaultValue = ColorFromArgb("#FFF0F0F0"), Predicate = c => true, Serializer = c => ColorToArgb(c), Deserializer = s => ColorFromArgb(s) });
            Dict.Add(nameof(ScrollBarThumbBackColor), new Item { DefaultValue = ColorFromArgb("#FFCDCDCD"), Predicate = c => true, Serializer = c => ColorToArgb(c), Deserializer = s => ColorFromArgb(s) });
            Dict.Add(nameof(ScrollBarThumbMouseOverBackColor), new Item { DefaultValue = ColorFromArgb("#FFCDCDCD"), Predicate = c => true, Serializer = c => ColorToArgb(c), Deserializer = s => ColorFromArgb(s) });
            Dict.Add(nameof(ScrollBarThumbPressedBackColor), new Item { DefaultValue = ColorFromArgb("#FFCDCDCD"), Predicate = c => true, Serializer = c => ColorToArgb(c), Deserializer = s => ColorFromArgb(s) });

            Dict.Add(nameof(ScrollBarButtonBackColor), new Item { DefaultValue = ColorFromArgb("#FFF0F0F0"), Predicate = c => true, Serializer = c => ColorToArgb(c), Deserializer = s => ColorFromArgb(s) });
            Dict.Add(nameof(ScrollBarButtonForeColor), new Item { DefaultValue = ColorFromArgb("#FF0A0A0A"), Predicate = c => true, Serializer = c => ColorToArgb(c), Deserializer = s => ColorFromArgb(s) });
            Dict.Add(nameof(ScrollBarButtonBorderColor), new Item { DefaultValue = ColorFromArgb("#FFCDCDCD"), Predicate = c => true, Serializer = c => ColorToArgb(c), Deserializer = s => ColorFromArgb(s) });
            Dict.Add(nameof(ScrollBarButtonDisabledBackColor), new Item { DefaultValue = ColorFromArgb("#FFF0F0F0"), Predicate = c => true, Serializer = c => ColorToArgb(c), Deserializer = s => ColorFromArgb(s) });
            Dict.Add(nameof(ScrollBarButtonDisabledForeColor), new Item { DefaultValue = ColorFromArgb("#FFDDDDDD"), Predicate = c => true, Serializer = c => ColorToArgb(c), Deserializer = s => ColorFromArgb(s) });
            Dict.Add(nameof(ScrollBarButtonDisabledBorderColor), new Item { DefaultValue = ColorFromArgb("#FFF0F0F0"), Predicate = c => true, Serializer = c => ColorToArgb(c), Deserializer = s => ColorFromArgb(s) });
            Dict.Add(nameof(ScrollBarButtonMouseOverBackColor), new Item { DefaultValue = ColorFromArgb("#FFDADADA"), Predicate = c => true, Serializer = c => ColorToArgb(c), Deserializer = s => ColorFromArgb(s) });
            Dict.Add(nameof(ScrollBarButtonMouseOverForeColor), new Item { DefaultValue = ColorFromArgb("#FF000000"), Predicate = c => true, Serializer = c => ColorToArgb(c), Deserializer = s => ColorFromArgb(s) });
            Dict.Add(nameof(ScrollBarButtonMouseOverBorderColor), new Item { DefaultValue = ColorFromArgb("#FFDADADA"), Predicate = c => true, Serializer = c => ColorToArgb(c), Deserializer = s => ColorFromArgb(s) });
            Dict.Add(nameof(ScrollBarButtonPressedBackColor), new Item { DefaultValue = ColorFromArgb("#FF606060"), Predicate = c => true, Serializer = c => ColorToArgb(c), Deserializer = s => ColorFromArgb(s) });
            Dict.Add(nameof(ScrollBarButtonPressedForeColor), new Item { DefaultValue = ColorFromArgb("#FFFFFFFF"), Predicate = c => true, Serializer = c => ColorToArgb(c), Deserializer = s => ColorFromArgb(s) });
            Dict.Add(nameof(ScrollBarButtonPressedBorderColor), new Item { DefaultValue = ColorFromArgb("#FF606060"), Predicate = c => true, Serializer = c => ColorToArgb(c), Deserializer = s => ColorFromArgb(s) });



            Dict.Add(nameof(IsAutoCheckIfUpdateExists), new Item { DefaultValue = true, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });

            Dict.Add(nameof(IsAddingNewCommentTop), new Item { DefaultValue = false, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });
            Dict.Add(nameof(IsUserNameWrapping), new Item { DefaultValue = false, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });
            Dict.Add(nameof(IsTopmost), new Item { DefaultValue = false, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });
            Dict.Add(nameof(IsPixelScrolling), new Item { DefaultValue = false, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });
            Dict.Add(nameof(IsEnabledSiteConnectionColor), new Item { DefaultValue = false, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });
            Dict.Add(nameof(SiteConnectionColorType), new Item { DefaultValue = SiteConnectionColorType.Site, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => SiteConnectionColorTypeRelatedOperations.ToSiteConnectionColorType(s) });
            Dict.Add(nameof(ShowingInfoLevel), new Item { DefaultValue = InfoType.Notice, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => InfoTypeRelatedOperations.ToInfoType(s) });
            Dict.Add(nameof(IsActiveCountEnabled), new Item { DefaultValue = true, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });
            Dict.Add(nameof(ActiveCountIntervalSec), new Item { DefaultValue = 1, Predicate = n => n > 0, Serializer = n => n.ToString(), Deserializer = s => int.Parse(s) });
            Dict.Add(nameof(ActiveMeasureSpanMin), new Item { DefaultValue = 10, Predicate = n => n > 0, Serializer = n => n.ToString(), Deserializer = s => int.Parse(s) });

            Dict.Add(nameof(YouTubeLiveBackColor), new Item { DefaultValue = ColorFromArgb("#FFe0efd0"), Predicate = c => true, Serializer = c => ColorToArgb(c), Deserializer = s => ColorFromArgb(s) });
            Dict.Add(nameof(YouTubeLiveForeColor), new Item { DefaultValue = ColorFromArgb("#FF008000"), Predicate = c => true, Serializer = c => ColorToArgb(c), Deserializer = s => ColorFromArgb(s) });
            Dict.Add(nameof(OpenrecBackColor), new Item { DefaultValue = ColorFromArgb("#FFEEE8AA"), Predicate = c => true, Serializer = c => ColorToArgb(c), Deserializer = s => ColorFromArgb(s) });
            Dict.Add(nameof(OpenrecForeColor), new Item { DefaultValue = ColorFromArgb("#FF483D8B"), Predicate = c => true, Serializer = c => ColorToArgb(c), Deserializer = s => ColorFromArgb(s) });
            Dict.Add(nameof(BLiveBackColor), new Item { DefaultValue = ColorFromArgb("#FFEEE8AA"), Predicate = c => true, Serializer = c => ColorToArgb(c), Deserializer = s => ColorFromArgb(s) });
            Dict.Add(nameof(BLiveForeColor), new Item { DefaultValue = ColorFromArgb("#FF483D8B"), Predicate = c => true, Serializer = c => ColorToArgb(c), Deserializer = s => ColorFromArgb(s) });
            Dict.Add(nameof(MixchBackColor), new Item { DefaultValue = ColorFromArgb("#FFEEE8AA"), Predicate = c => true, Serializer = c => ColorToArgb(c), Deserializer = s => ColorFromArgb(s) });
            Dict.Add(nameof(MixchForeColor), new Item { DefaultValue = ColorFromArgb("#FF483D8B"), Predicate = c => true, Serializer = c => ColorToArgb(c), Deserializer = s => ColorFromArgb(s) });
            Dict.Add(nameof(TwitchBackColor), new Item { DefaultValue = ColorFromArgb("#FF7FFFD4"), Predicate = c => true, Serializer = c => ColorToArgb(c), Deserializer = s => ColorFromArgb(s) });
            Dict.Add(nameof(TwitchForeColor), new Item { DefaultValue = ColorFromArgb("#FF0000FF"), Predicate = c => true, Serializer = c => ColorToArgb(c), Deserializer = s => ColorFromArgb(s) });
            Dict.Add(nameof(NicoLiveBackColor), new Item { DefaultValue = ColorFromArgb("#FFA52A2A"), Predicate = c => true, Serializer = c => ColorToArgb(c), Deserializer = s => ColorFromArgb(s) });
            Dict.Add(nameof(NicoLiveForeColor), new Item { DefaultValue = ColorFromArgb("#FF00FF7F"), Predicate = c => true, Serializer = c => ColorToArgb(c), Deserializer = s => ColorFromArgb(s) });
            Dict.Add(nameof(TwicasBackColor), new Item { DefaultValue = ColorFromArgb("#FFAFEEEE"), Predicate = c => true, Serializer = c => ColorToArgb(c), Deserializer = s => ColorFromArgb(s) });
            Dict.Add(nameof(TwicasForeColor), new Item { DefaultValue = ColorFromArgb("#FF20B2AA"), Predicate = c => true, Serializer = c => ColorToArgb(c), Deserializer = s => ColorFromArgb(s) });
            Dict.Add(nameof(LineLiveBackColor), new Item { DefaultValue = ColorFromArgb("#FFFFD700"), Predicate = c => true, Serializer = c => ColorToArgb(c), Deserializer = s => ColorFromArgb(s) });
            Dict.Add(nameof(LineLiveForeColor), new Item { DefaultValue = ColorFromArgb("#FFDC143C"), Predicate = c => true, Serializer = c => ColorToArgb(c), Deserializer = s => ColorFromArgb(s) });
            Dict.Add(nameof(WhowatchBackColor), new Item { DefaultValue = ColorFromArgb("#FF00FFFF"), Predicate = c => true, Serializer = c => ColorToArgb(c), Deserializer = s => ColorFromArgb(s) });
            Dict.Add(nameof(WhowatchForeColor), new Item { DefaultValue = ColorFromArgb("#FF4B0082"), Predicate = c => true, Serializer = c => ColorToArgb(c), Deserializer = s => ColorFromArgb(s) });
            Dict.Add(nameof(MirrativBackColor), new Item { DefaultValue = ColorFromArgb("#FFFA8072"), Predicate = c => true, Serializer = c => ColorToArgb(c), Deserializer = s => ColorFromArgb(s) });
            Dict.Add(nameof(MirrativForeColor), new Item { DefaultValue = ColorFromArgb("#FF008080"), Predicate = c => true, Serializer = c => ColorToArgb(c), Deserializer = s => ColorFromArgb(s) });
            Dict.Add(nameof(PeriscopeBackColor), new Item { DefaultValue = ColorFromArgb("#FFFA8072"), Predicate = c => true, Serializer = c => ColorToArgb(c), Deserializer = s => ColorFromArgb(s) });
            Dict.Add(nameof(PeriscopeForeColor), new Item { DefaultValue = ColorFromArgb("#FF008080"), Predicate = c => true, Serializer = c => ColorToArgb(c), Deserializer = s => ColorFromArgb(s) });
            Dict.Add(nameof(ShowRoomBackColor), new Item { DefaultValue = ColorFromArgb("#FFFA8072"), Predicate = c => true, Serializer = c => ColorToArgb(c), Deserializer = s => ColorFromArgb(s) });
            Dict.Add(nameof(ShowRoomForeColor), new Item { DefaultValue = ColorFromArgb("#FF008080"), Predicate = c => true, Serializer = c => ColorToArgb(c), Deserializer = s => ColorFromArgb(s) });
        }
        public ICommentOptions Clone()
        {
            return this.MemberwiseClone() as ICommentOptions;
        }

        public void Set(ICommentOptions options)
        {
            var props = typeof(ICommentOptions).GetProperties();
            foreach (var prop in props)
            {
                if (prop.CanRead && prop.CanWrite)
                {
                    var item = Dict[prop.Name];
                    var newVal = prop.GetValue(options);
                    if (item.Predicate(newVal))
                    {
                        item.Value = newVal;
                        RaisePropertyChanged(prop.Name);
                    }
                }
            }
        }
        #region Converters
        private FontFamily FontFamilyFromString(string str)
        {
            return new FontFamily(str);
        }
        private string FontFamilyToString(FontFamily family)
        {
            return family.FamilyNames.Values.First();
        }
        private FontStyle FontStyleFromString(string str)
        {
            return (FontStyle)new FontStyleConverter().ConvertFromString(str);
        }
        private string FontStyleToString(FontStyle style)
        {
            return new FontStyleConverter().ConvertToString(style);
        }
        private FontWeight FontWeightFromString(string str)
        {
            return (FontWeight)new FontWeightConverter().ConvertFromString(str);
        }
        private string FontWeightToString(FontWeight weight)
        {
            return new FontWeightConverter().ConvertToString(weight);
        }
        private Color ColorFromArgb(string argb)
        {
            if (argb == null)
                throw new ArgumentNullException("argb");
            var pattern = "#(?<a>[0-9a-fA-F]{2})(?<r>[0-9a-fA-F]{2})(?<g>[0-9a-fA-F]{2})(?<b>[0-9a-fA-F]{2})";
            var match = System.Text.RegularExpressions.Regex.Match(argb, pattern, System.Text.RegularExpressions.RegexOptions.Compiled);

            if (!match.Success)
            {
                throw new ArgumentException("形式が不正");
            }
            else
            {
                var a = byte.Parse(match.Groups["a"].Value, System.Globalization.NumberStyles.HexNumber);
                var r = byte.Parse(match.Groups["r"].Value, System.Globalization.NumberStyles.HexNumber);
                var g = byte.Parse(match.Groups["g"].Value, System.Globalization.NumberStyles.HexNumber);
                var b = byte.Parse(match.Groups["b"].Value, System.Globalization.NumberStyles.HexNumber);
                return Color.FromArgb(a, r, g, b);
            }
        }
        private string ColorToArgb(Color color)
        {
            var argb = color.ToString();
            return argb;
        }
        #endregion
    }
}
