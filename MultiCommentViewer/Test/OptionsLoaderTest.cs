using System.Windows;
using System.Windows.Media;
using SitePlugin;

namespace MultiCommentViewer.Test
{
    public class OptionsLoaderTest:IOptionsLoader
    {
        public IOptions LoadOptions()
        {
            var options = new OptionsTest()
            {
                FontFamily = new FontFamily("メイリオ"),
                FontStyle = FontStyles.Normal,
                FontWeight = FontWeights.Normal,
                FontSize = 14,
                BackColorArgb = "#FFEFEFEF",
                ForeColorArgb = "#FF00FFFF",
            };
            return options;
        }
    }

}
