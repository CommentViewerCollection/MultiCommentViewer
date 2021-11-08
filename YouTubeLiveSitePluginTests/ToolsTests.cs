using NUnit.Framework;

namespace YouTubeLiveSitePluginTests
{
    [TestFixture]
    class YtInitialDataTests
    {
        [Test]
        public void Test()
        {
            var data = Tools.GetSampleData("YtInitialData.txt");
            var ytInitialData = new YouTubeLiveSitePlugin.Next.YtInitialDataOld(data);
        }
    }
    //[TestFixture]
    //class GetLiveChatTests
    //{
    //    [Test]
    //    public void Test()
    //    {
    //        var data = Tools.GetSampleData("GetLiveChat_20210108.txt");
    //        var ytInitialData = new YouTubeLiveSitePlugin.Next.GetLiveChat(data);
    //        var actions = ytInitialData.GetActions();
    //        Assert.AreEqual("0ofMyAOHAhqyAUNqZ0tEUW9MVUc1bFZrVTVTblU1UXpBcUp3b1lWVU53TFRWME9WTnlUMUYzV0UxVk4ybEphbEZtUVZKbkVndFFibVZXUlRsS2RUbERNQnBEcXJuQnZRRTlDanRvZEhSd2N6b3ZMM2QzZHk1NWIzVjBkV0psTG1OdmJTOXNhWFpsWDJOb1lYUV9hWE5mY0c5d2IzVjBQVEVtZGoxUWJtVldSVGxLZFRsRE1DQUNLQUUlM0Qoi5LG78WM7gIwADgAQAFKGwgAEAAYACAAOgBAAEoAUJ3y_e_FjO4CWAN4AFDk9evvxYzuAljLp9bSnozuAmgBggECCAGIAQCgAamJg_DFjO4C", ytInitialData.GetContinuation().Continuation);
    //    }
    //    [Test]
    //    public void Test1()
    //    {
    //        var data = "{\"error\": {\"code\": 403,\"message\": \"The caller does not have permission\",\"errors\": [{\"message\": \"The caller does not have permission\",\"domain\": \"global\",\"reason\": \"forbidden\"}],\"status\": \"PERMISSION_DENIED\"}}";
    //        Assert.Throws<YouTubeLiveSitePlugin.Next.GetLiveChatException>(() => new YouTubeLiveSitePlugin.Next.GetLiveChat(data));
    //    }
    //}
    [TestFixture]
    class ToolsTests
    {
        [Test]
        public void Test()
        {
            var data = Tools.GetSampleData("YtInitialData.txt");
            var ytInitialData = YouTubeLiveSitePlugin.Next.Tools.ExtractYtInitialData(data);
        }
        //[Test]
        //public void ExtractYtCfgTest()
        //{
        //    var data = Tools.GetSampleData("LiveChat.txt");
        //    var json = YouTubeLiveSitePlugin.Next.Tools.ExtractYtCfg(data);
        //    var ytCfg = new YouTubeLiveSitePlugin.Next.YtCfgOld(json);
        //    Assert.AreEqual("103208314919748213421", ytCfg.DelegatedSessionId);
        //    Assert.AreEqual("AIzaSyAO_FJ2SlqU8Q4STEHLGCilw_Y9_11qcW8", ytCfg.InnerTubeApiKey);
        //    Assert.AreEqual("CgtDYkdkSzNGYjhHdyiD-eD_BQ%3D%3D", ytCfg.VisitorData);
        //    Assert.AreEqual("{\"client\":{\"hl\":\"ja\",\"gl\":\"JP\",\"visitorData\":\"CgtDYkdkSzNGYjhHdyiD-eD_BQ%3D%3D\",\"userAgent\":\"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/87.0.4280.88 Safari/537.36,gzip(gfe)\",\"clientName\":\"WEB\",\"clientVersion\":\"2.20210107.02.01\",\"osName\":\"Windows\",\"osVersion\":\"10.0\",\"browserName\":\"Chrome\",\"browserVersion\":\"87.0.4280.88\"},\"request\":{\"sessionId\":\"6915343762157300936\"}}", ytCfg.InnerTubeContext);
        //}
        //[Test]
        //public void ExtractYtCfg_2Test()
        //{
        //    var data = Tools.GetSampleData("LiveChat_2.txt");
        //    var json = YouTubeLiveSitePlugin.Next.Tools.ExtractYtCfg(data);
        //    var ytCfg = new YouTubeLiveSitePlugin.Next.YtCfgOld(json);
        //    Assert.IsNull(ytCfg.DelegatedSessionId);
        //    Assert.AreEqual("AIzaSyAO_FJ2SlqU8Q4STEHLGCilw_Y9_11qcW8", ytCfg.InnerTubeApiKey);
        //    Assert.AreEqual("CgtLLWZ4T1hPR2dlWSjuz4CABg%3D%3D", ytCfg.VisitorData);
        //    Assert.IsTrue(ytCfg.InnerTubeContext.StartsWith("{\"client\":{"));
        //}
        //[Test]
        //public void ExtractYtInitialFromSubscribedChannelHtmlTest()
        //{
        //    var data = "}</script><script nonce=\"orPyHr13z9j4Y/4tOnK69A\">var ytInitialData = {\"responseContext\":\"GEpHycQwjdT6A\"}]}}};</script><tit";
        //    var s = YouTubeLiveSitePlugin.Test2.Tools.ExtractYtInitialDataFromChannelHtml(data);
        //    Assert.IsTrue(!string.IsNullOrEmpty(s));
        //}
        //[Test]
        //public void YtInitialDataNotContainsContentsTest()
        //{
        //    var data = "{\"responseContext\":{\"errors\":{\"error\":[{\"domain\":\"gdata.CoreErrorDomain\",\"code\":\"INVALID_VALUE\",\"debugInfo\":\"Error decrypting and parsing the live chat ID.\",\"externalErrorMessage\":\"不明なエラーです。\"}]},\"serviceTrackingParams\":[{\"service\":\"CSI\",\"params\":[{\"key\":\"GetLiveChat_rid\",\"value\":\"0x3365759ba77f978f\"},{\"key\":\"c\",\"value\":\"WEB\"},{\"key\":\"cver\",\"value\":\"2.20190529\"},{\"key\":\"yt_li\",\"value\":\"1\"}]},{\"service\":\"GFEEDBACK\",\"params\":[{\"key\":\"e\",\"value\":\"23720702,23736685,23744176,23750984,23751767,23752869,23755886,23755898,23759224,23766102,23767634,23771992,23785333,23788845,23793834,23794471,23799777,23804281,23804294,23805410,23806435,23808949,23809331,23810273,23811378,23811593,23812530,23812566,23813310,23813548,23813622,23813949,23814199,23814507,23815144,23815164,23815172,23815485,23815949,23817343,23817794,23817825,23818213,9407610,9441381,9449243,9471235\"},{\"key\":\"logged_in\",\"value\":\"1\"}]},{\"service\":\"GUIDED_HELP\",\"params\":[{\"key\":\"creator_channel_id\",\"value\":\"UCK6F1ecql0T_9hHGTw7heBA\"},{\"key\":\"logged_in\",\"value\":\"1\"}]},{\"service\":\"ECATCHER\",\"params\":[{\"key\":\"client.name\",\"value\":\"WEB\"},{\"key\":\"client.version\",\"value\":\"2.20190529\"},{\"key\":\"innertube.build.changelist\",\"value\":\"250485423\"},{\"key\":\"innertube.build.experiments.source_version\",\"value\":\"250547910\"},{\"key\":\"innertube.build.label\",\"value\":\"youtube.ytfe.innertube_20190528_7_RC1\"},{\"key\":\"innertube.build.timestamp\",\"value\":\"1559140061\"},{\"key\":\"innertube.build.variants.checksum\",\"value\":\"7e46d96e46a45788f840d135c2cf4890\"},{\"key\":\"innertube.run.job\",\"value\":\"ytfe-innertube-replica-only.ytfe\"}]}],\"webResponseContextExtensionData\":{\"ytConfigData\":{\"csn\":\"4wLwXOyiG5OPgAOH4LYI\",\"visitorData\":\"CgtpTXJTMXZJR3ZLayjjhcDnBQ%3D%3D\",\"sessionIndex\":1}}},\"trackingParams\":\"CAAQ0b4BIhMIrKDMwNTD4gIVkwdgCh0HsA0B\"}";
        //    Assert.Throws<YouTubeLiveSitePlugin.Test2.YouTubeLiveServerErrorException>(() => YouTubeLiveSitePlugin.Test2.Tools.ParseYtInitialData(data));
        //}
        [Test]
        public void ExtractYtInitialDataFromChannelHtmlTest()
        {
            var data = "window[\"ytInitialData\"] = JSON.parse(\"{\\\"a\\\":\\\"b\\\"}\");abc";
            var s = YouTubeLiveSitePlugin.Test2.Tools.ExtractYtInitialDataFromChannelHtml(data);
            Assert.AreEqual("{\"a\":\"b\"}", s);
        }
        [Test]
        public void Test1()
        {
            var data = Tools.GetSampleData("LivePage_ytInitialPlayerResponse.txt");
            var liveBroadcastDetails = YouTubeLiveSitePlugin.Test2.Tools.ExtractLiveBroadcastDetailsFromLivePage(data);
            Assert.IsTrue(!string.IsNullOrEmpty(liveBroadcastDetails));
        }
        [Test]
        public void Test2()
        {
            var data = Tools.GetSampleData("LivePage_ytplayerconfig.txt");
            var liveBroadcastDetails = YouTubeLiveSitePlugin.Test2.Tools.ExtractLiveBroadcastDetailsFromLivePage(data);
            Assert.IsTrue(!string.IsNullOrEmpty(liveBroadcastDetails));
        }
    }
}
