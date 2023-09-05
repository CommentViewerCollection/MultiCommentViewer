//using TwitchSitePlugin;
//using NUnit.Framework;
//using Moq;
//using SitePlugin;
//using System.Windows;
//using System.Windows.Media;
//namespace TwitchSitePluginTests
//{
//    [TestFixture]
//    public class TwitchCommentViewerTests
//    {
//        [Test]
//        public void IsFirstCommentTest()
//        {
//            var fontFamily = new FontFamily("Meiryo");
//            var fontStyle = FontStyles.Italic;
//            var fontWeight = FontWeights.Black;
//            var fontSize = 14;
//            var firstFontFamily = new FontFamily("Yu Gothic");
//            var firstFontStyle = FontStyles.Oblique;
//            var firstFontWeight = FontWeights.Bold;
//            var firstFontSize = 20;

//            var connectionNameMock = new Mock<ConnectionName>();
//            var optionsMock = new Mock<IOptions>();
//            optionsMock.Setup(m => m.FontFamily).Returns(fontFamily);
//            optionsMock.Setup(m => m.FontStyle).Returns(fontStyle);
//            optionsMock.Setup(m => m.FontWeight).Returns(fontWeight);
//            optionsMock.Setup(m => m.FontSize).Returns(fontSize);
//            optionsMock.Setup(m => m.FirstCommentFontFamily).Returns(firstFontFamily);
//            optionsMock.Setup(m => m.FirstCommentFontStyle).Returns(firstFontStyle);
//            optionsMock.Setup(m => m.FirstCommentFontWeight).Returns(firstFontWeight);
//            optionsMock.Setup(m => m.FirstCommentFontSize).Returns(firstFontSize);

//            var siteOptionsMock = new Mock<TwitchSiteOptions>();
//            var data = new Mock<ICommentData>();
//            data.Setup(m => m.UserId).Returns("");
//            data.Setup(m => m.Username).Returns("");
//            data.Setup(m => m.Message).Returns("");
//            data.Setup(m => m.Emotes).Returns("");


//            var low = new Mock<TwitchSitePlugin.LowObject.Emoticons>();
//            var cvmFirst = new TwitchCommentViewModel(connectionNameMock.Object, optionsMock.Object, siteOptionsMock.Object, data.Object, low.Object, true);
//            Assert.AreEqual(firstFontFamily, cvmFirst.FontFamily);
//            Assert.AreEqual(firstFontStyle, cvmFirst.FontStyle);
//            Assert.AreEqual(firstFontWeight, cvmFirst.FontWeight);
//            Assert.AreEqual(firstFontSize, cvmFirst.FontSize);

//            var cvm = new TwitchCommentViewModel(connectionNameMock.Object, optionsMock.Object, siteOptionsMock.Object, data.Object, low.Object, false);
//            Assert.AreEqual(fontFamily, cvm.FontFamily);
//            Assert.AreEqual(fontStyle, cvm.FontStyle);
//            Assert.AreEqual(fontWeight, cvm.FontWeight);
//            Assert.AreEqual(fontSize, cvm.FontSize);
//        }
//    }
//}
