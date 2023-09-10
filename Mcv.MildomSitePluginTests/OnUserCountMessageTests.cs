using MildomSitePlugin;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Mcv.MildomSitePluginTests;
[TestFixture]
internal class OnUserCountMessageTests
{
    [Test]
    public void Test()
    {
        var raw = "{\"cmd\": \"onUserCount\", \"roomId\": 13443210, \"type\": 3, \"userCount\": 14}";
        dynamic? d = JsonConvert.DeserializeObject(raw);
        var a = OnUserCountMessage.Create(d, raw);
        Assert.AreEqual(14, a.UserCount);
    }
}
