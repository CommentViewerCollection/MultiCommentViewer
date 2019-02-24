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
    }
}
