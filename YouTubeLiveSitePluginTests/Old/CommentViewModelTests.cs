using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using NUnit.Framework;
using SitePlugin;
using YouTubeLiveSitePlugin.Old;
namespace YouTubeLiveSitePluginTests.Old
{
    [TestFixture]
    public class CommentViewModelTests
    {
        class TestOptions : IOptions
        {
            public FontFamily FontFamily { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
            public FontStyle FontStyle { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
            public FontWeight FontWeight { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
            public int FontSize { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
            public FontFamily FirstCommentFontFamily { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
            public FontStyle FirstCommentFontStyle { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
            public FontWeight FirstCommentFontWeight { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
            public int FirstCommentFontSize { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
            public string SettingsDirPath { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

            public Color BackColor => throw new NotImplementedException();

            public Color ForeColor => throw new NotImplementedException();

            public double MainViewHeight { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
            public double MainViewWidth { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
            public double MainViewLeft { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
            public double MainViewTop { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
            public Color HorizontalGridLineColor { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
            public Color VerticalGridLineColor { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        }
        
        [Test]
        public void 接続名を変更すると連動して通知する()
        {
            var connectionName = new ConnectionName() { Name = "a" };
            var cvm = new YouTubeCommentViewModel(connectionName, new TestOptions(), new YouTubeSiteOptions());
            var b = false;
            cvm.PropertyChanged += (s, e) =>
            {
                switch (e.PropertyName)
                {
                    case nameof(cvm.ConnectionName):
                        b = true;
                        break;
                }                
            };
            connectionName.Name = "b";
            Assert.IsTrue(b);
        }
    }
}
