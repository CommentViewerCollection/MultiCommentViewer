using NUnit.Framework;
using Moq;
using Mcv.PluginV2.AutoReconnection;
using Mcv.PluginV2;

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
