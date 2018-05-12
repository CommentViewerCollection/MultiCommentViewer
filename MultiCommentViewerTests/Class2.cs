using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using SitePlugin;
using Moq;
namespace MultiCommentViewerTests
{
    /// <summary>
    /// 各サイトプラグインのCommentViewModelが要件を満たしているか
    /// </summary>
    [TestFixtureSource(typeof(Class3), nameof(Class3.FixtureArgs))]
    class Class2
    {
        private readonly ICommentViewModel _cvm;
        public Class2(ICommentViewModel cvm)
        {
            _cvm = cvm;
        }
        [Test]
        public void CommentProviderNotNull()
        {
            //Assert.IsNotNull(_cvm.CommentProvider);
        }
        [Test]
        public void Test()
        {
            var b = false;
            _cvm.PropertyChanged += (s, e) =>
            {
                if(e.PropertyName == nameof(_cvm.ConnectionName))
                {
                    b = true;
                }
            };

            //
            //ConnectionName = "a";
            //Assert.IsTrue(b);
        }
    }
    class Class3
    {
        private static NicoSitePlugin.Old.NicoCommentViewModel2 CreateNicoCVM()
        {
            var connectionNameMock = new Mock<ConnectionName>();
            var optionsMock = new Mock<IOptions>();
            var commentProvider = new Mock<ICommentProvider>();
            var chat = new NicoSitePlugin.Old.chat("<chat thread=\"1621586179\" no=\"616\" vpos=\"154535\" date=\"1518747575\" date_usec=\"218368\" mail=\"184\" user_id=\"0mB1dBAnUjDsXYTFpDM2Jgm2wys\" premium=\"1\" anonymity=\"1\">なにこれきも</chat>");
            var userMock = new Mock<IUser>();
            var roomInfo = new NicoSitePlugin.Old.RoomInfo(new NicoSitePlugin.Old.MsTest("", 0, ""), "");
            return new NicoSitePlugin.Old.NicoCommentViewModel2(connectionNameMock.Object, optionsMock.Object, null, chat,roomInfo, userMock.Object, commentProvider.Object, false);
        }
        public static ICommentViewModel[] FixtureArgs =
        {
            CreateNicoCVM(),
        };
    }
}
