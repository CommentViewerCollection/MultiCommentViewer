using Common;
using MirrativSitePlugin;
using Moq;
using NUnit.Framework;
using SitePlugin;
using SitePluginCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MirrativSitePluginTests
{
    [TestFixture]
    class CommentProviderTests
    {
        //[Test]
        //public void コメント中にコテハンがあった場合にコテハンがちゃんと付くか()
        //{
        //    var serverMock = new Mock<IDataServer>();
        //    var loggerMock = new Mock<ILogger>();
        //    var optionsMock = new Mock<ICommentOptions>();
        //    var siteOptionsMock = new Mock<IMirrativSiteOptions>();
        //    siteOptionsMock.Setup(s => s.NeedAutoSubNickname).Returns(true);
        //    var userStoreMock = new Mock<IUserStoreManager>();
        //    var user = new UserTest("5867403");
        //    userStoreMock.Setup(u => u.GetUser(SiteType.Mixer, "5867403")).Returns(user);

        //    var server = serverMock.Object;
        //    var logger = loggerMock.Object;
        //    var options = optionsMock.Object;
        //    var siteOptions = siteOptionsMock.Object;
        //    var userStore = userStoreMock.Object;
        //    var cp = new MirrativCommentProvider2(server, logger, options, siteOptions, userStore);

        //    var commentText = "test@nick";
        //    var comment = MessageProvider.ParseMessage("{\"push_image_url\":\"\",\"speech\":\"abc\",\"d\":1,\"ac\":\"Ryu\",\"burl\":\"\",\"iurl\":\"https://cdn.mirrativ.com/mirrorman-prod/image/profile_image/c6d0a7dc58221445c3945e9cda3037de7fa4f089e4caf3f61d91c2bc120ac8b3_m.jpeg?1551989435\",\"cm\":\"" + commentText + "\",\"created_at\":1552156435,\"u\":\"5867403\",\"is_moderator\":0,\"lci\":\"1654341365\",\"t\":1}", (msg, type) => { });
        //    var method = typeof(MirrativCommentProvider2).GetMethod("CreateMessageContext", 
        //        BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Instance,
        //        Type.DefaultBinder,
        //        new[] { typeof(IMirrativMessage) }, null);
        //    var context = method.Invoke(cp, new[] { comment }) as MirrativMessageContext;
        //    Assert.AreEqual("nick", user.Nickname);
        //}
    }
}
