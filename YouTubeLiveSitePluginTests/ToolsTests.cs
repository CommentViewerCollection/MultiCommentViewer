using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YouTubeLiveSitePluginTests
{
    [TestFixture]
    class ToolsTests
    {
        [Test]
        public void ExtractYtInitialFromSubscribedChannelHtmlTest()
        {
            var data = "}</script><script nonce=\"orPyHr13z9j4Y/4tOnK69A\">var ytInitialData = {\"responseContext\":\"GEpHycQwjdT6A\"}]}}};</script><tit";
            var s = YouTubeLiveSitePlugin.Test2.Tools.ExtractYtInitialDataFromChannelHtml(data);
            Assert.IsTrue(!string.IsNullOrEmpty(s));
        }
        [Test]
        public void YtInitialDataNotContainsContentsTest()
        {
            var data = "{\"responseContext\":{\"errors\":{\"error\":[{\"domain\":\"gdata.CoreErrorDomain\",\"code\":\"INVALID_VALUE\",\"debugInfo\":\"Error decrypting and parsing the live chat ID.\",\"externalErrorMessage\":\"不明なエラーです。\"}]},\"serviceTrackingParams\":[{\"service\":\"CSI\",\"params\":[{\"key\":\"GetLiveChat_rid\",\"value\":\"0x3365759ba77f978f\"},{\"key\":\"c\",\"value\":\"WEB\"},{\"key\":\"cver\",\"value\":\"2.20190529\"},{\"key\":\"yt_li\",\"value\":\"1\"}]},{\"service\":\"GFEEDBACK\",\"params\":[{\"key\":\"e\",\"value\":\"23720702,23736685,23744176,23750984,23751767,23752869,23755886,23755898,23759224,23766102,23767634,23771992,23785333,23788845,23793834,23794471,23799777,23804281,23804294,23805410,23806435,23808949,23809331,23810273,23811378,23811593,23812530,23812566,23813310,23813548,23813622,23813949,23814199,23814507,23815144,23815164,23815172,23815485,23815949,23817343,23817794,23817825,23818213,9407610,9441381,9449243,9471235\"},{\"key\":\"logged_in\",\"value\":\"1\"}]},{\"service\":\"GUIDED_HELP\",\"params\":[{\"key\":\"creator_channel_id\",\"value\":\"UCK6F1ecql0T_9hHGTw7heBA\"},{\"key\":\"logged_in\",\"value\":\"1\"}]},{\"service\":\"ECATCHER\",\"params\":[{\"key\":\"client.name\",\"value\":\"WEB\"},{\"key\":\"client.version\",\"value\":\"2.20190529\"},{\"key\":\"innertube.build.changelist\",\"value\":\"250485423\"},{\"key\":\"innertube.build.experiments.source_version\",\"value\":\"250547910\"},{\"key\":\"innertube.build.label\",\"value\":\"youtube.ytfe.innertube_20190528_7_RC1\"},{\"key\":\"innertube.build.timestamp\",\"value\":\"1559140061\"},{\"key\":\"innertube.build.variants.checksum\",\"value\":\"7e46d96e46a45788f840d135c2cf4890\"},{\"key\":\"innertube.run.job\",\"value\":\"ytfe-innertube-replica-only.ytfe\"}]}],\"webResponseContextExtensionData\":{\"ytConfigData\":{\"csn\":\"4wLwXOyiG5OPgAOH4LYI\",\"visitorData\":\"CgtpTXJTMXZJR3ZLayjjhcDnBQ%3D%3D\",\"sessionIndex\":1}}},\"trackingParams\":\"CAAQ0b4BIhMIrKDMwNTD4gIVkwdgCh0HsA0B\"}";
            Assert.Throws<YouTubeLiveSitePlugin.Test2.YouTubeLiveServerErrorException>(() => YouTubeLiveSitePlugin.Test2.Tools.ParseYtInitialData(data));
        }
        [Test]
        public void ExtractYtInitialDataFromChannelHtmlTest()
        {
            var data = "window[\"ytInitialData\"] = JSON.parse(\"{\\\"a\\\":\\\"b\\\"}\");abc";
            var s = YouTubeLiveSitePlugin.Test2.Tools.ExtractYtInitialDataFromChannelHtml(data);
            Assert.AreEqual("{\"a\":\"b\"}", s);
        }
        [Test]
        public void ParseGetLiveChat_ReloadContinuationTest()
        {
            var data = Tools.GetSampleData("GetLiveChatJson_Reload.txt");
            var (c, d, t) = YouTubeLiveSitePlugin.Test2.Tools.ParseGetLiveChat(data);
            Assert.AreEqual("0ofMyANOGjRDaU1TSVFvWVZVTmFabDlmWldoc1EwVkNVRzl3TFY5emJHUndRbFZSRWdVdmJHbDJaU0FDMAFoBIIBBAgEEACIAQCgAcX4n-vnw-MC", c.Continuation);
            Assert.AreEqual(0, c.TimeoutMs);
            Assert.AreEqual("QUFFLUhqbGJxbGEzNWE1TGg0U2FYUzcyZXIyeF9hZTUxd3xBQ3Jtc0tuTEgtbGpITWdSQWlvUVZaZmJTU1IxQUMwQmw2djVUbXVLM19YMDFqeVpad0xxa1RPdjZOQ3A1MEtabk5xLWhUUExPdzMtVC16Z1ZHWE9ybjlZZ1hKeVo0R3ZCbzhGT2cta3pmLUtTeTQ0dlBPOHNCT01GSG1uZUpaUk5pa00tamhMeGdJclFmX1VtQU50UmgzdjhCUndMc0NjRnc=", t);
        }
    }
}
