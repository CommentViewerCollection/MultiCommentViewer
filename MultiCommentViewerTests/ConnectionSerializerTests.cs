using MultiCommentViewer;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace MultiCommentViewerTests
{
    [TestFixture]
    class ConnectionSerializerTests
    {
        [Test]
        public void SerializeDeserializeTest()
        {
            var name = "name";
            var url = "url";
            var siteName = "siteName";
            var browser = "browser";
            var ser = new ConnectionSerializer(name, siteName, url, browser, "#FF0000FF", "#FFFF0000");
            var serialized = ser.Serialize();
            var next = ConnectionSerializer.Deserialize(serialized);
            Assert.AreEqual(name, next.Name);
            Assert.AreEqual(url, next.Url);
            Assert.AreEqual(siteName, next.SiteName);
            Assert.AreEqual(browser, next.BrowserName);
            Assert.AreEqual("#FF0000FF", next.BackColorArgb);
            Assert.AreEqual("#FFFF0000", next.ForeColorArgb);
        }
    }
}
