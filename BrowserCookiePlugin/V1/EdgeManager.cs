using Mcv.PluginV2;
using System;
namespace ryu_s.BrowserCookie
{
    public class EdgeManager : ChromeManager
    {
        public override BrowserType Type => BrowserType.Edge;
        protected override string ChromeSettingsDirPath => Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Microsoft\Edge\User Data\";
    }
}
