using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.ComponentModel;
namespace SitePlugin
{
    public interface ICommentOptions : INotifyPropertyChanged
    {
        FontFamily FontFamily { get; set; }
        FontStyle FontStyle { get; set; }
        FontWeight FontWeight { get; set; }
        int FontSize { get; set; }
        FontFamily FirstCommentFontFamily { get; set; }
        FontStyle FirstCommentFontStyle { get; set; }
        FontWeight FirstCommentFontWeight { get; set; }
        int FirstCommentFontSize { get; set; }
        Color FirstCommentBackColor { get; set; }
        Color FirstCommentForeColor { get; set; }

        Color BackColor { get; set; }
        Color ForeColor { get; set; }

        Color HorizontalGridLineColor { get; set; }
        Color VerticalGridLineColor { get; set; }
        Color InfoForeColor { get; set; }
        Color InfoBackColor { get; set; }
        Color BroadcastInfoBackColor { get; set; }
        Color BroadcastInfoForeColor { get; set; }
        Color SelectedRowBackColor { get; set; }
        Color SelectedRowForeColor { get; set; }

        bool IsUserNameWrapping { get; set; }
        string SettingsDirPath { get; set; }

        bool IsActiveCountEnabled { get; set; }
        int ActiveCountIntervalSec { get; set; }
        int ActiveMeasureSpanMin { get; set; }

        ICommentOptions Clone();
        void Set(ICommentOptions options);
        void Reset();
    }

}
