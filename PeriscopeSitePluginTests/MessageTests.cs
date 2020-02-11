using NUnit.Framework;
using System.Collections.Generic;
using PeriscopeSitePlugin;
using SitePlugin;
using System;

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
            Assert.AreEqual("Yeah", comment.Text);
            Assert.AreEqual("BDDDFC39-FDE0-42D7-8977-D3945E8EC783", comment.Id);
            Assert.AreEqual("PanAm Style", comment.DisplayName);
            Assert.AreEqual(PeriscopeMessageType.Comment, comment.PeriscopeMessageType);
            Assert.AreEqual(new DateTime(2019, 4, 26, 19, 56, 45, 782), comment.PostedAt);
            Assert.AreEqual(SiteType.Periscope, comment.SiteType);
            Assert.AreEqual("1WgKgapJvplEv", comment.UserId);
        }
        [Test]
        public void PeriscopeComment_kind1type1_newtypeTest()
        {
            var data = "{\"kind\":1,\"payload\":\"{\\\"room\\\":\\\"1MYGNdDegvvxw\\\",\\\"body\\\":\\\"{\\\\\\\"body\\\\\\\":\\\\\\\"lol\\\\\\\",\\\\\\\"eggmojiOverride\\\\\\\":false,\\\\\\\"ntpForBroadcasterFrame\\\\\\\":16178099029025284000,\\\\\\\"ntpForLiveFrame\\\\\\\":16178098946832495000,\\\\\\\"participant_index\\\\\\\":1886231058,\\\\\\\"room\\\\\\\":\\\\\\\"1MYGNdDegvvxw\\\\\\\",\\\\\\\"sender\\\\\\\":{\\\\\\\"display_name\\\\\\\":\\\\\\\"MLCGoddessOfWar\\\\\\\",\\\\\\\"participant_index\\\\\\\":1886231058,\\\\\\\"profile_image_url\\\\\\\":\\\\\\\"https://prod-profile.pscp.tv/1eRKxBgxlwEwA/1a63d43c45a6b058e1337a9637970f05-128.jpg\\\\\\\",\\\\\\\"twitter_id\\\\\\\":\\\\\\\"41855602\\\\\\\",\\\\\\\"user_id\\\\\\\":\\\\\\\"1eRKxBgxlwEwA\\\\\\\",\\\\\\\"username\\\\\\\":\\\\\\\"MLCGoddessOfWar\\\\\\\"},\\\\\\\"type\\\\\\\":1,\\\\\\\"uuid\\\\\\\":\\\\\\\"92fc7d61-d96e-4433-9748-56edf7101268\\\\\\\",\\\\\\\"v\\\\\\\":2}\\\",\\\"lang\\\":\\\"en\\\",\\\"sender\\\":{\\\"user_id\\\":\\\"1eRKxBgxlwEwA\\\",\\\"username\\\":\\\"MLCGoddessOfWar\\\",\\\"display_name\\\":\\\"MLCGoddessOfWar\\\",\\\"profile_image_url\\\":\\\"https://prod-profile.pscp.tv/1eRKxBgxlwEwA/1a63d43c45a6b058e1337a9637970f05-128.jpg\\\",\\\"participant_index\\\":1886231058,\\\"locale\\\":\\\"en\\\",\\\"twitter_id\\\":\\\"41855602\\\",\\\"lang\\\":[\\\"en\\\"],\\\"superfan\\\":true},\\\"timestamp\\\":1557768410080950363,\\\"uuid\\\":\\\"92fc7d61-d96e-4433-9748-56edf7101268\\\"}\",\"signature\":\"37aMXoQrlXF8qISmJftijpC9i2VUX2LGGZp2Pyw\"}";
            var message = MessageParser.ParseWebsocketMessage(data);
            var kind1type1 = MessageParser.Parse(message) as Kind1Type1;
            var comment = new PeriscopeComment(kind1type1);
            Assert.AreEqual("lol", comment.Text);
            Assert.AreEqual("92fc7d61-d96e-4433-9748-56edf7101268", comment.Id);
            Assert.AreEqual("MLCGoddessOfWar", comment.DisplayName);
            Assert.AreEqual(PeriscopeMessageType.Comment, comment.PeriscopeMessageType);
            Assert.AreEqual(new DateTime(2019, 5, 14, 2, 26, 50, 80), comment.PostedAt);
            Assert.AreEqual(SiteType.Periscope, comment.SiteType);
            Assert.AreEqual("1eRKxBgxlwEwA", comment.UserId);
        }
    }
}
