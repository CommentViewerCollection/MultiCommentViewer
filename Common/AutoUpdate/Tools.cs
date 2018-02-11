using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Windows.Media.Imaging;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;
namespace Common.AutoUpdate
{
    public static class Tools
    {
        private class VersionInfo
        {
            public string name { get; set; }
            public string version { get; set; }
            public string url { get; set; }
        }
        public static async Task<LatestVersionInfo> GetLatestVersionInfo(string name)
        {
            name = name.ToLower();
            //APIが確定するまでアダプタを置いている。ここから本当のAPIを取得する。
            var permUrl = @"https://ryu-s.github.io/" + name + "_latest";

            var wc = new WebClient();
            var api = await wc.DownloadStringTaskAsync(permUrl);

            var jsonStr = await wc.DownloadStringTaskAsync(api);
            var json = Newtonsoft.Json.JsonConvert.DeserializeObject<VersionInfo>(jsonStr);

            var asm = System.Reflection.Assembly.GetExecutingAssembly();
            var ver = asm.GetName().Version;
            return new LatestVersionInfo(json.version, json.url);
        }
        /// <summary>
        /// マウス位置を取得
        /// </summary>
        /// <returns></returns>
        public static Point GetMousePos()
        {
            var element = Application.Current.MainWindow;
            Point mousePositionInApp = System.Windows.Input.Mouse.GetPosition(element);
            Point mousePositionInScreenCoordinates = element.PointToScreen(mousePositionInApp);
            return mousePositionInScreenCoordinates;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="mousePoint"></param>
        /// <param name="view"></param>
        /// <returns></returns>
        public static Point GetShowPos(Point mousePoint, Window view)
        {
            var left = mousePoint.X - view.Width / 2;
            if (left < 0)
                left = 0;
            var top = mousePoint.Y = view.Height / 2;
            if (top < 0)
                top = 0;
            return new Point(left, top);
        }
    }
}
