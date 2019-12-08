using MirrativSitePlugin;
using NUnit.Framework;
using SitePlugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MirrativSitePluginTests
{
    [TestFixture]
    class ParserTests
    {
        [Test]
        public void Test()
        {
            var data = "{\"push_image_url\":\"\",\"speech\":\"\",\"d\":1,\"ac\":\"Mirrativ bot\",\"burl\":\"https://www.mirrativ.com/assets/img/ic_badge_S.png?v2\",\"iurl\":\"https://cdn.mirrativ.com/mirrorman-prod/image/profile_image/ce6c9a48c7d08228af072c7de32fc750f237311c0755f95a7693c88e27cf1d90_m.jpeg?1508489473\",\"cm\":\"シェイク検知：60秒間、画面共有を停止するよ。再シェイクすると画面共有を再開できるよ！\",\"created_at\":1546438220,\"u\":\"1540862\",\"is_moderator\":0,\"lci\":1331546385,\"t\":1}";
            var json = Codeplex.Data.DynamicJson.Parse(data);
            var message = MirrativSitePlugin.Tools.ParseType1Data(json);
            Assert.AreEqual("シェイク検知：60秒間、画面共有を停止するよ。再シェイクすると画面共有を再開できるよ！", message.Comment);
            Assert.AreEqual(1546438220, message.CreatedAt);
            Assert.AreEqual("1331546385", message.Id);
            Assert.AreEqual("1540862", message.UserId);
            Assert.AreEqual("Mirrativ bot", message.Username);
            Assert.AreEqual(MessageType.Comment, message.Type);
        }
        [Test]
        public void Test1()
        {
            var data = "{\"gift_title\":\"かわいいエモモスナップ(300)\",\"photo_gift_id\":\"9162721\",\"burl\":\"\",\"coins\":\"300\",\"gift_small_image_url\":\"https:\\/\\/cdn.mirrativ.com\\/mirrorman-prod\\/assets\\/img\\/gift\\/small_64.png?v=5\",\"u\":\"4353835\",\"nameplate_enabled\":\"1\",\"t\":35,\"avatar_user_ids\":\"4072373,4383477,6221780,4353835,2921078,664329\",\"count\":1,\"is_photo_gift\":1,\"ac\":\"matsu【\\ud83c\\udfa8定期組】\\ud83c\\udf77\\ud83c\\udccf\\ud83d\\udc9c \",\"total_gift_coins\":\"25972\",\"iurl\":\"https:\\/\\/cdn.mirrativ.com\\/mirrorman-prod\\/image\\/profile_image\\/5b4ceb7de739f19491efe17165c7fa2f8c065170ef2b0c1ff039e96c48c6125e_m.jpeg?1552123860\",\"gift_id\":\"64\",\"pause_duration\":\"0\",\"orientations\":\"0\",\"gift_large_image_url\":\"https:\\/\\/cdn.mirrativ.com\\/mirrorman-prod\\/assets\\/img\\/gift\\/large_64.png?v=5\",\"photo_gift_image_url\":\"https:\\/\\/cdn.mirrativ.com\\/mirrorman-prod\\/image\\/photo_gift:1552124210:4353835:26477211\\/5b4ceb7de739f19491efe17165c7fa2f8c065170ef2b0c1ff039e96c48c6125e_origin.png?1552124211\",\"share_text\":\"@KURORO966_Blackさん,@akatukihawk3さん,@usausa_otomeさん,@0609_spitzさん,@uru_umiさん,カルルンバ\\ud83c\\udfa8さんとの  #エモモスナップ！ #エモモ #ミラティブ\"}";
            MessageParser.GetCurrent = () => new DateTime(2019, 12, 9, 1, 2, 3);
            var message = MessageParser.ParseMessage(data, (msg, type) => { });
            var photoGift = message as MirrativPhotoGift;
            Assert.IsNotNull(photoGift);
            Assert.IsNull(photoGift.BUrl);
            Assert.AreEqual(300, photoGift.Coins);
            Assert.AreEqual(new List<IMessagePart> { Common.MessagePartFactory.CreateMessageText("@KURORO966_Blackさん,@akatukihawk3さん,@usausa_otomeさん,@0609_spitzさん,@uru_umiさん,カルルンバ🎨さんとの  #エモモスナップ！ #エモモ #ミラティブ") }, photoGift.CommentItems);
            Assert.IsNull(photoGift.GiftSmallImageUrl);
            Assert.AreEqual("かわいいエモモスナップ(300)", photoGift.GiftTitle);
            Assert.IsNull(photoGift.Id);
            Assert.AreEqual(MirrativMessageType.Item, photoGift.MirrativMessageType);
            Assert.AreEqual(new List<IMessagePart> { Common.MessagePartFactory.CreateMessageText("matsu【🎨定期組】🍷🃏💜 ") }, photoGift.NameItems);
            Assert.IsNull(photoGift.PhotoGiftId);
            Assert.AreEqual("01:02:03", photoGift.PostTime);
            Assert.AreEqual("@KURORO966_Blackさん,@akatukihawk3さん,@usausa_otomeさん,@0609_spitzさん,@uru_umiさん,カルルンバ🎨さんとの  #エモモスナップ！ #エモモ #ミラティブ", photoGift.ShareText);
            Assert.AreEqual(SiteType.Mirrativ, photoGift.SiteType);
            Assert.AreEqual("4353835", photoGift.UserId);
        }
        [Test]
        public void Test2()
        {
            var data = "{\"count\":\"8\",\"gift_title\":\"小さな星\",\"total_gift_coins\":\"26306\",\"ac\":\"\\ud83d\\udc3e真顔ちゃん'-'\\ud83c\\udf4a\\ud83c\\udf4c\\ud83d\\udd4a\\ud83d\\udc36\\ud83c\\udf31\\ud83c\\udf75\",\"burl\":\"\",\"iurl\":\"https:\\/\\/cdn.mirrativ.com\\/mirrorman-prod\\/image\\/profile_image\\/fa3a29a81ece745badebc1fee44071997da131414ee7d53e2bb5228f2adf23cd_m.jpeg?1551797451\",\"coins\":\"1\",\"gift_small_image_url\":\"https:\\/\\/cdn.mirrativ.com\\/mirrorman-prod\\/assets\\/img\\/gift\\/small_1.png?v=2\",\"u\":\"5101297\",\"gift_id\":\"1\",\"nameplate_enabled\":\"1\",\"pause_duration\":\"0\",\"gift_large_image_url\":\"https:\\/\\/cdn.mirrativ.com\\/mirrorman-prod\\/assets\\/img\\/gift\\/large_1.png?v=2\",\"t\":35}";
            MessageParser.GetCurrent = () => new DateTime(2019, 12, 9, 1, 0, 0);
            var message = MessageParser.ParseMessage(data, (msg, type) => { });
            var gift = message as MirrativGift;
            Assert.IsNotNull(gift);
            Assert.AreEqual(8, gift.Count);
            Assert.AreEqual(new List<IMessagePart> { Common.MessagePartFactory.CreateMessageText("🐾真顔ちゃん'-'🍊🍌🕊🐶🌱🍵が小さな星を8個贈りました") }, gift.CommentItems);
            Assert.AreEqual(new List<IMessagePart> { Common.MessagePartFactory.CreateMessageText("🐾真顔ちゃん'-'🍊🍌🕊🐶🌱🍵") }, gift.NameItems);
        }
    }
}
