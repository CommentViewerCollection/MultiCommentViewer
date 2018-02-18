using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SitePlugin;
using System.Net;
using Codeplex.Data;
using System.Web;
namespace YouTubeLiveSitePlugin.Test2
{
    class MetadataProvider
    {
        public event EventHandler<IMetadata> MetadataReceived;
        public async Task ReceiveAsync(string ytCfg, string vid, CookieContainer cc)
        {
            var ytCfgJson = DynamicJson.Parse(ytCfg);
            var token = ytCfgJson.XSRF_TOKEN;
            var encodedToken = HttpUtility.UrlEncode(token);
            
            //cc = new CookieContainer();
            ////未ログイン時はVISITOR_INFO1_LIVE,YSCが必要
            ////ログイン時はVISITOR_INFO1_LIVE,SID,SSID,LOGIN_INFO,YSCが必要

            //cc.Add(new Cookie("VISITOR_INFO1_LIVE","CmktIkL30b4") { Domain = "youtube.com", Path = "/" });
            //cc.Add(new Cookie("SID", "ugWXLlULI9HGNxTWGU4vyN555oOvwrC2d5bT90NaOvkK1SDAeB7Xq5IE1F4M1HBOSAkZIw.") { Domain = "youtube.com", Path = "/" });
            //cc.Add(new Cookie("SSID", "AxBXJ3vfxlrtF1XUx") { Domain = "youtube.com", Path = "/" });
            //cc.Add(new Cookie("LOGIN_INFO", "AFmmF2swRQIhAPZqpodBCixkLZpSfYDZQrEHZppIFmdrKz3ugfznTttVAiA1sApJQGXTTweqm_1HuiK06p1058AU80ICiLc0jIrkPQ:QUQ3MjNmd0VlU2Zfd2J3cUNyb2JwajBxVmRoQm9RekRXcldzZHkwT25SaFBIUlhlakdwellRY1M5Q3RyMDh3Z0RKZDY3eHJZZFVucHdqaGphWnBGUXl1TjE1TllqNklKbjlYaHRpQzJWaTdfQkIyNnNYMnhLOGtEU25QTW5PbV9YUXI5UGZJRG1xNjQ5UlFCX3dnUzY5R1RCVmVCcG5kamNxQktCdWNtNTdpeVcwUHl4TXFNMER4allmcnBfTUVjNUVLYWR6MzVWMC1jcGN5YndlRmlFNHdMWTUySkljNDhrVld5OG1DOHNhcHJyX1RMSlp0RVJFcw==") { Domain = "youtube.com", Path = "/" });
            //cc.Add(new Cookie("YSC", "IMx1V4Dqq4Q") { Domain = "youtube.com", Path = "/" });


            var url = "https://www.youtube.com/service_ajax?name=updatedMetadataEndpoint";
            //application/json; charset=UTF-8
            var wc = new MyWebClient(cc);
            wc.Headers[HttpRequestHeader.UserAgent] = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/64.0.3282.167 Safari/537.36";
            wc.Headers["origin"] = "https://www.youtube.com";
            wc.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
            //wc.Encoding = Encoding.UTF8;
            wc.Headers[HttpRequestHeader.Accept] = "*/*";
            //wc.Headers[HttpRequestHeader.AcceptEncoding] = "gzip, deflate, br";

            var encoded = "sej=%7B%22clickTrackingParams%22%3A%22CIIBEMyrARgAIhMI5-DT8siv2QIVGrFYCh1xsQuhKPgd%22%2C%22commandMetadata%22%3A%7B%22webCommandMetadata%22%3A%7B%22url%22%3A%22%2Fservice_ajax%22%2C%22sendPost%22%3Atrue%7D%7D%2C%22updatedMetadataEndpoint%22%3A%7B%22videoId%22%3A%22" + vid + "%22%7D%7D&csn=iX-JWqf4Mpri4gLx4q6ICg&session_token=" + encodedToken;
            //var s = "continuation=-of5rQMTGgtrZHQwVDNZMHJfcyCHoaXUBQ%253D%253D&sej=%7B%22clickTrackingParams%22%3A%22CIQBEMyrARgAIhMI8bDc7JGv2QIVhjdgCh1PeQPIKPgd%22%2C%22commandMetadata%22%3A%7B%22webCommandMetadata%22%3A%7B%22url%22%3A%22%2Fservice_ajax%22%2C%22sendPost%22%3Atrue%7D%7D%2C%22updatedMetadataEndpoint%22%3A%7B%22videoId%22%3A%22kdt0T3Y0r_s%22%7D%7D&csn=0UWJWrGEG4bvgAPP8o3ADA&session_token=QUFFLUhqa2kxY09JN0RHRktTeTF3QVJOS0hTRXZJVDZiUXxBQ3Jtc0tsaU9ZUURKbjZmdzNCdTN1am56UjRPVlhIdVdsYk9pbnpTamY3em5PV0tSS0N6aGdtdXRoOUFBLThKMW5VZEFhWEc4M3N4cUpKd1lKaWRmSGdnX0tMQXlXMW4wcEZVTkY1a2w0WGRkdWpzOVRzeXdxTm0wQjRobjNXcmowSklTOWhDejZRd0otUFpFMlhaM2tmakNBWDV2Zkh0S0x6UG5tZVVrWkkyVzlwSFBycy1WRmM%3D";
            //var res = await wc.UploadStringTaskAsync(url, s);
            //var res = wc.UploadString(url,"POST", encoded);

            string encodedContinuation = "";
            while (true)
            {
                //2018/02/19　1周目しか成功しない。それ以降は400が返って来てしまう。
                var bytes = await wc.UploadDataTaskAsync(url, Encoding.UTF8.GetBytes(encodedContinuation + encoded));
                var res = Encoding.UTF8.GetString(bytes);
                var d = DynamicJson.Parse(res);
                string continuation;
                int timeoutMs;
                if (d.code != "SUCCESS")
                {

                }
                if (d.data.continuation.IsDefined("invalidationContinuationData"))
                {
                    throw new ParseException(res);
                }
                else
                {
                    continuation = d.data.continuation.timedContinuationData.continuation;
                    //encodedContinuation = "continuation=" + HttpUtility.UrlEncode(continuation) + "&";
                    encodedContinuation = "continuation=" + continuation + "&";
                    timeoutMs = (int)d.data.continuation.timedContinuationData.timeoutMs;
                }
                var actions = d.data.actions;
                var metadata = new Metadata();
                foreach (var action in actions)
                {
                    if (action.IsDefined("updateViewershipAction"))
                    {
                        var re = action.updateViewershipAction.viewership.videoViewCountRenderer;
                        var isLive = re.isLive;
                        metadata.IsLive = isLive;
                        var viewCount = re.viewCount.runs[0].text;//～人が視聴中
                        metadata.CurrentViewers = viewCount;
                    }
                    else if (action.IsDefined("updateTitleAction"))
                    {
                        var title = action.updateTitleAction.title.simpleText;
                        metadata.Title = title;
                    }
                    else if (action.IsDefined("updateDescriptionAction"))
                    {

                    }
                }
                MetadataReceived?.Invoke(this, metadata);
                await Task.Delay(timeoutMs);
            }
            return;
        }
        public void Disconnect()
        {

        }
        public MetadataProvider()
        {

        }
    }
}
