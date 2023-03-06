using NUnit.Framework;
using SitePluginCommon.AutoReconnection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SitePluginCommonTests
{
    internal class AutoReconnectionTests
    {
        [Test]
        public void Test()
        {
            var counter = new ReconnectionCounter();
            Assert.IsTrue(counter.Add(DateTime.Now));
            Assert.IsTrue(counter.Add(DateTime.Now));
            Assert.IsTrue(counter.Add(DateTime.Now));
            Assert.IsTrue(counter.Add(DateTime.Now));
            Assert.IsTrue(counter.Add(DateTime.Now));
            Assert.IsFalse(counter.Add(DateTime.Now));
        }
    }
}
