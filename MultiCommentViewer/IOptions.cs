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

        InfoType ShowingInfoLevel { get; set; }
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

