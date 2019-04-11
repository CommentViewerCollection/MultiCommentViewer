using NUnit.Framework;
using System.Collections.Generic;
using PeriscopeSitePlugin;
using SitePlugin;

namespace PeriscopeSitePluginTests
{
    [TestFixture]
    class MessageTests
    {
        [Test]
        public void PeriscopeCommentTest()
        {
            var data = "{\"kind\":1,\"payload\":\"{\\\"room\\\":\\\"1lPKqorLvldJb\\\",\\\"body\\\":\\\"{\\\\\\\"body\\\\\\\":\\\\\\\"Yeah\\\\\\\",\\\\\\\"displayName\\\\\\\":\\\\\\\"PanAm Style\\\\\\\",\\\\\\\"ntpForBroadcasterFrame\\\\\\\":16166206334443356160,\\\\\\\"ntpForLiveFrame\\\\\\\":16166206323526947868,\\\\\\\"participant_index\\\\\\\":1043054637,\\\\\\\"remoteID\\\\\\\":\\\\\\\"1WgKgapJvplEv\\\\\\\",\\\\\\\"timestamp\\\\\\\":1556276205782765838,\\\\\\\"type\\\\\\\":1,\\\\\\\"username\\\\\\\":\\\\\\\"PanamStyle\\\\\\\",\\\\\\\"uuid\\\\\\\":\\\\\\\"BDDDFC39-FDE0-42D7-8977-D3945E8EC783\\\\\\\",\\\\\\\"v\\\\\\\":2}\\\",\\\"lang\\\":\\\"en-ca\\\",\\\"sender\\\":{\\\"user_id\\\":\\\"1WgKgapJvplEv\\\",\\\"username\\\":\\\"PanamStyle\\\",\\\"display_name\\\":\\\"PanAm Style\\\",\\\"profile_image_url\\\":\\\"https://pbs.twimg.com/profile_images/882416178018308096/6sHokIzm_reasonably_small.jpg\\\",\\\"participant_index\\\":1043054637,\\\"locale\\\":\\\"en\\\",\\\"twitter_id\\\":\\\"712819818001354752\\\",\\\"lang\\\":[\\\"en\\\",\\\"fr\\\"],\\\"superfan\\\":true},\\\"timestamp\\\":1554999426415264432,\\\"uuid\\\":\\\"BDDDFC39-FDE0-42D7-8977-D3945E8EC783\\\"}\",\"signature\":\"3YcKCZ_FsTIGh0QdY_LfwnBlyFhj0-qC0Sh1dIQ\"}";
            var message = MessageParser.ParseWebsocketMessage(data);
            var kind1type1 = MessageParser.Parse(message) as Kind1Type1;
            var comment = new PeriscopeComment(kind1type1);
            Assert.AreEqual(new List<IMessagePart> { Common.MessagePartFactory.CreateMessageText("Yeah") }, comment.CommentItems);
            Assert.AreEqual("BDDDFC39-FDE0-42D7-8977-D3945E8EC783", comment.Id);
            Assert.AreEqual(new List<IMessagePart> { Common.MessagePartFactory.CreateMessageText("PanAm Style") }, comment.NameItems);
            Assert.AreEqual(PeriscopeMessageType.Comment, comment.PeriscopeMessageType);
            Assert.AreEqual("19:56:45", comment.PostTime);
            Assert.AreEqual(SiteType.Periscope, comment.SiteType);
            Assert.IsNull(comment.UserIcon);
            Assert.AreEqual("1WgKgapJvplEv", comment.UserId);
        }
    }
}
