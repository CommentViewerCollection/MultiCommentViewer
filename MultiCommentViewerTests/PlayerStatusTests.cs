using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MultiCommentViewer.Test.NicoLive;
using NUnit.Framework;
using NicoSitePlugin;
namespace MultiCommentViewerTests
{
    [TestFixture]
    class PlayerStatusTests
    {
        [Test]
        public void Test()
        {
            string xml;
            using (var sr = new System.IO.StreamReader(@"C:\Users\Shinmura\Desktop\getplayerstatus_lv310492598.xml"))
            {
                xml = sr.ReadToEnd();
            }
            var ps = Tools.Parse(xml);

            Assert.AreEqual("【初見歓迎】生着替えからの腰痛改善筋トレ:;(∩︎´﹏`∩︎);:", ps.Title);
            Assert.AreEqual(1516718536, ps.BaseTime);
            Assert.AreEqual(1516718536, ps.OpenTime);
            Assert.AreEqual(1516718650, ps.StartTime);
            Assert.AreEqual(1516725556, ps.EndTime);
            Assert.AreEqual("co2810321", ps.RoomLabel);
            Assert.AreEqual(ProviderType.Community, ps.ProviderType);
            Assert.AreEqual("msg104.live.nicovideo.jp", ps.Ms.Addr);
            Assert.AreEqual("1620366276", ps.Ms.Thread);
            Assert.AreEqual(2844, ps.Ms.Port);
        }
    }
}
