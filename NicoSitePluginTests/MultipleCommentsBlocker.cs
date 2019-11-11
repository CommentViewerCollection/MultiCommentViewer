using NicoSitePlugin;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NicoSitePluginTests
{
    [TestFixture]
    class MultipleCommentsBlockerTests
    {
        [Test]
        public void Test1()
        {
            var b = new MultipleCommentsBlocker();
            Assert.IsTrue(b.IsUniqueComment("a", "aaa", new DateTime(2019, 11, 6, 2, 0, 0)));
            Assert.IsTrue(b.IsUniqueComment("a", "bbb", new DateTime(2019, 11, 6, 2, 0, 0)));
            Assert.IsFalse(b.IsUniqueComment("a", "bbb", new DateTime(2019, 11, 6, 2, 0, 1)));
        }
    }
}
