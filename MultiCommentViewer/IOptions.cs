using Common;
using SitePlugin;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace MultiCommentViewer
{
    public interface IOptions : ICommentOptions, INotifyPropertyChanged
    {
        string PluginDir { get; }

        double MainViewHeight { get; set; }
        double MainViewWidth { get; set; }
        double MainViewLeft { get; set; }
        double MainViewTop { get; set; }

        double ConnectionViewHeight { get; set; }
        double MetadataViewHeight { get; set; }

        bool IsShowMetaConnectionName { get; set; }
        int MetadataViewConnectionNameDisplayIndex { get; set; }

        bool IsShowMetaTitle { get; set; }
        int MetadataViewTitleDisplayIndex { get; set; }

        bool IsShowMetaElapse { get; set; }
        int MetadataViewElapsedDisplayIndex { get; set; }

        bool IsShowMetaCurrentViewers { get; set; }
        int MetadataViewCurrentViewersDisplayIndex { get; set; }

        bool IsShowMetaTotalViewers { get; set; }
        int MetadataViewTotalViewersDisplayIndex { get; set; }

        bool IsShowMetaActive { get; set; }
        int MetadataViewActiveDisplayIndex { get; set; }

        bool IsShowMetaOthers { get; set; }
        int MetadataViewOthersDisplayIndex { get; set; }

        double ConnectionNameWidth { get; set; }
        bool IsShowConnectionName { get; set; }
        int ConnectionNameDisplayIndex { get; set; }

        double ThumbnailWidth { get; set; }
        int ThumbnailDisplayIndex { get; set; }
        bool IsShowThumbnail { get; set; }

        double CommentIdWidth { get; set; }
        int CommentIdDisplayIndex { get; set; }
        bool IsShowCommentId { get; set; }

        double UsernameWidth { get; set; }
        bool IsShowUsername { get; set; }
        int UsernameDisplayIndex { get; set; }

        double MessageWidth { get; set; }
        bool IsShowMessage { get; set; }
        int MessageDisplayIndex { get; set; }

        double PostTimeWidth { get; set; }
        bool IsShowPostTime { get; set; }
        int PostTimeDisplayIndex { get; set; }

        double InfoWidth { get; set; }
        bool IsShowInfo { get; set; }
        int InfoDisplayIndex { get; set; }
        bool IsShowVerticalGridLine { get; set; }
        bool IsShowHorizontalGridLine { get; set; }

        double MetadataViewConnectionNameColumnWidth { get; set; }
        double MetadataViewTitleColumnWidth { get; set; }
        double MetadataViewElapsedColumnWidth { get; set; }
        double MetadataViewCurrentViewersColumnWidth { get; set; }
        double MetadataViewTotalViewersColumnWidth { get; set; }
        double MetadataViewActiveColumnWidth { get; set; }
        double MetadataViewOthersColumnWidth { get; set; }

        Color TitleForeColor { get; set; }
        Color TitleBackColor { get; set; }
        Color ViewBackColor { get; set; }
        Color WindowBorderColor { get; set; }
        Color SystemButtonForeColor { get; set; }
        Color SystemButtonBackColor { get; set; }
        Color SystemButtonBorderColor { get; set; }
        Color SystemButtonMouseOverBackColor { get; set; }
        Color SystemButtonMouseOverForeColor { get; set; }
        Color SystemButtonMouseOverBorderColor { get; set; }
        Color MenuBackColor { get; set; }
        Color MenuForeColor { get; set; }
        Color MenuItemCheckMarkColor { get; set; }
        Color MenuItemMouseOverBackColor { get; set; }
        Color MenuItemMouseOverForeColor { get; set; }
        Color MenuItemMouseOverBorderColor { get; set; }
        Color MenuItemMouseOverCheckMarkColor { get; set; }
        Color MenuSeparatorBackColor { get; set; }
        Color MenuPopupBorderColor { get; set; }
        Color ButtonBackColor { get; set; }
        Color ButtonForeColor { get; set; }
        Color ButtonBorderColor { get; set; }
        Color CommentListBackColor { get; set; }
        Color CommentListHeaderBackColor { get; set; }
        Color CommentListHeaderForeColor { get; set; }
        Color CommentListHeaderBorderColor { get; set; }
        Color CommentListBorderColor { get; set; }
        Color CommentListSeparatorColor { get; set; }
        Color ConnectionListBackColor { get; set; }
        Color ConnectionListHeaderBackColor { get; set; }
        Color ConnectionListHeaderForeColor { get; set; }
        Color ConnectionListRowBackColor { get; set; }

        Color ScrollBarBackColor { get; set; }
        Color ScrollBarBorderColor { get; set; }
        Color ScrollBarThumbBackColor { get; set; }
        Color ScrollBarThumbMouseOverBackColor { get; set; }
        Color ScrollBarThumbPressedBackColor { get; set; }
        Color ScrollBarButtonBackColor { get; set; }
        Color ScrollBarButtonForeColor { get; set; }
        Color ScrollBarButtonBorderColor { get; set; }
        Color ScrollBarButtonDisabledBackColor { get; set; }
        Color ScrollBarButtonDisabledForeColor { get; set; }
        Color ScrollBarButtonDisabledBorderColor { get; set; }
        Color ScrollBarButtonMouseOverBackColor { get; set; }
        Color ScrollBarButtonMouseOverForeColor { get; set; }
        Color ScrollBarButtonMouseOverBorderColor { get; set; }
        Color ScrollBarButtonPressedBackColor { get; set; }
        Color ScrollBarButtonPressedForeColor { get; set; }
        Color ScrollBarButtonPressedBorderColor { get; set; }


        bool IsAutoCheckIfUpdateExists { get; set; }
        bool IsAddingNewCommentTop { get; set; }

        bool IsTopmost { get; set; }
        bool IsPixelScrolling { get; set; }
        bool IsEnabledSiteConnectionColor { get; set; }
        SiteConnectionColorType SiteConnectionColorType { get; set; }
        Color YouTubeLiveBackColor { get; set; }
        Color YouTubeLiveForeColor { get; set; }
        Color OpenrecBackColor { get; set; }
        Color OpenrecForeColor { get; set; }
        Color BLiveBackColor { get; set; }
        Color BLiveForeColor { get; set; }
        Color MixchBackColor { get; set; }
        Color MixchForeColor { get; set; }
        Color TwitchBackColor { get; set; }
        Color TwitchForeColor { get; set; }
        Color NicoLiveBackColor { get; set; }
        Color NicoLiveForeColor { get; set; }
        Color TwicasBackColor { get; set; }
        Color TwicasForeColor { get; set; }
        Color LineLiveBackColor { get; set; }
        Color LineLiveForeColor { get; set; }
        Color WhowatchBackColor { get; set; }
        Color WhowatchForeColor { get; set; }
        Color MirrativBackColor { get; set; }
        Color MirrativForeColor { get; set; }
        Color PeriscopeBackColor { get; set; }
        Color PeriscopeForeColor { get; set; }
        Color ShowRoomBackColor { get; set; }
        Color ShowRoomForeColor { get; set; }

        InfoType ShowingInfoLevel { get; set; }

        int ConnectionsViewSelectionDisplayIndex { get; set; }
        double ConnectionsViewSelectionWidth { get; set; }
        bool IsShowConnectionsViewSelection { get; set; }
        int ConnectionsViewSiteDisplayIndex { get; set; }
        double ConnectionsViewSiteWidth { get; set; }
        bool IsShowConnectionsViewSite { get; set; }
        int ConnectionsViewConnectionNameDisplayIndex { get; set; }
        double ConnectionsViewConnectionNameWidth { get; set; }
        bool IsShowConnectionsViewConnectionName { get; set; }
        int ConnectionsViewInputDisplayIndex { get; set; }
        double ConnectionsViewInputWidth { get; set; }
        bool IsShowConnectionsViewInput { get; set; }
        int ConnectionsViewBrowserDisplayIndex { get; set; }
        double ConnectionsViewBrowserWidth { get; set; }
        bool IsShowConnectionsViewBrowser { get; set; }
        int ConnectionsViewConnectionDisplayIndex { get; set; }
        double ConnectionsViewConnectionWidth { get; set; }
        bool IsShowConnectionsViewConnection { get; set; }
        int ConnectionsViewDisconnectionDisplayIndex { get; set; }
        double ConnectionsViewDisconnectionWidth { get; set; }
        bool IsShowConnectionsViewDisconnection { get; set; }
        int ConnectionsViewSaveDisplayIndex { get; set; }
        double ConnectionsViewSaveWidth { get; set; }
        bool IsShowConnectionsViewSave { get; set; }
        int ConnectionsViewLoggedinUsernameDisplayIndex { get; set; }
        double ConnectionsViewLoggedinUsernameWidth { get; set; }
        bool IsShowConnectionsViewLoggedinUsername { get; set; }
        int ConnectionsViewConnectionBackgroundDisplayIndex { get; set; }
        double ConnectionsViewConnectionBackgroundWidth { get; set; }
        int ConnectionsViewConnectionForegroundDisplayIndex { get; set; }
        double ConnectionsViewConnectionForegroundWidth { get; set; }
    }
    /// <summary>
    ///
    /// </summary>
    public enum SiteConnectionColorType
    {
        /// <summary>
        /// 配信サイト毎
        /// </summary>
        Site,
        /// <summary>
        /// 接続毎
        /// </summary>
        Connection,
    }
    public static class SiteConnectionColorTypeRelatedOperations
    {
        /// <summary>
        /// 文字列をInfoTypeに変換する。
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        /// <remarks>InfoTypeをEnumではなくclassにしてこのメソッドもそこに含めたほうが良いかも</remarks>
        public static SiteConnectionColorType ToSiteConnectionColorType(string s)
        {
            if (!Enum.TryParse(s, out SiteConnectionColorType type))
            {
                type = SiteConnectionColorType.Site;
            }
            return type;
        }
    }
    public class SiteConnectionColorTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var ParameterString = parameter as string;
            if (ParameterString == null)
            {
                return System.Windows.DependencyProperty.UnsetValue;
            }

            if (Enum.IsDefined(value.GetType(), value) == false)
            {
                return System.Windows.DependencyProperty.UnsetValue;
            }

            object paramvalue = Enum.Parse(value.GetType(), ParameterString);

            return (int)paramvalue == (int)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isChecked && isChecked && parameter is string paramStr)
                return Enum.Parse(targetType, paramStr);

            return System.Windows.DependencyProperty.UnsetValue;
        }
    }
}
