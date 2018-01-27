using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using System.Windows.Controls;
using MultiCommentViewer.Test;
using System.Windows.Media;
using System.Windows.Threading;
using System.Windows;
using NUnit;
using System.Threading;
using NUnit.Compatibility;
namespace MultiCommentViewerTests
{
    [TestFixture]
    public class OptionsTestTests
    {
        [Test]
        public void BackColorを変更すると変更通知が来る()
        {
            var b1 = false;
            var b2 = false;
            var options = new OptionsTest()
            {
                BackColor = Colors.Blue,
            };
            Assert.AreEqual(Colors.Blue, options.BackColor);
            Assert.AreEqual("#FF0000FF", options.BackColorArgb);

            options.PropertyChanged += (s, e) =>
            {
                switch (e.PropertyName)
                {
                    case nameof(options.BackColor):
                        b1 = true;
                        break;
                    case nameof(options.BackColorArgb):
                        b2 = true;
                        break;
                }
            };

            options.BackColor = Colors.Brown;
            Assert.IsTrue(b1);
            Assert.IsTrue(b2);
        }
        [Test]
        public void ForeColorを変更すると変更通知が来る()
        {
            var b1 = false;
            var b2 = false;
            var options = new OptionsTest()
            {
                ForeColor = Colors.Blue,
            };
            Assert.AreEqual(Colors.Blue, options.ForeColor);
            Assert.AreEqual("#FF0000FF", options.ForeColorArgb);

            options.PropertyChanged += (s, e) =>
            {
                switch (e.PropertyName)
                {
                    case nameof(options.ForeColor):
                        b1 = true;
                        break;
                    case nameof(options.ForeColorArgb):
                        b2 = true;
                        break;
                }
            };

            options.ForeColor = Colors.Brown;
            Assert.IsTrue(b1);
            Assert.IsTrue(b2);
        }
        [Test]
        public void BackColorArgbを変更すると変更通知が来る()
        {
            var b1 = false;
            var b2 = false;
            var options = new OptionsTest()
            {
                BackColorArgb = "#FF0000FF",
            };
            Assert.AreEqual(Colors.Blue, options.BackColor);
            Assert.AreEqual("#FF0000FF", options.BackColorArgb);

            options.PropertyChanged += (s, e) =>
            {
                switch (e.PropertyName)
                {
                    case nameof(options.BackColor):
                        b1 = true;
                        break;
                    case nameof(options.BackColorArgb):
                        b2 = true;
                        break;
                }
            };

            options.BackColorArgb = "#FFFFFFFF";
            Assert.IsTrue(b1);
            Assert.IsTrue(b2);
        }
        [Test]
        public void ForeColorArgbを変更すると変更通知が来る()
        {
            var b1 = false;
            var b2 = false;
            var options = new OptionsTest()
            {
                ForeColorArgb = "#FF0000FF",
            };
            Assert.AreEqual(Colors.Blue, options.ForeColor);
            Assert.AreEqual("#FF0000FF", options.ForeColorArgb);

            options.PropertyChanged += (s, e) =>
            {
                switch (e.PropertyName)
                {
                    case nameof(options.ForeColor):
                        b1 = true;
                        break;
                    case nameof(options.ForeColorArgb):
                        b2 = true;
                        break;
                }
            };

            options.ForeColorArgb = "#FFFFFFFF";
            Assert.IsTrue(b1);
            Assert.IsTrue(b2);
        }
    }
    [TestFixture]    
    public class Class1
    {
        [Test, Description("変更が反映されるか")]
        [Apartment(ApartmentState.STA)]
        public void ApplyTest()
        {
            //

            //var fontFamily = new FontFamily("メイリオ");
            //var fontWeight = FontWeights.Heavy;
            //var fontStyle = FontStyles.Italic;
            //var fontSize = 40;
            var siteOptions = new TestSiteOptions()
            {
                //FontFamily = fontFamily,
                //FontWeight =fontWeight,
                //FontStyle=fontStyle,
                //FontSize=fontSize,
                IsCheckBox = false,
                TextBoxText = "hey",
            };
            var panel = new TestSiteOptionsPagePanel();
            var vm = new TestSiteOptionsViewModel(siteOptions);
            panel.SetViewModel(vm);
            var tabPage = new TestOptionsTabPage("テスト", panel);
            vm.IsCheckBox = true;
            vm.TextBoxText = "take";
            tabPage.Apply();
            Assert.AreEqual(true, siteOptions.IsCheckBox);
            Assert.AreEqual("take", siteOptions.TextBoxText);

        }
        [Test, Description("変更が破棄されるか")]
        [Apartment(ApartmentState.STA)]
        public void CancelTest()
        {
            //var fontFamily = new FontFamily("メイリオ");
            //var fontWeight = FontWeights.Heavy;
            //var fontStyle = FontStyles.Italic;
            //var fontSize = 40;
            var siteOptions = new TestSiteOptions()
            {
                //FontFamily = fontFamily,
                //FontWeight =fontWeight,
                //FontStyle=fontStyle,
                //FontSize=fontSize,
                IsCheckBox = false,
                TextBoxText = "hey",
            };
            var panel = new TestSiteOptionsPagePanel();
            var vm = new TestSiteOptionsViewModel(siteOptions);
            panel.SetViewModel(vm);
            var tabPage = new TestOptionsTabPage("テスト", panel);
            vm.IsCheckBox = true;
            vm.TextBoxText = "take";
            tabPage.Cancel();
            Assert.AreEqual(false, siteOptions.IsCheckBox);
            Assert.AreEqual("hey", siteOptions.TextBoxText);

        }
    }
}
