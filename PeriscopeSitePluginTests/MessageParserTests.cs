using NUnit.Framework;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Codeplex.Data;
using PeriscopeSitePlugin;

namespace PeriscopeSitePluginTests
{
    [TestFixture]
    class MessageParserTests
    {
        [Test]
        public void ParseWebsocketMessageTest()
        {
            var data = "{\"kind\":2,\"payload\":\"abc\"}";
            var message = MessageParser.ParseWebsocketMessage(data);
            Assert.AreEqual(2, message.Kind);
            Assert.AreEqual("abc", message.Payload);
            Assert.IsTrue(data == message.Raw);
        }
        [Test]
        public void ParseKind1Type2Test()
        {
            var data = "{\"kind\":1,\"payload\":\"{\\\"room\\\":\\\"1lPKqorLvldJb\\\",\\\"body\\\":\\\"{\\\\\\\"displayName\\\\\\\":\\\\\\\"Bill\\\\\\\",\\\\\\\"ntpForBroadcasterFrame\\\\\\\":16166207731395686400,\\\\\\\"ntpForLiveFrame\\\\\\\":16166207721358794903,\\\\\\\"participant_index\\\\\\\":824787836,\\\\\\\"remoteID\\\\\\\":\\\\\\\"1476841\\\\\\\",\\\\\\\"timestamp\\\\\\\":1554999751502,\\\\\\\"type\\\\\\\":2,\\\\\\\"uuid\\\\\\\":\\\\\\\"3A6AE282-D227-46BF-AFE6-6BB1CD8E8C47\\\\\\\",\\\\\\\"v\\\\\\\":2}\\\",\\\"lang\\\":\\\"\\\",\\\"sender\\\":{\\\"user_id\\\":\\\"1476841\\\",\\\"participant_index\\\":824787836,\\\"twitter_id\\\":\\\"2160881996\\\"},\\\"timestamp\\\":1554999751631707918}\",\"signature\":\"3G8LdzgKHD9aP8IogF_cmdYqJGCrn4kL4xkG2Xw\"}";
            var message = MessageParser.ParseWebsocketMessage(data);
            var kind1type2 = MessageParser.Parse(message) as Kind1Type2;

        }
        [Test]
        public void ParseKind1Type1Test()
        {
            var data = "{\"kind\":1,\"payload\":\"{\\\"room\\\":\\\"1lPKqorLvldJb\\\",\\\"body\\\":\\\"{\\\\\\\"body\\\\\\\":\\\\\\\"Yeah\\\\\\\",\\\\\\\"displayName\\\\\\\":\\\\\\\"PanAm Style\\\\\\\",\\\\\\\"ntpForBroadcasterFrame\\\\\\\":16166206334443356160,\\\\\\\"ntpForLiveFrame\\\\\\\":16166206323526947868,\\\\\\\"participant_index\\\\\\\":1043054637,\\\\\\\"remoteID\\\\\\\":\\\\\\\"1WgKgapJvplEv\\\\\\\",\\\\\\\"timestamp\\\\\\\":1554999426288,\\\\\\\"type\\\\\\\":1,\\\\\\\"username\\\\\\\":\\\\\\\"PanamStyle\\\\\\\",\\\\\\\"uuid\\\\\\\":\\\\\\\"BDDDFC39-FDE0-42D7-8977-D3945E8EC783\\\\\\\",\\\\\\\"v\\\\\\\":2}\\\",\\\"lang\\\":\\\"en-ca\\\",\\\"sender\\\":{\\\"user_id\\\":\\\"1WgKgapJvplEv\\\",\\\"username\\\":\\\"PanamStyle\\\",\\\"display_name\\\":\\\"PanAm Style\\\",\\\"profile_image_url\\\":\\\"https://pbs.twimg.com/profile_images/882416178018308096/6sHokIzm_reasonably_small.jpg\\\",\\\"participant_index\\\":1043054637,\\\"locale\\\":\\\"en\\\",\\\"twitter_id\\\":\\\"712819818001354752\\\",\\\"lang\\\":[\\\"en\\\",\\\"fr\\\"],\\\"superfan\\\":true},\\\"timestamp\\\":1554999426415264432,\\\"uuid\\\":\\\"BDDDFC39-FDE0-42D7-8977-D3945E8EC783\\\"}\",\"signature\":\"3YcKCZ_FsTIGh0QdY_LfwnBlyFhj0-qC0Sh1dIQ\"}";
            var message = MessageParser.ParseWebsocketMessage(data);
            var kind1type1 = MessageParser.Parse(message) as Kind1Type1;
            Assert.AreEqual("Yeah", kind1type1.Body);
            Assert.AreEqual("PanAm Style", kind1type1.DisplayName);
            Assert.AreEqual(InternalMessageType.Chat_CHAT, kind1type1.MessageType);
            Assert.AreEqual(1043054637, kind1type1.ParticipantIndex);
            Assert.AreEqual("https://pbs.twimg.com/profile_images/882416178018308096/6sHokIzm_reasonably_small.jpg", kind1type1.ProfileImageUrl);
            Assert.AreEqual("Yeah", kind1type1.Body);
            Assert.AreEqual("1WgKgapJvplEv", kind1type1.RemoteId);
            Assert.AreEqual(1554999426288, kind1type1.Timestamp);
            Assert.AreEqual(1, kind1type1.Type);
            Assert.AreEqual("1WgKgapJvplEv", kind1type1.UserId);
            Assert.AreEqual("PanamStyle", kind1type1.Username);
            Assert.AreEqual("BDDDFC39-FDE0-42D7-8977-D3945E8EC783", kind1type1.Uuid);
            Assert.AreEqual(2, kind1type1.V);
        }
        [Test]
        public void ParseKind1Type1_newtype_Test()
        {
            var data = "{\"kind\":1,\"payload\":\"{\\\"room\\\":\\\"1MYGNdDegvvxw\\\",\\\"body\\\":\\\"{\\\\\\\"body\\\\\\\":\\\\\\\"lol\\\\\\\",\\\\\\\"eggmojiOverride\\\\\\\":false,\\\\\\\"ntpForBroadcasterFrame\\\\\\\":16178099029025284000,\\\\\\\"ntpForLiveFrame\\\\\\\":16178098946832495000,\\\\\\\"participant_index\\\\\\\":1886231058,\\\\\\\"room\\\\\\\":\\\\\\\"1MYGNdDegvvxw\\\\\\\",\\\\\\\"sender\\\\\\\":{\\\\\\\"display_name\\\\\\\":\\\\\\\"MLCGoddessOfWar\\\\\\\",\\\\\\\"participant_index\\\\\\\":1886231058,\\\\\\\"profile_image_url\\\\\\\":\\\\\\\"https://prod-profile.pscp.tv/1eRKxBgxlwEwA/1a63d43c45a6b058e1337a9637970f05-128.jpg\\\\\\\",\\\\\\\"twitter_id\\\\\\\":\\\\\\\"41855602\\\\\\\",\\\\\\\"user_id\\\\\\\":\\\\\\\"1eRKxBgxlwEwA\\\\\\\",\\\\\\\"username\\\\\\\":\\\\\\\"MLCGoddessOfWar\\\\\\\"},\\\\\\\"type\\\\\\\":1,\\\\\\\"uuid\\\\\\\":\\\\\\\"92fc7d61-d96e-4433-9748-56edf7101268\\\\\\\",\\\\\\\"v\\\\\\\":2}\\\",\\\"lang\\\":\\\"en\\\",\\\"sender\\\":{\\\"user_id\\\":\\\"1eRKxBgxlwEwA\\\",\\\"username\\\":\\\"MLCGoddessOfWar\\\",\\\"display_name\\\":\\\"MLCGoddessOfWar\\\",\\\"profile_image_url\\\":\\\"https://prod-profile.pscp.tv/1eRKxBgxlwEwA/1a63d43c45a6b058e1337a9637970f05-128.jpg\\\",\\\"participant_index\\\":1886231058,\\\"locale\\\":\\\"en\\\",\\\"twitter_id\\\":\\\"41855602\\\",\\\"lang\\\":[\\\"en\\\"],\\\"superfan\\\":true},\\\"timestamp\\\":1557768410080950363,\\\"uuid\\\":\\\"92fc7d61-d96e-4433-9748-56edf7101268\\\"}\",\"signature\":\"37aMXoQrlXF8qISmJftijpC9i2VUX2LGGZp2Pyw\"}";
            var message = MessageParser.ParseWebsocketMessage(data);
            var kind1type1 = MessageParser.Parse(message) as Kind1Type1;
            Assert.AreEqual("lol", kind1type1.Body);
            Assert.AreEqual("MLCGoddessOfWar", kind1type1.DisplayName);
            Assert.AreEqual(InternalMessageType.Chat_CHAT, kind1type1.MessageType);
            Assert.AreEqual(1886231058, kind1type1.ParticipantIndex);
            Assert.AreEqual("https://prod-profile.pscp.tv/1eRKxBgxlwEwA/1a63d43c45a6b058e1337a9637970f05-128.jpg", kind1type1.ProfileImageUrl);
            Assert.AreEqual("1eRKxBgxlwEwA", kind1type1.RemoteId);
            Assert.AreEqual(1557768410080950363, kind1type1.Timestamp);
            Assert.AreEqual(1, kind1type1.Type);
            Assert.AreEqual("1eRKxBgxlwEwA", kind1type1.UserId);
            Assert.AreEqual("MLCGoddessOfWar", kind1type1.Username);
            Assert.AreEqual("92fc7d61-d96e-4433-9748-56edf7101268", kind1type1.Uuid);
            Assert.AreEqual(2, kind1type1.V);
        }
        [Test]
        public void ParseKind2Kind1Test()
        {
            var data = "{\"kind\":2,\"payload\":\"{\\\"kind\\\":1,\\\"sender\\\":{\\\"user_id\\\":\\\"1YLKJyaYPPQNn\\\",\\\"username\\\":\\\"some30\\\",\\\"display_name\\\":\\\"katsumi someya\\\",\\\"profile_image_url\\\":\\\"https://pbs.twimg.com/profile_images/696179553488613376/lCRMIYAP_reasonably_small.jpg\\\",\\\"participant_index\\\":1386286478,\\\"locale\\\":\\\"ja\\\",\\\"twitter_id\\\":\\\"158356237\\\",\\\"lang\\\":[\\\"ja\\\"]},\\\"body\\\":\\\"{\\\\\\\"room\\\\\\\":\\\\\\\"1ypKdvajqdaJW\\\\\\\",\\\\\\\"following\\\\\\\":false,\\\\\\\"unlimited\\\\\\\":false}\\\"}\",\"signature\":\"3kqHuspmysojsTQ_TccqI5zGPgCINR_Y_bad3fA\"}";
            var message = MessageParser.ParseWebsocketMessage(data);
            var internalMessage = MessageParser.Parse(message) as Kind2Kind1;
            Assert.AreEqual("katsumi someya", internalMessage.DisplayName);
            Assert.IsFalse(internalMessage.Following);
            Assert.AreEqual(InternalMessageType.Control_JOIN, internalMessage.MessageType);
            Assert.AreEqual("1ypKdvajqdaJW", internalMessage.RoomId);
            Assert.IsFalse(internalMessage.Unlimited);
            Assert.AreEqual("1YLKJyaYPPQNn", internalMessage.UserId);
            Assert.AreEqual("some30", internalMessage.Username);
        }
        [Test]
        public void ParseKind2Kind2Test()
        {
            var data = "{\"kind\":2,\"payload\":\"{\\\"kind\\\":2,\\\"sender\\\":{\\\"user_id\\\":\\\"1xeEWPNdOyOQP\\\",\\\"username\\\":\\\"mohammadamir1\\\",\\\"display_name\\\":\\\"Mohammad Amir\\\",\\\"profile_image_url\\\":\\\"https://platform-lookaside.fbsbx.com/platform/profilepic/?asid=1303299106487104\\\\u0026height=50\\\\u0026width=50\\\\u0026ext=1558863314\\\\u0026hash=AeSBy203S8rdJ0VK\\\",\\\"participant_index\\\":1168116142,\\\"locale\\\":\\\"hy\\\",\\\"new_user\\\":true,\\\"lang\\\":[\\\"hy\\\",\\\"en\\\",\\\"ar\\\"]},\\\"body\\\":\\\"{\\\\\\\"room\\\\\\\":\\\\\\\"1vOxwqmBoWRGB\\\\\\\",\\\\\\\"following\\\\\\\":false}\\\"}\"}";
            var message = MessageParser.ParseWebsocketMessage(data);
            var internalMessage = MessageParser.Parse(message) as Kind2Kind2;
            Assert.AreEqual("Mohammad Amir", internalMessage.DisplayName);
            Assert.IsFalse(internalMessage.Following);
            Assert.AreEqual(InternalMessageType.Control_LEAVE, internalMessage.MessageType);
            Assert.AreEqual("1vOxwqmBoWRGB", internalMessage.RoomId);
            Assert.IsFalse(internalMessage.Unlimited);
            Assert.AreEqual("1xeEWPNdOyOQP", internalMessage.UserId);
            Assert.AreEqual("mohammadamir1", internalMessage.Username);
        }
        [Test]
        public void ParseKind2Kind4Test()
        {
            var data = "{\"kind\":2,\"payload\":\"{\\\"kind\\\":4,\\\"sender\\\":{\\\"user_id\\\":\\\"\\\"},\\\"body\\\":\\\"{\\\\\\\"room\\\\\\\":\\\\\\\"1lPKqorLvldJb\\\\\\\",\\\\\\\"occupancy\\\\\\\":50,\\\\\\\"total_participants\\\\\\\":740}\\\"}\"}";
            var message = MessageParser.ParseWebsocketMessage(data);
            var internalMessage = MessageParser.Parse(message);

        }
    }
}
