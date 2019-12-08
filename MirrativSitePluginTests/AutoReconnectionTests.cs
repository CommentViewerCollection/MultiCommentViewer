using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using Common;
using MirrativSitePlugin;
using SitePluginCommon.AutoReconnection;

namespace MirrativSitePluginTests
{
    /// <summary>
    /// 自動再接続機能をテストするよ
    /// </summary>
    [TestFixture]
    class AutoReconnectionTests
    {
        [Test]
        public void Test()
        {
            var logger = new Mock<ILogger>().Object;
            var m = new ConnectionManager(logger);
        }
    }
}
