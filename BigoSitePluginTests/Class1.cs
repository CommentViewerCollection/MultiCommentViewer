using BigoSitePlugin;
using NUnit.Framework;

namespace BigoSitePluginTests
{
    [TestFixture]
    public class Class1
    {
        [Test]
        public void Parse()
        {
            var data = "2584\t{\"from_uid\":\"464080989\",\"oriUri\":\"2060425\",\"payload\":{\"content\":\"eyJrIjoi4oCt8J+Sq+KArFRIQU5IIExJRU3igK3wn5Kr4oCsIiwibnUiOjAsImQiOiIxODA3OTg0MzkxIiwibSI6IkDigK3wn5Kr4oCsVEhBTkggTElFTeKArfCfkqvigKwg44GK44KB44Gn44Go44GG44GU44GW44GE44G+44GZ8J+kl+OBiuOBpOOBp+OBmfCfmYfigI3imYLvuI8iLCJuIjoi44KG44GN8J+mlPCfjLjjgajjgZfwn5C74p2k77iPIiwiYSI6MSwiYiI6MH0=\",\"contribution\":\"0\",\"grade\":\"38\",\"others\":[{\"key\":\"bImg\",\"value\":\"https://giftesx.bigo.sg/live/3s2/0O4Msp.png\"},{\"key\":\"beanGrade\",\"value\":\"1\"},{\"key\":\"ca\",\"value\":\"https://giftesx.bigo.sg/live/7h4/M09/03/F3/bPsbAF2Aw1CIBgqpAADPwlC3YCgAAqZ2gLuOz0AAM_a18.webp\"},{\"key\":\"fLevel\",\"value\":\"27\"},{\"key\":\"fTag\",\"value\":\"6\"},{\"key\":\"fText\",\"value\":\"のあfam\"},{\"key\":\"is_rv\",\"value\":\"0\"},{\"key\":\"l\",\"value\":\"{\\\\\\\"l\\\\\\\":[{\\\\\\\"u\\\\\\\":\\\\\\\"https://giftesx.bigo.sg/live/g2/M09/1B/2A/CYAIAFxBteWIc0YWAAAGdfXA75kAAfjAQIuERUAAAaN442.png\\\\\\\"}]}\"},{\"key\":\"labelTag\",\"value\":\"0\"},{\"key\":\"nb\",\"value\":\"9900\"}],\"owner\":\"1857992589\",\"seqId\":\"1012904877\",\"tag\":\"1\",\"timestamp\":\"1609481550\",\"uid\":\"464080989\"},\"room_id\":\"6756143314643421415\",\"seqId\":\"1012904878\"}\n";
            var parser = new MessageParser();
            var internalMessage = parser.Parse(data) as NormalText;
            Assert.IsNotNull(internalMessage);
            Assert.AreEqual("@‭💫‬THANH LIEM‭💫‬ おめでとうございます🤗おつです🙇‍♂️", internalMessage.Message);
            Assert.AreEqual("ゆき\U0001f994🌸とし🐻❤️", internalMessage.Name);
        }
        [Test]
        public void ParseNormalGiftTextTest()
        {
            var data = "2584\t{\"from_uid\":\"485302037\",\"oriUri\":\"2060425\",\"payload\":{\"content\":\"eyJhIjoiMCIsImIiOiIwIiwiYyI6IjEiLCJrIjoiIiwibSI6IjI2NjIiLCJuIjoiXHUzMGRkXHUzMGMzXHUzMGFiXHUzMGVhIiwidCI6IiIsInUiOiIifQ==\",\"contribution\":\"0\",\"grade\":\"17\",\"others\":[{\"key\":\"aN\",\"value\":\"\"},{\"key\":\"beanGrade\",\"value\":\"22\"},{\"key\":\"ca\",\"value\":\"https://giftesx.bigo.sg/live/7h3/M09/C3/FD/MPsbAF2AwzSIRASDAADfdHeRNBQAAhBWAEgDE0AAN-M05.webp\"},{\"key\":\"ct\",\"value\":\"3\"},{\"key\":\"fLevel\",\"value\":\"19\"},{\"key\":\"fTag\",\"value\":\"4\"},{\"key\":\"fText\",\"value\":\"🍯もも🍭\"},{\"key\":\"familyLevel\",\"value\":\"0\"},{\"key\":\"familySubLevel\",\"value\":\"0\"},{\"key\":\"l\",\"value\":\"{\\\\\\\"l\\\\\\\":[{\\\\\\\"u\\\\\\\":\\\\\\\"https://giftesx.bigo.sg/live/g2/M09/1B/2A/CYAIAFxBteWIc0YWAAAGdfXA75kAAfjAQIuERUAAAaN442.png\\\\\\\"}]}\"},{\"key\":\"labelTag\",\"value\":\"0\"},{\"key\":\"nb\",\"value\":\"9950\"},{\"key\":\"src\",\"value\":\"b\"},{\"key\":\"t\",\"value\":\"3\"},{\"key\":\"tu\",\"value\":\"1749813713\"},{\"key\":\"uN\",\"value\":\"\"}],\"owner\":\"1749813713\",\"seqId\":\"485790380\",\"tag\":\"6\",\"timestamp\":\"0\",\"uid\":\"485302037\"},\"room_id\":\"6593467330806872620\",\"seqId\":\"485790380\"}\n";
            var parser = new MessageParser();
            var internalMessage = parser.Parse(data) as NormalGiftText;
            Assert.IsNotNull(internalMessage);
            Assert.AreEqual("0", internalMessage.A);
            Assert.AreEqual("0", internalMessage.B);
            Assert.AreEqual("1", internalMessage.C);
            Assert.AreEqual("", internalMessage.K);
            Assert.AreEqual("2662", internalMessage.M);
            Assert.AreEqual("ポッカリ", internalMessage.N);
            Assert.AreEqual("", internalMessage.T);
            Assert.AreEqual("", internalMessage.U);
        }
        [Test]
        public void ParseLightMyHeartTextTest()
        {
            var data = "2584\t{\"from_uid\":\"491234139\",\"oriUri\":\"2060425\",\"payload\":{\"content\":\"eyJiIjowLCJuIjoiR3JhY2VlIiwibnUiOjAsImEiOjB9\",\"contribution\":\"0\",\"grade\":\"10\",\"others\":[{\"key\":\"beanGrade\",\"value\":\"14\"},{\"key\":\"ca\",\"value\":\"https://giftesx.bigo.sg/live/7h4/M09/03/F3/bPsbAF2Aw1CIBgqpAADPwlC3YCgAAqZ2gLuOz0AAM_a18.webp\"},{\"key\":\"fLevel\",\"value\":\"5\"},{\"key\":\"fTag\",\"value\":\"1\"},{\"key\":\"fText\",\"value\":\"エリ天🍤\"},{\"key\":\"is_rv\",\"value\":\"0\"},{\"key\":\"l\",\"value\":\"{\\\\\\\"l\\\\\\\":[{\\\\\\\"u\\\\\\\":\\\\\\\"https://giftesx.bigo.sg/live/g2/M09/1B/2A/CYAIAFxBteWIc0YWAAAGdfXA75kAAfjAQIuERUAAAaN442.png\\\\\\\"}]}\"},{\"key\":\"labelTag\",\"value\":\"0\"},{\"key\":\"nb\",\"value\":\"9900\"}],\"owner\":\"1749813713\",\"seqId\":\"721878366\",\"tag\":\"3\",\"timestamp\":\"1609187381\",\"uid\":\"491234139\"},\"room_id\":\"6593467330806872620\",\"seqId\":\"721878367\"}\n";
            var parser = new MessageParser();
            var internalMessage = parser.Parse(data) as LightMyHeartText;
            Assert.IsNotNull(internalMessage);
            Assert.AreEqual("495", internalMessage.ItemId);
            Assert.AreEqual(1609187381, internalMessage.Timestamp);
            Assert.AreEqual("491234139", internalMessage.UserId);
            Assert.AreEqual("Gracee", internalMessage.Username);
        }
    }
}
