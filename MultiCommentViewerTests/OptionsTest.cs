using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MultiCommentViewer;
using MultiCommentViewer.Test;
using SitePlugin;
using NUnit.Framework;
namespace MultiCommentViewerTests
{
    /// <summary>
    /// IOptionsに対するテスト
    /// </summary>
    [TestFixture]
    class OptionsTests
    {
        [SetUp]
        public void SetUp()
        {
            _options = new DynamicOptionsTest();
        }
        private IOptions _options;
        [Test]
        public void MainViewWidthAccessorTest()
        {
            _options.MainViewWidth = 256;
            Assert.AreEqual(256, _options.MainViewWidth);
        }
    }

}
