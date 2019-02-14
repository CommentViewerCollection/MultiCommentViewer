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
    class StreamCheckerParseTests
    {
        [Test]
        public void Test()
        {
            var data = "507331047	0	17289	20000	4	82139	2008	%E6%9C%89%E5%90%8D%E3%82%AD%E3%83%A3%E3%82%B9%E6%AD%8C%E3%81%84%E6%89%8B%E9%9B%86%E5%9B%A3%E3%81%AE%E8%A3%8F		#korekore19	1	212229	0	0			%20212229%09https%3A%2F%2Fs01.twitcasting.tv%2Fimg%2Fitem_tea_10.png%09%09https%3A%2F%2Fimagegw02.twitcasting.tv%2Fimage3s%2Fpbs.twimg.com%2Fprofile_images%2F1051833323176124417%2FVM_TEFEt_normal.jpg%09R29tZXpfMTExNw%3D%3D%09tea.baku%09like%0930%09Zmxvd2l0ZW0oImh0dHBzOi8vdHdpdGNhc3RpbmcudHYvaW1nL2FuaW0vYW5pbV90ZWFfMTAiLCAzMDAwLCAxLCAyLCA1KQ%3D%3D%091%09gomez_1117%091049689726981046272%0944K044Oq44G9QM6y4pmh%0944GK6Iy254iGMTA%3D%091%09%0A		0	0";
            var sc = TwicasSitePlugin.LowObject.StreamChecker2.ParseStreamChcker(data);
            Assert.AreEqual("有名キャス歌い手集団の裏", sc.Telop);
            return;
        }
    }
}
