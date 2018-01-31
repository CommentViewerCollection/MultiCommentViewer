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
        Color InfoForeColor { get; set; }
        Color InfoBackColor { get; set; }
                
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

        double InfoWidth { get; set; }
        bool IsShowInfo { get; set; }
        int InfoDisplayIndex { get; set; }
    }
}
