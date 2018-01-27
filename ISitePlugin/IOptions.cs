using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
namespace SitePlugin
{
    public interface IOptions
    {
        FontFamily FontFamily { get; set; }
        FontStyle FontStyle { get; set; }
        FontWeight FontWeight { get; set; }
        int FontSize { get; set; }
        FontFamily FirstCommentFontFamily { get; set; }
        FontStyle FirstCommentFontStyle { get; set; }
        FontWeight FirstCommentFontWeight { get; set; }
        int FirstCommentFontSize { get; set; }
        
        string SettingsDirPath { get; set; }

        Color BackColor { get; }
        Color ForeColor { get; }

        double MainViewHeight { get; set; }
        double MainViewWidth { get; set; }
        double MainViewLeft { get; set; }
        double MainViewTop { get; set; }

        Color HorizontalGridLineColor { get; set; }
        Color VerticalGridLineColor { get; set; }
    }
}
