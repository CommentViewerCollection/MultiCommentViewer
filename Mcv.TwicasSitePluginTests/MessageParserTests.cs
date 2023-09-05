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
    class MessageParserTests
    {
        [Test]
        public void ParseTest()
        {
            var data = "["
                + "{\"type\":\"comment\",\"id\":17484180373,\"message\":\"\\u672c\\u6c17\\u306e\\u610f\\u898b\\u307b\\u3057\\u3044\\u3093\\u3060\\u308d\",\"rawMessage\":\"\\u672c\\u6c17\\u306e\\u610f\\u898b\\u307b\\u3057\\u3044\\u3093\\u3060\\u308d\",\"hasMention\":false,\"createdAt\":1577551368000,\"specialImage\":null,\"author\":{\"id\":\"c:maguma12\",\"name\":\"\\u307e\\u3050\\u307e\",\"screenName\":\"c:maguma12\",\"profileImage\":\"\\/\\/imagegw02.twitcasting.tv\\/image3s\\/img-twitcasting.s3-us-west-1.amazonaws.com\\/7f\\/46\\/5d8fc6c58149a_64.jpg\",\"grade\":0},\"numComments\":37018},"
                + "{\"type\":\"comment\",\"id\":17484180395,\"message\":\"\\u6b21\\u306f\\u77e5\\u3089\\u306a\\u3044\\u30b2\\u30b9\\u30c8\\u3055\\u3093\\u3068\\u7d61\\u3081\\u308b\\u5834\\u3068\\u5272\\u308a\\u5207\\u308d\\u3046\",\"rawMessage\":\"\\u6b21\\u306f\\u77e5\\u3089\\u306a\\u3044\\u30b2\\u30b9\\u30c8\\u3055\\u3093\\u3068\\u7d61\\u3081\\u308b\\u5834\\u3068\\u5272\\u308a\\u5207\\u308d\\u3046\",\"hasMention\":false,\"createdAt\":1577551368000,\"specialImage\":null,\"author\":{\"id\":\"c:sugarwater\",\"name\":\"\\u3055\\u3068\\u3046\\u307f\\u305a\",\"screenName\":\"c:sugarwater\",\"profileImage\":\"\\/\\/imagegw02.twitcasting.tv\\/image3\\/img-twitcasting.s3-us-west-1.amazonaws.com\\/5f\\/0b\\/5c0e8523e7a78_64.jpg?dev\",\"grade\":1},\"numComments\":37019}"
                + "]";
            var list = MessageParser.Parse(data);
            Assert.AreEqual(2, list.Count);
            var comment1 = list[0] as InternalComment;
            Assert.AreEqual(17484180373, comment1.Id);

            var comment2 = list[1] as InternalComment;
            Assert.AreEqual(17484180395, comment2.Id);
        }
    }
}
