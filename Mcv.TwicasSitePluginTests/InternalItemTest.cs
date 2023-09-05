using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwicasSitePlugin;
using TwicasSitePlugin.LowObject;

namespace TwicasSitePluginTests
{
    [TestFixture]
    class InternalItemTests
    {
        [Test]
        public void Test()
        {
            var data = "586375284\t0\t545\t1276\t4\t5753\t2333\t%E4%BB%8A%E5%B9%B4%E5%88%9D%E3%81%AE%E4%BD%93%E9%87%8D%E3%82%92%E8%A8%88%E3%82%8B\t\t\t1\t41149\t0\t0\t\t\t%2041149%09https%3A%2F%2Fs01.twitcasting.tv%2Fimg%2Fitem_tea_10.png%09%09https%3A%2F%2Fimagegw02.twitcasting.tv%2Fimage3s%2Fpbs.twimg.com%2Fprofile_images%2F1124531323761070082%2Ftr1t5bQf_normal.jpg%09QW9pU3Rvcm0%3D%09tea.baku%09like%0930%09Zmxvd2l0ZW0oImh0dHBzOi8vdHdpdGNhc3RpbmcudHYvaW1nL2FuaW0vYW5pbV90ZWFfMTAiLCAzMDAwLCAxLCAxLCA1KQ%3D%3D%091%09aoistorm%09634599665%0944GC44KS44G%2F44Gk%0944GK6Iy2772YMTA%3D%091%09%0A\t\t0\t0\t0\n";
            var streamChecker = StreamChecker2.ParseStreamChcker(data);
            var itemLow = streamChecker.Items[0];
            var item = new InternalItem(itemLow);
            Assert.AreEqual("お茶ｘ10", item.Name);

        }
    }
}
