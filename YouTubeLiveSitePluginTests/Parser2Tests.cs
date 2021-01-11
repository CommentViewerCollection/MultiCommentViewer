using NUnit.Framework;
using SitePlugin;
using System.Collections.Generic;
using YouTubeLiveSitePlugin.Next;

namespace YouTubeLiveSitePluginTests
{
    [TestFixture]
    class Parser2Tests
    {
        [Test]
        public void ParseLiveChatMembershipItemTest()
        {
            var data = "{\"addChatItemAction\":{\"item\":{\"liveChatMembershipItemRenderer\":{\"id\":\"ChwKGkNNYTBvZmI2anU0Q0ZZT0R3Z0VkVk4wSWNn\",\"timestampUsec\":\"1610199056715518\",\"authorExternalChannelId\":\"UC3NDq4U3m399k6Xvu3Xjmdw\",\"headerSubtext\":{\"runs\":[{\"text\":\"★THEかなた★\"},{\"text\":\"へようこそ！\"}]},\"authorName\":{\"simpleText\":\"NightStrix\"},\"authorPhoto\":{\"thumbnails\":[{\"url\":\"https://yt4.ggpht.com/ytc/AAUvwnhRqfpVnCOX-xA6HyfwiAGXePe_Ahc3MjLaetfwYQ=s32-c-k-c0x00ffffff-no-rj\",\"width\":32,\"height\":32},{\"url\":\"https://yt4.ggpht.com/ytc/AAUvwnhRqfpVnCOX-xA6HyfwiAGXePe_Ahc3MjLaetfwYQ=s64-c-k-c0x00ffffff-no-rj\",\"width\":64,\"height\":64}]},\"authorBadges\":[{\"liveChatAuthorBadgeRenderer\":{\"customThumbnail\":{\"thumbnails\":[{\"url\":\"https://yt3.ggpht.com/kjXx5nboby_LOvHUnWn4phLsmJw-zyUjZccLSCV3vXx2pvouqWxALzm2KFtWcf7ylkTQVcodow=s16-c-k\"},{\"url\":\"https://yt3.ggpht.com/kjXx5nboby_LOvHUnWn4phLsmJw-zyUjZccLSCV3vXx2pvouqWxALzm2KFtWcf7ylkTQVcodow=s32-c-k\"}]},\"tooltip\":\"新規メンバー\",\"accessibility\":{\"accessibilityData\":{\"label\":\"新規メンバー\"}}}}],\"contextMenuEndpoint\":{\"commandMetadata\":{\"webCommandMetadata\":{\"ignoreNavigation\":true}},\"liveChatItemContextMenuEndpoint\":{\"params\":\"Q2g0S0hBb2FRMDFoTUc5bVlqWnFkVFJEUmxsUFJIZG5SV1JXVGpCSlkyY1FBQm80Q2cwS0MxZ3plRE52YldONFJGSnJLaWNLR0ZWRFdteEVXSHBIYjI4M1pEUTBZbmRrVGs5aVJtRmpaeElMV0RONE0yOXRZM2hFVW1zZ0FpZ0JNaG9LR0ZWRE0wNUVjVFJWTTIwek9UbHJObGgyZFROWWFtMWtkdyUzRCUzRA==\"}},\"contextMenuAccessibility\":{\"accessibilityData\":{\"label\":\"コメントの操作\"}}}}}}";
            var membership = Parser2.ParseAction(data) as InternalMembership;
            Assert.IsNotNull(membership);
            Assert.AreEqual("ChwKGkNNYTBvZmI2anU0Q0ZZT0R3Z0VkVk4wSWNn", membership.Id);
            Assert.AreEqual(1610199056715518, membership.TimestampUsec);
            Assert.AreEqual("UC3NDq4U3m399k6Xvu3Xjmdw", membership.UserId);
            Assert.AreEqual(new List<IMessagePart> {
                Common.MessagePartFactory.CreateMessageText("★THEかなた★"),
                Common.MessagePartFactory.CreateMessageText("へようこそ！") }, membership.MessageItems);
            Assert.AreEqual(Common.MessagePartFactory.CreateMessageText("NightStrix"), membership.NameItems[0]);
        }
    }
}
