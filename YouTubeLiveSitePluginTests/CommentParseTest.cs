using Codeplex.Data;
using NUnit.Framework;
using SitePlugin;
using System.Collections.Generic;
using YouTubeLiveSitePlugin.Test2;

namespace YouTubeLiveSitePluginTests
{
    [TestFixture]
    class CommentParseTests
    {
        [Test]
        public void ParseLiveChatTextMessageRendererTest()
        {
            var data = Tools.GetSampleData("AddChatItemAction_authorBadge.txt");
            var d = DynamicJson.Parse(data);
            CommentData commentData = YouTubeLiveSitePlugin.Test2.Parser.ParseLiveChatTextMessageRenderer(d.addChatItemAction.item.liveChatTextMessageRenderer.ToString());
            Assert.AreEqual("UC0jl5mtntIH8O0ajZ0_V-tw", commentData.UserId);
            Assert.IsFalse(commentData.IsPaidMessage);
            Assert.AreEqual(new List<IMessagePart>
            {
                Common.MessagePartFactory.CreateMessageText("haza max//"),
                new Common.MessageImage{ Alt = "新規メンバー", Url = "https://yt3.ggpht.com/qPJX8KBpz4yOeltrmSzw1j0_K0TV_J3mddsGQ5D0ilq02_kDNl1O9-D9BOw3noFjdEsqpqRBdA=s16-c-k", Width = 16, Height = 16},
            }, commentData.NameItems);
            Assert.AreEqual(new List<IMessagePart>
            {
                Common.MessagePartFactory.CreateMessageText("古坂大魔王"),
            }, commentData.MessageItems);
            Assert.AreEqual(1541607345750974, commentData.TimestampUsec);
            Assert.AreEqual(new Common.MessageImage { Alt = null, Url = "https://yt3.ggpht.com/-G-W5PcqlqqY/AAAAAAAAAAI/AAAAAAAAAAA/nZt7m0jgEGg/s32-c-k-no-mo-rj-c0xffffff/photo.jpg", Width = 32, Height = 32 }, commentData.Thumbnail);
        }
        [Test]
        public void ParseLiveChatTextMessageRenderer_ModeratorTest()
        {
            var data = Tools.GetSampleData("AddChatItemAction_authorBadge_moderator.txt");
            var d = DynamicJson.Parse(data);
            CommentData commentData = YouTubeLiveSitePlugin.Test2.Parser.ParseLiveChatTextMessageRenderer(d.addChatItemAction.item.liveChatTextMessageRenderer.ToString());
            Assert.AreEqual("UCTBOZy7Q97gvK_TeaqkFQjQ", commentData.UserId);
            Assert.IsFalse(commentData.IsPaidMessage);
            Assert.AreEqual(new List<IMessagePart>
            {
                Common.MessagePartFactory.CreateMessageText("Taku73"),
            }, commentData.NameItems);
            Assert.AreEqual(new List<IMessagePart>
            {
                Common.MessagePartFactory.CreateMessageText("逆さでも使えたよ"),
            }, commentData.MessageItems);
            Assert.AreEqual(1541606987038506, commentData.TimestampUsec);
            Assert.AreEqual(new Common.MessageImage { Alt = null, Url = "https://yt3.ggpht.com/--ifK17iopQY/AAAAAAAAAAI/AAAAAAAAAAA/xnoUPmWt5mY/s32-c-k-no-mo-rj-c0xffffff/photo.jpg", Width = 32, Height = 32 }, commentData.Thumbnail);
        }
        [Test]
        public void ParseLiveChatPaidMessageRenderer()
        {
            var data = Tools.GetSampleData("AddChatAction_liveChatPaidMessageRenderer.txt");
            var d = DynamicJson.Parse(data);
            CommentData commentData = YouTubeLiveSitePlugin.Test2.Parser.ParseLiveChatPaidMessageRenderer(d.addChatItemAction.item.liveChatPaidMessageRenderer.ToString());
            Assert.AreEqual("UCWzefMLeuAhrCyhqYsFLl5Q", commentData.UserId);
            Assert.IsTrue(commentData.IsPaidMessage);
            Assert.AreEqual("￥1,200", commentData.PurchaseAmount);
            Assert.AreEqual(new List<IMessagePart>
            {
                Common.MessagePartFactory.CreateMessageText("ん ん"),
            }, commentData.NameItems);
            Assert.AreEqual(new List<IMessagePart>
            {
                Common.MessagePartFactory.CreateMessageText("面白かったです ではまた^ ^"),
            }, commentData.MessageItems);
            Assert.AreEqual(1541607010250939, commentData.TimestampUsec);
            Assert.AreEqual(new Common.MessageImage { Alt = null, Url = "https://yt3.ggpht.com/-BrEaZAWz_Do/AAAAAAAAAAI/AAAAAAAAAAA/n2m4xm5WT40/s32-c-k-no-mo-rj-c0xffffff/photo.jpg", Width = 32, Height = 32 }, commentData.Thumbnail);
        }

    }
}
