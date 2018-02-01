using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using NicoSitePlugin.Test;
namespace NicoSitePluginTests
{
    [TestFixture]
    class RoomInfoTests
    {
        [Test]
        public void RoomInfoEqualsTest()
        {
            var info1 = new RoomInfo(new MsTest("a", "b", 123), "c");
            var info2 = new RoomInfo(new MsTest("a", "b", 123), "c");
            var info3 = new RoomInfo(new MsTest("k", "b", 123), "c");
            Assert.IsTrue(info1 == info2);
            Assert.IsTrue(info1.Equals(info2));
            Assert.IsTrue(info1 != info3);
            Assert.IsTrue(!info1.Equals(info3));

        }
    }
}
