using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwicasSitePlugin;

namespace TwicasSitePluginTests
{
    [TestFixture]
    class ToolsTests
    {
        [Test]
        public void Test()
        {
            Assert.AreEqual("flowitem(\"https://twitcasting.tv/img/anim/anim_tea_10\", 3000, 1, 1, 5)", Tools.DecodeBase64("Zmxvd2l0ZW0oImh0dHBzOi8vdHdpdGNhc3RpbmcudHYvaW1nL2FuaW0vYW5pbV90ZWFfMTAiLCAzMDAwLCAxLCAxLCA1KQ=="));
        }
    }
}
