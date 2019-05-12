using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Common;
using LineLiveSitePlugin;
using Moq;
using NUnit.Framework;
using ryu_s.BrowserCookie;
using SitePlugin;
using SitePluginCommon;

namespace LineLiveSitePluginTests
{
    //LineLiveCommentProviderのBlackListProviderに対する反応をテストしたい

    class TestBlackListProvider : IBlackListProvider
    {
        public event EventHandler<long[]> Received;

        public void Disconnect()
        {
            throw new NotImplementedException();
        }

        public Task ReceiveAsync(List<Cookie> cookies)
        {
            throw new NotImplementedException();
        }
    }
    class Class3 : LineLiveCommentProvider
    {
        public Class3(IDataServer server, ILogger logger, ICommentOptions options, LineLiveSiteOptions siteOptions, IUserStoreManager userStoreManager)
            : base(server, logger, options, siteOptions, userStoreManager)
        {

        }
        public void SetNgUserPub(long[] old, long[] @new)
        {
            base.SetNgUser(old, @new);
        }
    }
    [TestFixture]
    class LineLiveCommentProvider_BlackListTest
    {
        IDataServer server;
        ILogger logger;
        ICommentOptions options;
        LineLiveSiteOptions siteOptions;
        IUser use1;
        IUser use2;
        IUser use3;
        IUser use4;
        IUser use5;
        IUser use6;
        IUserStoreManager userStoreManager;

        [SetUp]
        public void SetUp()
        {
            var serverMock = new Mock<IDataServer>();
            server = serverMock.Object;
            logger = new Mock<ILogger>().Object;
            var optionsMock = new Mock<ICommentOptions>();
            options = optionsMock.Object;
            siteOptions = new LineLiveSiteOptions();

            use1 = new UserTest("1");
            use2 = new UserTest("2");
            use3 = new UserTest("3");
            use4 = new UserTest("4");
            use5 = new UserTest("5");
            use6 = new UserTest("6");
            var userStoreMock = new Mock<IUserStoreManager>();
            userStoreMock.Setup(s => s.GetUser(SiteType.LineLive, "1")).Returns(use1);
            userStoreMock.Setup(s => s.GetUser(SiteType.LineLive, "2")).Returns(use2);
            userStoreMock.Setup(s => s.GetUser(SiteType.LineLive, "3")).Returns(use3);
            userStoreMock.Setup(s => s.GetUser(SiteType.LineLive, "4")).Returns(use4);
            userStoreMock.Setup(s => s.GetUser(SiteType.LineLive, "5")).Returns(use5);
            userStoreMock.Setup(s => s.GetUser(SiteType.LineLive, "6")).Returns(use6);

            userStoreManager = userStoreMock.Object;
        }
        [Test]
        public void Test()
        {
            use1.IsNgUser = true;
            use2.IsNgUser = true;
            use3.IsNgUser = true;
            use4.IsNgUser = true;
            use5.IsNgUser = false;
            use6.IsNgUser = false;

            var cp = new Class3(server, logger, options, siteOptions, userStoreManager);
            cp.SetNgUserPub(new long[] { 1, 2, 3, 4 }, new long[] { 3, 4, 5, 6 });
            Assert.IsFalse(use1.IsNgUser);
            Assert.IsFalse(use2.IsNgUser);
            Assert.IsTrue(use3.IsNgUser);
            Assert.IsTrue(use4.IsNgUser);
            Assert.IsTrue(use5.IsNgUser);
            Assert.IsTrue(use6.IsNgUser);
        }
        [Test]
        public void Test1()
        {
            use1.IsNgUser = false;
            use2.IsNgUser = false;
            use3.IsNgUser = false;
            use4.IsNgUser = false;
            var cp = new Class3(server, logger, options, siteOptions, userStoreManager);
            cp.SetNgUserPub(null, new long[] { 1, 2, 3, 4 });
            Assert.IsTrue(use1.IsNgUser);
            Assert.IsTrue(use2.IsNgUser);
            Assert.IsTrue(use3.IsNgUser);
            Assert.IsTrue(use4.IsNgUser);
        }
        [Test]
        public void Test2()
        {
            use1.IsNgUser = true;
            use2.IsNgUser = true;
            use3.IsNgUser = true;
            use4.IsNgUser = true;
            var cp = new Class3(server, logger, options, siteOptions, userStoreManager);
            cp.SetNgUserPub(new long[] { 1, 2, 3, 4 }, null);
            Assert.IsFalse(use1.IsNgUser);
            Assert.IsFalse(use2.IsNgUser);
            Assert.IsFalse(use3.IsNgUser);
            Assert.IsFalse(use4.IsNgUser);
        }
        [Test]
        public void Test3()
        {
            use1.IsNgUser = true;
            use2.IsNgUser = true;
            use3.IsNgUser = true;
            use4.IsNgUser = true;
            use5.IsNgUser = true;
            use6.IsNgUser = true;
            var cp = new Class3(server, logger, options, siteOptions, userStoreManager);
            cp.SetNgUserPub(null, null);
            Assert.IsTrue(use1.IsNgUser);
            Assert.IsTrue(use2.IsNgUser);
            Assert.IsTrue(use3.IsNgUser);
            Assert.IsTrue(use4.IsNgUser);
            Assert.IsTrue(use5.IsNgUser);
            Assert.IsTrue(use6.IsNgUser);
        }
    }
}
