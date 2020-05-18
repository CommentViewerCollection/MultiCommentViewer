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
            var data = "507331047	0	17289	20000	4	82139	2008	%E6%9C%89%E5%90%8D%E3%82%AD%E3%83%A3%E3%82%B9%E6%AD%8C%E3%81%84%E6%89%8B%E9%9B%86%E5%9B%A3%E3%81%AE%E8%A3%8F		#korekore19	1	212229	0	0			%20212229%09https%3A%2F%2Fs01.twitcasting.tv%2Fimg%2Fitem_tea_10.png%09%09https%3A%2F%2Fimagegw02.twitcasting.tv%2Fimage3s%2Fpbs.twimg.com%2Fprofile_images%2F1051833323176124417%2FVM_TEFEt_normal.jpg%09R29tZXpfMTExNw%3D%3D%09tea.baku%09like%0930%09Zmxvd2l0ZW0oImh0dHBzOi8vdHdpdGNhc3RpbmcudHYvaW1nL2FuaW0vYW5pbV90ZWFfMTAiLCAzMDAwLCAxLCAyLCA1KQ%3D%3D%091%09gomez_1117%091049689726981046272%0944K044Oq44G9QM6y4pmh%0944GK6Iy254iGMTA%3D%091%09%0A		0	0\t0";
            var sc = TwicasSitePlugin.LowObject.StreamChecker2.ParseStreamChcker(data);
            Assert.AreEqual("有名キャス歌い手集団の裏", sc.Telop);
            return;
        }
        [Test]
        public void Test2()
        {
            var data = "507653177	0	94	419	4	625	101	%E5%83%95%E3%82%88%E3%82%8A%E3%82%A4%E3%82%B1%E3%83%9C%E3%81%AA%E4%BA%BA%E5%87%B8%E5%BE%85%E3%81%A1			0	688	0	0			%20688%09https%3A%2F%2Ftwitcasting.tv%2Fimg%2Fitem_funding_stamp.png%096YGU5oiQOiA1MCUgOiDjgYTjgaTjgoLjg6rjgrnjg4rjg7zjgZXjgpPjgYLjgorjgYzjgajjgYbjgaDjg6%2Fjg7MoICrCtOiJuO%2B9gCnvvIHvvIE%3D%09https%3A%2F%2Ftwitcasting.tv%2Fimg%2Fitem_funding_user.png%09a2lpdG9zX2Nhcw%3D%3D%09event.base%09stamp%090%09c3RhbXBpdGVtKCJodHRwczovL3R3aXRjYXN0aW5nLnR2L2ltZy9zdGFtcC9zdGFtcF9mdW5kaW5nXzUwLnBuZyIsMSwzMDAwKQ%3D%3D%091%09kiitos_cas%09965900769839915008%0944Kt44O844OI44K5%095pSv5o%2B0OiA1MDDlhoY%3D%093%09https%3A%2F%2Ftwitcasting.tv%2Fimg%2Fstamp%2Fstamp_funding_50.png%0A		0	0\t0";
            var sc = TwicasSitePlugin.LowObject.StreamChecker2.ParseStreamChcker(data);
            Assert.AreEqual("僕よりイケボな人凸待ち", sc.Telop);
            return;
        }

        [Test]
        public void KiitosTest()
        {
            var data = "%203639%09https%3A%2F%2Ftwitcasting.tv%2Fimg%2Fitem_funding_stamp.png%096YGU5oiQOiAxMDAlIDog8J%2BSm%2BODquOCueODiuODvOOBleOCk%2BOBruOCs%2BODoeODs%2BODiOOBp%2BS4ieinkuOCs%2BODvOODiuODvOmjr%2BS9nOOCi%2FCfkps%3D%09https%3A%2F%2Ftwitcasting.tv%2Fimg%2Fitem_funding_user.png%09a2lpdG9zX2Nhcw%3D%3D%09event.base%09stamp%090%09c3RhbXBpdGVtKCJodHRwczovL3R3aXRjYXN0aW5nLnR2L2ltZy9zdGFtcC9zdGFtcF9mdW5kaW5nXzEwMC5wbmciLDEsMzAwMCk%3D%091%09kiitos_cas%09965900769839915008%0944Kt44O844OI44K5%095pSv5o%2B0OiAxMDAw5YaG%093%09https%3A%2F%2Ftwitcasting.tv%2Fimg%2Fstamp%2Fstamp_funding_100.png";
            var item = Parse(data);

            Assert.AreEqual("stampitem(\"https://twitcasting.tv/img/stamp/stamp_funding_100.png\",1,3000)", item.Effect);
            Assert.AreEqual("3639", item.Id);
            Assert.AreEqual("https://twitcasting.tv/img/item_funding_stamp.png", item.ItemImage);
            Assert.AreEqual("達成: 100% : 💛リスナーさんのコメントで三角コーナー飯作る💛", item.Message);
            Assert.AreEqual("https://twitcasting.tv/img/item_funding_user.png", item.SenderImage);
            Assert.AreEqual("kiitos_cas", item.SenderName);

            var screenName = item.t12;
            var username = item.SenderName;
            var message = $"[{item.t13}] {item.Message}";
        }
        [Test]
        public void ContinueCoinTest()
        {
            var data = "3703%09https%3A%2F%2Fs01.twitcasting.tv%2Fimg%2Fitem_coin.png%0944Kq44Oi44Ot44Kk5Lq644Gg44Gt%09https%3A%2F%2Fimagegw02.twitcasting.tv%2Fimage3%2Fimg-twitcasting.s3-us-west-1.amazonaws.com%2F9a%2F1f%2F5be434d22723a_64.jpg%09YzpoYXJ1NTU3Ng%3D%3D%09coin%09continue%091%09%091%09c%3Aharu5576%09c%3Aharu5576%0944Gx44KL%0944Kz44Oz44OG44Kj44OL44Ol44O844Kz44Kk44Oz%092%09%0A";
            var item = Parse(data);

            Assert.AreEqual("", item.Effect);
            Assert.AreEqual("3703", item.Id);
            Assert.AreEqual("https://s01.twitcasting.tv/img/item_coin.png", item.ItemImage);
            Assert.AreEqual("オモロイ人だね", item.Message);
            Assert.AreEqual("https://imagegw02.twitcasting.tv/image3/img-twitcasting.s3-us-west-1.amazonaws.com/9a/1f/5be434d22723a_64.jpg", item.SenderImage);
            Assert.AreEqual("c:haru5576", item.SenderName);
            Assert.AreEqual("コンティニューコイン", item.t13);
            var screenName = item.t12;
            var username = item.SenderName;
            var message = item.Message;
        }
        [Test]
        public void FrameTest()
        {
            var data = "3707%09https%3A%2F%2Fs01.twitcasting.tv%2Fimg%2Fitem_frame_child.png%09%09https%3A%2F%2Fimagegw02.twitcasting.tv%2Fimage3s%2Fpbs.twimg.com%2Fprofile_images%2F1082451300220989440%2F46bLVM4F_normal.jpg%09MTJfRGFobGlhXzA5%09frame.child%09frame%098%09ZnJhbWVpdGVtKCJodHRwczovL3R3aXRjYXN0aW5nLnR2L2ltZy9hbmltL2FuaW1fZnJhbWVfYnVyZ2VyX3YyXzUucG5nIik%3D%091%0912_dahlia_09%092430030277%09yZDEsWzJpcmQ4ZehKOOBoOOCiuOBginjgoLjgYbkuIDluqbmhJvjgZXjgozjgZ%2FjgYTinaQ%3D%0944Oa44OD44OR44O844O744OH44Kz44OV44Os44O844Og%094%09%0A%203708%09https%3A%2F%2Fs01.twitcasting.tv%2Fimg%2Fitem_tea.png%09%09https%3A%2F%2Fimagegw02.twitcasting.tv%2Fimage3s%2Fpbs.twimg.com%2Fprofile_images%2F1098652329069273089%2FLapkwEsQ_normal.jpg%09YXJpbmNvX3h4eA%3D%3D%09tea%09like%093%09%091%09arinco_xxx%09976953320391634944%094ouG4ricIOOBguODquOCk%2BOBk%2BOBpuOCg%2BOCkyjwn42TKc%2BC4p65IOK4neKLhg%3D%3D%0944GK6Iy2%092%09";
            var item = Parse(data);

            Assert.AreEqual("frameitem(\"https://twitcasting.tv/img/anim/anim_frame_burger_v2_5.png\")", item.Effect);
            Assert.AreEqual("3707", item.Id);
            Assert.AreEqual("https://s01.twitcasting.tv/img/item_frame_child.png", item.ItemImage);
            Assert.IsTrue(string.IsNullOrEmpty(item.Message));
            Assert.AreEqual("https://imagegw02.twitcasting.tv/image3s/pbs.twimg.com/profile_images/1082451300220989440/46bLVM4F_normal.jpg", item.SenderImage);
            Assert.AreEqual("12_Dahlia_09", item.SenderName);
            Assert.AreEqual("ペッパー・デコフレーム", item.t13);
            var screenName = item.t12;
            var username = item.SenderName;
            var message = item.Message;
        }
        private Item Parse(string data)
        {
            var method1 = typeof(TwicasSitePlugin.LowObject.StreamChecker2).GetMethod("UrlDecode", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
            var method2 = typeof(TwicasSitePlugin.LowObject.StreamChecker2).GetMethod("Parse", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
            var decoded = method1.Invoke(null, new[] { data }) as string;
            var item = method2.Invoke(null, new[] { decoded }) as Item;
            return item;
        }
    }
}
