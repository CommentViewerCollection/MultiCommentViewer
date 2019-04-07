using NUnit.Framework;
using ryu_s.BrowserCookie.Firefox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrowserCookieImplementationsTests
{
    [TestFixture]
    public class FirefoxProfileTests
    {
        [Test]
        public void GetProfilesTest()
        {
            var data = DataLoader.GetSampleData("profiles.ini.txt");
            var lines = data.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            var moz_path = "path";
            var ps = FirefoxProfile.GetProfiles(lines, moz_path);
            Assert.AreEqual(2, ps.Count);
            Assert.AreEqual("default", ps[0].Name);
            Assert.IsTrue(ps[0].IsRelative);
            Assert.IsFalse(ps[0].IsDefault);
            Assert.AreEqual("path\\Profiles\\f3ezfk6m.default", ps[0].path);
            Assert.AreEqual("dev-edition-default", ps[1].Name);
            Assert.IsTrue(ps[1].IsRelative);
            Assert.IsTrue(ps[1].IsDefault);
            Assert.AreEqual("path\\Profiles\\42522y5w.dev-edition-default-1516971409783", ps[1].path);
        }
        [Test]
        public void SplitByEqualTest()
        {
            var a = FirefoxProfile.SplitByEqual("a=b=c");
            Assert.AreEqual("a", a.Key);
            Assert.AreEqual("b=c", a.Value);
        }
    }
}
