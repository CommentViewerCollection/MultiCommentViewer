using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NicoSitePlugin;
using NUnit.Framework;

namespace NicoSitePluginTests
{
    [TestFixture]
    class RoomTests
    {
        [Test]
        public void EqualsTest()
        {
            var a = new Room { XmlSocketAddr = "addr1", XmlSocketPort = 1, ThreadId = "thread1" };
            var b = new Room { XmlSocketAddr = "addr1", XmlSocketPort = 1, ThreadId = "thread1" };
            var c = new Room { XmlSocketAddr = "addr2", XmlSocketPort = 2, ThreadId = "thread2" };
            Assert.AreEqual(a, b);
            Assert.AreNotEqual(a, c);
        }
    }
}
